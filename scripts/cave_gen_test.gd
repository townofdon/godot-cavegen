@tool
extends Node3D

@export var cfg:Config

@onready var testCube:CSGBox3D = %CSGBox3D

func _ready() -> void:
	pass

func _process(_delta: float) -> void:
	update_test_cube()

func update_test_cube():
	if !cfg: return
	if !testCube: return
	testCube.size.x = cfg.RoomWidth
	testCube.size.y = cfg.RoomHeight
	testCube.size.z = cfg.RoomDepth
	testCube.global_position = Vector3(cfg.RoomWidth/2,cfg.RoomHeight/2,cfg.RoomDepth/2)
	var mat:ShaderMaterial = testCube.material
	if mat && mat is ShaderMaterial:
		var y_ceil := cfg.RoomHeight * cfg.Ceiling - cfg.ActivePlaneOffset
		mat.set_shader_parameter("y_ceil", y_ceil)
		mat.set_shader_parameter("y_min", 0.0)
