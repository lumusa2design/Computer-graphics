// Author:
// Title:

#ifdef GL_ES
precision mediump float;
#endif
uniform vec2 u_resolution;
uniform float u_time;
float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    float a = hash(i);
    float b = hash(i + vec2(1.0, 0.0));
    float c = hash(i + vec2(0.0, 1.0));
    float d = hash(i + vec2(1.0, 1.0));
    vec2 u = f*f*(3.0 - 2.0*f);
    return mix(a,b,u.x) +
           (c-a)*u.y*(1.0-u.x) +
           (d-b)*u.x*u.y;
}

float fbm(vec2 p) {
    float v = 0.0;
    float a = 0.5;
    mat2 M = mat2(1.6,1.2,-1.2,1.6);
    for(int i=0;i<5;i++){
        v+=a*noise(p);
        p = M*p;
        a*=0.5;
    }
    return v;
}

vec3 palette(float t){
    vec3 a=vec3(0.905,0.081,0.107);
    vec3 b=vec3(0.850,0.768,0.059);
    vec3 c=vec3(0.848,0.745,0.900);
    vec3 d=vec3(0.670,0.328,0.118);
    return a + b*cos(6.2831*(c*t+d));
}

vec2 rot(vec2 p,float a){
    float s=sin(a), c=cos(a);
    return mat2(c,-s,s,c)*p;
}
vec3 mandalaLayer(vec2 uv,float t,float petals,float radialScale,float hueShift,float ringFreq){
    float PI = 3.14159265;
    float r = length(uv);
    float ang = atan(uv.y,uv.x);

    float wedge = 2.0*PI/petals;
    ang = abs(mod(ang,wedge)-0.5*wedge);

    vec2 p = vec2(ang*radialScale, r*3.0);

    float f1 = fbm(p+vec2(0.0,t));
    float f2 = fbm(p*2.0-vec2(t*1.3,t*0.7));
    vec2 pw = p + 0.94*vec2(f1,f2);
    float f3 = fbm(pw+vec2(t*0.8,-t*0.6));

    float mask = smoothstep(-0.18,0.9,f1+f2-r*1.85);

    float rings = sin(ringFreq*r + f3*6.0);
    float pattern = mix(f3,rings,0.6);
    pattern = 0.5 + 0.5*pattern;

    float hue = pattern + 1.3*r + 0.06*f2 + hueShift;

    vec3 colR = palette(hue+0.015);
    vec3 colG = palette(hue);
    vec3 colB = palette(hue-0.015);
    vec3 col = vec3(colR.r,colG.g,colB.b);
    col *= 0.3 + 1.6*mask;
    float edge = smoothstep(0.4,0.0,abs(rings))*mask;
    col += vec3(0.7,0.8,1.0)*edge*0.25; 
    float eps = 0.01;
    float h0 = f3;
    float hx = fbm(pw+vec2(eps,0.0)+vec2(t*0.8,-t*0.6));
    float hy = fbm(pw+vec2(0.0,eps)+vec2(t*0.8,-t*0.6));
    vec3 n = normalize(vec3(hx-h0,hy-h0,0.5));

    float lightAng = u_time*0.35;
    vec3 lightDir = normalize(vec3(cos(lightAng),sin(lightAng),0.7));

    float diff = clamp(dot(n,lightDir),0.0,1.0);
    float spec = pow(max(dot(reflect(-lightDir,n),vec3(0,0,1)),0.0),10.0);

    col *= 0.55 + 0.7*diff;        
    col += vec3(1.2,1.1,1.3)*spec*0.25; 

    return col;
}

void main(){
    vec2 uv = (gl_FragCoord.xy - 0.5*u_resolution.xy)/u_resolution.y;
    float t = u_time*0.25;
    float r = length(uv);
    float zoom = 1.0 + 0.07*sin(u_time*0.4);
    uv *= zoom;
    uv = rot(uv,0.12*sin(u_time*0.2));
    float swirl = 0.22*sin(4.0*r - u_time*0.7);
    uv = rot(uv,swirl);
    float bg = fbm(uv*1.6 + vec2(0.2*u_time, -0.15*u_time));
    bg = 0.5 + 0.5*bg;
    vec3 bgCol = mix(vec3(0.01,0,0.03), vec3(0.08,0,0.14), bg);
    float s = noise(uv*38.0 + vec2(8.0,-4.0));
    float stars = pow(max(0.0,1.0 - fract(s*23.0)),24.0);
    stars *= 0.45 + 0.35*sin(u_time*2.8 + s*20.0); 
    bgCol += vec3(0.8,0.8,1.0)*stars*0.45;
    float petals = mix(10.0,18.0,0.5+0.5*sin(u_time*0.3));
    vec3 layer2 = mandalaLayer(uv*0.85, t+4.0, petals*0.5+6.0, 3.0, 0.3, 5.0);
    vec3 layer1 = mandalaLayer(uv*1.25, t, petals, 2.5, 0.0, 8.0);
    vec3 col = bgCol + layer2*0.65 + layer1*1.0;
    float glow = exp(-r*4.0)*(0.4 + 0.4*sin(u_time*1.3));
    col += vec3(1.2,1.0,1.4)*glow*0.55;
    float outer = smoothstep(0.7,0.95,r)*(1.0 - smoothstep(0.95,1.15,r));
    col += vec3(0.3,0.5,1.0)*outer*0.35;
    float arcAng = atan(uv.y,uv.x);
    float arcN = 6.0;
    float idx = floor((arcAng+3.14159265)/(6.2831/arcN));
    float phase = idx/arcN*6.2831 + u_time*0.55;
    float band = sin(phase)*0.14 + 0.68;
    float arc = 1.0 - smoothstep(0.01,0.05,abs(r-band));
    arc *= smoothstep(0.2,0.8,r);
    col += vec3(1.1,0.9,0.6)*arc*0.28;

    float vignette = smoothstep(1.15,0.1,r);
    col *= vignette;

    float grain = hash(gl_FragCoord.xy + u_time*37.0);
    col += (grain-0.5)*0.025;

    col = clamp(col,0.0,1.2);

    gl_FragColor = vec4(col,1.0);
}
