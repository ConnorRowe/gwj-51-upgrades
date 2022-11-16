shader_type canvas_item;

uniform vec4 outline:hint_color;
uniform vec4 fill1:hint_color;
uniform vec4 fill2:hint_color;
uniform vec4 wet_colour:hint_color;
uniform float wetness;

void fragment()
{
	if(COLOR.g > .5)
	{
		COLOR = outline;
	}
	else if(COLOR.r >= .5)
	{
		COLOR = fill1;
	}
	else if (COLOR.b > .5)
	{
		COLOR = fill2;
	}
	
	COLOR = mix(COLOR, wet_colour, wetness * .5);
}