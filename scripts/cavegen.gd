extends Node3D
class_name CaveGen

@export var cfg:Config
@export var noise:FastNoiseLite
@export var borderNoise:FastNoiseLite

@onready var meshGen:MeshGen = %mesh
@onready var notifTimer:Timer = %Timer

var noiseB:FastNoiseLite

func _ready() -> void:
	assert(cfg)
	assert(noise)
	assert(borderNoise)
	assert(meshGen)
	cfg.Initialize()
	cfg.OnChanged.connect(_notify_change)

	# setup notification timer
	notifTimer.autostart = false
	notifTimer.one_shot = true
	notifTimer.timeout.connect(regenerate)
	notifTimer.stop()

	# setup meshgen
	meshGen.SetConfig(cfg)
	regenerate()

func regenerate():
	if !meshGen: return
	if !noise: return
	if !borderNoise: return
	meshGen.SetConfig(cfg)
	meshGen.Generate(noise, borderNoise)

	if meshGen.mesh is ImmediateMesh:
		var mat:ShaderMaterial = meshGen.material_override
		if mat && mat is ShaderMaterial:
			var y_ceil := cfg.RoomHeight * cfg.Ceiling - cfg.ActivePlaneOffset
			mat.set_shader_parameter("y_ceil", y_ceil)
			mat.set_shader_parameter("y_min", 0.0)

func _process(_delta: float) -> void:
	if _did_noise_change():
		_notify_change()
	noiseB = noise.duplicate()

func _notify_change():
	if notifTimer.is_stopped(): notifTimer.start()

func _did_noise_change() -> bool:
	if !noise || !noiseB:
		return false
	var a := noise
	var b := noiseB
	if a.frequency != b.frequency: return true
	if a.fractal_octaves != b.fractal_octaves: return true
	if a.noise_type != b.noise_type: return true
	if a.seed != b.seed: return true
	if a.offset != b.offset: return true
	if a.fractal_type != b.fractal_type: return true
	if a.fractal_octaves != b.fractal_octaves: return true
	if a.fractal_lacunarity != b.fractal_lacunarity: return true
	if a.fractal_gain != b.fractal_gain: return true
	if a.fractal_weighted_strength != b.fractal_weighted_strength: return true
	if a.domain_warp_enabled != b.domain_warp_enabled: return true
	if a.domain_warp_type != b.domain_warp_type: return true
	if a.domain_warp_fractal_type != b.domain_warp_fractal_type: return true
	if a.domain_warp_amplitude != b.domain_warp_amplitude: return true
	if a.domain_warp_frequency != b.domain_warp_frequency: return true
	if a.domain_warp_fractal_octaves != b.domain_warp_fractal_octaves: return true
	if a.domain_warp_fractal_lacunarity != b.domain_warp_fractal_lacunarity: return true
	if a.domain_warp_fractal_gain != b.domain_warp_fractal_gain: return true
	if a.cellular_distance_function != b.cellular_distance_function: return true
	if a.cellular_jitter != b.cellular_jitter: return true
	if a.cellular_return_type != b.cellular_return_type: return true
	return false
