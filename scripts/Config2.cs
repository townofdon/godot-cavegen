using Godot;

public partial class Config2 : RefCounted
{

    public struct DataFormat
    {
        // room
        public float RoomWidth;
        public float RoomHeight;
        public float RoomDepth;
        public float CellSize;
        public int BorderSize;
        // noise
        public int NoiseSeed;
        public float IsoValue;
        public float Ceiling;
        public float Curve;
        public bool Interpolate;
    }

    public DataFormat data = new();

    [Signal]
    public delegate void OnChangedEventHandler();

    [ExportGroup("Room")]

    [Export(PropertyHint.Range, "5,50,")]
    public float RoomWidth { get { return _roomWidth; } set { _roomWidth = value; notifyChanged(); } }
    float _roomWidth = 30f;

    [Export(PropertyHint.Range, "5,50,")]
    public float RoomHeight { get { return _roomHeight; } set { _roomHeight = value; notifyChanged(); } }
    float _roomHeight = 30f;

    [Export(PropertyHint.Range, "5,50,")]
    public float RoomDepth { get { return _roomDepth; } set { _roomDepth = value; notifyChanged(); } }
    float _roomDepth = 30f;

    [Export(PropertyHint.Range, "0.1,5.0,0.01")]
    public float CellSize { get { return _cellSize; } set { _cellSize = value; notifyChanged(); } }
    float _cellSize = 0.5f;

    [Export(PropertyHint.Range, "0,10,")]
    public int BorderSize { get { return _borderSize; } set { _borderSize = value; notifyChanged(); } }
    int _borderSize = 1;

    [ExportGroup("Noise")]

    [Export]
    public int NoiseSeed { get { return _noiseSeed; } set { _noiseSeed = value; notifyChanged(); } }
    int _noiseSeed = 0;

    [Export(PropertyHint.Range, "0,1,0.01")]
    public float IsoValue { get { return _isoValue; } set { _isoValue = value; notifyChanged(); } }
    float _isoValue = 0.5f;

    [Export(PropertyHint.Range, "0,1,0.01")]
    public float Ceiling { get { return _ceiling; } set { _ceiling = value; notifyChanged(); } }
    float _ceiling = 0.75f;

    [Export(PropertyHint.Range, "0,2,0.01")]
    public float Curve { get { return _curve; } set { _curve = value; notifyChanged(); } }
    float _curve = 1f;

    [Export]
    public bool Interpolate { get { return _interpolate; } set { _interpolate = value; notifyChanged(); } }
    bool _interpolate = true;

    private void notifyChanged()
    {
        EmitSignal(SignalName.OnChanged);

        // hydrate data
        // room
        data.RoomHeight = RoomHeight;
        data.RoomWidth = RoomWidth;
        data.RoomDepth = RoomDepth;
        data.CellSize = CellSize;
        data.BorderSize = BorderSize;
        // noise
        data.NoiseSeed = NoiseSeed;
        data.IsoValue = IsoValue;
        data.Ceiling = Ceiling;
        data.Curve = Curve;
        data.Interpolate = Interpolate;
    }
}
