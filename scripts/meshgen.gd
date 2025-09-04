extends MeshInstance3D
class_name MeshGen

class Props:
	signal on_changed

func initialize(props: Props) -> void:
	props.on_changed.connect(generate)

func generate(cfg: Config, noise: Noise) -> void:
	march_cubes(cfg, noise)
	debug_noise_grid(cfg, noise)
	#hello_world()

func march_cubes(cfg: Config, noise: Noise) -> void:
	pass

func debug_noise_grid(cfg: Config, noise: Noise) -> void:
	mesh = ImmediateMesh.new()
	mesh.surface_begin(Mesh.PrimitiveType.PRIMITIVE_POINTS)
	var numCells:Vector3i = Vector3i(
		floor(cfg.room_width / cfg.cell_size),
		floor(cfg.room_height / cfg.cell_size),
		floor(cfg.room_depth / cfg.cell_size),
	)

	var min := INF
	var max := -INF
	var noise_samples:Array[float] = []
	# first pass - sample all noise values in grid
	for z in numCells.z:
		for y in numCells.y:
			for x in numCells.x:
				var val := noise.get_noise_3d(x, y, z)
				noise_samples.append(val)
				if val < min: min = val
				if val > max: max = val

	# second pass - normalize noise values and create mesh
	for z in numCells.z:
		for y in numCells.y:
			for x in numCells.x:
				var i := x + y*numCells.x + z*numCells.y*numCells.x
				var val:float = inverse_lerp(min, max, noise_samples.get(i))
				var active := val >= cfg.noise_cutoff
				if active:
					mesh.surface_set_color(Color.AQUAMARINE)
				else:
					mesh.surface_set_color(Color.DARK_OLIVE_GREEN)
				mesh.surface_add_vertex(Vector3(x * cfg.cell_size, y * cfg.cell_size, z * cfg.cell_size))

	mesh.surface_end()

func hello_world():
	var size := 2.0
	mesh = ImmediateMesh.new()
	mesh.surface_begin(Mesh.PrimitiveType.PRIMITIVE_TRIANGLES)
	mesh.surface_add_vertex(Vector3.LEFT * size + randvec())
	mesh.surface_add_vertex(Vector3.FORWARD * size + randvec())
	mesh.surface_add_vertex(Vector3.ZERO * size + randvec())
	mesh.surface_end()

func randvec():
	var strength := 0.2
	return Vector3(
		randf_range(-strength, strength),
		randf_range(-strength, strength),
		randf_range(-strength, strength),
	)
