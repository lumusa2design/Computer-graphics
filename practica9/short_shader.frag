#ifdef GL_ES
precision mediump float;
#endif
uniform vec2 u_resolution;
uniform float u_time;

void main(){
    vec2 u = (gl_FragCoord.xy - 0.5 * u_resolution.xy) / u_resolution.y;
    float r = length(u);
    float a = atan(u.y, u.x);
    float k = 10.0;
    a = abs(mod(a, 6.28318 / k) - 3.14159 / k);
    float v = sin(18.0 * r + 4.0 * cos(9.0 * a + u_time))
            + 0.5 * sin(3.0 * u_time + 7.0 * r);
    vec3 col = 0.5 + 0.5 * cos(v + vec3(0.0, 2.0, 4.0));
    col *= smoothstep(1.0, 0.1, r);
    gl_FragColor = vec4(col, 1.0);
}
