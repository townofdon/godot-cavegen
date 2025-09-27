shader_type spatial;

render_mode unshaded;

uniform vec3 light_direction = vec3(0, 0, 1);

uniform vec4 color_lit : source_color = vec4(1.0);

uniform vec4 color_shadow : source_color = vec4(0.1, 0.1, 0.1, 1.0);

void vertex() {
	vec3 light_dir = normalize(light_direction);
	float light = dot(-light_dir, normalize(NORMAL.xyz));
	// map light value [-1,1] to [0,1]
	light = (light + 1.0) * 0.5;
	COLOR = mix(color_lit, color_shadow, light);
}

void fragment() {
	ALBEDO = COLOR.xyz;
}
