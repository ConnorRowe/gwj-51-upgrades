shader_type canvas_item;

uniform int maxFrames;
uniform int frame = 0;

void vertex() {
	VERTEX /= vec2(float(maxFrames), 1.0);
}

void fragment() {
	vec2 uv = UV;
	uv.x /= float(maxFrames);
	uv.x += float(frame) / float(maxFrames);
	COLOR = texture(TEXTURE, uv);
}