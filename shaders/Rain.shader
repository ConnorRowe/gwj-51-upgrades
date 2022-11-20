shader_type canvas_item;

uniform float speed = 1;
uniform float x_offset = 0;

void vertex()
{
	UV += vec2((TIME*speed) + x_offset, -TIME*speed);
}