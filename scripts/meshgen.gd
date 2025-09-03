extends MeshInstance3D
class_name MeshGen

class Props:
	signal on_changed

func initialize(props: Props) -> void:
	props.on_changed.connect(generate)

func generate(cfg: Config, noise: Noise) -> void:
	if mesh is ImmediateMesh:
		mesh.clear_surfaces()

	debug_noise_grid(cfg, noise)
	#hello_world()

func debug_noise_grid(cfg: Config, noise: Noise) -> void:
	mesh = ImmediateMesh.new()
	mesh.surface_begin(Mesh.PrimitiveType.PRIMITIVE_POINTS)
	var numCells = Vector3(
		floor(cfg.room_width / cfg.cell_size),
		floor(cfg.room_height / cfg.cell_size),
		floor(cfg.room_depth / cfg.cell_size),
	)

	for z in numCells.z:
		for y in numCells.y:
			for x in numCells.x:
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
