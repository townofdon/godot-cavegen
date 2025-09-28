@tool
extends Resource
class_name Config

const Config2 = preload("res://scripts/Config2.cs")

@export_group("Room")
@export var room_width:float = 30:
	set(v): room_width = v; _notify_changed(1)
@export var room_height:float = 30:
	set(v): room_height = v; _notify_changed(1)
@export var room_depth:float = 30:
	set(v): room_depth = v; _notify_changed(1)
@export var cell_size:float = 0.5:
	set(v): cell_size = v; _notify_changed(1)
@export var border_size:int = 1:
	set(v): border_size = v; _notify_changed(1)

# TODO: THESE NEED TO BE CONFIGURABLE PER-ROOM
@export_group("Noise")
@export var noise_seed: int = 0:
	set(v): noise_seed = v; _notify_changed(1)
## threshold noise cutoff to determine where "inside" is
@export var iso_value:float = 0.5:
	set(v): iso_value = v; _notify_changed(1)
## threshold designating where the ceiling should be
@export var ceiling: float = 0.75:
	set(v): ceiling = v; _notify_changed(1)
## ease curve applied to noise: 0 => easeIn, 1 => linear, 2 => easeOut
@export_range(0, 2) var curve:float = 1:
	set(v): curve = v; _notify_changed(1)
## when enabled, vertices are interpolated to more closely match the isosurface
@export var interpolate:bool = true:
	set(v): interpolate = v; _notify_changed(1)

#@export_group("Noise")
#@export var noise_type := FastNoiseLite.NoiseType.TYPE_SIMPLEX_SMOOTH: set = _notify_changed
#@export var fractal_type := FastNoiseLite.FractalType.FRACTAL_NONE: set = _notify_changed
#@export var fractal_gain := 0.5: set = _notify_changed
#@export var fractal_lacunarity := 2.0: set = _notify_changed
#@export var fractal_octaves := 5: set = _notify_changed
#
#@export var frequency := 0.01: set = _notify_changed
#@export var fractal_weighted_strength := 0.0: set = _notify_changed
#@export var offset:Vector3: set = _notify_changed
#
#@export_group("Noise - extra")
#@export var cellular_distance_function := FastNoiseLite.CellularDistanceFunction.DISTANCE_EUCLIDEAN: set = _notify_changed
#@export var cellular_jitter: float = 1.0: set = _notify_changed
#@export var cellular_return_type := FastNoiseLite.CellularReturnType.RETURN_DISTANCE: set = _notify_changed
#@export var ping_pong_strength := 2.0: set = _notify_changed
#
#@export_group("Noise - domain warp")
#@export var domain_warp_type := FastNoiseLite.DomainWarpType.DOMAIN_WARP_SIMPLEX_REDUCED: set = _notify_changed
#@export var domain_warp_fractal_type := FastNoiseLite.DomainWarpFractalType.DOMAIN_WARP_FRACTAL_NONE: set = _notify_changed
#@export var domain_warp_enabled:bool: set = _notify_changed
#@export var domain_warp_frequency:float = 0.05: set = _notify_changed
#@export var domain_warp_amplitude := 30.0: set = _notify_changed
#

func as_config_2():
	var c2 = Config2.new()
	c2.RoomWidth = room_width
	c2.RoomHeight = room_height
	c2.RoomDepth = room_depth
	c2.CellSize = cell_size
	c2.BorderSize = border_size
	c2.NoiseSeed = noise_seed
	c2.IsoValue = iso_value
	c2.Ceiling = ceiling
	c2.Curve = curve
	c2.Interpolate = interpolate
	return c2

signal on_changed

func _notify_changed(_arg) -> void:
	on_changed.emit()
