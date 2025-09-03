@tool
extends Resource
class_name Config

@export_group("Room")
@export var room_width:float = 30:
	set(v): room_width = v; _notify_changed(1)
@export var room_height:float = 30:
	set(v): room_height = v; _notify_changed(1)
@export var room_depth:float = 10:
	set(v): room_depth = v; _notify_changed(1)
@export var cell_size:float = 0.5:
	set(v): cell_size = v; _notify_changed(1)

@export_group("Noise")
@export var noise_seed: int = 0:
	set(v): noise_seed = v; _notify_changed(1)
@export var noise_cutoff := 0.5:
	set(v): noise_cutoff = v; _notify_changed(1)

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

signal on_changed

func _notify_changed(_arg) -> void:
	on_changed.emit()
