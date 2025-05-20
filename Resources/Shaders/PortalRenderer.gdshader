shader_type spatial;
render_mode unshaded, cull_disabled, shadows_disabled, depth_draw_never;

uniform sampler2D screen_tex: source_color, hint_default_black;

void fragment()
{
	ALBEDO = texture(screen_tex, SCREEN_UV).rgb;
}
