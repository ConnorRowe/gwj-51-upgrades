shader_type canvas_item;

// must set global_transform uniform & y_cutoff_pos (global space) via script to work

uniform bool invert = false;
uniform float y_cutoff_pos = 0;
uniform mat4 global_transform;
varying vec2 world_pos;

void vertex()
{
	world_pos = (global_transform * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment()
{	
	if((invert && world_pos.y <= y_cutoff_pos) || (!invert && world_pos.y > y_cutoff_pos))
		discard;
}