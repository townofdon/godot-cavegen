extends Camera3D

@export var acceleration := 13.0
@export var moveSpeed := 5.0
@export var lookSpeed := 5.0

var _is_mouse_in_window := false
var _mouse_motion = Vector2()
var move:Vector3 = Vector3.ZERO
var look:Vector3 = Vector3.ZERO
var velocity:Vector3 = Vector3.ZERO

func _ready() -> void:
	#Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	look = rotation

func _physics_process(delta: float) -> void:
	# Mouse movement.
	_mouse_motion.y = clamp(_mouse_motion.y, -1560, 1560)
	transform.basis = Basis.from_euler(Vector3(_mouse_motion.y * -0.001, _mouse_motion.x * -0.001, 0))

	# Keyboard input
	var inputH := Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	var inputV := 0.0
	if Input.is_action_pressed("move_down"):
		inputV -= 1.0
	if Input.is_action_pressed("move_up"):
		inputV += 1.0
	var movement := transform.basis * (Vector3(inputH.x, 0, inputH.y))
	movement = Vector3(movement.x, inputV, movement.z) * moveSpeed * 0.1
	velocity = velocity.lerp(movement, 1 - exp(-acceleration * delta))
	global_position = global_position + velocity


func _input(event):
	if event is InputEventMouseMotion:
		_mouse_motion += event.relative
		#if Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
			#_mouse_motion += event.relative

# I think this connects to the SceneTree notifs
# see: https://www.reddit.com/r/godot/comments/e36zyq/how_can_i_detect_when_the_mouse_cursor_is_inside/
func _notification(msg):
	match msg:
		NOTIFICATION_WM_MOUSE_EXIT:
			_is_mouse_in_window = false
		NOTIFICATION_WM_MOUSE_ENTER:
			_is_mouse_in_window = true
