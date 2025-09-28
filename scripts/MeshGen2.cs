using Godot;
using CaveGen.Types;
using CaveGen.Constants;
// using System.Runtime.CompilerServices;

using System.Diagnostics;

public partial class MeshGen2 : MeshInstance3D
{
    Config2 cfgSrc;
    Config2.DataPacked cfg;
    Vec3i numCells;
    float[] noiseSamples;

    const bool DEBUG = false;

    public void SetConfig(Config2 _cfg)
    {
        cfgSrc = _cfg;
        cfg = cfgSrc.data;
    }

    public void Generate(Noise noise)
    {
        Stopwatch stopwatch = new Stopwatch();

        cfg = cfgSrc.data;

        stopwatch.Restart();
        ProcessNoise(noise);
        stopwatch.Stop();
        var elapsed = stopwatch.Elapsed.TotalMilliseconds;
        GD.Print("ProcessNoise() - " + elapsed + "ms");

        if (DEBUG)
        {
            DebugNoiseGrid();
        }
        else
        {
            stopwatch.Restart();
            MarchCubes();
            stopwatch.Stop();
            elapsed = stopwatch.Elapsed.TotalMilliseconds;
            GD.Print("MarchCubes() - " + elapsed + "ms");
        }
    }

    void ProcessNoise(Noise noise)
    {
        numCells = new Vec3i(
            Mathf.FloorToInt(cfg.RoomWidth / cfg.CellSize) + cfg.BorderSize * 2,
            Mathf.FloorToInt(cfg.RoomHeight / cfg.CellSize) + cfg.BorderSize * 2,
            Mathf.FloorToInt(cfg.RoomDepth / cfg.CellSize) + cfg.BorderSize * 2
        );
        noiseSamples = new float[numCells.x * numCells.y * numCells.z];
        var minV = float.PositiveInfinity;
        var maxV = float.NegativeInfinity;

        // first pass - sample all noise values in grid
        for (int z = 0; z < numCells.z; z++)
        {
            for (int y = 0; y < numCells.y; y++)
            {
                for (int x = 0; x < numCells.x; x++)
                {
                    int i = x + y * numCells.x + z * numCells.y * numCells.z;
                    float val = noise.GetNoise3D(x * cfg.CellSize, y * cfg.CellSize, z * cfg.CellSize);
                    noiseSamples[i] = val;
                    if (val < minV) { minV = val; }
                    if (val > maxV) { maxV = val; }
                }
            }
        }
        // second pass - normalize noise values
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
                    noiseSamples[i] = val * GetAboveCeilMultiplier(y);
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
                    if (IsAtBoundary(x, y, z))
                    {
                        noiseSamples[i] = Mathf.Min(noiseSamples[i], cfg.IsoValue - 0.1f);
                        continue;
                    }
                    if (IsBelowCeiling(y) && IsAtBorder(x, y, z))
                    {
                        noiseSamples[i] = Mathf.Max(noiseSamples[i], cfg.IsoValue + 0.1f);
                        continue;
                    }
                }
            }
        }
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    float GetAboveCeilMultiplier(int y)
    {
        int ceiling = Mathf.FloorToInt(numCells.y * cfg.Ceiling);
        int max_y = numCells.y - 1;
        if (y < ceiling)
        {
            return 1.0f;
        }
        if (y > max_y)
        {
            return 0.0f;
        }
        if (ceiling >= max_y)
        {
            return 0.0f;
        }
        return Mathf.Clamp(Mathf.InverseLerp(max_y, ceiling, y), 0.0f, 1.0f);
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IsAtBoundary(int x, int y, int z)
    {
        return (
            x == 0 ||
            y == 0 ||
            z == 0 ||
            x == numCells.x - 1 ||
            y == numCells.y - 1 ||
            z == numCells.z - 1
        );
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IsBelowCeiling(int y)
    {
        return y + 1 < cfg.Ceiling * numCells.y;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int DistFromBorder(int x, int y, int z)
    {
        int distX = Mathf.Min(Mathf.Abs(x - cfg.BorderSize), Mathf.Abs(numCells.x - 1 - cfg.BorderSize - x));
        int distY = Mathf.Abs(y - cfg.BorderSize);
        int distZ = Mathf.Min(Mathf.Abs(z - cfg.BorderSize), Mathf.Abs(numCells.z - 1 - cfg.BorderSize - z));
        return Mathf.Min(Mathf.Min(distX, distY), distZ);
    }

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
                        var a = new Vec3(x + x0, y + y0, z + z0);
                        var b = new Vec3(x + x1, y + y1, z + z1);
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

    void AddTriangleToMesh(Vector3[] points, Vector2 uv)
    {
        Assert(points.Length == 3);
        var p1 = points[0];
        var p2 = points[1];
        var p3 = points[2];
        var normal = -(p2 - p1).Cross(p3 - p1).Normalized();
        foreach (var point in points)
        {
            var x = (point.X - cfg.BorderSize) * cfg.CellSize;
            var y = (point.Y - cfg.BorderSize) * cfg.CellSize;
            var z = (point.Z - cfg.BorderSize) * cfg.CellSize;
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
    Vec3 InterpolatePoints(Vec3 a, Vec3 b)
    {
        if (!cfg.Interpolate)
        {
            return (a + b) * 0.5f;
        }
        var noise_a = GetNoiseValue((int)a.x, (int)a.y, (int)a.z);
        var noise_b = GetNoiseValue((int)b.x, (int)b.y, (int)b.z);
        Assert(noise_a >= cfg.IsoValue || noise_b >= cfg.IsoValue);
        if (Mathf.IsZeroApprox(Mathf.Abs(cfg.IsoValue - noise_a)))
        {
            return a;
        }
        if (Mathf.IsZeroApprox(Mathf.Abs(cfg.IsoValue - noise_b)))
        {
            return b;
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

    void DebugNoiseGrid()
    {
        ImmediateMesh mesh = new();
        Mesh = mesh;
        mesh.SurfaceBegin(Mesh.PrimitiveType.Points);
        for (int z = 0; z < numCells.z; z++)
        {
            for (int y = 0; y < numCells.y; y++)
            {
                for (int x = 0; x < numCells.x; x++)
                {
                    var active = IsPointActive(x, y, z);
                    var color = new Color(1, 0.5f, 0, 0.25f);
                    if (active)
                    {
                        color = new Color(0.1f, 0.25f, 1f, 0.8f);
                    }
                    mesh.SurfaceSetColor(color);
                    mesh.SurfaceAddVertex(new Vector3(x * cfg.CellSize, y * cfg.CellSize, z * cfg.CellSize));
                }
            }
        }
    }
}
