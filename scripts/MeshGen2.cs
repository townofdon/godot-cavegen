using Godot;

public partial class MeshGen2 : MeshInstance3D
{
    Config2 cfgSrc;
    Config2.DataFormat cfg;
    Vector3I numCells;
    float[] noiseSamples;

    public void Initialize(Config2 _cfg)
    {
        cfgSrc = _cfg;
        cfg = cfgSrc.data;
        GD.Print("Interpolate=" + cfg.Interpolate);
    }

    public void Generate(Noise noise)
    {
        cfg = cfgSrc.data;
        ProcessNoise(noise);
    }

    void ProcessNoise(Noise noise)
    {
        var num = 5;
        noiseSamples = new float[num];
        for (int x = 0; x < num; x++)
        {
            noiseSamples[x] = noise.GetNoise3D(x, 0, 0);
        }
        foreach (var sample in noiseSamples)
        {
            GD.Print(sample);
        }
    }
}
