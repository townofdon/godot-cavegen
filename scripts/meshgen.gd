extends MeshInstance3D
class_name MeshGen

var cfg: Config
var num_cells:Vector3i
var noise_samples:Array[float]

const DEBUG_NOISE_GRID = false

func initialize(_cfg: Config) -> void:
	cfg = _cfg
	if material_override && material_override is StandardMaterial3D:
		material_override.use_point_size = DEBUG_NOISE_GRID

func generate(noise: Noise) -> void:
	process_noise(noise)
	if DEBUG_NOISE_GRID:
		debug_noise_grid()
	else:
		march_cubes()
	#hello_world()

func process_noise(noise: Noise) -> void:
	num_cells = Vector3i(
		floor(cfg.room_width / cfg.cell_size),
		floor(cfg.room_height / cfg.cell_size),
		floor(cfg.room_depth / cfg.cell_size),
	)
	noise_samples = []
	var minV := INF
	var maxV := -INF
	# first pass - sample all noise values in grid
	for z in num_cells.z:
		for y in num_cells.y:
			for x in num_cells.x:
				var val := noise.get_noise_3d(x, y, z)
				noise_samples.append(val)
				if val < minV: minV = val
				if val > maxV: maxV = val
	# second pass - normalize noise values
	for i in len(noise_samples):
		var val:float = inverse_lerp(minV, maxV, noise_samples.get(i))
		noise_samples.set(i, val)

func get_noise_value(x:int, y:int, z:int) -> float:
	var i := x + y*num_cells.x + z*num_cells.y*num_cells.x
	if (i < 0 || i >= len(noise_samples)):
		return INF;
	return noise_samples.get(i)

func march_cubes() -> void:
	mesh = ImmediateMesh.new()
	mesh.surface_begin(Mesh.PrimitiveType.PRIMITIVE_TRIANGLES)
	for z in num_cells.z-1:
		for y in num_cells.y-1:
			for x in num_cells.x-1:
				var idx := getTriangulation(x, y, z)
				var points:Array[Vector3] = []
				var uv := Vector2(
					(x + 1) / float(num_cells.x),
					max(
						(y + 1) / float(num_cells.y),
						(z + 1) / float(num_cells.z),
					)
				)
				var edgeIndices = Constants.TRIANGULATIONS[idx]
				assert(edgeIndices)
				assert(len(edgeIndices) == 15)
				for edgeIdx in edgeIndices:
					if edgeIdx < 0: break
					#var (p0, p1) = EDGES[edgeIndex];
					var edges = Constants.EDGES[edgeIdx]
					assert(edges)
					assert(len(edges) == 2)
					#var (x0, y0, z0) = POINTS[p0];
					#var (x1, y1, z1) = POINTS[p1];
					var points0 = Constants.POINTS[edges[0]]
					var points1 = Constants.POINTS[edges[1]]
					assert(points0)
					assert(points1)
					assert(len(points0) == 3)
					assert(len(points1) == 3)
					var x0:int = points0[0]
					var y0:int = points0[1]
					var z0:int = points0[2]
					var x1:int = points1[0]
					var y1:int = points1[1]
					var z1:int = points1[2]
					var a = Vector3(x + x0, y + y0, z + z0)
					var b = Vector3(x + x1, y + y1, z + z1)
					var point = (a + b) * 0.5;
					points.append(point)
				@warning_ignore("integer_division")
				for i in len(points) / 3:
					var point0 := points[i*3 + 0]
					var point1 := points[i*3 + 1]
					var point2 := points[i*3 + 2]
					add_triangle_to_mesh([point0, point1, point2], uv)
	mesh.surface_end()

func add_triangle_to_mesh(points: Array[Vector3], uv: Vector2) -> void:
	assert(points)
	assert(len(points) == 3)
	var p1 := points[0]
	var p2 := points[1]
	var p3 := points[2]
	var normal := (p2 - p1).cross(p3 - p1).normalized();
	for point in points:
		mesh.surface_set_uv(uv)
		mesh.surface_set_normal(normal)
		mesh.surface_add_vertex(point)

func getTriangulation(x:int, y:int, z:int) -> int:
	var idx:int = 0
	idx |= (1 if is_point_active(x + 0, y + 0, z + 0) else 0) << 0
	idx |= (1 if is_point_active(x + 0, y + 0, z + 1) else 0) << 1
	idx |= (1 if is_point_active(x + 1, y + 0, z + 1) else 0) << 2
	idx |= (1 if is_point_active(x + 1, y + 0, z + 0) else 0) << 3
	idx |= (1 if is_point_active(x + 0, y + 1, z + 0) else 0) << 4
	idx |= (1 if is_point_active(x + 0, y + 1, z + 1) else 0) << 5
	idx |= (1 if is_point_active(x + 1, y + 1, z + 1) else 0) << 6
	idx |= (1 if is_point_active(x + 1, y + 1, z + 0) else 0) << 7
	return idx

func is_point_active(x:int, y:int, z:int) -> bool:
	var val := get_noise_value(x, y, z)
	var active := val >= cfg.noise_cutoff
	var ycent := float(y + 1) / float(num_cells.y)
	var below_ceiling := ycent < cfg.ceiling
	if active && !below_ceiling && is_point_orphan(x, y, z):
		active = false
	return active

func is_point_orphan(x:int, y:int, z:int) -> bool:
	# walk down from y to ceiling, checking if any gaps
	for y2 in range(y-1, floori(num_cells.y * cfg.ceiling), -1):
		var val := get_noise_value(x, y2, z)
		var active := val >= cfg.noise_cutoff
		if !active: return true
	return false




#
# debug
#

func debug_noise_grid() -> void:
	mesh = ImmediateMesh.new()
	mesh.surface_begin(Mesh.PrimitiveType.PRIMITIVE_POINTS)
	for z in num_cells.z:
		for y in num_cells.y:
			for x in num_cells.x:
				var active := is_point_active(x, y, z)
				var color := Color.DARK_OLIVE_GREEN
				color.a = 0.25
				if active:
					color = Color.AQUAMARINE
					color.a = 0.8
				mesh.surface_set_color(color)
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
