#ifdef GL_ES
precision mediump float;
#endif
uniform vec2 u_resolution;uniform float u_time;void main(){vec2 u=(gl_FragCoord.xy-.5*u_resolution.xy)/u_resolution.y;float r=length(u),a=atan(u.y,u.x),k=15.;a=abs(mod(a,6.28318/k)-3.14159/k);float v=sin(10.*r+4.*cos(8.*a+u_time))+.5*sin(3.*u_time+5.*r);vec3 c=vec3(.905,.081,.107)+vec3(.85,.768,.059)*cos(v+vec3(0.,2.,4.));c*=smoothstep(1.,.1,r);gl_FragColor=vec4(c,1.);}