using Godot;
using CaveGen.Types;
using CaveGen.Constants;

using System.Diagnostics;

[GlobalClass]
public partial class MeshGen : MeshInstance3D
{
    Config cfgSrc;
    Config.DataPacked cfg;
    Vec3i numCells;
    float[] noiseSamples;

    public void SetConfig(Config _cfg)
    {
        cfgSrc = _cfg;
        cfg = cfgSrc.data;
    }

    public void Generate(Noise noise, Noise borderNoise)
    {
        Stopwatch stopwatch = new();

        cfg = cfgSrc.data;

        stopwatch.Restart();
        ProcessNoise(noise, borderNoise);
        stopwatch.Stop();
        var elapsed = stopwatch.Elapsed.TotalMilliseconds;
        GD.Print("ProcessNoise() - " + elapsed + "ms");

        stopwatch.Restart();
        MarchCubes();
        stopwatch.Stop();
        elapsed = stopwatch.Elapsed.TotalMilliseconds;
        GD.Print("MarchCubes() - " + elapsed + "ms");
    }

    void ProcessNoise(Noise noise, Noise borderNoise)
    {
        numCells = new Vec3i(
            Mathf.FloorToInt(cfg.RoomWidth / cfg.CellSize),
            Mathf.FloorToInt(cfg.RoomHeight / cfg.CellSize),
            Mathf.FloorToInt(cfg.RoomDepth / cfg.CellSize)
        );
        if (noiseSamples == null || noiseSamples.Length != numCells.x * numCells.y * numCells.z)
        {
            noiseSamples = new float[numCells.x * numCells.y * numCells.z];
        }
        float[] noiseBuffer = new float[numCells.x * numCells.y * numCells.z];
        var minV = float.PositiveInfinity;
        var maxV = float.NegativeInfinity;

        #region BaseNoise
        // first pass - initialize && sample all noise values in grid
        for (int z = 0; z < numCells.z; z++)
        {
            for (int y = 0; y < numCells.y; y++)
            {
                for (int x = 0; x < numCells.x; x++)
                {
                    int i = x + y * numCells.x + z * numCells.y * numCells.z;
                    noiseBuffer[i] = 0f;
                    noiseSamples[i] = 0f;
                    if (cfg.ShowNoise)
                    {
                        float val = noise.GetNoise3D(x * cfg.CellSize, y * cfg.CellSize, z * cfg.CellSize);
                        noiseSamples[i] = val;
                        if (val < minV) { minV = val; }
                        if (val > maxV) { maxV = val; }
                    }
                }
            }
        }
        // second pass - normalize noise values
        if (cfg.ShowNoise)
        {
            for (int z = 0; z < numCells.z; z++)
            {
                for (int y = 0; y < numCells.y; y++)
                {
                    for (int x = 0; x < numCells.x; x++)
                    {
                        int i = x + y * numCells.x + z * numCells.y * numCells.z;
                        float val;
                        val = Mathf.InverseLerp(minV, maxV, noiseSamples[i]);
                        val = Mathf.Clamp(val, 0f, 1f);
                        // apply noise curve
                        var valEaseIn = Easing.InCubic(val);
                        var valEaseOut = Easing.OutCubic(val);
                        val = Mathf.Lerp(valEaseIn, val, Mathf.Clamp(cfg.Curve, 0, 1));
                        val = Mathf.Lerp(val, valEaseOut, Mathf.Clamp(cfg.Curve - 1, 0, 1));
                        var zeroValue = Mathf.Min(noiseSamples[i], cfg.IsoValue - 0.1f);
                        zeroValue = Mathf.Lerp(0f, zeroValue, cfg.FalloffAboveCeiling);
                        noiseSamples[i] = Mathf.Lerp(val, zeroValue, GetAboveCeilAmount(y));
                    }
                }
            }
        }
        // third pass - apply bounds, borders
        for (int z = 0; z < numCells.z; z++)
        {
            for (int y = 0; y < numCells.y; y++)
            {
                for (int x = 0; x < numCells.x; x++)
                {
                    int i = x + y * numCells.x + z * numCells.y * numCells.z;
                    // apply bounds
                    if (IsAtBoundaryXZ(x, z) && cfg.ShowOuterWalls || IsAtBoundaryY(y))
                    {
                        noiseSamples[i] = Mathf.Min(noiseSamples[i], cfg.IsoValue - 0.1f);
                        continue;
                    }
                    // apply border
                    if (IsAtBorder(x, y, z) && (!cfg.UseBorderNoise || IsAtBorderEdge(x, y, z)))
                    {
                        noiseSamples[i] = Mathf.Min(noiseSamples[i], cfg.IsoValue - 0.1f);
                        if (IsBelowCeiling(y) && cfg.ShowBorder)
                        {
                            noiseSamples[i] = Mathf.Max(noiseSamples[i], cfg.IsoValue + 0.1f);
                        }
                        continue;
                    }
                    // apply falloff to noise above ceil && close to border
                    if (!IsBelowCeiling(y) && cfg.FalloffNearBorder > 0)
                    {
                        var dist = DistFromBorder(x, y, z);
                        var t = Mathf.InverseLerp(0f, (float)cfg.FalloffNearBorder, dist - 1) * (1 - GetAboveCeilAmount(y));
                        var zeroValue = Mathf.Min(noiseSamples[i], cfg.IsoValue - 0.1f);
                        noiseSamples[i] = Mathf.Lerp(zeroValue, noiseSamples[i], Mathf.Clamp(t, 0, 1));
                    }
                }
            }
        }
        #endregion BaseNoise

        // apply border noise
        #region ApplyBorderNoise
        if (cfg.ShowBorder && cfg.UseBorderNoise && cfg.BorderSize > 1)
        {
            var ceiling = GetCeiling();
            // first pass - sample and normalize border noise on x plane
            // step 0 => sample noise at x=0
            // step 1 => normalize at x=0
            // step 2 => sample noise at x=max
            // step 3 => normalize at x=max
            for (int step = 0; step < 4; step++)
            {
                if (step == 0 || step == 2)
                {
                    minV = float.PositiveInfinity;
                    maxV = float.NegativeInfinity;
                }
                for (int z = 0; z < numCells.z; z++)
                {
                    for (int y = 0; y < numCells.y && y <= ceiling; y++)
                    {
                        int x = 0;
                        if (step >= 2)
                        {
                            x = numCells.x - 1;
                        }
                        if (step == 0 || step == 2)
                        {
                            var val = borderNoise.GetNoise3D(x, y, z);
                            noiseBuffer[NoiseIndex(x, y, z)] = val;
                            if (val < minV) { minV = val; }
                            if (val > maxV) { maxV = val; }
                        }
                        else
                        {
                            int i = NoiseIndex(x, y, z);
                            float val = Mathf.Clamp(Mathf.InverseLerp(minV, maxV, noiseBuffer[i]), 0f, 1f);
                            noiseBuffer[i] = val;
                        }
                    }
                }
            }
            // first pass - sample and normalize border noise on z plane
            // step 0 => sample noise at z=0
            // step 1 => normalize at z=0
            // step 2 => sample noise at z=max
            // step 3 => normalize at z=max
            for (int step = 0; step < 4; step++)
            {
                if (step == 0 || step == 2)
                {
                    minV = float.PositiveInfinity;
                    maxV = float.NegativeInfinity;
                }
                for (int x = 0; x < numCells.x; x++)
                {
                    for (int y = 0; y < numCells.y && y <= ceiling; y++)
                    {
                        int z = 0;
                        if (step >= 2)
                        {
                            z = numCells.z - 1;
                        }
                        if (step == 0 || step == 2)
                        {
                            var val = borderNoise.GetNoise3D(x, y, z);
                            noiseBuffer[NoiseIndex(x, y, z)] = val;
                            if (val < minV) { minV = val; }
                            if (val > maxV) { maxV = val; }
                        }
                        else
                        {
                            int i = NoiseIndex(x, y, z);
                            float val = Mathf.Clamp(Mathf.InverseLerp(minV, maxV, noiseBuffer[i]), 0f, 1f);
                            noiseBuffer[i] = val;
                        }
                    }
                }
            }
            // third pass: apply noise to border points
            // side: x=0
            for (int z = 2; z < numCells.z - 2; z++)
            {
                for (int y = 2; y < numCells.y && y <= ceiling; y++)
                {
                    for (int x = 2; x < cfg.BorderSize + 1; x++)
                    {
                        float n0 = noiseBuffer[NoiseIndex(0, y, z)];
                        // use surrounding cells for avg smoothing
                        float kernel = 0f;
                        if (cfg.SmoothBorderNoise > 0)
                        {
                            for (int j = -1; j <= 1; j++)
                            {
                                for (int k = -1; k <= 1; k++)
                                {
                                    if (j == 0 && k == 0) continue;
                                    kernel += noiseBuffer[NoiseIndex(0, y + j, z + k)] * 0.125f;
                                }
                            }
                        }
                        float val = n0 * Mathf.Lerp(1f, 0.1f, cfg.SmoothBorderNoise)
                            + kernel * Mathf.Lerp(0f, 0.9f, cfg.SmoothBorderNoise);
                        int i = NoiseIndex(x, y, z);
                        float t = (x - 1) / (float)cfg.BorderSize;
                        float strength = Mathf.Lerp(1f, cfg.IsoValue + 0.001f, t);
                        noiseSamples[i] = Mathf.Max(noiseSamples[i], val * strength);
                    }
                }
            }
            // side: z=max
            for (int x = 2; x < numCells.x - 2; x++)
            {
                for (int y = 2; y < numCells.y && y <= ceiling; y++)
                {
                    for (int z = numCells.z - 3; z > numCells.z - cfg.BorderSize - 2; z--)
                    {
                        float n0 = noiseBuffer[NoiseIndex(x, y, numCells.z - 1)];
                        // use surrounding cells for avg smoothing
                        float kernel = 0f;
                        if (cfg.SmoothBorderNoise > 0)
                        {
                            for (int j = -1; j <= 1; j++)
                            {
                                for (int k = -1; k <= 1; k++)
                                {
                                    if (j == 0 && k == 0) continue;
                                    kernel += noiseBuffer[NoiseIndex(x + j, y + k, numCells.z - 1)] * 0.125f;
                                }
                            }
                        }
                        float val = n0 * Mathf.Lerp(1f, 0.1f, cfg.SmoothBorderNoise)
                            + kernel * Mathf.Lerp(0f, 0.9f, cfg.SmoothBorderNoise);
                        int i = NoiseIndex(x, y, z);
                        float t = (numCells.z - 2 - z) / (float)(cfg.BorderSize - 1);
                        float strength = Mathf.Lerp(1f, cfg.IsoValue + 0.001f, t);
                        noiseSamples[i] = Mathf.Max(noiseSamples[i], val * strength);
                    }
                }
            }
            // side: x=max
            for (int z = 2; z < numCells.z - 2; z++)
            {
                for (int y = 2; y < numCells.y && y <= ceiling; y++)
                {
                    for (int x = numCells.x - 3; x > numCells.x - cfg.BorderSize - 2; x--)
                    {
                        float n0 = noiseBuffer[NoiseIndex(numCells.x - 1, y, z)];
                        // use surrounding cells for avg smoothing
                        float kernel = 0f;
                        if (cfg.SmoothBorderNoise > 0)
                        {
                            for (int j = -1; j <= 1; j++)
                            {
                                for (int k = -1; k <= 1; k++)
                                {
                                    if (j == 0 && k == 0) continue;
                                    kernel += noiseBuffer[NoiseIndex(numCells.x - 1, y + j, z + k)] * 0.125f;
                                }
                            }
                        }
                        float val = n0 * Mathf.Lerp(1f, 0.1f, cfg.SmoothBorderNoise)
                            + kernel * Mathf.Lerp(0f, 0.9f, cfg.SmoothBorderNoise);
                        int i = NoiseIndex(x, y, z);
                        float t = (numCells.x - 2 - x) / (float)(cfg.BorderSize - 1);
                        float strength = Mathf.Lerp(1f, cfg.IsoValue + 0.001f, t);
                        noiseSamples[i] = Mathf.Max(noiseSamples[i], val * strength);
                    }
                }
            }
            // side: z=0
            for (int x = 2; x < numCells.x - 2; x++)
            {
                for (int y = 2; y < numCells.y && y <= ceiling; y++)
                {
                    for (int z = 2; z < cfg.BorderSize + 1; z++)
                    {
                        float n0 = noiseBuffer[NoiseIndex(x, y, 0)];
                        // use surrounding cells for avg smoothing
                        float kernel = 0f;
                        for (int j = -1; j <= 1; j++)
                        {
                            for (int k = -1; k <= 1; k++)
                            {
                                if (j == 0 && k == 0) continue;
                                kernel += noiseBuffer[NoiseIndex(x + j, y + k, 0)] * 0.125f;
                            }
                        }
                        float val = n0 * Mathf.Lerp(1f, 0.1f, cfg.SmoothBorderNoise)
                            + kernel * Mathf.Lerp(0f, 0.9f, cfg.SmoothBorderNoise);
                        int i = NoiseIndex(x, y, z);
                        float t = (z - 1) / (float)cfg.BorderSize;
                        float strength = Mathf.Lerp(1f, cfg.IsoValue + 0.001f, t);
                        noiseSamples[i] = Mathf.Max(noiseSamples[i], val * strength);
                    }
                }
            }
        }
        #endregion ApplyBorderNoise
    }

    int NoiseIndex(int x, int y, int z)
    {
        return x + y * numCells.x + z * numCells.y * numCells.z;
    }

    float GetAboveCeilAmount(int y)
    {
        if (cfg.Ceiling >= 1)
        {
            return 0f;
        }
        float ceiling = GetCeiling();
        float maxY = numCells.y - 1 - cfg.BorderSize * 2;
        if (Mathf.IsZeroApprox(Mathf.Abs(ceiling - maxY))) return 0f;
        maxY = Mathf.Lerp(ceiling, maxY, cfg.FalloffAboveCeiling);
        if (y < ceiling)
        {
            return 0f;
        }
        if (y >= maxY)
        {
            return 1f;
        }
        if (ceiling >= maxY || Mathf.IsZeroApprox(Mathf.Abs(ceiling - maxY)))
        {
            return 1f;
        }
        return Mathf.Clamp(Mathf.InverseLerp(ceiling, maxY, y), 0f, 1f);
    }

    bool IsAtBoundaryXZ(int x, int z)
    {
        return (
            x == 0 ||
            z == 0 ||
            x == numCells.x - 1 ||
            z == numCells.z - 1
        );
    }

    bool IsAtBoundaryY(int y)
    {
        return (
            y == 0 ||
            y == numCells.y - 1
        );
    }

    bool IsAtBorder(int x, int y, int z)
    {
        return (
            x <= cfg.BorderSize ||
            y <= cfg.BorderSize ||
            z <= cfg.BorderSize ||
            x >= numCells.x - 1 - cfg.BorderSize ||
            z >= numCells.z - 1 - cfg.BorderSize
        );
    }

    bool IsAtBorderEdge(int x, int y, int z)
    {
        return (
            x == 1 ||
            y == 1 ||
            z == 1 ||
            x == numCells.x - 2 ||
            z == numCells.z - 2
        );
    }

    bool IsBelowCeiling(int y)
    {
        if (cfg.Ceiling >= 1)
        {
            return true;
        }
        return y <= GetCeiling();
    }

    float GetCeiling()
    {
        return Mathf.Min(cfg.Ceiling * (numCells.y - 1), numCells.y - 2);
    }

    int DistFromBorder(int x, int y, int z)
    {
        int distX = Mathf.Min(Mathf.Abs(x - cfg.BorderSize), Mathf.Abs(numCells.x - 1 - cfg.BorderSize - x));
        int distY = Mathf.Abs(y - cfg.BorderSize);
        int distZ = Mathf.Min(Mathf.Abs(z - cfg.BorderSize), Mathf.Abs(numCells.z - 1 - cfg.BorderSize - z));
        return Mathf.Min(Mathf.Min(distX, distY), distZ);
    }

    #region MarchCubes
    void MarchCubes()
    {
        // Mesh = new ImmediateMesh();
        Assert(Mesh != null);
        Assert(Mesh is ImmediateMesh);
        (Mesh as ImmediateMesh).ClearSurfaces();
        (Mesh as ImmediateMesh).SurfaceBegin(Mesh.PrimitiveType.Triangles);
        Vector3[] points = new Vector3[3];
        for (int z = 0; z < numCells.z - 1; z++)
        {
            for (int y = 0; y < numCells.y - 1; y++)
            {
                for (int x = 0; x < numCells.x - 1; x++)
                {
                    int pointIndex = 0;
                    int idx = GetTriangulation(x, y, z);
                    var uv = new Vector2(
                        (x + 1) / (float)numCells.x,
                        Mathf.Max(
                            (y + 1) / (float)numCells.y,
                            (z + 1) / (float)numCells.z
                        )
                    );
                    var edgeIndices = Constants.TRIANGULATIONS[idx];
                    foreach (var edgeIdx in edgeIndices)
                    {
                        if (edgeIdx < 0) break;
                        var (p0, p1) = Constants.EDGES[edgeIdx];
                        var (x0, y0, z0) = Constants.POINTS[p0];
                        var (x1, y1, z1) = Constants.POINTS[p1];
                        var a = new Vec3i(x + x0, y + y0, z + z0);
                        var b = new Vec3i(x + x1, y + y1, z + z1);
                        var p = InterpolatePoints(a, b);
                        points[pointIndex] = new Vector3(p.x, p.y, p.z);
                        pointIndex++;
                        // if we have 3 points, add triangle and reset
                        if (pointIndex == 3)
                        {
                            AddTriangleToMesh(points, uv);
                            pointIndex = 0;
                        }
                    }
                }
            }
        }
        (Mesh as ImmediateMesh).SurfaceEnd();
    }
    #endregion MarchCubes

    void AddTriangleToMesh(Vector3[] points, Vector2 uv)
    {
        Assert(points.Length == 3);
        var p1 = points[0];
        var p2 = points[1];
        var p3 = points[2];
        var normal = -(p2 - p1).Cross(p3 - p1).Normalized();
        foreach (var point in points)
        {
            var x = point.X * cfg.CellSize;
            var y = point.Y * cfg.CellSize;
            var z = point.Z * cfg.CellSize;
            (Mesh as ImmediateMesh).SurfaceSetUV(uv);
            (Mesh as ImmediateMesh).SurfaceSetNormal(normal);
            (Mesh as ImmediateMesh).SurfaceAddVertex(GlobalPosition + new Vector3(x, y, z));
        }
    }

    int GetTriangulation(int x, int y, int z)
    {
        int idx = 0;
        idx |= (IsPointActive(x + 0, y + 0, z + 0) ? 1 : 0) << 0;
        idx |= (IsPointActive(x + 0, y + 0, z + 1) ? 1 : 0) << 1;
        idx |= (IsPointActive(x + 1, y + 0, z + 1) ? 1 : 0) << 2;
        idx |= (IsPointActive(x + 1, y + 0, z + 0) ? 1 : 0) << 3;
        idx |= (IsPointActive(x + 0, y + 1, z + 0) ? 1 : 0) << 4;
        idx |= (IsPointActive(x + 0, y + 1, z + 1) ? 1 : 0) << 5;
        idx |= (IsPointActive(x + 1, y + 1, z + 1) ? 1 : 0) << 6;
        idx |= (IsPointActive(x + 1, y + 1, z + 0) ? 1 : 0) << 7;
        return idx;
    }

    bool IsPointActive(int x, int y, int z)
    {
        var val = GetNoiseValue(x, y, z);
        var active = val >= cfg.IsoValue;
        if (active && !IsBelowCeiling(y) && IsPointOrphan(x, y, z))
        {
            active = false;
        }
        return active;
    }

    // TODO: replace with flood-fill from known border
    bool IsPointOrphan(int x, int y, int z)
    {
        if (!cfg.RemoveOrphans) return false;
        // walk down from y to slightly below the ceiling, checking if any gaps
        for (int y2 = y - 1; y2 >= Mathf.Floor(numCells.y * cfg.Ceiling) - 2; y2--)
        {
            var val = GetNoiseValue(x, y2, z);
            var active = val > cfg.IsoValue;
            if (!active) return true;
        }
        return false;
    }

    void Assert(bool condition, string msg = "Assertion failed")
    {
        if (!condition) throw new System.Exception(msg);
    }

    float GetNoiseValue(int x, int y, int z)
    {
        int i = x + y * numCells.x + z * numCells.y * numCells.x;
        Assert(i >= 0);
        Assert(i < noiseSamples.Length);
        return noiseSamples[i];
    }

    // source: https://cs.stackexchange.com/a/71116
    Vec3 InterpolatePoints(Vec3i a, Vec3i b)
    {
        // if one of the points is on a boundary plane, return that point so that our room meshes line up perfectly.
        bool aBound = IsAtBoundaryXZ(a.x, a.z);
        bool bBound = IsAtBoundaryXZ(b.x, b.z);
        bool onSamePlane = a.x == b.x || a.z == b.z;
        if (aBound && !(aBound && bBound && onSamePlane))
        {
            return new Vec3(a);
        }
        if (bBound && !(aBound && bBound && onSamePlane))
        {
            return new Vec3(b);
        }
        if (!cfg.Interpolate)
        {
            return (a + b) * 0.5f;
        }
        var noise_a = GetNoiseValue(a.x, a.y, a.z);
        var noise_b = GetNoiseValue(b.x, b.y, b.z);
        Assert(noise_a >= cfg.IsoValue || noise_b >= cfg.IsoValue);
        if (Mathf.IsZeroApprox(Mathf.Abs(cfg.IsoValue - noise_a)))
        {
            return new Vec3(a);
        }
        if (Mathf.IsZeroApprox(Mathf.Abs(cfg.IsoValue - noise_b)))
        {
            return new Vec3(b);
        }
        if (Mathf.IsZeroApprox(Mathf.Abs(noise_a - noise_b)))
        {
            return (a + b) * 0.5f;
        }
        float mu = (cfg.IsoValue - noise_a) / (noise_b - noise_a);
        mu = Mathf.Clamp(mu, 0f, 1f);
        var p = Vec3.ZERO;
        p.x = a.x + mu * (b.x - a.x);
        p.y = a.y + mu * (b.y - a.y);
        p.z = a.z + mu * (b.z - a.z);
        return p;
    }

    // void DebugNoiseGrid()
    // {
    //     ImmediateMesh mesh = new();
    //     Mesh = mesh;
    //     mesh.SurfaceBegin(Mesh.PrimitiveType.Points);
    //     for (int z = 0; z < numCells.z; z++)
    //     {
    //         for (int y = 0; y < numCells.y; y++)
    //         {
    //             for (int x = 0; x < numCells.x; x++)
    //             {
    //                 var active = IsPointActive(x, y, z);
    //                 var color = new Color(1, 0.5f, 0, 0.25f);
    //                 if (active)
    //                 {
    //                     color = new Color(0.1f, 0.25f, 1f, 0.8f);
    //                 }
    //                 mesh.SurfaceSetColor(color);
    //                 mesh.SurfaceAddVertex(new Vector3(x * cfg.CellSize, y * cfg.CellSize, z * cfg.CellSize));
    //             }
    //         }
    //     }
    // }
}
