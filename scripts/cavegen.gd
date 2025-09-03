extends Node3D
class_name CaveGen

@export var config:Config

@onready var meshGen: MeshGen = %mesh
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
	var meshGenProps:MeshGen.Props = MeshGen.Props.new()
	meshGen.initialize(meshGenProps)
	regenerate()

func regenerate():
	if !meshGen: return
	if !noise: return
	meshGen.generate(config, noise)

func _process(delta: float) -> void:
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
