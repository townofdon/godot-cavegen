extends Node3D
class_name CaveGen

const Config2 = preload("res://scripts/Config2.cs")

@export var config:Config

@onready var meshGen:MeshGen = %mesh
@onready var meshGen2:MeshInstance3D = %mesh2
@onready var textureRect:TextureRect = %TextureRect
@onready var notifTimer:Timer = %Timer

var noise:FastNoiseLite
var noiseB:FastNoiseLite

func _ready() -> void:
	assert(config)
	config.on_changed.connect(_notify_change)

	# setup texture noise
	var texture := textureRect.texture
	if (texture is NoiseTexture2D && texture.noise):
		noise = texture.noise as FastNoiseLite
		if !noise.seed: noise.seed = config.noise_seed
	textureRect.visible = false

	# setup notification timer
	notifTimer.autostart = false
	notifTimer.one_shot = true
	notifTimer.timeout.connect(regenerate)
	notifTimer.stop()

	# setup meshgen
	#meshGen.initialize(config)
	meshGen2.SetConfig(config.as_config_2())
	regenerate()

func regenerate():
	if !meshGen2: return
	if !noise: return
	#meshGen.generate(noise)
	meshGen2.SetConfig(config.as_config_2())
	meshGen2.Generate(noise)

	if meshGen2.mesh is ImmediateMesh:
		var mat:ShaderMaterial = meshGen2.material_override
		if mat && mat is ShaderMaterial:
			var y_ceil := config.room_height * config.ceiling
			mat.set_shader_parameter("y_ceil", y_ceil * 0.8)
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
