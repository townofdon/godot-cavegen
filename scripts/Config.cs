using Godot;

[Tool]
[GlobalClass]
public partial class Config : Resource
{

    public struct DataPacked
    {
        // debug
        public bool ShowNoise;
        public bool ShowBorder;
        // global values
        public float RoomWidth;
        public float RoomHeight;
        public float RoomDepth;
        public float CellSize;
        public float Ceiling;
        public float ActivePlaneOffset;
        // room noise
        public float IsoValue;
        public float Curve;
        public float FalloffAboveCeiling;
        public bool Interpolate;
        public bool RemoveOrphans;
        // room border
        public bool UseBorderNoise;
        public int BorderSize;
        public float SmoothBorderNoise;
        public int FalloffNearBorder;
    }

    public DataPacked data = new();

    public void Initialize()
    {
        SyncData();
    }

    [Signal]
    public delegate void OnChangedEventHandler();

    [ExportGroup("Debug")]

    [Export]
    public bool ShowNoise { get { return _showNoise; } set { _showNoise = value; notifyChanged(); } }
    bool _showNoise = true;

    [Export]
    public bool ShowBorder { get { return _showBorder; } set { _showBorder = value; notifyChanged(); } }
    bool _showBorder = true;

    [ExportGroup("Global Values")]

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
    float _cellSize = 1f;

    [Export(PropertyHint.Range, "0,1,0.01")]
    public float Ceiling { get { return _ceiling; } set { _ceiling = value; notifyChanged(); } }
    float _ceiling = 0.75f;

    [Export(PropertyHint.Range, "0,50,0.01")]
    public float ActivePlaneOffset { get { return _activePlaneOffset; } set { _activePlaneOffset = value; notifyChanged(); } }
    float _activePlaneOffset = 10;

    [ExportGroup("Room Noise")]

    [Export(PropertyHint.Range, "0,1,0.01")]
    public float IsoValue { get { return _isoValue; } set { _isoValue = value; notifyChanged(); } }
    float _isoValue = 0.5f;

    [Export(PropertyHint.Range, "0,2,0.01")]
    public float Curve { get { return _curve; } set { _curve = value; notifyChanged(); } }
    float _curve = 1f;

    [Export(PropertyHint.Range, "0,1,0.01")]
    public float FalloffAboveCeiling { get { return _falloffAboveCeiling; } set { _falloffAboveCeiling = value; notifyChanged(); } }
    float _falloffAboveCeiling = 0.5f;

    [Export]
    public bool Interpolate { get { return _interpolate; } set { _interpolate = value; notifyChanged(); } }
    bool _interpolate = true;

    [Export]
    public bool RemoveOrphans { get { return _removeOrphans; } set { _removeOrphans = value; notifyChanged(); } }
    bool _removeOrphans = true;

    [ExportGroup("Room Border")]

    [Export]
    public bool UseBorderNoise { get { return _useBorderNoise; } set { _useBorderNoise = value; notifyChanged(); } }
    bool _useBorderNoise = false;

    [Export(PropertyHint.Range, "0,10,")]
    public int BorderSize { get { return _borderSize; } set { _borderSize = value; notifyChanged(); } }
    int _borderSize = 1;

    [Export(PropertyHint.Range, "0,1,0.01")]
    public float SmoothBorderNoise { get { return _smoothBorderNoise; } set { _smoothBorderNoise = value; notifyChanged(); } }
    float _smoothBorderNoise = 0.5f;

    [Export(PropertyHint.Range, "0,10,")]
    public int FalloffNearBorder { get { return _nearBorderFalloff; } set { _nearBorderFalloff = value; notifyChanged(); } }
    int _nearBorderFalloff = 2;

    private void SyncData()
    {
        // debug
        data.ShowNoise = ShowNoise;
        data.ShowBorder = ShowBorder;
        // global values
        data.RoomHeight = RoomHeight;
        data.RoomWidth = RoomWidth;
        data.RoomDepth = RoomDepth;
        data.CellSize = CellSize;
        data.Ceiling = Ceiling;
        data.ActivePlaneOffset = ActivePlaneOffset;
        // room noise
        data.IsoValue = IsoValue;
        data.Curve = Curve;
        data.FalloffAboveCeiling = FalloffAboveCeiling;
        data.Interpolate = Interpolate;
        data.RemoveOrphans = RemoveOrphans;
        // room border
        data.UseBorderNoise = UseBorderNoise;
        data.BorderSize = BorderSize;
        data.SmoothBorderNoise = SmoothBorderNoise;
        data.FalloffNearBorder = FalloffNearBorder;
    }

    private void notifyChanged()
    {
        SyncData();
        EmitSignal(SignalName.OnChanged);
    }
}
