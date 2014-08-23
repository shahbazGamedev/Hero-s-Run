// Simplified Bumped Specular shader. Differences from regular Bumped Specular one:
// - no Main Color nor Specular Color
// - specular lighting directions are approximated per vertex
// - writes zero to alpha channel
// - Normalmap uses Tiling/Offset of the Base texture
// - no Deferred Lighting support
// - no Lightmap support
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Bumped Specular_2side" {
Properties {
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
}


SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 250
	Cull Off
	
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardBase" }
Program "vp" {
// Vertex combos: 4
//   opengl - ALU: 50 to 86
//   d3d9 - ALU: 53 to 90
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Bind "vertex" Vertex
Bind "tangent" ATTR14
Bind "normal" Normal
Bind "texcoord" TexCoord0
Vector 13 [unity_Scale]
Vector 14 [_WorldSpaceCameraPos]
Vector 15 [_WorldSpaceLightPos0]
Matrix 5 [_Object2World]
Matrix 9 [_World2Object]
Vector 16 [unity_SHAr]
Vector 17 [unity_SHAg]
Vector 18 [unity_SHAb]
Vector 19 [unity_SHBr]
Vector 20 [unity_SHBg]
Vector 21 [unity_SHBb]
Vector 22 [unity_SHC]
Vector 23 [_MainTex_ST]
"!!ARBvp1.0
# 50 ALU
PARAM c[24] = { { 1 },
		state.matrix.mvp,
		program.local[5..23] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.xyz, vertex.attrib[14];
MUL R1.xyz, vertex.normal.zxyw, R0.yzxw;
MAD R0.xyz, vertex.normal.yzxw, R0.zxyw, -R1;
MUL R3.xyz, R0, vertex.attrib[14].w;
MOV R1.w, c[0].x;
MOV R1.xyz, c[14];
DP4 R2.z, R1, c[11];
DP4 R2.x, R1, c[9];
DP4 R2.y, R1, c[10];
MAD R1.xyz, R2, c[13].w, -vertex.position;
DP3 R0.y, R3, R1;
MOV R2, c[15];
DP3 R0.x, vertex.attrib[14], R1;
DP3 R0.z, vertex.normal, R1;
DP3 R0.w, R0, R0;
DP4 R1.z, R2, c[11];
DP4 R1.x, R2, c[9];
DP4 R1.y, R2, c[10];
DP3 R2.y, R1, R3;
DP3 R2.x, R1, vertex.attrib[14];
DP3 R2.z, vertex.normal, R1;
MUL R1.xyz, vertex.normal, c[13].w;
RSQ R0.w, R0.w;
MAD R0.xyz, R0.w, R0, R2;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL result.texcoord[3].xyz, R0.w, R0;
DP3 R2.w, R1, c[6];
DP3 R0.x, R1, c[5];
DP3 R0.z, R1, c[7];
MOV R0.y, R2.w;
MOV R0.w, c[0].x;
MUL R1, R0.xyzz, R0.yzzx;
DP4 R3.z, R0, c[18];
DP4 R3.y, R0, c[17];
DP4 R3.x, R0, c[16];
MUL R0.w, R2, R2;
MAD R0.w, R0.x, R0.x, -R0;
DP4 R0.z, R1, c[21];
DP4 R0.y, R1, c[20];
DP4 R0.x, R1, c[19];
MUL R1.xyz, R0.w, c[22];
ADD R0.xyz, R3, R0;
ADD result.texcoord[2].xyz, R0, R1;
MOV result.texcoord[1].xyz, R2;
MAD result.texcoord[0].xy, vertex.texcoord[0], c[23], c[23].zwzw;
DP4 result.position.w, vertex.position, c[4];
DP4 result.position.z, vertex.position, c[3];
DP4 result.position.y, vertex.position, c[2];
DP4 result.position.x, vertex.position, c[1];
END
# 50 instructions, 4 R-regs
"
}


SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Vector 12 [unity_Scale]
Vector 13 [_WorldSpaceCameraPos]
Vector 14 [_WorldSpaceLightPos0]
Matrix 4 [_Object2World]
Matrix 8 [_World2Object]
Vector 15 [unity_SHAr]
Vector 16 [unity_SHAg]
Vector 17 [unity_SHAb]
Vector 18 [unity_SHBr]
Vector 19 [unity_SHBg]
Vector 20 [unity_SHBb]
Vector 21 [unity_SHC]
Vector 22 [_MainTex_ST]
"vs_2_0
; 53 ALU
def c23, 1.00000000, 0, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
mov r0.xyz, v1
mul r1.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r1.xyz, v2.yzxw, r0.zxyw, -r1
mul r3.xyz, r1, v1.w
mov r0.w, c23.x
mov r0.xyz, c13
dp4 r2.z, r0, c10
dp4 r2.x, r0, c8
dp4 r2.y, r0, c9
mad r0.xyz, r2, c12.w, -v0
dp3 r2.y, r3, r0
dp3 r2.x, v1, r0
dp3 r2.z, v2, r0
dp3 r1.x, r2, r2
rsq r2.w, r1.x
mov r1, c8
dp4 r4.x, c14, r1
mov r0, c10
dp4 r4.z, c14, r0
mov r0, c9
dp4 r4.y, c14, r0
dp3 r0.y, r4, r3
mov r1.w, c23.x
dp3 r0.x, r4, v1
dp3 r0.z, v2, r4
mad r1.xyz, r2.w, r2, r0
dp3 r0.w, r1, r1
rsq r0.w, r0.w
mul r2.xyz, v2, c12.w
mul oT3.xyz, r0.w, r1
dp3 r0.w, r2, c5
mov r1.y, r0.w
dp3 r1.x, r2, c4
dp3 r1.z, r2, c6
mul r0.w, r0, r0
mul r2, r1.xyzz, r1.yzzx
dp4 r3.z, r1, c17
dp4 r3.y, r1, c16
dp4 r3.x, r1, c15
mad r0.w, r1.x, r1.x, -r0
dp4 r1.z, r2, c20
dp4 r1.y, r2, c19
dp4 r1.x, r2, c18
mul r2.xyz, r0.w, c21
add r1.xyz, r3, r1
add oT2.xyz, r1, r2
mov oT1.xyz, r0
mad oT0.xy, v3, c22, c22.zwzw
dp4 oPos.w, v0, c3
dp4 oPos.z, v0, c2
dp4 oPos.y, v0, c1
dp4 oPos.x, v0, c0
"
}

SubProgram "xbox360 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Vector 21 [_MainTex_ST]
Matrix 7 [_Object2World] 3
Matrix 10 [_World2Object] 4
Vector 5 [_WorldSpaceCameraPos]
Vector 6 [_WorldSpaceLightPos0]
Matrix 0 [glstate_matrix_mvp] 4
Vector 16 [unity_SHAb]
Vector 15 [unity_SHAg]
Vector 14 [unity_SHAr]
Vector 19 [unity_SHBb]
Vector 18 [unity_SHBg]
Vector 17 [unity_SHBr]
Vector 20 [unity_SHC]
Vector 4 [unity_Scale]
// Shader Timing Estimate, in Cycles/64 vertex vector:
// ALU: 58.67 (44 instructions), vertex: 32, texture: 0,
//   sequencer: 22,  9 GPRs, 21 threads,
// Performance (if enough threads): ~58 cycles per vector
// * Vertex cycle estimates are assuming 3 vfetch_minis for every vfetch_full,
//     with <= 32 bytes per vfetch_full group.

"vs_360
backbbabaaaaacliaaaaacjeaaaaaaaaaaaaaaceaaaaaaaaaaaaacgaaaaaaaaa
aaaaaaaaaaaaacdiaaaaaabmaaaaacclpppoadaaaaaaaaaoaaaaaabmaaaaaaaa
aaaaacceaaaaabdeaaacaabfaaabaaaaaaaaabeaaaaaaaaaaaaaabfaaaacaaah
aaadaaaaaaaaabgaaaaaaaaaaaaaabhaaaacaaakaaaeaaaaaaaaabgaaaaaaaaa
aaaaabhoaaacaaafaaabaaaaaaaaabjeaaaaaaaaaaaaabkeaaacaaagaaabaaaa
aaaaabeaaaaaaaaaaaaaabljaaacaaaaaaaeaaaaaaaaabgaaaaaaaaaaaaaabmm
aaacaabaaaabaaaaaaaaabeaaaaaaaaaaaaaabnhaaacaaapaaabaaaaaaaaabea
aaaaaaaaaaaaabocaaacaaaoaaabaaaaaaaaabeaaaaaaaaaaaaaabonaaacaabd
aaabaaaaaaaaabeaaaaaaaaaaaaaabpiaaacaabcaaabaaaaaaaaabeaaaaaaaaa
aaaaacadaaacaabbaaabaaaaaaaaabeaaaaaaaaaaaaaacaoaaacaabeaaabaaaa
aaaaabeaaaaaaaaaaaaaacbiaaacaaaeaaabaaaaaaaaabeaaaaaaaaafpengbgj
gofegfhifpfdfeaaaaabaaadaaabaaaeaaabaaaaaaaaaaaafpepgcgkgfgdhedc
fhgphcgmgeaaklklaaadaaadaaaeaaaeaaabaaaaaaaaaaaafpfhgphcgmgedcep
gcgkgfgdheaafpfhgphcgmgefdhagbgdgfedgbgngfhcgbfagphdaaklaaabaaad
aaabaaadaaabaaaaaaaaaaaafpfhgphcgmgefdhagbgdgfemgjghgihefagphdda
aaghgmhdhegbhegffpgngbhehcgjhifpgnhghaaahfgogjhehjfpfdeiebgcaahf
gogjhehjfpfdeiebghaahfgogjhehjfpfdeiebhcaahfgogjhehjfpfdeiecgcaa
hfgogjhehjfpfdeiecghaahfgogjhehjfpfdeiechcaahfgogjhehjfpfdeiedaa
hfgogjhehjfpfdgdgbgmgfaahghdfpddfpdaaadccodacodcdadddfddcodaaakl
aaaaaaaaaaaaacjeaadbaaaiaaaaaaaaaaaaaaaaaaaacmieaaaaaaabaaaaaaae
aaaaaaaeaaaaacjaaabaaaagaaaagaahaaaadaaiaacafaajaaaadafaaaabhbfb
aaachcfcaaadhdfdaaaabaccaaaabaceaaaabacoaaaabadfpaffeaagaaaabcaa
mcaaaaaaaaaaeaakaaaabcaameaaaaaaaaaagaaogabebcaabcaaaaaaaaaagabk
gacabcaabcaaaaaaaaaagacggacmbcaabcaaaaaaaaaaeadcaaaaccaaaaaaaaaa
afpifaaaaaaaagiiaaaaaaaaafpigaaaaaaaagiiaaaaaaaaafpidaaaaaaaaeeh
aaaaaaaaafpiaaaaaaaaacdpaaaaaaaamiapaaabaabliiaakbafadaamiapaaab
aamgiiaaklafacabmiapaaabaalbdejeklafababmiapiadoaagmaadeklafaaab
miahaaaeaaleblaacbanagaamiahaaabaamamgmaalamafanmiahaaacaamdgfaa
obadagaamiahaaaiaalelbleclalafabmialaaabaalkblaakbadaeaamiahaaae
aamamgleclamagaemiahaaahaalelbleclalagaemiahaaaeaalbleaakbabajaa
miahaaaiaamagmleclakafaimiahaaacablklomaoladagacmiahaaacaamablaa
obacagaamiahaaafabmablmaklaiaeafmiahaaaeaagmlemaklabaiaemiahaaab
aamagmleclakagahmiabaaaaaaloloaapaabagaamiacaaaaaalomdaapaabadaa
miahaaaeaabllemaklabahaemiaiaaacaaloloaapaacafaamiabaaadaaloloaa
paafagaaceicaeadaalomdgmpaafadiamiadiaaaaabklabkilaabfbfaibeahaa
aalologgpaacabaemiahiaabaaleleaaocaaaaaaaicbahagaadoanmbgpaoaeae
aiecahagaadoanlbgpapaeaeaiieahagaadoanlmgpbaaeaemiabaaafaakhkhaa
kpahbbaaaiicabafaakhkhgmkpahbcaeaiieaaafaakhkhmgkpahbdaeaiiiagaa
acblblgmoaabaaadaiihafaeaablmablkbaabeacaiipaeafaakhkhlboaagafad
miahiaacaalkmaaaoaafaeaamiaiaaaaaagmblaaoaafaeaafiiaabaaaaaaaabl
ocaaaaiamiakaaaaaalmbllmoladabaamiaeaaaaaakhkhaaopacabaamiabaaaa
aamdmdaapaaaaaaafibaaaaaaaaaaagmocaaaaiamiahiaadaabfgmaaobaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaa"
}

SubProgram "ps3 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Matrix 256 [glstate_matrix_mvp]
Vector 467 [unity_Scale]
Vector 466 [_WorldSpaceCameraPos]
Vector 465 [_WorldSpaceLightPos0]
Matrix 260 [_Object2World]
Matrix 264 [_World2Object]
Vector 464 [unity_SHAr]
Vector 463 [unity_SHAg]
Vector 462 [unity_SHAb]
Vector 461 [unity_SHBr]
Vector 460 [unity_SHBg]
Vector 459 [unity_SHBb]
Vector 458 [unity_SHC]
Vector 457 [_MainTex_ST]
"sce_vp_rsx // 46 instructions using 7 registers
[Configuration]
8
0000002e41050700
[Microcode]
736
00009c6c005d100d8186c0836041fffc00001c6c00400e0c0106c0836041dffc
00019c6c005d200c0186c0836041dffc00011c6c009d320c013fc0c36041dffc
401f9c6c011c9808010400d740619f9c401f9c6c01d0300d8106c0c360403f80
401f9c6c01d0200d8106c0c360405f80401f9c6c01d0100d8106c0c360409f80
00029c6c01d0a00d8286c0c360405ffc00029c6c01d0900d8286c0c360409ffc
00029c6c01d0800d8286c0c360411ffc00009c6c0150400c048600c360411ffc
00009c6c0150600c048600c360405ffc00001c6c0150500c048600c360403ffc
00011c6c0190a00c0686c0c360405ffc00011c6c0190900c0686c0c360409ffc
00011c6c0190800c0686c0c360411ffc00019c6c00800243011840436041dffc
00001c6c010002308121806301a1dffc00021c6c011d300c04bfc0e30041dffc
00011c6c0140020c0106054360405ffc00011c6c01400e0c0a86008360411ffc
00009c6c0080007f80bfc04360403ffc00009c6c0040007f8086c08360409ffc
00019c6c00800e0c00bfc0836041dffc00001c6c019ce00c0286c0c360405ffc
00001c6c019cf00c0286c0c360409ffc00001c6c019d000c0286c0c360411ffc
00031c6c0140020c0106044360405ffc00031c6c01400e0c0106044360411ffc
00001c6c010000000280017fe0a03ffc00009c6c0080000d029a01436041fffc
00011c6c0140000c0a86034360409ffc00031c6c0140000c0686044360409ffc
00019c6c01dcb00d8286c0c360405ffc00011c6c0140000c0c86064360403ffc
00019c6c01dcc00d8286c0c360409ffc00019c6c21dcd00d8286c0dfe123017c
00001c6c00c0000c0086c08301a1dffc00009c6c0100007f848606430121dffc
00019c6c009ca07f808600c36041dffc00001c6c0140000c0286014360403ffc
401f9c6c00c0000c0686c0830021dfa4401f9c6c21d0000d8106c0dfe0310000
401f9c6c0040000c0486c0836041dfa0401f9c6c00800000008601436041dfa9
"
}

SubProgram "gles " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;
uniform highp vec4 unity_SHC;
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;

uniform lowp vec4 _WorldSpaceLightPos0;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _World2Object;
uniform highp mat4 _Object2World;
uniform highp vec4 _MainTex_ST;
attribute vec4 _glesTANGENT;
attribute vec4 _glesMultiTexCoord0;
attribute vec3 _glesNormal;
attribute vec4 _glesVertex;
void main ()
{
  vec4 tmpvar_1;
  tmpvar_1.xyz = normalize (_glesTANGENT.xyz);
  tmpvar_1.w = _glesTANGENT.w;
  vec3 tmpvar_2;
  tmpvar_2 = normalize (_glesNormal);
  highp vec3 shlight;
  lowp vec3 tmpvar_3;
  lowp vec3 tmpvar_4;
  lowp vec3 tmpvar_5;
  mat3 tmpvar_6;
  tmpvar_6[0] = _Object2World[0].xyz;
  tmpvar_6[1] = _Object2World[1].xyz;
  tmpvar_6[2] = _Object2World[2].xyz;
  highp mat3 tmpvar_7;
  tmpvar_7[0] = tmpvar_1.xyz;
  tmpvar_7[1] = (cross (tmpvar_2, tmpvar_1.xyz) * _glesTANGENT.w);
  tmpvar_7[2] = tmpvar_2;
  mat3 tmpvar_8;
  tmpvar_8[0].x = tmpvar_7[0].x;
  tmpvar_8[0].y = tmpvar_7[1].x;
  tmpvar_8[0].z = tmpvar_7[2].x;
  tmpvar_8[1].x = tmpvar_7[0].y;
  tmpvar_8[1].y = tmpvar_7[1].y;
  tmpvar_8[1].z = tmpvar_7[2].y;
  tmpvar_8[2].x = tmpvar_7[0].z;
  tmpvar_8[2].y = tmpvar_7[1].z;
  tmpvar_8[2].z = tmpvar_7[2].z;
  highp vec3 tmpvar_9;
  tmpvar_9 = (tmpvar_8 * (_World2Object * _WorldSpaceLightPos0).xyz);
  tmpvar_3 = tmpvar_9;
  highp vec4 tmpvar_10;
  tmpvar_10.w = 1.0;
  tmpvar_10.xyz = _WorldSpaceCameraPos;
  highp vec3 tmpvar_11;
  tmpvar_11 = normalize ((tmpvar_9 + normalize ((tmpvar_8 * (((_World2Object * tmpvar_10).xyz * unity_Scale.w) - _glesVertex.xyz)))));
  tmpvar_5 = tmpvar_11;
  highp vec4 tmpvar_12;
  tmpvar_12.w = 1.0;
  tmpvar_12.xyz = (tmpvar_6 * (tmpvar_2 * unity_Scale.w));
  mediump vec3 tmpvar_13;
  mediump vec4 normal;
  normal = tmpvar_12;
  mediump vec3 x3;
  highp float vC;
  mediump vec3 x2;
  mediump vec3 x1;
  highp float tmpvar_14;
  tmpvar_14 = dot (unity_SHAr, normal);
  x1.x = tmpvar_14;
  highp float tmpvar_15;
  tmpvar_15 = dot (unity_SHAg, normal);
  x1.y = tmpvar_15;
  highp float tmpvar_16;
  tmpvar_16 = dot (unity_SHAb, normal);
  x1.z = tmpvar_16;
  mediump vec4 tmpvar_17;
  tmpvar_17 = (normal.xyzz * normal.yzzx);
  highp float tmpvar_18;
  tmpvar_18 = dot (unity_SHBr, tmpvar_17);
  x2.x = tmpvar_18;
  highp float tmpvar_19;
  tmpvar_19 = dot (unity_SHBg, tmpvar_17);
  x2.y = tmpvar_19;
  highp float tmpvar_20;
  tmpvar_20 = dot (unity_SHBb, tmpvar_17);
  x2.z = tmpvar_20;
  mediump float tmpvar_21;
  tmpvar_21 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_21;
  highp vec3 tmpvar_22;
  tmpvar_22 = (unity_SHC.xyz * vC);
  x3 = tmpvar_22;
  tmpvar_13 = ((x1 + x2) + x3);
  shlight = tmpvar_13;
  tmpvar_4 = shlight;
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = tmpvar_3;
  xlv_TEXCOORD2 = tmpvar_4;
  xlv_TEXCOORD3 = tmpvar_5;
}



#endif
#ifdef FRAGMENT

varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform mediump float _Shininess;
uniform sampler2D _MainTex;
uniform lowp vec4 _LightColor0;
uniform sampler2D _BumpMap;
void main ()
{
  lowp vec4 c;
  lowp vec4 tmpvar_1;
  tmpvar_1 = texture2D (_MainTex, xlv_TEXCOORD0);
  lowp vec3 tmpvar_2;
  tmpvar_2 = ((texture2D (_BumpMap, xlv_TEXCOORD0).xyz * 2.0) - 1.0);
  lowp vec4 c_i0;
  lowp float spec;
  lowp float tmpvar_3;
  tmpvar_3 = max (0.0, dot (tmpvar_2, xlv_TEXCOORD3));
  mediump float tmpvar_4;
  tmpvar_4 = (pow (tmpvar_3, (_Shininess * 128.0)) * tmpvar_1.w);
  spec = tmpvar_4;
  c_i0.xyz = ((((tmpvar_1.xyz * _LightColor0.xyz) * max (0.0, dot (tmpvar_2, xlv_TEXCOORD1))) + (_LightColor0.xyz * spec)) * 2.0);
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_1.xyz * xlv_TEXCOORD2));
  gl_FragData[0] = c;
}



#endif"
}

SubProgram "glesdesktop " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;
uniform highp vec4 unity_SHC;
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;

uniform lowp vec4 _WorldSpaceLightPos0;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _World2Object;
uniform highp mat4 _Object2World;
uniform highp vec4 _MainTex_ST;
attribute vec4 _glesTANGENT;
attribute vec4 _glesMultiTexCoord0;
attribute vec3 _glesNormal;
attribute vec4 _glesVertex;
void main ()
{
  vec4 tmpvar_1;
  tmpvar_1.xyz = normalize (_glesTANGENT.xyz);
  tmpvar_1.w = _glesTANGENT.w;
  vec3 tmpvar_2;
  tmpvar_2 = normalize (_glesNormal);
  highp vec3 shlight;
  lowp vec3 tmpvar_3;
  lowp vec3 tmpvar_4;
  lowp vec3 tmpvar_5;
  mat3 tmpvar_6;
  tmpvar_6[0] = _Object2World[0].xyz;
  tmpvar_6[1] = _Object2World[1].xyz;
  tmpvar_6[2] = _Object2World[2].xyz;
  highp mat3 tmpvar_7;
  tmpvar_7[0] = tmpvar_1.xyz;
  tmpvar_7[1] = (cross (tmpvar_2, tmpvar_1.xyz) * _glesTANGENT.w);
  tmpvar_7[2] = tmpvar_2;
  mat3 tmpvar_8;
  tmpvar_8[0].x = tmpvar_7[0].x;
  tmpvar_8[0].y = tmpvar_7[1].x;
  tmpvar_8[0].z = tmpvar_7[2].x;
  tmpvar_8[1].x = tmpvar_7[0].y;
  tmpvar_8[1].y = tmpvar_7[1].y;
  tmpvar_8[1].z = tmpvar_7[2].y;
  tmpvar_8[2].x = tmpvar_7[0].z;
  tmpvar_8[2].y = tmpvar_7[1].z;
  tmpvar_8[2].z = tmpvar_7[2].z;
  highp vec3 tmpvar_9;
  tmpvar_9 = (tmpvar_8 * (_World2Object * _WorldSpaceLightPos0).xyz);
  tmpvar_3 = tmpvar_9;
  highp vec4 tmpvar_10;
  tmpvar_10.w = 1.0;
  tmpvar_10.xyz = _WorldSpaceCameraPos;
  highp vec3 tmpvar_11;
  tmpvar_11 = normalize ((tmpvar_9 + normalize ((tmpvar_8 * (((_World2Object * tmpvar_10).xyz * unity_Scale.w) - _glesVertex.xyz)))));
  tmpvar_5 = tmpvar_11;
  highp vec4 tmpvar_12;
  tmpvar_12.w = 1.0;
  tmpvar_12.xyz = (tmpvar_6 * (tmpvar_2 * unity_Scale.w));
  mediump vec3 tmpvar_13;
  mediump vec4 normal;
  normal = tmpvar_12;
  mediump vec3 x3;
  highp float vC;
  mediump vec3 x2;
  mediump vec3 x1;
  highp float tmpvar_14;
  tmpvar_14 = dot (unity_SHAr, normal);
  x1.x = tmpvar_14;
  highp float tmpvar_15;
  tmpvar_15 = dot (unity_SHAg, normal);
  x1.y = tmpvar_15;
  highp float tmpvar_16;
  tmpvar_16 = dot (unity_SHAb, normal);
  x1.z = tmpvar_16;
  mediump vec4 tmpvar_17;
  tmpvar_17 = (normal.xyzz * normal.yzzx);
  highp float tmpvar_18;
  tmpvar_18 = dot (unity_SHBr, tmpvar_17);
  x2.x = tmpvar_18;
  highp float tmpvar_19;
  tmpvar_19 = dot (unity_SHBg, tmpvar_17);
  x2.y = tmpvar_19;
  highp float tmpvar_20;
  tmpvar_20 = dot (unity_SHBb, tmpvar_17);
  x2.z = tmpvar_20;
  mediump float tmpvar_21;
  tmpvar_21 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_21;
  highp vec3 tmpvar_22;
  tmpvar_22 = (unity_SHC.xyz * vC);
  x3 = tmpvar_22;
  tmpvar_13 = ((x1 + x2) + x3);
  shlight = tmpvar_13;
  tmpvar_4 = shlight;
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = tmpvar_3;
  xlv_TEXCOORD2 = tmpvar_4;
  xlv_TEXCOORD3 = tmpvar_5;
}



#endif
#ifdef FRAGMENT

varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform mediump float _Shininess;
uniform sampler2D _MainTex;
uniform lowp vec4 _LightColor0;
uniform sampler2D _BumpMap;
void main ()
{
  lowp vec4 c;
  lowp vec4 tmpvar_1;
  tmpvar_1 = texture2D (_MainTex, xlv_TEXCOORD0);
  lowp vec3 normal;
  normal.xy = ((texture2D (_BumpMap, xlv_TEXCOORD0).wy * 2.0) - 1.0);
  normal.z = sqrt (((1.0 - (normal.x * normal.x)) - (normal.y * normal.y)));
  lowp vec4 c_i0;
  lowp float spec;
  lowp float tmpvar_2;
  tmpvar_2 = max (0.0, dot (normal, xlv_TEXCOORD3));
  mediump float tmpvar_3;
  tmpvar_3 = (pow (tmpvar_2, (_Shininess * 128.0)) * tmpvar_1.w);
  spec = tmpvar_3;
  c_i0.xyz = ((((tmpvar_1.xyz * _LightColor0.xyz) * max (0.0, dot (normal, xlv_TEXCOORD1))) + (_LightColor0.xyz * spec)) * 2.0);
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_1.xyz * xlv_TEXCOORD2));
  gl_FragData[0] = c;
}



#endif"
}

SubProgram "flash " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Vector 12 [unity_Scale]
Vector 13 [_WorldSpaceCameraPos]
Vector 14 [_WorldSpaceLightPos0]
Matrix 4 [_Object2World]
Matrix 8 [_World2Object]
Vector 15 [unity_SHAr]
Vector 16 [unity_SHAg]
Vector 17 [unity_SHAb]
Vector 18 [unity_SHBr]
Vector 19 [unity_SHBg]
Vector 20 [unity_SHBb]
Vector 21 [unity_SHC]
Vector 22 [_MainTex_ST]
"agal_vs
c23 1.0 0.0 0.0 0.0
[bc]
aaaaaaaaaaaaahacafaaaaoeaaaaaaaaaaaaaaaaaaaaaaaa mov r0.xyz, a5
adaaaaaaabaaahacabaaaancaaaaaaaaaaaaaaajacaaaaaa mul r1.xyz, a1.zxyw, r0.yzxx
aaaaaaaaaaaaahacafaaaaoeaaaaaaaaaaaaaaaaaaaaaaaa mov r0.xyz, a5
adaaaaaaacaaahacabaaaamjaaaaaaaaaaaaaafcacaaaaaa mul r2.xyz, a1.yzxw, r0.zxyy
acaaaaaaabaaahacacaaaakeacaaaaaaabaaaakeacaaaaaa sub r1.xyz, r2.xyzz, r1.xyzz
adaaaaaaadaaahacabaaaakeacaaaaaaafaaaappaaaaaaaa mul r3.xyz, r1.xyzz, a5.w
aaaaaaaaaaaaaiacbhaaaaaaabaaaaaaaaaaaaaaaaaaaaaa mov r0.w, c23.x
aaaaaaaaaaaaahacanaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0.xyz, c13
bdaaaaaaacaaaeacaaaaaaoeacaaaaaaakaaaaoeabaaaaaa dp4 r2.z, r0, c10
bdaaaaaaacaaabacaaaaaaoeacaaaaaaaiaaaaoeabaaaaaa dp4 r2.x, r0, c8
bdaaaaaaacaaacacaaaaaaoeacaaaaaaajaaaaoeabaaaaaa dp4 r2.y, r0, c9
adaaaaaaaeaaahacacaaaakeacaaaaaaamaaaappabaaaaaa mul r4.xyz, r2.xyzz, c12.w
acaaaaaaaaaaahacaeaaaakeacaaaaaaaaaaaaoeaaaaaaaa sub r0.xyz, r4.xyzz, a0
bcaaaaaaacaaacacadaaaakeacaaaaaaaaaaaakeacaaaaaa dp3 r2.y, r3.xyzz, r0.xyzz
bcaaaaaaacaaabacafaaaaoeaaaaaaaaaaaaaakeacaaaaaa dp3 r2.x, a5, r0.xyzz
bcaaaaaaacaaaeacabaaaaoeaaaaaaaaaaaaaakeacaaaaaa dp3 r2.z, a1, r0.xyzz
bcaaaaaaabaaabacacaaaakeacaaaaaaacaaaakeacaaaaaa dp3 r1.x, r2.xyzz, r2.xyzz
akaaaaaaacaaaiacabaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rsq r2.w, r1.x
aaaaaaaaabaaapacaiaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r1, c8
bdaaaaaaaeaaabacaoaaaaoeabaaaaaaabaaaaoeacaaaaaa dp4 r4.x, c14, r1
aaaaaaaaaaaaapacakaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0, c10
bdaaaaaaaeaaaeacaoaaaaoeabaaaaaaaaaaaaoeacaaaaaa dp4 r4.z, c14, r0
aaaaaaaaaaaaapacajaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0, c9
bdaaaaaaaeaaacacaoaaaaoeabaaaaaaaaaaaaoeacaaaaaa dp4 r4.y, c14, r0
bcaaaaaaaaaaacacaeaaaakeacaaaaaaadaaaakeacaaaaaa dp3 r0.y, r4.xyzz, r3.xyzz
aaaaaaaaabaaaiacbhaaaaaaabaaaaaaaaaaaaaaaaaaaaaa mov r1.w, c23.x
bcaaaaaaaaaaabacaeaaaakeacaaaaaaafaaaaoeaaaaaaaa dp3 r0.x, r4.xyzz, a5
bcaaaaaaaaaaaeacabaaaaoeaaaaaaaaaeaaaakeacaaaaaa dp3 r0.z, a1, r4.xyzz
adaaaaaaabaaahacacaaaappacaaaaaaacaaaakeacaaaaaa mul r1.xyz, r2.w, r2.xyzz
abaaaaaaabaaahacabaaaakeacaaaaaaaaaaaakeacaaaaaa add r1.xyz, r1.xyzz, r0.xyzz
bcaaaaaaaaaaaiacabaaaakeacaaaaaaabaaaakeacaaaaaa dp3 r0.w, r1.xyzz, r1.xyzz
akaaaaaaaaaaaiacaaaaaappacaaaaaaaaaaaaaaaaaaaaaa rsq r0.w, r0.w
adaaaaaaacaaahacabaaaaoeaaaaaaaaamaaaappabaaaaaa mul r2.xyz, a1, c12.w
adaaaaaaadaaahaeaaaaaappacaaaaaaabaaaakeacaaaaaa mul v3.xyz, r0.w, r1.xyzz
bcaaaaaaaaaaaiacacaaaakeacaaaaaaafaaaaoeabaaaaaa dp3 r0.w, r2.xyzz, c5
aaaaaaaaabaaacacaaaaaappacaaaaaaaaaaaaaaaaaaaaaa mov r1.y, r0.w
bcaaaaaaabaaabacacaaaakeacaaaaaaaeaaaaoeabaaaaaa dp3 r1.x, r2.xyzz, c4
bcaaaaaaabaaaeacacaaaakeacaaaaaaagaaaaoeabaaaaaa dp3 r1.z, r2.xyzz, c6
adaaaaaaaaaaaiacaaaaaappacaaaaaaaaaaaappacaaaaaa mul r0.w, r0.w, r0.w
adaaaaaaacaaapacabaaaakeacaaaaaaabaaaacjacaaaaaa mul r2, r1.xyzz, r1.yzzx
bdaaaaaaadaaaeacabaaaaoeacaaaaaabbaaaaoeabaaaaaa dp4 r3.z, r1, c17
bdaaaaaaadaaacacabaaaaoeacaaaaaabaaaaaoeabaaaaaa dp4 r3.y, r1, c16
bdaaaaaaadaaabacabaaaaoeacaaaaaaapaaaaoeabaaaaaa dp4 r3.x, r1, c15
adaaaaaaadaaaiacabaaaaaaacaaaaaaabaaaaaaacaaaaaa mul r3.w, r1.x, r1.x
acaaaaaaaaaaaiacadaaaappacaaaaaaaaaaaappacaaaaaa sub r0.w, r3.w, r0.w
bdaaaaaaabaaaeacacaaaaoeacaaaaaabeaaaaoeabaaaaaa dp4 r1.z, r2, c20
bdaaaaaaabaaacacacaaaaoeacaaaaaabdaaaaoeabaaaaaa dp4 r1.y, r2, c19
bdaaaaaaabaaabacacaaaaoeacaaaaaabcaaaaoeabaaaaaa dp4 r1.x, r2, c18
adaaaaaaacaaahacaaaaaappacaaaaaabfaaaaoeabaaaaaa mul r2.xyz, r0.w, c21
abaaaaaaabaaahacadaaaakeacaaaaaaabaaaakeacaaaaaa add r1.xyz, r3.xyzz, r1.xyzz
abaaaaaaacaaahaeabaaaakeacaaaaaaacaaaakeacaaaaaa add v2.xyz, r1.xyzz, r2.xyzz
aaaaaaaaabaaahaeaaaaaakeacaaaaaaaaaaaaaaaaaaaaaa mov v1.xyz, r0.xyzz
adaaaaaaaaaaadacadaaaaoeaaaaaaaabgaaaaoeabaaaaaa mul r0.xy, a3, c22
abaaaaaaaaaaadaeaaaaaafeacaaaaaabgaaaaooabaaaaaa add v0.xy, r0.xyyy, c22.zwzw
bdaaaaaaaaaaaiadaaaaaaoeaaaaaaaaadaaaaoeabaaaaaa dp4 o0.w, a0, c3
bdaaaaaaaaaaaeadaaaaaaoeaaaaaaaaacaaaaoeabaaaaaa dp4 o0.z, a0, c2
bdaaaaaaaaaaacadaaaaaaoeaaaaaaaaabaaaaoeabaaaaaa dp4 o0.y, a0, c1
bdaaaaaaaaaaabadaaaaaaoeaaaaaaaaaaaaaaoeabaaaaaa dp4 o0.x, a0, c0
aaaaaaaaaaaaamaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v0.zw, c0
aaaaaaaaabaaaiaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v1.w, c0
aaaaaaaaacaaaiaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v2.w, c0
aaaaaaaaadaaaiaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v3.w, c0
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Bind "vertex" Vertex
Bind "tangent" ATTR14
Bind "normal" Normal
Bind "texcoord" TexCoord0
Vector 13 [_ProjectionParams]
Vector 14 [unity_Scale]
Vector 15 [_WorldSpaceCameraPos]
Vector 16 [_WorldSpaceLightPos0]
Matrix 5 [_Object2World]
Matrix 9 [_World2Object]
Vector 17 [unity_SHAr]
Vector 18 [unity_SHAg]
Vector 19 [unity_SHAb]
Vector 20 [unity_SHBr]
Vector 21 [unity_SHBg]
Vector 22 [unity_SHBb]
Vector 23 [unity_SHC]
Vector 24 [_MainTex_ST]
"!!ARBvp1.0
# 55 ALU
PARAM c[25] = { { 1, 0.5 },
		state.matrix.mvp,
		program.local[5..24] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.xyz, vertex.attrib[14];
MUL R1.xyz, vertex.normal.zxyw, R0.yzxw;
MAD R0.xyz, vertex.normal.yzxw, R0.zxyw, -R1;
MUL R3.xyz, R0, vertex.attrib[14].w;
MOV R1.w, c[0].x;
MOV R1.xyz, c[15];
DP4 R2.z, R1, c[11];
DP4 R2.x, R1, c[9];
DP4 R2.y, R1, c[10];
MAD R1.xyz, R2, c[14].w, -vertex.position;
DP3 R0.y, R3, R1;
MOV R2, c[16];
DP3 R0.x, vertex.attrib[14], R1;
DP3 R0.z, vertex.normal, R1;
DP3 R0.w, R0, R0;
DP4 R1.z, R2, c[11];
DP4 R1.x, R2, c[9];
DP4 R1.y, R2, c[10];
DP3 R2.y, R1, R3;
DP3 R2.x, R1, vertex.attrib[14];
DP3 R2.z, vertex.normal, R1;
MUL R1.xyz, vertex.normal, c[14].w;
RSQ R0.w, R0.w;
MAD R0.xyz, R0.w, R0, R2;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL result.texcoord[3].xyz, R0.w, R0;
DP3 R2.w, R1, c[6];
DP3 R0.x, R1, c[5];
DP3 R0.z, R1, c[7];
MOV R0.y, R2.w;
MOV R0.w, c[0].x;
MUL R1, R0.xyzz, R0.yzzx;
DP4 R3.z, R0, c[19];
DP4 R3.y, R0, c[18];
DP4 R3.x, R0, c[17];
MUL R0.w, R2, R2;
MAD R0.w, R0.x, R0.x, -R0;
DP4 R0.z, R1, c[22];
DP4 R0.y, R1, c[21];
DP4 R0.x, R1, c[20];
MUL R1.xyz, R0.w, c[23];
ADD R0.xyz, R3, R0;
ADD result.texcoord[2].xyz, R0, R1;
DP4 R0.w, vertex.position, c[4];
DP4 R0.z, vertex.position, c[3];
DP4 R0.x, vertex.position, c[1];
DP4 R0.y, vertex.position, c[2];
MUL R1.xyz, R0.xyww, c[0].y;
MUL R1.y, R1, c[13].x;
MOV result.texcoord[1].xyz, R2;
ADD result.texcoord[4].xy, R1, R1.z;
MOV result.position, R0;
MOV result.texcoord[4].zw, R0;
MAD result.texcoord[0].xy, vertex.texcoord[0], c[24], c[24].zwzw;
END
# 55 instructions, 4 R-regs
"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Vector 12 [_ProjectionParams]
Vector 13 [_ScreenParams]
Vector 14 [unity_Scale]
Vector 15 [_WorldSpaceCameraPos]
Vector 16 [_WorldSpaceLightPos0]
Matrix 4 [_Object2World]
Matrix 8 [_World2Object]
Vector 17 [unity_SHAr]
Vector 18 [unity_SHAg]
Vector 19 [unity_SHAb]
Vector 20 [unity_SHBr]
Vector 21 [unity_SHBg]
Vector 22 [unity_SHBb]
Vector 23 [unity_SHC]
Vector 24 [_MainTex_ST]
"vs_2_0
; 59 ALU
def c25, 1.00000000, 0.50000000, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
mov r0.xyz, v1
mul r1.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r1.xyz, v2.yzxw, r0.zxyw, -r1
mul r3.xyz, r1, v1.w
mov r0.w, c25.x
mov r0.xyz, c15
dp4 r2.z, r0, c10
dp4 r2.x, r0, c8
dp4 r2.y, r0, c9
mad r0.xyz, r2, c14.w, -v0
dp3 r2.y, r3, r0
dp3 r2.x, v1, r0
dp3 r2.z, v2, r0
dp3 r1.x, r2, r2
rsq r2.w, r1.x
mov r1, c8
dp4 r4.x, c16, r1
mov r0, c10
dp4 r4.z, c16, r0
mov r0, c9
dp4 r4.y, c16, r0
dp3 r0.y, r4, r3
mov r1.w, c25.x
dp3 r0.x, r4, v1
dp3 r0.z, v2, r4
mad r1.xyz, r2.w, r2, r0
dp3 r0.w, r1, r1
rsq r0.w, r0.w
mul r2.xyz, v2, c14.w
mul oT3.xyz, r0.w, r1
dp3 r0.w, r2, c5
mov r1.y, r0.w
dp3 r1.x, r2, c4
dp3 r1.z, r2, c6
mul r0.w, r0, r0
mul r2, r1.xyzz, r1.yzzx
dp4 r3.z, r1, c19
dp4 r3.y, r1, c18
dp4 r3.x, r1, c17
mad r0.w, r1.x, r1.x, -r0
dp4 r1.w, v0, c3
dp4 r1.z, r2, c22
dp4 r1.y, r2, c21
dp4 r1.x, r2, c20
add r1.xyz, r3, r1
mul r2.xyz, r0.w, c23
add oT2.xyz, r1, r2
dp4 r1.z, v0, c2
dp4 r1.x, v0, c0
dp4 r1.y, v0, c1
mul r2.xyz, r1.xyww, c25.y
mov oT1.xyz, r0
mov r0.x, r2
mul r0.y, r2, c12.x
mad oT4.xy, r2.z, c13.zwzw, r0
mov oPos, r1
mov oT4.zw, r1
mad oT0.xy, v3, c24, c24.zwzw
"
}

SubProgram "xbox360 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Vector 23 [_MainTex_ST]
Matrix 9 [_Object2World] 3
Vector 4 [_ProjectionParams]
Vector 5 [_ScreenParams]
Matrix 12 [_World2Object] 4
Vector 7 [_WorldSpaceCameraPos]
Vector 8 [_WorldSpaceLightPos0]
Matrix 0 [glstate_matrix_mvp] 4
Vector 18 [unity_SHAb]
Vector 17 [unity_SHAg]
Vector 16 [unity_SHAr]
Vector 21 [unity_SHBb]
Vector 20 [unity_SHBg]
Vector 19 [unity_SHBr]
Vector 22 [unity_SHC]
Vector 6 [unity_Scale]
// Shader Timing Estimate, in Cycles/64 vertex vector:
// ALU: 64.00 (48 instructions), vertex: 32, texture: 0,
//   sequencer: 24,  10 GPRs, 18 threads,
// Performance (if enough threads): ~64 cycles per vector
// * Vertex cycle estimates are assuming 3 vfetch_minis for every vfetch_full,
//     with <= 32 bytes per vfetch_full group.

"vs_360
backbbabaaaaaddeaaaaadaeaaaaaaaaaaaaaaceaaaaackiaaaaacnaaaaaaaaa
aaaaaaaaaaaaaciaaaaaaabmaaaaachdpppoadaaaaaaaabaaaaaaabmaaaaaaaa
aaaaacgmaaaaabfmaaacaabhaaabaaaaaaaaabgiaaaaaaaaaaaaabhiaaacaaaj
aaadaaaaaaaaabiiaaaaaaaaaaaaabjiaaacaaaeaaabaaaaaaaaabgiaaaaaaaa
aaaaabkkaaacaaafaaabaaaaaaaaabgiaaaaaaaaaaaaabliaaacaaamaaaeaaaa
aaaaabiiaaaaaaaaaaaaabmgaaacaaahaaabaaaaaaaaabnmaaaaaaaaaaaaabom
aaacaaaiaaabaaaaaaaaabgiaaaaaaaaaaaaacabaaacaaaaaaaeaaaaaaaaabii
aaaaaaaaaaaaacbeaaacaabcaaabaaaaaaaaabgiaaaaaaaaaaaaacbpaaacaabb
aaabaaaaaaaaabgiaaaaaaaaaaaaacckaaacaabaaaabaaaaaaaaabgiaaaaaaaa
aaaaacdfaaacaabfaaabaaaaaaaaabgiaaaaaaaaaaaaaceaaaacaabeaaabaaaa
aaaaabgiaaaaaaaaaaaaacelaaacaabdaaabaaaaaaaaabgiaaaaaaaaaaaaacfg
aaacaabgaaabaaaaaaaaabgiaaaaaaaaaaaaacgaaaacaaagaaabaaaaaaaaabgi
aaaaaaaafpengbgjgofegfhifpfdfeaaaaabaaadaaabaaaeaaabaaaaaaaaaaaa
fpepgcgkgfgdhedcfhgphcgmgeaaklklaaadaaadaaaeaaaeaaabaaaaaaaaaaaa
fpfahcgpgkgfgdhegjgpgofagbhcgbgnhdaafpfdgdhcgfgfgofagbhcgbgnhdaa
fpfhgphcgmgedcepgcgkgfgdheaafpfhgphcgmgefdhagbgdgfedgbgngfhcgbfa
gphdaaklaaabaaadaaabaaadaaabaaaaaaaaaaaafpfhgphcgmgefdhagbgdgfem
gjghgihefagphddaaaghgmhdhegbhegffpgngbhehcgjhifpgnhghaaahfgogjhe
hjfpfdeiebgcaahfgogjhehjfpfdeiebghaahfgogjhehjfpfdeiebhcaahfgogj
hehjfpfdeiecgcaahfgogjhehjfpfdeiecghaahfgogjhehjfpfdeiechcaahfgo
gjhehjfpfdeiedaahfgogjhehjfpfdgdgbgmgfaahghdfpddfpdaaadccodacodc
dadddfddcodaaaklaaaaaaaaaaaaaaabaaaaaaaaaaaaaaaaaaaaaabeaapmaaba
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaeaaaaaacmeaaebaaajaaaaaaaa
aaaaaaaaaaaadmkfaaaaaaabaaaaaaaeaaaaaaagaaaaacjaaabaaaagaaaagaah
aaaadaaiaacafaajaaaadafaaaabhbfbaaachcfcaaadhdfdaaaepefeaaaabach
aaaabacgaaaabadcaaaabadjaaaaaacfaaaabadaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaadpaaaaaaaaaaaaaaaaaaaaaaaaaaaaaapaffeaagaaaabcaamcaaaaaa
aaaafaakaaaabcaameaaaaaaaaaagaapgabfbcaabcaaaaaaaaaagablgacbbcaa
bcaaaaaaaaaagachgacnbcaabcaaaaaaaaaagaddbadjbcaaccaaaaaaafpihaaa
aaaaagiiaaaaaaaaafpiiaaaaaaaagiiaaaaaaaaafpiaaaaaaaaaeehaaaaaaaa
afpieaaaaaaaacdpaaaaaaaamiapaaabaabliiaakbahadaamiapaaabaamgnapi
klahacabmiapaaabaalbdepiklahababmiapaaagaagmnajeklahaaabmiapiado
aananaaaocagagaamiahaaadaaleblaacbapaiaamiahaaabaamamgmaalaoahap
miahaaajaalelbleclanahabmialaaabaalkblaakbaaagaamiahaaacaamdgfaa
obaaaiaamiahaaadaamamgleclaoaiadmiahaaadaalelbleclanaiadmiahaaac
ablklomaolaaaiacmiahaaafaalbleaakbabalaamiahaaajaamagmleclamahaj
miahaaahabmablmaklajagahmiahaaafaagmlemaklabakafmiahaaacaamablaa
obacaiaamiahaaabaamagmleclamaiadmiabaaadaaloloaapaabaiaamiacaaad
aalomdaapaabaaaamiaeaaadaaloloaapaacabaamiahaaafaabllemaklabajaf
miaiaaacaaloloaapaacahaamiabaaaeaaloloaapaahaiaaceicafaeaalomdgm
paahaaiaaibhaiaaaamagmggkbagppafmiamiaaeaanlnlaaocagagaamiahiaab
aaleleaaocadadaamiadiaaaaabklabkilaebhbhaicbaiahaadoanmbgpbaafaf
aiecaiahaadoanlbgpbbafafaiieaiahaadoanlmgpbcafafaiibahagaakhkhgm
kpaibdaeaiicabagaakhkhgmkpaibeafaiieaaagaakhkhmgkpaibfafaiiiagaa
acblblbloaabaaackiihaaafaablmaebibaabgaemiadiaaeaamgbkbiklaaafaa
aiipafaaaakhkhlboaahagaemiahiaacaalkmaaaoaaaafaamiabaaaaaagmblaa
oaaaafaafiiaabaaaaaaaagmocaaaaiamiakaaaaaalmbllmolaeabadmiaeaaaa
aakhkhaaopacabaamiabaaaaaamdmdaapaaaaaaafibaaaaaaaaaaagmocaaaaia
miahiaadaabfgmaaobaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
}

SubProgram "ps3 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Matrix 256 [glstate_matrix_mvp]
Vector 467 [_ProjectionParams]
Vector 466 [unity_Scale]
Vector 465 [_WorldSpaceCameraPos]
Vector 464 [_WorldSpaceLightPos0]
Matrix 260 [_Object2World]
Matrix 264 [_World2Object]
Vector 463 [unity_SHAr]
Vector 462 [unity_SHAg]
Vector 461 [unity_SHAb]
Vector 460 [unity_SHBr]
Vector 459 [unity_SHBg]
Vector 458 [unity_SHBb]
Vector 457 [unity_SHC]
Vector 456 [_MainTex_ST]
"sce_vp_rsx // 51 instructions using 8 registers
[Configuration]
8
0000003341050800
[Defaults]
1
455 1
3f000000
[Microcode]
816
00009c6c005d000d8186c0836041fffc00001c6c00400e0c0106c0836041dffc
00019c6c005d100c0186c0836041dffc00011c6c009d220c013fc0c36041dffc
401f9c6c011c8808010400d740619f9c00031c6c01d0300d8106c0c360403ffc
00031c6c01d0200d8106c0c360405ffc00031c6c01d0100d8106c0c360409ffc
00031c6c01d0000d8106c0c360411ffc00029c6c01d0a00d8286c0c360405ffc
00029c6c01d0900d8286c0c360409ffc00029c6c01d0800d8286c0c360411ffc
00009c6c0150400c048600c360411ffc00009c6c0150600c048600c360405ffc
00001c6c0150500c048600c360403ffc00011c6c0190a00c0686c0c360405ffc
00011c6c0190900c0686c0c360409ffc00011c6c0190800c0686c0c360411ffc
00019c6c00800243011840436041dffc00001c6c010002308121806301a1dffc
00021c6c011d200c04bfc0e30041dffc401f9c6c0040000d8c86c0836041ff80
00019c6c009c700e0c8000c36041dffc00011c6c0140020c0106054360405ffc
00011c6c01400e0c0a86008360411ffc00019c6c009d302a868000c360409ffc
00009c6c0080007f80bfc04360403ffc00009c6c0040007f8086c08360409ffc
401f9c6c00c000080686c09541a19fac00019c6c00800e0c00bfc0836041dffc
00001c6c019cd00c0286c0c360405ffc00001c6c019ce00c0286c0c360409ffc
00001c6c019cf00c0286c0c360411ffc00039c6c0140020c0106044360405ffc
00039c6c01400e0c0106044360411ffc00001c6c010000000280017fe0a03ffc
00009c6c0080000d029a01436041fffc00011c6c0140000c0a86034360409ffc
00039c6c0140000c0686044360409ffc00019c6c01dca00d8286c0c360405ffc
00011c6c0140000c0e86074360403ffc00019c6c01dcb00d8286c0c360409ffc
00019c6c21dcc00d8286c0dfe123017c00001c6c00c0000c0086c08301a1dffc
00009c6c0100007f848607430121dffc00019c6c009c907f808600c36041dffc
00001c6c0140000c0286014360403ffc401f9c6c00c0000c0686c0830021dfa4
401f9c6c204000558c86c09fe030602c401f9c6c0040000c0486c0836041dfa0
401f9c6c00800000008601436041dfa9
"
}

SubProgram "gles " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying highp vec4 xlv_TEXCOORD4;
varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;
uniform highp vec4 unity_SHC;
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;

uniform lowp vec4 _WorldSpaceLightPos0;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _World2Object;
uniform highp vec4 _ProjectionParams;
uniform highp mat4 _Object2World;
uniform highp vec4 _MainTex_ST;
attribute vec4 _glesTANGENT;
attribute vec4 _glesMultiTexCoord0;
attribute vec3 _glesNormal;
attribute vec4 _glesVertex;
void main ()
{
  vec4 tmpvar_1;
  tmpvar_1.xyz = normalize (_glesTANGENT.xyz);
  tmpvar_1.w = _glesTANGENT.w;
  vec3 tmpvar_2;
  tmpvar_2 = normalize (_glesNormal);
  highp vec3 shlight;
  lowp vec3 tmpvar_3;
  lowp vec3 tmpvar_4;
  lowp vec3 tmpvar_5;
  highp vec4 tmpvar_6;
  tmpvar_6 = (gl_ModelViewProjectionMatrix * _glesVertex);
  mat3 tmpvar_7;
  tmpvar_7[0] = _Object2World[0].xyz;
  tmpvar_7[1] = _Object2World[1].xyz;
  tmpvar_7[2] = _Object2World[2].xyz;
  highp mat3 tmpvar_8;
  tmpvar_8[0] = tmpvar_1.xyz;
  tmpvar_8[1] = (cross (tmpvar_2, tmpvar_1.xyz) * _glesTANGENT.w);
  tmpvar_8[2] = tmpvar_2;
  mat3 tmpvar_9;
  tmpvar_9[0].x = tmpvar_8[0].x;
  tmpvar_9[0].y = tmpvar_8[1].x;
  tmpvar_9[0].z = tmpvar_8[2].x;
  tmpvar_9[1].x = tmpvar_8[0].y;
  tmpvar_9[1].y = tmpvar_8[1].y;
  tmpvar_9[1].z = tmpvar_8[2].y;
  tmpvar_9[2].x = tmpvar_8[0].z;
  tmpvar_9[2].y = tmpvar_8[1].z;
  tmpvar_9[2].z = tmpvar_8[2].z;
  highp vec3 tmpvar_10;
  tmpvar_10 = (tmpvar_9 * (_World2Object * _WorldSpaceLightPos0).xyz);
  tmpvar_3 = tmpvar_10;
  highp vec4 tmpvar_11;
  tmpvar_11.w = 1.0;
  tmpvar_11.xyz = _WorldSpaceCameraPos;
  highp vec3 tmpvar_12;
  tmpvar_12 = normalize ((tmpvar_10 + normalize ((tmpvar_9 * (((_World2Object * tmpvar_11).xyz * unity_Scale.w) - _glesVertex.xyz)))));
  tmpvar_5 = tmpvar_12;
  highp vec4 tmpvar_13;
  tmpvar_13.w = 1.0;
  tmpvar_13.xyz = (tmpvar_7 * (tmpvar_2 * unity_Scale.w));
  mediump vec3 tmpvar_14;
  mediump vec4 normal;
  normal = tmpvar_13;
  mediump vec3 x3;
  highp float vC;
  mediump vec3 x2;
  mediump vec3 x1;
  highp float tmpvar_15;
  tmpvar_15 = dot (unity_SHAr, normal);
  x1.x = tmpvar_15;
  highp float tmpvar_16;
  tmpvar_16 = dot (unity_SHAg, normal);
  x1.y = tmpvar_16;
  highp float tmpvar_17;
  tmpvar_17 = dot (unity_SHAb, normal);
  x1.z = tmpvar_17;
  mediump vec4 tmpvar_18;
  tmpvar_18 = (normal.xyzz * normal.yzzx);
  highp float tmpvar_19;
  tmpvar_19 = dot (unity_SHBr, tmpvar_18);
  x2.x = tmpvar_19;
  highp float tmpvar_20;
  tmpvar_20 = dot (unity_SHBg, tmpvar_18);
  x2.y = tmpvar_20;
  highp float tmpvar_21;
  tmpvar_21 = dot (unity_SHBb, tmpvar_18);
  x2.z = tmpvar_21;
  mediump float tmpvar_22;
  tmpvar_22 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_22;
  highp vec3 tmpvar_23;
  tmpvar_23 = (unity_SHC.xyz * vC);
  x3 = tmpvar_23;
  tmpvar_14 = ((x1 + x2) + x3);
  shlight = tmpvar_14;
  tmpvar_4 = shlight;
  highp vec4 o_i0;
  highp vec4 tmpvar_24;
  tmpvar_24 = (tmpvar_6 * 0.5);
  o_i0 = tmpvar_24;
  highp vec2 tmpvar_25;
  tmpvar_25.x = tmpvar_24.x;
  tmpvar_25.y = (tmpvar_24.y * _ProjectionParams.x);
  o_i0.xy = (tmpvar_25 + tmpvar_24.w);
  o_i0.zw = tmpvar_6.zw;
  gl_Position = tmpvar_6;
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = tmpvar_3;
  xlv_TEXCOORD2 = tmpvar_4;
  xlv_TEXCOORD3 = tmpvar_5;
  xlv_TEXCOORD4 = o_i0;
}



#endif
#ifdef FRAGMENT

varying highp vec4 xlv_TEXCOORD4;
varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform mediump float _Shininess;
uniform sampler2D _ShadowMapTexture;
uniform sampler2D _MainTex;
uniform lowp vec4 _LightColor0;
uniform sampler2D _BumpMap;
void main ()
{
  lowp vec4 c;
  lowp vec4 tmpvar_1;
  tmpvar_1 = texture2D (_MainTex, xlv_TEXCOORD0);
  lowp vec3 tmpvar_2;
  tmpvar_2 = ((texture2D (_BumpMap, xlv_TEXCOORD0).xyz * 2.0) - 1.0);
  lowp vec4 c_i0;
  lowp float spec;
  lowp float tmpvar_3;
  tmpvar_3 = max (0.0, dot (tmpvar_2, xlv_TEXCOORD3));
  mediump float tmpvar_4;
  tmpvar_4 = (pow (tmpvar_3, (_Shininess * 128.0)) * tmpvar_1.w);
  spec = tmpvar_4;
  c_i0.xyz = ((((tmpvar_1.xyz * _LightColor0.xyz) * max (0.0, dot (tmpvar_2, xlv_TEXCOORD1))) + (_LightColor0.xyz * spec)) * (texture2DProj (_ShadowMapTexture, xlv_TEXCOORD4).x * 2.0));
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_1.xyz * xlv_TEXCOORD2));
  gl_FragData[0] = c;
}



#endif"
}

SubProgram "glesdesktop " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying highp vec4 xlv_TEXCOORD4;
varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;
uniform highp vec4 unity_SHC;
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;

uniform lowp vec4 _WorldSpaceLightPos0;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _World2Object;
uniform highp vec4 _ProjectionParams;
uniform highp mat4 _Object2World;
uniform highp vec4 _MainTex_ST;
attribute vec4 _glesTANGENT;
attribute vec4 _glesMultiTexCoord0;
attribute vec3 _glesNormal;
attribute vec4 _glesVertex;
void main ()
{
  vec4 tmpvar_1;
  tmpvar_1.xyz = normalize (_glesTANGENT.xyz);
  tmpvar_1.w = _glesTANGENT.w;
  vec3 tmpvar_2;
  tmpvar_2 = normalize (_glesNormal);
  highp vec3 shlight;
  lowp vec3 tmpvar_3;
  lowp vec3 tmpvar_4;
  lowp vec3 tmpvar_5;
  highp vec4 tmpvar_6;
  tmpvar_6 = (gl_ModelViewProjectionMatrix * _glesVertex);
  mat3 tmpvar_7;
  tmpvar_7[0] = _Object2World[0].xyz;
  tmpvar_7[1] = _Object2World[1].xyz;
  tmpvar_7[2] = _Object2World[2].xyz;
  highp mat3 tmpvar_8;
  tmpvar_8[0] = tmpvar_1.xyz;
  tmpvar_8[1] = (cross (tmpvar_2, tmpvar_1.xyz) * _glesTANGENT.w);
  tmpvar_8[2] = tmpvar_2;
  mat3 tmpvar_9;
  tmpvar_9[0].x = tmpvar_8[0].x;
  tmpvar_9[0].y = tmpvar_8[1].x;
  tmpvar_9[0].z = tmpvar_8[2].x;
  tmpvar_9[1].x = tmpvar_8[0].y;
  tmpvar_9[1].y = tmpvar_8[1].y;
  tmpvar_9[1].z = tmpvar_8[2].y;
  tmpvar_9[2].x = tmpvar_8[0].z;
  tmpvar_9[2].y = tmpvar_8[1].z;
  tmpvar_9[2].z = tmpvar_8[2].z;
  highp vec3 tmpvar_10;
  tmpvar_10 = (tmpvar_9 * (_World2Object * _WorldSpaceLightPos0).xyz);
  tmpvar_3 = tmpvar_10;
  highp vec4 tmpvar_11;
  tmpvar_11.w = 1.0;
  tmpvar_11.xyz = _WorldSpaceCameraPos;
  highp vec3 tmpvar_12;
  tmpvar_12 = normalize ((tmpvar_10 + normalize ((tmpvar_9 * (((_World2Object * tmpvar_11).xyz * unity_Scale.w) - _glesVertex.xyz)))));
  tmpvar_5 = tmpvar_12;
  highp vec4 tmpvar_13;
  tmpvar_13.w = 1.0;
  tmpvar_13.xyz = (tmpvar_7 * (tmpvar_2 * unity_Scale.w));
  mediump vec3 tmpvar_14;
  mediump vec4 normal;
  normal = tmpvar_13;
  mediump vec3 x3;
  highp float vC;
  mediump vec3 x2;
  mediump vec3 x1;
  highp float tmpvar_15;
  tmpvar_15 = dot (unity_SHAr, normal);
  x1.x = tmpvar_15;
  highp float tmpvar_16;
  tmpvar_16 = dot (unity_SHAg, normal);
  x1.y = tmpvar_16;
  highp float tmpvar_17;
  tmpvar_17 = dot (unity_SHAb, normal);
  x1.z = tmpvar_17;
  mediump vec4 tmpvar_18;
  tmpvar_18 = (normal.xyzz * normal.yzzx);
  highp float tmpvar_19;
  tmpvar_19 = dot (unity_SHBr, tmpvar_18);
  x2.x = tmpvar_19;
  highp float tmpvar_20;
  tmpvar_20 = dot (unity_SHBg, tmpvar_18);
  x2.y = tmpvar_20;
  highp float tmpvar_21;
  tmpvar_21 = dot (unity_SHBb, tmpvar_18);
  x2.z = tmpvar_21;
  mediump float tmpvar_22;
  tmpvar_22 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_22;
  highp vec3 tmpvar_23;
  tmpvar_23 = (unity_SHC.xyz * vC);
  x3 = tmpvar_23;
  tmpvar_14 = ((x1 + x2) + x3);
  shlight = tmpvar_14;
  tmpvar_4 = shlight;
  highp vec4 o_i0;
  highp vec4 tmpvar_24;
  tmpvar_24 = (tmpvar_6 * 0.5);
  o_i0 = tmpvar_24;
  highp vec2 tmpvar_25;
  tmpvar_25.x = tmpvar_24.x;
  tmpvar_25.y = (tmpvar_24.y * _ProjectionParams.x);
  o_i0.xy = (tmpvar_25 + tmpvar_24.w);
  o_i0.zw = tmpvar_6.zw;
  gl_Position = tmpvar_6;
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = tmpvar_3;
  xlv_TEXCOORD2 = tmpvar_4;
  xlv_TEXCOORD3 = tmpvar_5;
  xlv_TEXCOORD4 = o_i0;
}



#endif
#ifdef FRAGMENT

varying highp vec4 xlv_TEXCOORD4;
varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform mediump float _Shininess;
uniform sampler2D _ShadowMapTexture;
uniform sampler2D _MainTex;
uniform lowp vec4 _LightColor0;
uniform sampler2D _BumpMap;
void main ()
{
  lowp vec4 c;
  lowp vec4 tmpvar_1;
  tmpvar_1 = texture2D (_MainTex, xlv_TEXCOORD0);
  lowp vec3 normal;
  normal.xy = ((texture2D (_BumpMap, xlv_TEXCOORD0).wy * 2.0) - 1.0);
  normal.z = sqrt (((1.0 - (normal.x * normal.x)) - (normal.y * normal.y)));
  lowp vec4 c_i0;
  lowp float spec;
  lowp float tmpvar_2;
  tmpvar_2 = max (0.0, dot (normal, xlv_TEXCOORD3));
  mediump float tmpvar_3;
  tmpvar_3 = (pow (tmpvar_2, (_Shininess * 128.0)) * tmpvar_1.w);
  spec = tmpvar_3;
  c_i0.xyz = ((((tmpvar_1.xyz * _LightColor0.xyz) * max (0.0, dot (normal, xlv_TEXCOORD1))) + (_LightColor0.xyz * spec)) * (texture2DProj (_ShadowMapTexture, xlv_TEXCOORD4).x * 2.0));
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_1.xyz * xlv_TEXCOORD2));
  gl_FragData[0] = c;
}



#endif"
}

SubProgram "flash " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Vector 12 [_ProjectionParams]
Vector 13 [unity_NPOTScale]
Vector 14 [unity_Scale]
Vector 15 [_WorldSpaceCameraPos]
Vector 16 [_WorldSpaceLightPos0]
Matrix 4 [_Object2World]
Matrix 8 [_World2Object]
Vector 17 [unity_SHAr]
Vector 18 [unity_SHAg]
Vector 19 [unity_SHAb]
Vector 20 [unity_SHBr]
Vector 21 [unity_SHBg]
Vector 22 [unity_SHBb]
Vector 23 [unity_SHC]
Vector 24 [_MainTex_ST]
"agal_vs
c25 1.0 0.5 0.0 0.0
[bc]
aaaaaaaaaaaaahacafaaaaoeaaaaaaaaaaaaaaaaaaaaaaaa mov r0.xyz, a5
adaaaaaaabaaahacabaaaancaaaaaaaaaaaaaaajacaaaaaa mul r1.xyz, a1.zxyw, r0.yzxx
aaaaaaaaaaaaahacafaaaaoeaaaaaaaaaaaaaaaaaaaaaaaa mov r0.xyz, a5
adaaaaaaacaaahacabaaaamjaaaaaaaaaaaaaafcacaaaaaa mul r2.xyz, a1.yzxw, r0.zxyy
acaaaaaaabaaahacacaaaakeacaaaaaaabaaaakeacaaaaaa sub r1.xyz, r2.xyzz, r1.xyzz
adaaaaaaadaaahacabaaaakeacaaaaaaafaaaappaaaaaaaa mul r3.xyz, r1.xyzz, a5.w
aaaaaaaaaaaaaiacbjaaaaaaabaaaaaaaaaaaaaaaaaaaaaa mov r0.w, c25.x
aaaaaaaaaaaaahacapaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0.xyz, c15
bdaaaaaaacaaaeacaaaaaaoeacaaaaaaakaaaaoeabaaaaaa dp4 r2.z, r0, c10
bdaaaaaaacaaabacaaaaaaoeacaaaaaaaiaaaaoeabaaaaaa dp4 r2.x, r0, c8
bdaaaaaaacaaacacaaaaaaoeacaaaaaaajaaaaoeabaaaaaa dp4 r2.y, r0, c9
adaaaaaaaeaaahacacaaaakeacaaaaaaaoaaaappabaaaaaa mul r4.xyz, r2.xyzz, c14.w
acaaaaaaaaaaahacaeaaaakeacaaaaaaaaaaaaoeaaaaaaaa sub r0.xyz, r4.xyzz, a0
bcaaaaaaacaaacacadaaaakeacaaaaaaaaaaaakeacaaaaaa dp3 r2.y, r3.xyzz, r0.xyzz
bcaaaaaaacaaabacafaaaaoeaaaaaaaaaaaaaakeacaaaaaa dp3 r2.x, a5, r0.xyzz
bcaaaaaaacaaaeacabaaaaoeaaaaaaaaaaaaaakeacaaaaaa dp3 r2.z, a1, r0.xyzz
bcaaaaaaabaaabacacaaaakeacaaaaaaacaaaakeacaaaaaa dp3 r1.x, r2.xyzz, r2.xyzz
akaaaaaaacaaaiacabaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rsq r2.w, r1.x
aaaaaaaaabaaapacaiaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r1, c8
bdaaaaaaaeaaabacbaaaaaoeabaaaaaaabaaaaoeacaaaaaa dp4 r4.x, c16, r1
aaaaaaaaaaaaapacakaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0, c10
bdaaaaaaaeaaaeacbaaaaaoeabaaaaaaaaaaaaoeacaaaaaa dp4 r4.z, c16, r0
aaaaaaaaaaaaapacajaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0, c9
bdaaaaaaaeaaacacbaaaaaoeabaaaaaaaaaaaaoeacaaaaaa dp4 r4.y, c16, r0
bcaaaaaaaaaaacacaeaaaakeacaaaaaaadaaaakeacaaaaaa dp3 r0.y, r4.xyzz, r3.xyzz
aaaaaaaaabaaaiacbjaaaaaaabaaaaaaaaaaaaaaaaaaaaaa mov r1.w, c25.x
bcaaaaaaaaaaabacaeaaaakeacaaaaaaafaaaaoeaaaaaaaa dp3 r0.x, r4.xyzz, a5
bcaaaaaaaaaaaeacabaaaaoeaaaaaaaaaeaaaakeacaaaaaa dp3 r0.z, a1, r4.xyzz
adaaaaaaabaaahacacaaaappacaaaaaaacaaaakeacaaaaaa mul r1.xyz, r2.w, r2.xyzz
abaaaaaaabaaahacabaaaakeacaaaaaaaaaaaakeacaaaaaa add r1.xyz, r1.xyzz, r0.xyzz
bcaaaaaaaaaaaiacabaaaakeacaaaaaaabaaaakeacaaaaaa dp3 r0.w, r1.xyzz, r1.xyzz
akaaaaaaaaaaaiacaaaaaappacaaaaaaaaaaaaaaaaaaaaaa rsq r0.w, r0.w
adaaaaaaacaaahacabaaaaoeaaaaaaaaaoaaaappabaaaaaa mul r2.xyz, a1, c14.w
adaaaaaaadaaahaeaaaaaappacaaaaaaabaaaakeacaaaaaa mul v3.xyz, r0.w, r1.xyzz
bcaaaaaaaaaaaiacacaaaakeacaaaaaaafaaaaoeabaaaaaa dp3 r0.w, r2.xyzz, c5
aaaaaaaaabaaacacaaaaaappacaaaaaaaaaaaaaaaaaaaaaa mov r1.y, r0.w
bcaaaaaaabaaabacacaaaakeacaaaaaaaeaaaaoeabaaaaaa dp3 r1.x, r2.xyzz, c4
bcaaaaaaabaaaeacacaaaakeacaaaaaaagaaaaoeabaaaaaa dp3 r1.z, r2.xyzz, c6
adaaaaaaaaaaaiacaaaaaappacaaaaaaaaaaaappacaaaaaa mul r0.w, r0.w, r0.w
adaaaaaaacaaapacabaaaakeacaaaaaaabaaaacjacaaaaaa mul r2, r1.xyzz, r1.yzzx
bdaaaaaaadaaaeacabaaaaoeacaaaaaabdaaaaoeabaaaaaa dp4 r3.z, r1, c19
bdaaaaaaadaaacacabaaaaoeacaaaaaabcaaaaoeabaaaaaa dp4 r3.y, r1, c18
bdaaaaaaadaaabacabaaaaoeacaaaaaabbaaaaoeabaaaaaa dp4 r3.x, r1, c17
adaaaaaaadaaaiacabaaaaaaacaaaaaaabaaaaaaacaaaaaa mul r3.w, r1.x, r1.x
acaaaaaaaaaaaiacadaaaappacaaaaaaaaaaaappacaaaaaa sub r0.w, r3.w, r0.w
bdaaaaaaabaaaiacaaaaaaoeaaaaaaaaadaaaaoeabaaaaaa dp4 r1.w, a0, c3
bdaaaaaaabaaaeacacaaaaoeacaaaaaabgaaaaoeabaaaaaa dp4 r1.z, r2, c22
bdaaaaaaabaaacacacaaaaoeacaaaaaabfaaaaoeabaaaaaa dp4 r1.y, r2, c21
bdaaaaaaabaaabacacaaaaoeacaaaaaabeaaaaoeabaaaaaa dp4 r1.x, r2, c20
abaaaaaaabaaahacadaaaakeacaaaaaaabaaaakeacaaaaaa add r1.xyz, r3.xyzz, r1.xyzz
adaaaaaaacaaahacaaaaaappacaaaaaabhaaaaoeabaaaaaa mul r2.xyz, r0.w, c23
abaaaaaaacaaahaeabaaaakeacaaaaaaacaaaakeacaaaaaa add v2.xyz, r1.xyzz, r2.xyzz
bdaaaaaaabaaaeacaaaaaaoeaaaaaaaaacaaaaoeabaaaaaa dp4 r1.z, a0, c2
bdaaaaaaabaaabacaaaaaaoeaaaaaaaaaaaaaaoeabaaaaaa dp4 r1.x, a0, c0
bdaaaaaaabaaacacaaaaaaoeaaaaaaaaabaaaaoeabaaaaaa dp4 r1.y, a0, c1
adaaaaaaacaaahacabaaaapeacaaaaaabjaaaaffabaaaaaa mul r2.xyz, r1.xyww, c25.y
aaaaaaaaabaaahaeaaaaaakeacaaaaaaaaaaaaaaaaaaaaaa mov v1.xyz, r0.xyzz
adaaaaaaaaaaacacacaaaaffacaaaaaaamaaaaaaabaaaaaa mul r0.y, r2.y, c12.x
aaaaaaaaaaaaabacacaaaaaaacaaaaaaaaaaaaaaaaaaaaaa mov r0.x, r2.x
abaaaaaaaaaaadacaaaaaafeacaaaaaaacaaaakkacaaaaaa add r0.xy, r0.xyyy, r2.z
adaaaaaaaeaaadaeaaaaaafeacaaaaaaanaaaaoeabaaaaaa mul v4.xy, r0.xyyy, c13
aaaaaaaaaaaaapadabaaaaoeacaaaaaaaaaaaaaaaaaaaaaa mov o0, r1
aaaaaaaaaeaaamaeabaaaaopacaaaaaaaaaaaaaaaaaaaaaa mov v4.zw, r1.wwzw
adaaaaaaaaaaadacadaaaaoeaaaaaaaabiaaaaoeabaaaaaa mul r0.xy, a3, c24
abaaaaaaaaaaadaeaaaaaafeacaaaaaabiaaaaooabaaaaaa add v0.xy, r0.xyyy, c24.zwzw
aaaaaaaaaaaaamaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v0.zw, c0
aaaaaaaaabaaaiaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v1.w, c0
aaaaaaaaacaaaiaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v2.w, c0
aaaaaaaaadaaaiaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v3.w, c0
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "tangent" ATTR14
Bind "normal" Normal
Bind "texcoord" TexCoord0
Vector 13 [unity_Scale]
Vector 14 [_WorldSpaceCameraPos]
Vector 15 [_WorldSpaceLightPos0]
Matrix 5 [_Object2World]
Matrix 9 [_World2Object]
Vector 16 [unity_4LightPosX0]
Vector 17 [unity_4LightPosY0]
Vector 18 [unity_4LightPosZ0]
Vector 19 [unity_4LightAtten0]
Vector 20 [unity_LightColor0]
Vector 21 [unity_LightColor1]
Vector 22 [unity_LightColor2]
Vector 23 [unity_LightColor3]
Vector 24 [unity_SHAr]
Vector 25 [unity_SHAg]
Vector 26 [unity_SHAb]
Vector 27 [unity_SHBr]
Vector 28 [unity_SHBg]
Vector 29 [unity_SHBb]
Vector 30 [unity_SHC]
Vector 31 [_MainTex_ST]
"!!ARBvp1.0
# 81 ALU
PARAM c[32] = { { 1, 0 },
		state.matrix.mvp,
		program.local[5..31] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
TEMP R5;
MUL R4.xyz, vertex.normal, c[13].w;
DP4 R0.x, vertex.position, c[6];
DP4 R2.x, vertex.position, c[5];
DP3 R3.x, R4, c[5];
DP3 R4.w, R4, c[6];
ADD R0, -R0.x, c[17];
MUL R1, R4.w, R0;
ADD R2, -R2.x, c[16];
MUL R0, R0, R0;
MAD R1, R3.x, R2, R1;
DP3 R5.w, R4, c[7];
DP4 R3.y, vertex.position, c[7];
MAD R0, R2, R2, R0;
ADD R2, -R3.y, c[18];
MAD R0, R2, R2, R0;
MAD R1, R5.w, R2, R1;
MUL R2, R0, c[19];
RSQ R0.x, R0.x;
RSQ R0.y, R0.y;
RSQ R0.w, R0.w;
RSQ R0.z, R0.z;
MUL R0, R1, R0;
ADD R1, R2, c[0].x;
MUL R2.w, R4, R4;
MAX R0, R0, c[0].y;
MAD R2.w, R3.x, R3.x, -R2;
RCP R1.x, R1.x;
RCP R1.y, R1.y;
RCP R1.w, R1.w;
RCP R1.z, R1.z;
MUL R1, R0, R1;
MOV R0.xyz, vertex.attrib[14];
MUL R2.xyz, R1.y, c[21];
MUL R4.xyz, vertex.normal.zxyw, R0.yzxw;
MAD R4.xyz, vertex.normal.yzxw, R0.zxyw, -R4;
MAD R2.xyz, R1.x, c[20], R2;
MUL R4.xyz, R4, vertex.attrib[14].w;
MOV R0.w, c[0].x;
MOV R0.xyz, c[14];
DP4 R5.z, R0, c[11];
DP4 R5.x, R0, c[9];
DP4 R5.y, R0, c[10];
MAD R0.xyz, R5, c[13].w, -vertex.position;
DP3 R5.y, R4, R0;
DP3 R5.x, vertex.attrib[14], R0;
DP3 R5.z, vertex.normal, R0;
MOV R0, c[15];
DP3 R1.x, R5, R5;
DP4 R3.w, R0, c[11];
DP4 R3.y, R0, c[9];
DP4 R3.z, R0, c[10];
DP3 R4.y, R3.yzww, R4;
MAD R0.xyz, R1.z, c[22], R2;
DP3 R4.x, R3.yzww, vertex.attrib[14];
DP3 R4.z, vertex.normal, R3.yzww;
RSQ R1.x, R1.x;
MAD R5.xyz, R1.x, R5, R4;
MOV R3.y, R4.w;
MOV R3.z, R5.w;
MOV R3.w, c[0].x;
DP4 R2.z, R3, c[26];
DP4 R2.y, R3, c[25];
DP4 R2.x, R3, c[24];
MAD R1.xyz, R1.w, c[23], R0;
DP3 R0.w, R5, R5;
RSQ R1.w, R0.w;
MUL R0, R3.xyzz, R3.yzzx;
DP4 R3.z, R0, c[29];
DP4 R3.y, R0, c[28];
DP4 R3.x, R0, c[27];
MUL R0.xyz, R2.w, c[30];
ADD R2.xyz, R2, R3;
ADD R0.xyz, R2, R0;
ADD result.texcoord[2].xyz, R0, R1;
MUL result.texcoord[3].xyz, R1.w, R5;
MOV result.texcoord[1].xyz, R4;
MAD result.texcoord[0].xy, vertex.texcoord[0], c[31], c[31].zwzw;
DP4 result.position.w, vertex.position, c[4];
DP4 result.position.z, vertex.position, c[3];
DP4 result.position.y, vertex.position, c[2];
DP4 result.position.x, vertex.position, c[1];
END
# 81 instructions, 6 R-regs
"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Vector 12 [unity_Scale]
Vector 13 [_WorldSpaceCameraPos]
Vector 14 [_WorldSpaceLightPos0]
Matrix 4 [_Object2World]
Matrix 8 [_World2Object]
Vector 15 [unity_4LightPosX0]
Vector 16 [unity_4LightPosY0]
Vector 17 [unity_4LightPosZ0]
Vector 18 [unity_4LightAtten0]
Vector 19 [unity_LightColor0]
Vector 20 [unity_LightColor1]
Vector 21 [unity_LightColor2]
Vector 22 [unity_LightColor3]
Vector 23 [unity_SHAr]
Vector 24 [unity_SHAg]
Vector 25 [unity_SHAb]
Vector 26 [unity_SHBr]
Vector 27 [unity_SHBg]
Vector 28 [unity_SHBb]
Vector 29 [unity_SHC]
Vector 30 [_MainTex_ST]
"vs_2_0
; 84 ALU
def c31, 1.00000000, 0.00000000, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
mul r3.xyz, v2, c12.w
dp4 r0.x, v0, c5
add r1, -r0.x, c16
dp3 r3.w, r3, c5
mul r2, r3.w, r1
dp4 r0.x, v0, c4
dp3 r4.x, r3, c4
add r0, -r0.x, c15
mul r1, r1, r1
mad r2, r4.x, r0, r2
dp3 r5.w, r3, c6
dp4 r4.y, v0, c6
mad r1, r0, r0, r1
add r0, -r4.y, c17
mad r1, r0, r0, r1
mad r0, r5.w, r0, r2
mul r2, r1, c18
rsq r1.x, r1.x
rsq r1.y, r1.y
rsq r1.w, r1.w
rsq r1.z, r1.z
mul r0, r0, r1
add r1, r2, c31.x
max r0, r0, c31.y
rcp r1.x, r1.x
rcp r1.y, r1.y
rcp r1.w, r1.w
rcp r1.z, r1.z
mul r6, r0, r1
mul r1.xyz, r6.y, c20
mad r5.xyz, r6.x, c19, r1
mov r0.xyz, v1
mul r1.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r1.xyz, v2.yzxw, r0.zxyw, -r1
mul r3.xyz, r1, v1.w
mov r0.w, c31.x
mov r0.xyz, c13
dp4 r2.z, r0, c10
dp4 r2.x, r0, c8
dp4 r2.y, r0, c9
mad r0.xyz, r2, c12.w, -v0
dp3 r2.y, r3, r0
dp3 r2.x, v1, r0
dp3 r2.z, v2, r0
dp3 r1.x, r2, r2
rsq r2.w, r1.x
mov r1, c8
dp4 r4.y, c14, r1
mov r0, c10
dp4 r4.w, c14, r0
mov r0, c9
dp4 r4.z, c14, r0
mul r1.w, r3, r3
dp3 r0.y, r4.yzww, r3
dp3 r0.x, r4.yzww, v1
dp3 r0.z, v2, r4.yzww
mad r1.xyz, r2.w, r2, r0
mad r2.xyz, r6.z, c21, r5
dp3 r0.w, r1, r1
rsq r0.w, r0.w
mov r4.y, r3.w
mov r4.z, r5.w
mov r4.w, c31.x
mad r3.xyz, r6.w, c22, r2
mul r2, r4.xyzz, r4.yzzx
dp4 r5.z, r4, c25
dp4 r5.y, r4, c24
dp4 r5.x, r4, c23
mad r1.w, r4.x, r4.x, -r1
dp4 r4.z, r2, c28
dp4 r4.y, r2, c27
dp4 r4.x, r2, c26
mul r2.xyz, r1.w, c29
add r4.xyz, r5, r4
add r2.xyz, r4, r2
add oT2.xyz, r2, r3
mul oT3.xyz, r0.w, r1
mov oT1.xyz, r0
mad oT0.xy, v3, c30, c30.zwzw
dp4 oPos.w, v0, c3
dp4 oPos.z, v0, c2
dp4 oPos.y, v0, c1
dp4 oPos.x, v0, c0
"
}

SubProgram "xbox360 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Vector 30 [_MainTex_ST]
Matrix 7 [_Object2World] 4
Matrix 11 [_World2Object] 4
Vector 5 [_WorldSpaceCameraPos]
Vector 6 [_WorldSpaceLightPos0]
Matrix 0 [glstate_matrix_mvp] 4
Vector 18 [unity_4LightAtten0]
Vector 15 [unity_4LightPosX0]
Vector 16 [unity_4LightPosY0]
Vector 17 [unity_4LightPosZ0]
Vector 19 [unity_LightColor0]
Vector 20 [unity_LightColor1]
Vector 21 [unity_LightColor2]
Vector 22 [unity_LightColor3]
Vector 25 [unity_SHAb]
Vector 24 [unity_SHAg]
Vector 23 [unity_SHAr]
Vector 28 [unity_SHBb]
Vector 27 [unity_SHBg]
Vector 26 [unity_SHBr]
Vector 29 [unity_SHC]
Vector 4 [unity_Scale]
// Shader Timing Estimate, in Cycles/64 vertex vector:
// ALU: 94.67 (71 instructions), vertex: 32, texture: 0,
//   sequencer: 32,  13 GPRs, 12 threads,
// Performance (if enough threads): ~94 cycles per vector
// * Vertex cycle estimates are assuming 3 vfetch_minis for every vfetch_full,
//     with <= 32 bytes per vfetch_full group.

"vs_360
backbbabaaaaadlaaaaaaedaaaaaaaaaaaaaaaceaaaaaddaaaaaadfiaaaaaaaa
aaaaaaaaaaaaadaiaaaaaabmaaaaacplpppoadaaaaaaaabdaaaaaabmaaaaaaaa
aaaaacpeaaaaabjiaaacaaboaaabaaaaaaaaabkeaaaaaaaaaaaaableaaacaaah
aaaeaaaaaaaaabmeaaaaaaaaaaaaabneaaacaaalaaaeaaaaaaaaabmeaaaaaaaa
aaaaabocaaacaaafaaabaaaaaaaaabpiaaaaaaaaaaaaacaiaaacaaagaaabaaaa
aaaaabkeaaaaaaaaaaaaacbnaaacaaaaaaaeaaaaaaaaabmeaaaaaaaaaaaaacda
aaacaabcaaabaaaaaaaaabkeaaaaaaaaaaaaacedaaacaaapaaabaaaaaaaaabke
aaaaaaaaaaaaacffaaacaabaaaabaaaaaaaaabkeaaaaaaaaaaaaacghaaacaabb
aaabaaaaaaaaabkeaaaaaaaaaaaaachjaaacaabdaaaeaaaaaaaaacimaaaaaaaa
aaaaacjmaaacaabjaaabaaaaaaaaabkeaaaaaaaaaaaaackhaaacaabiaaabaaaa
aaaaabkeaaaaaaaaaaaaaclcaaacaabhaaabaaaaaaaaabkeaaaaaaaaaaaaacln
aaacaabmaaabaaaaaaaaabkeaaaaaaaaaaaaacmiaaacaablaaabaaaaaaaaabke
aaaaaaaaaaaaacndaaacaabkaaabaaaaaaaaabkeaaaaaaaaaaaaacnoaaacaabn
aaabaaaaaaaaabkeaaaaaaaaaaaaacoiaaacaaaeaaabaaaaaaaaabkeaaaaaaaa
fpengbgjgofegfhifpfdfeaaaaabaaadaaabaaaeaaabaaaaaaaaaaaafpepgcgk
gfgdhedcfhgphcgmgeaaklklaaadaaadaaaeaaaeaaabaaaaaaaaaaaafpfhgphc
gmgedcepgcgkgfgdheaafpfhgphcgmgefdhagbgdgfedgbgngfhcgbfagphdaakl
aaabaaadaaabaaadaaabaaaaaaaaaaaafpfhgphcgmgefdhagbgdgfemgjghgihe
fagphddaaaghgmhdhegbhegffpgngbhehcgjhifpgnhghaaahfgogjhehjfpdeem
gjghgiheebhehegfgodaaahfgogjhehjfpdeemgjghgihefagphdfidaaahfgogj
hehjfpdeemgjghgihefagphdfjdaaahfgogjhehjfpdeemgjghgihefagphdfkda
aahfgogjhehjfpemgjghgiheedgpgmgphcaaklklaaabaaadaaabaaaeaaaeaaaa
aaaaaaaahfgogjhehjfpfdeiebgcaahfgogjhehjfpfdeiebghaahfgogjhehjfp
fdeiebhcaahfgogjhehjfpfdeiecgcaahfgogjhehjfpfdeiecghaahfgogjhehj
fpfdeiechcaahfgogjhehjfpfdeiedaahfgogjhehjfpfdgdgbgmgfaahghdfpdd
fpdaaadccodacodcdadddfddcodaaaklaaaaaaaaaaaaaaabaaaaaaaaaaaaaaaa
aaaaaabeaapmaabaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaeaaaaaadpa
aadbaaamaaaaaaaaaaaaaaaaaaaacmieaaaaaaabaaaaaaaeaaaaaaaeaaaaacja
aabaaaaiaaaagaajaaaadaakaacafaalaaaadafaaaabhbfbaaachcfcaaadhdfd
aaaabaceaaaabaebaaaabafcaaaabaeoaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
dpiaaaaaaaaaaaaaaaaaaaaaaaaaaaaapaffeaaiaaaabcaamcaaaaaaaaaaeaam
aaaabcaameaaaaaaaaaagabagabgbcaabcaaaaaaaaaagabmgaccbcaabcaaaaaa
aaaagacigacobcaabcaaaaaaaaaagadegadkbcaabcaaaaaaaaaagaeagaegbcaa
bcaaaaaaaaaagaembafcbcaaccaaaaaaafpidaaaaaaaaanbaaaaaaaaafpijaaa
aaaaagiiaaaaaaaaafpibaaaaaaaaoiiaaaaaaaaafpicaaaaaaaapmiaaaaaaaa
miapaaaaaamgiiaakbadadaamiapaaaaaalbiiaakladacaamiapaaaaaagmdeje
kladabaamiapiadoaablaadekladaaaamiahaaaeaamamgmaalanafaomiahaaaa
aalelbaacbamagaamiaoaaahaapmgmimclalagaakmboaaafaaimmgaaabanagae
kmioabaaaamgimabibadakaekmehacagaalogfacmbabajaemiahaaaeaalelble
clamafaemiahaaaeaamagmleclalafaemiaoaaaaaalbpmnbkladajaamiahaaai
aamgleaakbacajaamiahaaagabgflomaolabajagmiahaaagaamablaaobagajaa
miahaaaiaabllemaklabaiaimiaoaaaaaagmimnbkladaiaamiahaaadabmabllp
klaeaeadmiabaaaeaaloloaapaadajaamiacaaaeaaloloaapaadabaamiaoaaaa
aablimabkladahaamiahaaakaagmlemaklaaahaimiaiaaagaaloloaapaagadaa
miadiaaaaalalabkilacboboceipakadaalehcgmobakakiaaibpahalaelbaabl
kaaaapagaibpacaiaemgaagmkaaabbakmiabaaafaagngnlbnbaeaeppbeacaaac
abdoanblgpbhakaaaebeamacaadoangmepbiakbabeaiaaacabdoanblgpbjakaa
aecbamaaaakhkhlbipadbkbabeacaaaaabkhkhblkpadblaaaeeeamaaaakhkhmg
ipadbmbabeapaaadabpipiblobaiaiaaaeipamaiaapilbblmbaiakbamiapaaad
aajejepiolamamadmiapaaaiaajemgpiolamakaimiapaaaiaajegmaaolalakai
miapaaadaajejeaaolalaladaichacaaaamdlomgoaacaaakgeipaaafaaejffgb
oaahafacmiahaaacaabllemnklaabnaafiiaafaaaaaaaablocaaaaiffibaahaa
aaaaaagmocaaaaidficaahaaaaaaaablocaaaaidmiapaaaaaadoipgmiladbcpp
miahaaafaamablleclaoagaffiebahadaalololbpaafajidfiicahadaalolomg
paafabidemeeaaadaalolomgpaagafaaembpaaabaapiaagmobaiahaamiahiaab
aaleleaaocadadaamiafaaahaalabllaolaeafademcpabadaapilblbkcabppaa
emieababaagmlbblobadabaabeacaaaaaablmglbobadaaadamehaaaeaambmagm
kbaabfaamiaiaaaeaamgblaaobaeadaabeacaaahaakhkhmgopagafadambcabaa
aaloloblpaahahabkibhabafaabmmaecibabbebgkicoabagaambpmicibabbdbg
kiebabagaalbgmmambagadbgfibiaaafaamgmglbobafadiamiahiaadaamagmaa
obahaaaabeahaaaaaabebamgoaagafabamihabaaaamabalboaaaaeadmiahaaaa
aamabaaaoaaaabaamiahiaacaalemaaaoaacaaaaaaaaaaaaaaaaaaaaaaaaaaaa
"
}

SubProgram "ps3 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Matrix 256 [glstate_matrix_mvp]
Vector 467 [unity_Scale]
Vector 466 [_WorldSpaceCameraPos]
Vector 465 [_WorldSpaceLightPos0]
Matrix 260 [_Object2World]
Matrix 264 [_World2Object]
Vector 464 [unity_4LightPosX0]
Vector 463 [unity_4LightPosY0]
Vector 462 [unity_4LightPosZ0]
Vector 461 [unity_4LightAtten0]
Vector 460 [unity_LightColor0]
Vector 459 [unity_LightColor1]
Vector 458 [unity_LightColor2]
Vector 457 [unity_LightColor3]
Vector 456 [unity_SHAr]
Vector 455 [unity_SHAg]
Vector 454 [unity_SHAb]
Vector 453 [unity_SHBr]
Vector 452 [unity_SHBg]
Vector 451 [unity_SHBb]
Vector 450 [unity_SHC]
Vector 449 [_MainTex_ST]
"sce_vp_rsx // 68 instructions using 10 registers
[Configuration]
8
0000004441050a00
[Defaults]
1
448 2
000000003f800000
[Microcode]
1088
00011c6c005d100d8186c0836041fffc00031c6c00400e0c0106c0836041dffc
00001c6c005d200c0186c0836041dffc00009c6c009d320c013fc0c36041dffc
401f9c6c011c1808010400d740619f9c401f9c6c01d0300d8106c0c360403f80
401f9c6c01d0200d8106c0c360405f80401f9c6c01d0100d8106c0c360409f80
401f9c6c01d0000d8106c0c360411f8000019c6c01d0500d8106c0c360411ffc
00009c6c01d0400d8106c0c360403ffc00001c6c01d0600d8106c0c360403ffc
00029c6c01d0a00d8486c0c360405ffc00029c6c01d0900d8486c0c360409ffc
00029c6c01d0800d8486c0c360411ffc00021c6c0150400c028600c360411ffc
00021c6c0150600c028600c360403ffc00021c6c0150500c028600c360409ffc
00011c6c0190a00c0086c0c360405ffc00011c6c0190900c0086c0c360409ffc
00011c6c0190800c0086c0c360411ffc00001c6c00dce00d8186c0bfe021fffc
00009c6c00dd000d8186c0bfe0a1fffc00019c6c00dcf00d8186c0a001a1fffc
00039c6c00800243011846436041dffc00031c6c010002308121866303a1dffc
00039c6c011d300c04bfc0e30041dffc00011c6c0080002a8886c3436041fffc
00019c6c0080000d8686c3436041fffc00029c6c0080002a8895444360403ffc
00021c6c0040007f8886c08360405ffc00011c6c010000000886c1436121fffc
00009c6c0100000d8286c14361a1fffc00041c6c00800e0c0cbfc0836041dffc
00049c6c0140020c0106074360405ffc00031c6c019c600c0886c0c360405ffc
00031c6c019c700c0886c0c360409ffc00031c6c019c800c0886c0c360411ffc
00029c6c010000000880047fe2a03ffc00019c6c0080000d089a04436041fffc
00011c6c0100007f8886c0436121fffc00001c6c0100000d8086c04360a1fffc
00009c6c01dc300d8686c0c360405ffc00009c6c01dc400d8686c0c360409ffc
00009c6c01dc500d8686c0c360411ffc00009c6c00c0000c0c86c08300a1dffc
00019c6c009c207f8a8600c36041dffc00019c6c00c0000c0686c08300a1dffc
00049c6c21400e0c01060740003100fc00049c6c2140000c1086074aa02880fc
00021c6c209cd00d8086c0d54025e0fc00019c6c2140000c1286095fe02220fc
00001c6c00dc002a8186c0836221fffc00021c6c2140020c0106055fe1a241fc
00021c6c11400e0c0a8600800031007c00021c6c1140000c0a86084aa028807c
00009c6c1080000d8486c1554025e07c00009c6c129c000d828000dfe023e07c
00011c6c0100007f868609430221dffc00001c6c0080000d8286c0436041fffc
00009c6c009cb02a808600c36041dffc00009c6c0140000c0486024360403ffc
00009c6c011cc000008600c300a1dffc00001c6c011ca055008600c300a1dffc
401f9c6c2040000c0886c09fe0b1c0a000001c6c011c907f808600c30021dffc
401f9c6c00c0000c0686c0830021dfa4401f9c6c00800000028602436041dfa9
"
}

SubProgram "gles " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" "VERTEXLIGHT_ON" }
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;
uniform highp vec4 unity_SHC;
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
uniform highp vec4 unity_LightColor[4];
uniform highp vec4 unity_4LightPosZ0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightAtten0;

uniform lowp vec4 _WorldSpaceLightPos0;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _World2Object;
uniform highp mat4 _Object2World;
uniform highp vec4 _MainTex_ST;
attribute vec4 _glesTANGENT;
attribute vec4 _glesMultiTexCoord0;
attribute vec3 _glesNormal;
attribute vec4 _glesVertex;
void main ()
{
  vec4 tmpvar_1;
  tmpvar_1.xyz = normalize (_glesTANGENT.xyz);
  tmpvar_1.w = _glesTANGENT.w;
  vec3 tmpvar_2;
  tmpvar_2 = normalize (_glesNormal);
  highp vec3 shlight;
  lowp vec3 tmpvar_3;
  lowp vec3 tmpvar_4;
  lowp vec3 tmpvar_5;
  mat3 tmpvar_6;
  tmpvar_6[0] = _Object2World[0].xyz;
  tmpvar_6[1] = _Object2World[1].xyz;
  tmpvar_6[2] = _Object2World[2].xyz;
  highp vec3 tmpvar_7;
  tmpvar_7 = (tmpvar_6 * (tmpvar_2 * unity_Scale.w));
  highp mat3 tmpvar_8;
  tmpvar_8[0] = tmpvar_1.xyz;
  tmpvar_8[1] = (cross (tmpvar_2, tmpvar_1.xyz) * _glesTANGENT.w);
  tmpvar_8[2] = tmpvar_2;
  mat3 tmpvar_9;
  tmpvar_9[0].x = tmpvar_8[0].x;
  tmpvar_9[0].y = tmpvar_8[1].x;
  tmpvar_9[0].z = tmpvar_8[2].x;
  tmpvar_9[1].x = tmpvar_8[0].y;
  tmpvar_9[1].y = tmpvar_8[1].y;
  tmpvar_9[1].z = tmpvar_8[2].y;
  tmpvar_9[2].x = tmpvar_8[0].z;
  tmpvar_9[2].y = tmpvar_8[1].z;
  tmpvar_9[2].z = tmpvar_8[2].z;
  highp vec3 tmpvar_10;
  tmpvar_10 = (tmpvar_9 * (_World2Object * _WorldSpaceLightPos0).xyz);
  tmpvar_3 = tmpvar_10;
  highp vec4 tmpvar_11;
  tmpvar_11.w = 1.0;
  tmpvar_11.xyz = _WorldSpaceCameraPos;
  highp vec3 tmpvar_12;
  tmpvar_12 = normalize ((tmpvar_10 + normalize ((tmpvar_9 * (((_World2Object * tmpvar_11).xyz * unity_Scale.w) - _glesVertex.xyz)))));
  tmpvar_5 = tmpvar_12;
  highp vec4 tmpvar_13;
  tmpvar_13.w = 1.0;
  tmpvar_13.xyz = tmpvar_7;
  mediump vec3 tmpvar_14;
  mediump vec4 normal;
  normal = tmpvar_13;
  mediump vec3 x3;
  highp float vC;
  mediump vec3 x2;
  mediump vec3 x1;
  highp float tmpvar_15;
  tmpvar_15 = dot (unity_SHAr, normal);
  x1.x = tmpvar_15;
  highp float tmpvar_16;
  tmpvar_16 = dot (unity_SHAg, normal);
  x1.y = tmpvar_16;
  highp float tmpvar_17;
  tmpvar_17 = dot (unity_SHAb, normal);
  x1.z = tmpvar_17;
  mediump vec4 tmpvar_18;
  tmpvar_18 = (normal.xyzz * normal.yzzx);
  highp float tmpvar_19;
  tmpvar_19 = dot (unity_SHBr, tmpvar_18);
  x2.x = tmpvar_19;
  highp float tmpvar_20;
  tmpvar_20 = dot (unity_SHBg, tmpvar_18);
  x2.y = tmpvar_20;
  highp float tmpvar_21;
  tmpvar_21 = dot (unity_SHBb, tmpvar_18);
  x2.z = tmpvar_21;
  mediump float tmpvar_22;
  tmpvar_22 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_22;
  highp vec3 tmpvar_23;
  tmpvar_23 = (unity_SHC.xyz * vC);
  x3 = tmpvar_23;
  tmpvar_14 = ((x1 + x2) + x3);
  shlight = tmpvar_14;
  tmpvar_4 = shlight;
  highp vec3 tmpvar_24;
  tmpvar_24 = (_Object2World * _glesVertex).xyz;
  highp vec4 tmpvar_25;
  tmpvar_25 = (unity_4LightPosX0 - tmpvar_24.x);
  highp vec4 tmpvar_26;
  tmpvar_26 = (unity_4LightPosY0 - tmpvar_24.y);
  highp vec4 tmpvar_27;
  tmpvar_27 = (unity_4LightPosZ0 - tmpvar_24.z);
  highp vec4 tmpvar_28;
  tmpvar_28 = (((tmpvar_25 * tmpvar_25) + (tmpvar_26 * tmpvar_26)) + (tmpvar_27 * tmpvar_27));
  highp vec4 tmpvar_29;
  tmpvar_29 = (max (vec4(0.0, 0.0, 0.0, 0.0), ((((tmpvar_25 * tmpvar_7.x) + (tmpvar_26 * tmpvar_7.y)) + (tmpvar_27 * tmpvar_7.z)) * inversesqrt (tmpvar_28))) * (1.0/((1.0 + (tmpvar_28 * unity_4LightAtten0)))));
  highp vec3 tmpvar_30;
  tmpvar_30 = (tmpvar_4 + ((((unity_LightColor[0].xyz * tmpvar_29.x) + (unity_LightColor[1].xyz * tmpvar_29.y)) + (unity_LightColor[2].xyz * tmpvar_29.z)) + (unity_LightColor[3].xyz * tmpvar_29.w)));
  tmpvar_4 = tmpvar_30;
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = tmpvar_3;
  xlv_TEXCOORD2 = tmpvar_4;
  xlv_TEXCOORD3 = tmpvar_5;
}



#endif
#ifdef FRAGMENT

varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform mediump float _Shininess;
uniform sampler2D _MainTex;
uniform lowp vec4 _LightColor0;
uniform sampler2D _BumpMap;
void main ()
{
  lowp vec4 c;
  lowp vec4 tmpvar_1;
  tmpvar_1 = texture2D (_MainTex, xlv_TEXCOORD0);
  lowp vec3 tmpvar_2;
  tmpvar_2 = ((texture2D (_BumpMap, xlv_TEXCOORD0).xyz * 2.0) - 1.0);
  lowp vec4 c_i0;
  lowp float spec;
  lowp float tmpvar_3;
  tmpvar_3 = max (0.0, dot (tmpvar_2, xlv_TEXCOORD3));
  mediump float tmpvar_4;
  tmpvar_4 = (pow (tmpvar_3, (_Shininess * 128.0)) * tmpvar_1.w);
  spec = tmpvar_4;
  c_i0.xyz = ((((tmpvar_1.xyz * _LightColor0.xyz) * max (0.0, dot (tmpvar_2, xlv_TEXCOORD1))) + (_LightColor0.xyz * spec)) * 2.0);
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_1.xyz * xlv_TEXCOORD2));
  gl_FragData[0] = c;
}



#endif"
}

SubProgram "glesdesktop " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" "VERTEXLIGHT_ON" }
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;
uniform highp vec4 unity_SHC;
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
uniform highp vec4 unity_LightColor[4];
uniform highp vec4 unity_4LightPosZ0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightAtten0;

uniform lowp vec4 _WorldSpaceLightPos0;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _World2Object;
uniform highp mat4 _Object2World;
uniform highp vec4 _MainTex_ST;
attribute vec4 _glesTANGENT;
attribute vec4 _glesMultiTexCoord0;
attribute vec3 _glesNormal;
attribute vec4 _glesVertex;
void main ()
{
  vec4 tmpvar_1;
  tmpvar_1.xyz = normalize (_glesTANGENT.xyz);
  tmpvar_1.w = _glesTANGENT.w;
  vec3 tmpvar_2;
  tmpvar_2 = normalize (_glesNormal);
  highp vec3 shlight;
  lowp vec3 tmpvar_3;
  lowp vec3 tmpvar_4;
  lowp vec3 tmpvar_5;
  mat3 tmpvar_6;
  tmpvar_6[0] = _Object2World[0].xyz;
  tmpvar_6[1] = _Object2World[1].xyz;
  tmpvar_6[2] = _Object2World[2].xyz;
  highp vec3 tmpvar_7;
  tmpvar_7 = (tmpvar_6 * (tmpvar_2 * unity_Scale.w));
  highp mat3 tmpvar_8;
  tmpvar_8[0] = tmpvar_1.xyz;
  tmpvar_8[1] = (cross (tmpvar_2, tmpvar_1.xyz) * _glesTANGENT.w);
  tmpvar_8[2] = tmpvar_2;
  mat3 tmpvar_9;
  tmpvar_9[0].x = tmpvar_8[0].x;
  tmpvar_9[0].y = tmpvar_8[1].x;
  tmpvar_9[0].z = tmpvar_8[2].x;
  tmpvar_9[1].x = tmpvar_8[0].y;
  tmpvar_9[1].y = tmpvar_8[1].y;
  tmpvar_9[1].z = tmpvar_8[2].y;
  tmpvar_9[2].x = tmpvar_8[0].z;
  tmpvar_9[2].y = tmpvar_8[1].z;
  tmpvar_9[2].z = tmpvar_8[2].z;
  highp vec3 tmpvar_10;
  tmpvar_10 = (tmpvar_9 * (_World2Object * _WorldSpaceLightPos0).xyz);
  tmpvar_3 = tmpvar_10;
  highp vec4 tmpvar_11;
  tmpvar_11.w = 1.0;
  tmpvar_11.xyz = _WorldSpaceCameraPos;
  highp vec3 tmpvar_12;
  tmpvar_12 = normalize ((tmpvar_10 + normalize ((tmpvar_9 * (((_World2Object * tmpvar_11).xyz * unity_Scale.w) - _glesVertex.xyz)))));
  tmpvar_5 = tmpvar_12;
  highp vec4 tmpvar_13;
  tmpvar_13.w = 1.0;
  tmpvar_13.xyz = tmpvar_7;
  mediump vec3 tmpvar_14;
  mediump vec4 normal;
  normal = tmpvar_13;
  mediump vec3 x3;
  highp float vC;
  mediump vec3 x2;
  mediump vec3 x1;
  highp float tmpvar_15;
  tmpvar_15 = dot (unity_SHAr, normal);
  x1.x = tmpvar_15;
  highp float tmpvar_16;
  tmpvar_16 = dot (unity_SHAg, normal);
  x1.y = tmpvar_16;
  highp float tmpvar_17;
  tmpvar_17 = dot (unity_SHAb, normal);
  x1.z = tmpvar_17;
  mediump vec4 tmpvar_18;
  tmpvar_18 = (normal.xyzz * normal.yzzx);
  highp float tmpvar_19;
  tmpvar_19 = dot (unity_SHBr, tmpvar_18);
  x2.x = tmpvar_19;
  highp float tmpvar_20;
  tmpvar_20 = dot (unity_SHBg, tmpvar_18);
  x2.y = tmpvar_20;
  highp float tmpvar_21;
  tmpvar_21 = dot (unity_SHBb, tmpvar_18);
  x2.z = tmpvar_21;
  mediump float tmpvar_22;
  tmpvar_22 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_22;
  highp vec3 tmpvar_23;
  tmpvar_23 = (unity_SHC.xyz * vC);
  x3 = tmpvar_23;
  tmpvar_14 = ((x1 + x2) + x3);
  shlight = tmpvar_14;
  tmpvar_4 = shlight;
  highp vec3 tmpvar_24;
  tmpvar_24 = (_Object2World * _glesVertex).xyz;
  highp vec4 tmpvar_25;
  tmpvar_25 = (unity_4LightPosX0 - tmpvar_24.x);
  highp vec4 tmpvar_26;
  tmpvar_26 = (unity_4LightPosY0 - tmpvar_24.y);
  highp vec4 tmpvar_27;
  tmpvar_27 = (unity_4LightPosZ0 - tmpvar_24.z);
  highp vec4 tmpvar_28;
  tmpvar_28 = (((tmpvar_25 * tmpvar_25) + (tmpvar_26 * tmpvar_26)) + (tmpvar_27 * tmpvar_27));
  highp vec4 tmpvar_29;
  tmpvar_29 = (max (vec4(0.0, 0.0, 0.0, 0.0), ((((tmpvar_25 * tmpvar_7.x) + (tmpvar_26 * tmpvar_7.y)) + (tmpvar_27 * tmpvar_7.z)) * inversesqrt (tmpvar_28))) * (1.0/((1.0 + (tmpvar_28 * unity_4LightAtten0)))));
  highp vec3 tmpvar_30;
  tmpvar_30 = (tmpvar_4 + ((((unity_LightColor[0].xyz * tmpvar_29.x) + (unity_LightColor[1].xyz * tmpvar_29.y)) + (unity_LightColor[2].xyz * tmpvar_29.z)) + (unity_LightColor[3].xyz * tmpvar_29.w)));
  tmpvar_4 = tmpvar_30;
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = tmpvar_3;
  xlv_TEXCOORD2 = tmpvar_4;
  xlv_TEXCOORD3 = tmpvar_5;
}



#endif
#ifdef FRAGMENT

varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform mediump float _Shininess;
uniform sampler2D _MainTex;
uniform lowp vec4 _LightColor0;
uniform sampler2D _BumpMap;
void main ()
{
  lowp vec4 c;
  lowp vec4 tmpvar_1;
  tmpvar_1 = texture2D (_MainTex, xlv_TEXCOORD0);
  lowp vec3 normal;
  normal.xy = ((texture2D (_BumpMap, xlv_TEXCOORD0).wy * 2.0) - 1.0);
  normal.z = sqrt (((1.0 - (normal.x * normal.x)) - (normal.y * normal.y)));
  lowp vec4 c_i0;
  lowp float spec;
  lowp float tmpvar_2;
  tmpvar_2 = max (0.0, dot (normal, xlv_TEXCOORD3));
  mediump float tmpvar_3;
  tmpvar_3 = (pow (tmpvar_2, (_Shininess * 128.0)) * tmpvar_1.w);
  spec = tmpvar_3;
  c_i0.xyz = ((((tmpvar_1.xyz * _LightColor0.xyz) * max (0.0, dot (normal, xlv_TEXCOORD1))) + (_LightColor0.xyz * spec)) * 2.0);
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_1.xyz * xlv_TEXCOORD2));
  gl_FragData[0] = c;
}



#endif"
}

SubProgram "flash " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Vector 12 [unity_Scale]
Vector 13 [_WorldSpaceCameraPos]
Vector 14 [_WorldSpaceLightPos0]
Matrix 4 [_Object2World]
Matrix 8 [_World2Object]
Vector 15 [unity_4LightPosX0]
Vector 16 [unity_4LightPosY0]
Vector 17 [unity_4LightPosZ0]
Vector 18 [unity_4LightAtten0]
Vector 19 [unity_LightColor0]
Vector 20 [unity_LightColor1]
Vector 21 [unity_LightColor2]
Vector 22 [unity_LightColor3]
Vector 23 [unity_SHAr]
Vector 24 [unity_SHAg]
Vector 25 [unity_SHAb]
Vector 26 [unity_SHBr]
Vector 27 [unity_SHBg]
Vector 28 [unity_SHBb]
Vector 29 [unity_SHC]
Vector 30 [_MainTex_ST]
"agal_vs
c31 1.0 0.0 0.0 0.0
[bc]
adaaaaaaadaaahacabaaaaoeaaaaaaaaamaaaappabaaaaaa mul r3.xyz, a1, c12.w
bdaaaaaaaaaaabacaaaaaaoeaaaaaaaaafaaaaoeabaaaaaa dp4 r0.x, a0, c5
bfaaaaaaabaaabacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa neg r1.x, r0.x
abaaaaaaabaaapacabaaaaaaacaaaaaabaaaaaoeabaaaaaa add r1, r1.x, c16
bcaaaaaaadaaaiacadaaaakeacaaaaaaafaaaaoeabaaaaaa dp3 r3.w, r3.xyzz, c5
adaaaaaaacaaapacadaaaappacaaaaaaabaaaaoeacaaaaaa mul r2, r3.w, r1
bdaaaaaaaaaaabacaaaaaaoeaaaaaaaaaeaaaaoeabaaaaaa dp4 r0.x, a0, c4
bcaaaaaaaeaaabacadaaaakeacaaaaaaaeaaaaoeabaaaaaa dp3 r4.x, r3.xyzz, c4
bfaaaaaaaaaaabacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa neg r0.x, r0.x
abaaaaaaaaaaapacaaaaaaaaacaaaaaaapaaaaoeabaaaaaa add r0, r0.x, c15
adaaaaaaabaaapacabaaaaoeacaaaaaaabaaaaoeacaaaaaa mul r1, r1, r1
adaaaaaaafaaapacaeaaaaaaacaaaaaaaaaaaaoeacaaaaaa mul r5, r4.x, r0
abaaaaaaacaaapacafaaaaoeacaaaaaaacaaaaoeacaaaaaa add r2, r5, r2
bcaaaaaaafaaaiacadaaaakeacaaaaaaagaaaaoeabaaaaaa dp3 r5.w, r3.xyzz, c6
bdaaaaaaaeaaacacaaaaaaoeaaaaaaaaagaaaaoeabaaaaaa dp4 r4.y, a0, c6
adaaaaaaagaaapacaaaaaaoeacaaaaaaaaaaaaoeacaaaaaa mul r6, r0, r0
abaaaaaaabaaapacagaaaaoeacaaaaaaabaaaaoeacaaaaaa add r1, r6, r1
bfaaaaaaaaaaacacaeaaaaffacaaaaaaaaaaaaaaaaaaaaaa neg r0.y, r4.y
abaaaaaaaaaaapacaaaaaaffacaaaaaabbaaaaoeabaaaaaa add r0, r0.y, c17
adaaaaaaahaaapacaaaaaaoeacaaaaaaaaaaaaoeacaaaaaa mul r7, r0, r0
abaaaaaaabaaapacahaaaaoeacaaaaaaabaaaaoeacaaaaaa add r1, r7, r1
adaaaaaaaaaaapacafaaaappacaaaaaaaaaaaaoeacaaaaaa mul r0, r5.w, r0
abaaaaaaaaaaapacaaaaaaoeacaaaaaaacaaaaoeacaaaaaa add r0, r0, r2
adaaaaaaacaaapacabaaaaoeacaaaaaabcaaaaoeabaaaaaa mul r2, r1, c18
akaaaaaaabaaabacabaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rsq r1.x, r1.x
akaaaaaaabaaacacabaaaaffacaaaaaaaaaaaaaaaaaaaaaa rsq r1.y, r1.y
akaaaaaaabaaaiacabaaaappacaaaaaaaaaaaaaaaaaaaaaa rsq r1.w, r1.w
akaaaaaaabaaaeacabaaaakkacaaaaaaaaaaaaaaaaaaaaaa rsq r1.z, r1.z
adaaaaaaaaaaapacaaaaaaoeacaaaaaaabaaaaoeacaaaaaa mul r0, r0, r1
abaaaaaaabaaapacacaaaaoeacaaaaaabpaaaaaaabaaaaaa add r1, r2, c31.x
ahaaaaaaaaaaapacaaaaaaoeacaaaaaabpaaaaffabaaaaaa max r0, r0, c31.y
afaaaaaaabaaabacabaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rcp r1.x, r1.x
afaaaaaaabaaacacabaaaaffacaaaaaaaaaaaaaaaaaaaaaa rcp r1.y, r1.y
afaaaaaaabaaaiacabaaaappacaaaaaaaaaaaaaaaaaaaaaa rcp r1.w, r1.w
afaaaaaaabaaaeacabaaaakkacaaaaaaaaaaaaaaaaaaaaaa rcp r1.z, r1.z
adaaaaaaagaaapacaaaaaaoeacaaaaaaabaaaaoeacaaaaaa mul r6, r0, r1
adaaaaaaabaaahacagaaaaffacaaaaaabeaaaaoeabaaaaaa mul r1.xyz, r6.y, c20
adaaaaaaafaaahacagaaaaaaacaaaaaabdaaaaoeabaaaaaa mul r5.xyz, r6.x, c19
abaaaaaaafaaahacafaaaakeacaaaaaaabaaaakeacaaaaaa add r5.xyz, r5.xyzz, r1.xyzz
aaaaaaaaaaaaahacafaaaaoeaaaaaaaaaaaaaaaaaaaaaaaa mov r0.xyz, a5
adaaaaaaabaaahacabaaaancaaaaaaaaaaaaaaajacaaaaaa mul r1.xyz, a1.zxyw, r0.yzxx
aaaaaaaaaaaaahacafaaaaoeaaaaaaaaaaaaaaaaaaaaaaaa mov r0.xyz, a5
adaaaaaaahaaahacabaaaamjaaaaaaaaaaaaaafcacaaaaaa mul r7.xyz, a1.yzxw, r0.zxyy
acaaaaaaabaaahacahaaaakeacaaaaaaabaaaakeacaaaaaa sub r1.xyz, r7.xyzz, r1.xyzz
adaaaaaaadaaahacabaaaakeacaaaaaaafaaaappaaaaaaaa mul r3.xyz, r1.xyzz, a5.w
aaaaaaaaaaaaaiacbpaaaaaaabaaaaaaaaaaaaaaaaaaaaaa mov r0.w, c31.x
aaaaaaaaaaaaahacanaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0.xyz, c13
bdaaaaaaacaaaeacaaaaaaoeacaaaaaaakaaaaoeabaaaaaa dp4 r2.z, r0, c10
bdaaaaaaacaaabacaaaaaaoeacaaaaaaaiaaaaoeabaaaaaa dp4 r2.x, r0, c8
bdaaaaaaacaaacacaaaaaaoeacaaaaaaajaaaaoeabaaaaaa dp4 r2.y, r0, c9
adaaaaaaahaaahacacaaaakeacaaaaaaamaaaappabaaaaaa mul r7.xyz, r2.xyzz, c12.w
acaaaaaaaaaaahacahaaaakeacaaaaaaaaaaaaoeaaaaaaaa sub r0.xyz, r7.xyzz, a0
bcaaaaaaacaaacacadaaaakeacaaaaaaaaaaaakeacaaaaaa dp3 r2.y, r3.xyzz, r0.xyzz
bcaaaaaaacaaabacafaaaaoeaaaaaaaaaaaaaakeacaaaaaa dp3 r2.x, a5, r0.xyzz
bcaaaaaaacaaaeacabaaaaoeaaaaaaaaaaaaaakeacaaaaaa dp3 r2.z, a1, r0.xyzz
bcaaaaaaabaaabacacaaaakeacaaaaaaacaaaakeacaaaaaa dp3 r1.x, r2.xyzz, r2.xyzz
akaaaaaaacaaaiacabaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rsq r2.w, r1.x
aaaaaaaaabaaapacaiaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r1, c8
bdaaaaaaaeaaacacaoaaaaoeabaaaaaaabaaaaoeacaaaaaa dp4 r4.y, c14, r1
aaaaaaaaaaaaapacakaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0, c10
bdaaaaaaaeaaaiacaoaaaaoeabaaaaaaaaaaaaoeacaaaaaa dp4 r4.w, c14, r0
aaaaaaaaaaaaapacajaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0, c9
bdaaaaaaaeaaaeacaoaaaaoeabaaaaaaaaaaaaoeacaaaaaa dp4 r4.z, c14, r0
adaaaaaaabaaaiacadaaaappacaaaaaaadaaaappacaaaaaa mul r1.w, r3.w, r3.w
bcaaaaaaaaaaacacaeaaaapjacaaaaaaadaaaakeacaaaaaa dp3 r0.y, r4.yzww, r3.xyzz
bcaaaaaaaaaaabacaeaaaapjacaaaaaaafaaaaoeaaaaaaaa dp3 r0.x, r4.yzww, a5
bcaaaaaaaaaaaeacabaaaaoeaaaaaaaaaeaaaapjacaaaaaa dp3 r0.z, a1, r4.yzww
adaaaaaaabaaahacacaaaappacaaaaaaacaaaakeacaaaaaa mul r1.xyz, r2.w, r2.xyzz
abaaaaaaabaaahacabaaaakeacaaaaaaaaaaaakeacaaaaaa add r1.xyz, r1.xyzz, r0.xyzz
adaaaaaaacaaahacagaaaakkacaaaaaabfaaaaoeabaaaaaa mul r2.xyz, r6.z, c21
abaaaaaaacaaahacacaaaakeacaaaaaaafaaaakeacaaaaaa add r2.xyz, r2.xyzz, r5.xyzz
bcaaaaaaaaaaaiacabaaaakeacaaaaaaabaaaakeacaaaaaa dp3 r0.w, r1.xyzz, r1.xyzz
akaaaaaaaaaaaiacaaaaaappacaaaaaaaaaaaaaaaaaaaaaa rsq r0.w, r0.w
aaaaaaaaaeaaacacadaaaappacaaaaaaaaaaaaaaaaaaaaaa mov r4.y, r3.w
aaaaaaaaaeaaaeacafaaaappacaaaaaaaaaaaaaaaaaaaaaa mov r4.z, r5.w
aaaaaaaaaeaaaiacbpaaaaaaabaaaaaaaaaaaaaaaaaaaaaa mov r4.w, c31.x
adaaaaaaadaaahacagaaaappacaaaaaabgaaaaoeabaaaaaa mul r3.xyz, r6.w, c22
abaaaaaaadaaahacadaaaakeacaaaaaaacaaaakeacaaaaaa add r3.xyz, r3.xyzz, r2.xyzz
adaaaaaaacaaapacaeaaaakeacaaaaaaaeaaaacjacaaaaaa mul r2, r4.xyzz, r4.yzzx
bdaaaaaaafaaaeacaeaaaaoeacaaaaaabjaaaaoeabaaaaaa dp4 r5.z, r4, c25
bdaaaaaaafaaacacaeaaaaoeacaaaaaabiaaaaoeabaaaaaa dp4 r5.y, r4, c24
bdaaaaaaafaaabacaeaaaaoeacaaaaaabhaaaaoeabaaaaaa dp4 r5.x, r4, c23
adaaaaaaahaaaiacaeaaaaaaacaaaaaaaeaaaaaaacaaaaaa mul r7.w, r4.x, r4.x
acaaaaaaabaaaiacahaaaappacaaaaaaabaaaappacaaaaaa sub r1.w, r7.w, r1.w
bdaaaaaaaeaaaeacacaaaaoeacaaaaaabmaaaaoeabaaaaaa dp4 r4.z, r2, c28
bdaaaaaaaeaaacacacaaaaoeacaaaaaablaaaaoeabaaaaaa dp4 r4.y, r2, c27
bdaaaaaaaeaaabacacaaaaoeacaaaaaabkaaaaoeabaaaaaa dp4 r4.x, r2, c26
adaaaaaaacaaahacabaaaappacaaaaaabnaaaaoeabaaaaaa mul r2.xyz, r1.w, c29
abaaaaaaaeaaahacafaaaakeacaaaaaaaeaaaakeacaaaaaa add r4.xyz, r5.xyzz, r4.xyzz
abaaaaaaacaaahacaeaaaakeacaaaaaaacaaaakeacaaaaaa add r2.xyz, r4.xyzz, r2.xyzz
abaaaaaaacaaahaeacaaaakeacaaaaaaadaaaakeacaaaaaa add v2.xyz, r2.xyzz, r3.xyzz
adaaaaaaadaaahaeaaaaaappacaaaaaaabaaaakeacaaaaaa mul v3.xyz, r0.w, r1.xyzz
aaaaaaaaabaaahaeaaaaaakeacaaaaaaaaaaaaaaaaaaaaaa mov v1.xyz, r0.xyzz
adaaaaaaahaaadacadaaaaoeaaaaaaaaboaaaaoeabaaaaaa mul r7.xy, a3, c30
abaaaaaaaaaaadaeahaaaafeacaaaaaaboaaaaooabaaaaaa add v0.xy, r7.xyyy, c30.zwzw
bdaaaaaaaaaaaiadaaaaaaoeaaaaaaaaadaaaaoeabaaaaaa dp4 o0.w, a0, c3
bdaaaaaaaaaaaeadaaaaaaoeaaaaaaaaacaaaaoeabaaaaaa dp4 o0.z, a0, c2
bdaaaaaaaaaaacadaaaaaaoeaaaaaaaaabaaaaoeabaaaaaa dp4 o0.y, a0, c1
bdaaaaaaaaaaabadaaaaaaoeaaaaaaaaaaaaaaoeabaaaaaa dp4 o0.x, a0, c0
aaaaaaaaaaaaamaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v0.zw, c0
aaaaaaaaabaaaiaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v1.w, c0
aaaaaaaaacaaaiaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v2.w, c0
aaaaaaaaadaaaiaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v3.w, c0
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "tangent" ATTR14
Bind "normal" Normal
Bind "texcoord" TexCoord0
Vector 13 [_ProjectionParams]
Vector 14 [unity_Scale]
Vector 15 [_WorldSpaceCameraPos]
Vector 16 [_WorldSpaceLightPos0]
Matrix 5 [_Object2World]
Matrix 9 [_World2Object]
Vector 17 [unity_4LightPosX0]
Vector 18 [unity_4LightPosY0]
Vector 19 [unity_4LightPosZ0]
Vector 20 [unity_4LightAtten0]
Vector 21 [unity_LightColor0]
Vector 22 [unity_LightColor1]
Vector 23 [unity_LightColor2]
Vector 24 [unity_LightColor3]
Vector 25 [unity_SHAr]
Vector 26 [unity_SHAg]
Vector 27 [unity_SHAb]
Vector 28 [unity_SHBr]
Vector 29 [unity_SHBg]
Vector 30 [unity_SHBb]
Vector 31 [unity_SHC]
Vector 32 [_MainTex_ST]
"!!ARBvp1.0
# 86 ALU
PARAM c[33] = { { 1, 0, 0.5 },
		state.matrix.mvp,
		program.local[5..32] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
TEMP R5;
MUL R4.xyz, vertex.normal, c[14].w;
DP4 R0.x, vertex.position, c[6];
DP4 R2.x, vertex.position, c[5];
DP3 R3.x, R4, c[5];
DP3 R4.w, R4, c[6];
ADD R0, -R0.x, c[18];
MUL R1, R4.w, R0;
ADD R2, -R2.x, c[17];
MUL R0, R0, R0;
MAD R1, R3.x, R2, R1;
DP3 R5.w, R4, c[7];
DP4 R3.y, vertex.position, c[7];
MAD R0, R2, R2, R0;
ADD R2, -R3.y, c[19];
MAD R0, R2, R2, R0;
MAD R1, R5.w, R2, R1;
MUL R2, R0, c[20];
RSQ R0.x, R0.x;
RSQ R0.y, R0.y;
RSQ R0.w, R0.w;
RSQ R0.z, R0.z;
MUL R0, R1, R0;
ADD R1, R2, c[0].x;
MUL R2.w, R4, R4;
MAX R0, R0, c[0].y;
MAD R2.w, R3.x, R3.x, -R2;
RCP R1.x, R1.x;
RCP R1.y, R1.y;
RCP R1.w, R1.w;
RCP R1.z, R1.z;
MUL R1, R0, R1;
MOV R0.xyz, vertex.attrib[14];
MUL R2.xyz, R1.y, c[22];
MUL R4.xyz, vertex.normal.zxyw, R0.yzxw;
MAD R4.xyz, vertex.normal.yzxw, R0.zxyw, -R4;
MAD R2.xyz, R1.x, c[21], R2;
MUL R4.xyz, R4, vertex.attrib[14].w;
MOV R0.w, c[0].x;
MOV R0.xyz, c[15];
DP4 R5.z, R0, c[11];
DP4 R5.x, R0, c[9];
DP4 R5.y, R0, c[10];
MAD R0.xyz, R5, c[14].w, -vertex.position;
DP3 R5.y, R4, R0;
DP3 R5.x, vertex.attrib[14], R0;
DP3 R5.z, vertex.normal, R0;
MOV R0, c[16];
DP3 R1.x, R5, R5;
DP4 R3.w, R0, c[11];
DP4 R3.y, R0, c[9];
DP4 R3.z, R0, c[10];
DP3 R4.y, R3.yzww, R4;
MAD R0.xyz, R1.z, c[23], R2;
DP3 R4.x, R3.yzww, vertex.attrib[14];
DP3 R4.z, vertex.normal, R3.yzww;
RSQ R1.x, R1.x;
MAD R5.xyz, R1.x, R5, R4;
MOV R3.y, R4.w;
MOV R3.z, R5.w;
MOV R3.w, c[0].x;
MAD R1.xyz, R1.w, c[24], R0;
DP3 R0.w, R5, R5;
RSQ R1.w, R0.w;
MUL R0, R3.xyzz, R3.yzzx;
DP4 R2.z, R3, c[27];
DP4 R2.y, R3, c[26];
DP4 R2.x, R3, c[25];
DP4 R3.z, R0, c[30];
DP4 R3.y, R0, c[29];
DP4 R3.x, R0, c[28];
DP4 R0.w, vertex.position, c[4];
MUL R0.xyz, R2.w, c[31];
ADD R2.xyz, R2, R3;
ADD R0.xyz, R2, R0;
ADD result.texcoord[2].xyz, R0, R1;
DP4 R0.z, vertex.position, c[3];
DP4 R0.x, vertex.position, c[1];
DP4 R0.y, vertex.position, c[2];
MUL R1.xyz, R0.xyww, c[0].z;
MUL R1.y, R1, c[13].x;
MUL result.texcoord[3].xyz, R1.w, R5;
MOV result.texcoord[1].xyz, R4;
ADD result.texcoord[4].xy, R1, R1.z;
MOV result.position, R0;
MOV result.texcoord[4].zw, R0;
MAD result.texcoord[0].xy, vertex.texcoord[0], c[32], c[32].zwzw;
END
# 86 instructions, 6 R-regs
"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Vector 12 [_ProjectionParams]
Vector 13 [_ScreenParams]
Vector 14 [unity_Scale]
Vector 15 [_WorldSpaceCameraPos]
Vector 16 [_WorldSpaceLightPos0]
Matrix 4 [_Object2World]
Matrix 8 [_World2Object]
Vector 17 [unity_4LightPosX0]
Vector 18 [unity_4LightPosY0]
Vector 19 [unity_4LightPosZ0]
Vector 20 [unity_4LightAtten0]
Vector 21 [unity_LightColor0]
Vector 22 [unity_LightColor1]
Vector 23 [unity_LightColor2]
Vector 24 [unity_LightColor3]
Vector 25 [unity_SHAr]
Vector 26 [unity_SHAg]
Vector 27 [unity_SHAb]
Vector 28 [unity_SHBr]
Vector 29 [unity_SHBg]
Vector 30 [unity_SHBb]
Vector 31 [unity_SHC]
Vector 32 [_MainTex_ST]
"vs_2_0
; 90 ALU
def c33, 1.00000000, 0.00000000, 0.50000000, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
mul r3.xyz, v2, c14.w
dp4 r0.x, v0, c5
add r1, -r0.x, c18
dp3 r3.w, r3, c5
mul r2, r3.w, r1
dp4 r0.x, v0, c4
dp3 r4.x, r3, c4
add r0, -r0.x, c17
mul r1, r1, r1
mad r2, r4.x, r0, r2
dp3 r5.w, r3, c6
dp4 r4.y, v0, c6
mad r1, r0, r0, r1
add r0, -r4.y, c19
mad r1, r0, r0, r1
mad r0, r5.w, r0, r2
mul r2, r1, c20
rsq r1.x, r1.x
rsq r1.y, r1.y
rsq r1.w, r1.w
rsq r1.z, r1.z
mul r0, r0, r1
add r1, r2, c33.x
max r0, r0, c33.y
rcp r1.x, r1.x
rcp r1.y, r1.y
rcp r1.w, r1.w
rcp r1.z, r1.z
mul r6, r0, r1
mul r1.xyz, r6.y, c22
mad r5.xyz, r6.x, c21, r1
mov r0.xyz, v1
mul r1.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r1.xyz, v2.yzxw, r0.zxyw, -r1
mul r3.xyz, r1, v1.w
mov r0.w, c33.x
mov r0.xyz, c15
dp4 r2.z, r0, c10
dp4 r2.x, r0, c8
dp4 r2.y, r0, c9
mad r0.xyz, r2, c14.w, -v0
dp3 r2.y, r3, r0
dp3 r2.x, v1, r0
dp3 r2.z, v2, r0
dp3 r1.x, r2, r2
rsq r2.w, r1.x
mov r1, c8
dp4 r4.y, c16, r1
mov r0, c10
dp4 r4.w, c16, r0
mov r0, c9
dp4 r4.z, c16, r0
mul r1.w, r3, r3
dp3 r0.y, r4.yzww, r3
dp3 r0.x, r4.yzww, v1
dp3 r0.z, v2, r4.yzww
mad r1.xyz, r2.w, r2, r0
dp3 r0.w, r1, r1
mad r2.xyz, r6.z, c23, r5
rsq r0.w, r0.w
mul oT3.xyz, r0.w, r1
mov r4.y, r3.w
mov r4.z, r5.w
mov r4.w, c33.x
mad r3.xyz, r6.w, c24, r2
mul r2, r4.xyzz, r4.yzzx
dp4 r5.z, r4, c27
dp4 r5.y, r4, c26
dp4 r5.x, r4, c25
mad r1.w, r4.x, r4.x, -r1
dp4 r4.z, r2, c30
dp4 r4.y, r2, c29
dp4 r4.x, r2, c28
mul r2.xyz, r1.w, c31
add r4.xyz, r5, r4
add r2.xyz, r4, r2
dp4 r1.w, v0, c3
dp4 r1.z, v0, c2
dp4 r1.x, v0, c0
dp4 r1.y, v0, c1
add oT2.xyz, r2, r3
mul r2.xyz, r1.xyww, c33.z
mov oT1.xyz, r0
mov r0.x, r2
mul r0.y, r2, c12.x
mad oT4.xy, r2.z, c13.zwzw, r0
mov oPos, r1
mov oT4.zw, r1
mad oT0.xy, v3, c32, c32.zwzw
"
}

SubProgram "xbox360 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Vector 32 [_MainTex_ST]
Matrix 9 [_Object2World] 4
Vector 4 [_ProjectionParams]
Vector 5 [_ScreenParams]
Matrix 13 [_World2Object] 4
Vector 7 [_WorldSpaceCameraPos]
Vector 8 [_WorldSpaceLightPos0]
Matrix 0 [glstate_matrix_mvp] 4
Vector 20 [unity_4LightAtten0]
Vector 17 [unity_4LightPosX0]
Vector 18 [unity_4LightPosY0]
Vector 19 [unity_4LightPosZ0]
Vector 21 [unity_LightColor0]
Vector 22 [unity_LightColor1]
Vector 23 [unity_LightColor2]
Vector 24 [unity_LightColor3]
Vector 27 [unity_SHAb]
Vector 26 [unity_SHAg]
Vector 25 [unity_SHAr]
Vector 30 [unity_SHBb]
Vector 29 [unity_SHBg]
Vector 28 [unity_SHBr]
Vector 31 [unity_SHC]
Vector 6 [unity_Scale]
// Shader Timing Estimate, in Cycles/64 vertex vector:
// ALU: 100.00 (75 instructions), vertex: 32, texture: 0,
//   sequencer: 32,  15 GPRs, 12 threads,
// Performance (if enough threads): ~100 cycles per vector
// * Vertex cycle estimates are assuming 3 vfetch_minis for every vfetch_full,
//     with <= 32 bytes per vfetch_full group.

"vs_360
backbbabaaaaaeaeaaaaaegaaaaaaaaaaaaaaaceaaaaadhiaaaaadkaaaaaaaaa
aaaaaaaaaaaaadfaaaaaaabmaaaaadedpppoadaaaaaaaabfaaaaaabmaaaaaaaa
aaaaaddmaaaaabmaaaacaacaaaabaaaaaaaaabmmaaaaaaaaaaaaabnmaaacaaaj
aaaeaaaaaaaaabomaaaaaaaaaaaaabpmaaacaaaeaaabaaaaaaaaabmmaaaaaaaa
aaaaacaoaaacaaafaaabaaaaaaaaabmmaaaaaaaaaaaaacbmaaacaaanaaaeaaaa
aaaaabomaaaaaaaaaaaaacckaaacaaahaaabaaaaaaaaaceaaaaaaaaaaaaaacfa
aaacaaaiaaabaaaaaaaaabmmaaaaaaaaaaaaacgfaaacaaaaaaaeaaaaaaaaabom
aaaaaaaaaaaaachiaaacaabeaaabaaaaaaaaabmmaaaaaaaaaaaaacilaaacaabb
aaabaaaaaaaaabmmaaaaaaaaaaaaacjnaaacaabcaaabaaaaaaaaabmmaaaaaaaa
aaaaackpaaacaabdaaabaaaaaaaaabmmaaaaaaaaaaaaacmbaaacaabfaaaeaaaa
aaaaacneaaaaaaaaaaaaacoeaaacaablaaabaaaaaaaaabmmaaaaaaaaaaaaacop
aaacaabkaaabaaaaaaaaabmmaaaaaaaaaaaaacpkaaacaabjaaabaaaaaaaaabmm
aaaaaaaaaaaaadafaaacaaboaaabaaaaaaaaabmmaaaaaaaaaaaaadbaaaacaabn
aaabaaaaaaaaabmmaaaaaaaaaaaaadblaaacaabmaaabaaaaaaaaabmmaaaaaaaa
aaaaadcgaaacaabpaaabaaaaaaaaabmmaaaaaaaaaaaaaddaaaacaaagaaabaaaa
aaaaabmmaaaaaaaafpengbgjgofegfhifpfdfeaaaaabaaadaaabaaaeaaabaaaa
aaaaaaaafpepgcgkgfgdhedcfhgphcgmgeaaklklaaadaaadaaaeaaaeaaabaaaa
aaaaaaaafpfahcgpgkgfgdhegjgpgofagbhcgbgnhdaafpfdgdhcgfgfgofagbhc
gbgnhdaafpfhgphcgmgedcepgcgkgfgdheaafpfhgphcgmgefdhagbgdgfedgbgn
gfhcgbfagphdaaklaaabaaadaaabaaadaaabaaaaaaaaaaaafpfhgphcgmgefdha
gbgdgfemgjghgihefagphddaaaghgmhdhegbhegffpgngbhehcgjhifpgnhghaaa
hfgogjhehjfpdeemgjghgiheebhehegfgodaaahfgogjhehjfpdeemgjghgihefa
gphdfidaaahfgogjhehjfpdeemgjghgihefagphdfjdaaahfgogjhehjfpdeemgj
ghgihefagphdfkdaaahfgogjhehjfpemgjghgiheedgpgmgphcaaklklaaabaaad
aaabaaaeaaaeaaaaaaaaaaaahfgogjhehjfpfdeiebgcaahfgogjhehjfpfdeieb
ghaahfgogjhehjfpfdeiebhcaahfgogjhehjfpfdeiecgcaahfgogjhehjfpfdei
ecghaahfgogjhehjfpfdeiechcaahfgogjhehjfpfdeiedaahfgogjhehjfpfdgd
gbgmgfaahghdfpddfpdaaadccodacodcdadddfddcodaaaklaaaaaaaaaaaaaaab
aaaaaaaaaaaaaaaaaaaaaabeaapmaabaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaeaaaaaaecaaaebaaaoaaaaaaaaaaaaaaaaaaaadmkfaaaaaaabaaaaaaae
aaaaaaagaaaaacjaaabaaaaiaaaagaajaaaadaakaacafaalaaaadafaaaabhbfb
aaachcfcaaadhdfdaaaepefeaaaabackaaaabaefaaaabafgaaaabafcaaaaaacj
aaaabadgaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaadpaaaaaaaaaaaaaadpiaaaaa
aaaaaaaapaffeaaiaaaabcaamcaaaaaaaaaafaamaaaabcaameaaaaaaaaaagabb
gabhbcaabcaaaaaaaaaagabngacdbcaabcaaaaaaaaaagacjgacpbcaabcaaaaaa
aaaagadfgadlbcaabcaaaaaaaaaagaebgaehbcaabcaaaaaaaaaagaeneafdbcaa
ccaaaaaaafpieaaaaaaaagbbaaaaaaaaafpijaaaaaaaagiiaaaaaaaaafpibaaa
aaaaaoiiaaaaaaaaafpicaaaaaaaaoehaaaaaaaamiapaaaaaabliiaakbaeadaa
miapaaaaaalbnapiklaeacaamiapaaaaaagmdepiklaeabaamiapaaalaamgnaje
klaeaaaamiapiadoaananaaaocalalaamiahaaaaaamamgmaalapahbamiahaaad
aalelbaacbaoaiaamiaoaaahaapmgmimclanaiadkmioaaafaaimmgaaabapaiag
kmihabadaablleabibaeamagkmbhacagaalogfacmbabajagmiahaaaaaalelble
claoahaamiahaaaaaamagmleclanahaamiahaaadaalbmaleklaealadmiahaaai
aagmleaakbacalaamiahaaagabgflomaolabajagmiahaaagaamablaaobagajaa
miahaaaiaabllemaklabakaimiahaaadaagmleleklaeakadmiahaaaaabmabllo
klaaagaemiabaaaeaaloloaapaaaajaamiacaaaeaaloloaapaaaabaamiahaaad
aamgmaleklaeajadmiahaaakaabllemaklaaajaimiaiaaagaaloloaapaagaaaa
ceihakaaaamagmgmkbalppiaaibpahaoaalehcblobakakagaibpacamaegmaagm
kaadbbakbeapaaaiafmgaalbkaadbdadmiamiaaeaanlnlaaocalalaamiadiaaa
aamflabkilaccacamiabaaafaagngnlbnbaeaeppaebbanalaadoangmepbjakbc
beacaaalabdoanlbgpbkakadaeceanalaadoanlbepblakbcbeacaaacabkhkhlb
kpaobmadaeeeanacaakhkhmgipaobnbcbeaiaaacabkhkhlbkpaoboadaeipanad
aapipiblmbaiaibckiipaaaiaapilbebmbaiakaemiapaaadaajejepiolananad
miapaaaiaajemgpiolanakaimiadiaaeaamgbkbiklaaafaamiapaaaiaajegmaa
olamakaimiapaaadaajejeaaolamamadaichacaaaalomdmgoaalacakgeipaaaf
aaejffgboaahafacmiahaaacaabllemnklaabpaafiiaafaaaaaaaablocaaaaif
fibaahaaaaaaaagmocaaaaidficaahaaaaaaaablocaaaaidmiapaaaaaadoipmg
iladbeppmiahaaafaamablleclbaaiaffiebahadaalololbpaafajidfiicahad
aalolomgpaafabidemeeaaadaalolomgpaagafaaembpaaabaapiaagmobaiahaa
miahiaabaaleleaaocadadaamiafaaahaalabllaolaeafademcpabadaapilblb
kcabppaaemieababaagmlbblobadabaabeacaaaaaablmglbobadaaadamehaaae
aambmagmkbaabhaamiaiaaaeaamgblaaobaeadaabeacaaahaakhkhmgopagafad
ambcabaaaaloloblpaahahabkibhabafaabmmaecibabbgbikicoabagaambpmic
ibabbfbikiebabagaalbgmmambagadbifibiaaafaamgmglbobafadiamiahiaad
aamagmaaobahaaaabeahaaaaaabebamgoaagafabamihabaaaamabalboaaaaead
miahaaaaaamabaaaoaaaabaamiahiaacaalemaaaoaacaaaaaaaaaaaaaaaaaaaa
aaaaaaaa"
}

SubProgram "ps3 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Matrix 256 [glstate_matrix_mvp]
Vector 467 [_ProjectionParams]
Vector 466 [unity_Scale]
Vector 465 [_WorldSpaceCameraPos]
Vector 464 [_WorldSpaceLightPos0]
Matrix 260 [_Object2World]
Matrix 264 [_World2Object]
Vector 463 [unity_4LightPosX0]
Vector 462 [unity_4LightPosY0]
Vector 461 [unity_4LightPosZ0]
Vector 460 [unity_4LightAtten0]
Vector 459 [unity_LightColor0]
Vector 458 [unity_LightColor1]
Vector 457 [unity_LightColor2]
Vector 456 [unity_LightColor3]
Vector 455 [unity_SHAr]
Vector 454 [unity_SHAg]
Vector 453 [unity_SHAb]
Vector 452 [unity_SHBr]
Vector 451 [unity_SHBg]
Vector 450 [unity_SHBb]
Vector 449 [unity_SHC]
Vector 448 [_MainTex_ST]
"sce_vp_rsx // 73 instructions using 10 registers
[Configuration]
8
0000004941050a00
[Defaults]
1
447 3
000000003f8000003f000000
[Microcode]
1168
00009c6c005d000d8186c0836041fffc00031c6c00400e0c0106c0836041dffc
00001c6c005d100c0186c0836041dffc00019c6c009d220c013fc0c36041dffc
401f9c6c011c0808010400d740619f9c00011c6c01d0300d8106c0c360403ffc
00011c6c01d0200d8106c0c360405ffc00011c6c01d0100d8106c0c360409ffc
00011c6c01d0000d8106c0c360411ffc00019c6c01d0500d8106c0c360403ffc
00021c6c01d0400d8106c0c360405ffc00001c6c01d0600d8106c0c360403ffc
00029c6c01d0a00d8286c0c360405ffc00029c6c01d0900d8286c0c360409ffc
00029c6c01d0800d8286c0c360411ffc00021c6c0150400c068600c360411ffc
00021c6c0150600c068600c360403ffc00021c6c0150500c068600c360409ffc
00039c6c0190a00c0086c0c360405ffc00039c6c0190900c0086c0c360409ffc
00039c6c0190800c0086c0c360411ffc00001c6c00dcd00d8186c0bfe021fffc
00009c6c00dcf00d8186c0b54221fffc00019c6c00dce00d8186c0bfe1a1fffc
00041c6c00800243011846436041dffc00031c6c01000230812186630421dffc
401f9c6c0040000d8486c0836041ff80401f9c6c004000558486c08360407fac
00039c6c011d200c0ebfc0e30041dffc00041c6c009bf00e04aa80c36041dffc
00011c6c0080002a8886c3436041fffc00041c6c009d302a908000c360409ffc
00019c6c0080000d8686c3436041fffc00029c6c0080002a8895444360403ffc
00021c6c0040007f8886c08360405ffc00011c6c010000000886c1436121fffc
00009c6c0100000d8286c14361a1fffc401f9c6c00c000081086c09544219fac
00041c6c00800e0c0cbfc0836041dffc00049c6c0140020c0106074360405ffc
00031c6c019c500c0886c0c360405ffc00031c6c019c600c0886c0c360409ffc
00031c6c019c700c0886c0c360411ffc00029c6c010000000880047fe2a03ffc
00019c6c0080000d089a04436041fffc00011c6c0100007f8886c0436121fffc
00001c6c0100000d8086c04360a1fffc00009c6c01dc200d8686c0c360405ffc
00009c6c01dc300d8686c0c360409ffc00009c6c01dc400d8686c0c360411ffc
00009c6c00c0000c0c86c08300a1dffc00019c6c009c107f8a8600c36041dffc
00019c6c00c0000c0686c08300a1dffc00049c6c21400e0c01060740003100fc
00049c6c2140000c1086074aa02880fc00021c6c209cc00d8086c0d54025e0fc
00019c6c2140000c1286095fe02220fc00001c6c00dbf02a8186c0836221fffc
00021c6c2140020c0106055fe1a241fc00021c6c11400e0c0a8600800031007c
00021c6c1140000c0a86084aa028807c00009c6c1080000d8486c1554025e07c
00009c6c129bf00d828000dfe023e07c00011c6c0100007f868609430221dffc
00001c6c0080000d8286c0436041fffc00009c6c009ca02a808600c36041dffc
00009c6c0140000c0486024360403ffc00009c6c011cb000008600c300a1dffc
00001c6c011c9055008600c300a1dffc401f9c6c2040000c0886c09fe0b1c0a0
00001c6c011c807f808600c30021dffc401f9c6c00c0000c0686c0830021dfa4
401f9c6c00800000028602436041dfa9
"
}

SubProgram "gles " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" "VERTEXLIGHT_ON" }
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying highp vec4 xlv_TEXCOORD4;
varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;
uniform highp vec4 unity_SHC;
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
uniform highp vec4 unity_LightColor[4];
uniform highp vec4 unity_4LightPosZ0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightAtten0;

uniform lowp vec4 _WorldSpaceLightPos0;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _World2Object;
uniform highp vec4 _ProjectionParams;
uniform highp mat4 _Object2World;
uniform highp vec4 _MainTex_ST;
attribute vec4 _glesTANGENT;
attribute vec4 _glesMultiTexCoord0;
attribute vec3 _glesNormal;
attribute vec4 _glesVertex;
void main ()
{
  vec4 tmpvar_1;
  tmpvar_1.xyz = normalize (_glesTANGENT.xyz);
  tmpvar_1.w = _glesTANGENT.w;
  vec3 tmpvar_2;
  tmpvar_2 = normalize (_glesNormal);
  highp vec3 shlight;
  lowp vec3 tmpvar_3;
  lowp vec3 tmpvar_4;
  lowp vec3 tmpvar_5;
  highp vec4 tmpvar_6;
  tmpvar_6 = (gl_ModelViewProjectionMatrix * _glesVertex);
  mat3 tmpvar_7;
  tmpvar_7[0] = _Object2World[0].xyz;
  tmpvar_7[1] = _Object2World[1].xyz;
  tmpvar_7[2] = _Object2World[2].xyz;
  highp vec3 tmpvar_8;
  tmpvar_8 = (tmpvar_7 * (tmpvar_2 * unity_Scale.w));
  highp mat3 tmpvar_9;
  tmpvar_9[0] = tmpvar_1.xyz;
  tmpvar_9[1] = (cross (tmpvar_2, tmpvar_1.xyz) * _glesTANGENT.w);
  tmpvar_9[2] = tmpvar_2;
  mat3 tmpvar_10;
  tmpvar_10[0].x = tmpvar_9[0].x;
  tmpvar_10[0].y = tmpvar_9[1].x;
  tmpvar_10[0].z = tmpvar_9[2].x;
  tmpvar_10[1].x = tmpvar_9[0].y;
  tmpvar_10[1].y = tmpvar_9[1].y;
  tmpvar_10[1].z = tmpvar_9[2].y;
  tmpvar_10[2].x = tmpvar_9[0].z;
  tmpvar_10[2].y = tmpvar_9[1].z;
  tmpvar_10[2].z = tmpvar_9[2].z;
  highp vec3 tmpvar_11;
  tmpvar_11 = (tmpvar_10 * (_World2Object * _WorldSpaceLightPos0).xyz);
  tmpvar_3 = tmpvar_11;
  highp vec4 tmpvar_12;
  tmpvar_12.w = 1.0;
  tmpvar_12.xyz = _WorldSpaceCameraPos;
  highp vec3 tmpvar_13;
  tmpvar_13 = normalize ((tmpvar_11 + normalize ((tmpvar_10 * (((_World2Object * tmpvar_12).xyz * unity_Scale.w) - _glesVertex.xyz)))));
  tmpvar_5 = tmpvar_13;
  highp vec4 tmpvar_14;
  tmpvar_14.w = 1.0;
  tmpvar_14.xyz = tmpvar_8;
  mediump vec3 tmpvar_15;
  mediump vec4 normal;
  normal = tmpvar_14;
  mediump vec3 x3;
  highp float vC;
  mediump vec3 x2;
  mediump vec3 x1;
  highp float tmpvar_16;
  tmpvar_16 = dot (unity_SHAr, normal);
  x1.x = tmpvar_16;
  highp float tmpvar_17;
  tmpvar_17 = dot (unity_SHAg, normal);
  x1.y = tmpvar_17;
  highp float tmpvar_18;
  tmpvar_18 = dot (unity_SHAb, normal);
  x1.z = tmpvar_18;
  mediump vec4 tmpvar_19;
  tmpvar_19 = (normal.xyzz * normal.yzzx);
  highp float tmpvar_20;
  tmpvar_20 = dot (unity_SHBr, tmpvar_19);
  x2.x = tmpvar_20;
  highp float tmpvar_21;
  tmpvar_21 = dot (unity_SHBg, tmpvar_19);
  x2.y = tmpvar_21;
  highp float tmpvar_22;
  tmpvar_22 = dot (unity_SHBb, tmpvar_19);
  x2.z = tmpvar_22;
  mediump float tmpvar_23;
  tmpvar_23 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_23;
  highp vec3 tmpvar_24;
  tmpvar_24 = (unity_SHC.xyz * vC);
  x3 = tmpvar_24;
  tmpvar_15 = ((x1 + x2) + x3);
  shlight = tmpvar_15;
  tmpvar_4 = shlight;
  highp vec3 tmpvar_25;
  tmpvar_25 = (_Object2World * _glesVertex).xyz;
  highp vec4 tmpvar_26;
  tmpvar_26 = (unity_4LightPosX0 - tmpvar_25.x);
  highp vec4 tmpvar_27;
  tmpvar_27 = (unity_4LightPosY0 - tmpvar_25.y);
  highp vec4 tmpvar_28;
  tmpvar_28 = (unity_4LightPosZ0 - tmpvar_25.z);
  highp vec4 tmpvar_29;
  tmpvar_29 = (((tmpvar_26 * tmpvar_26) + (tmpvar_27 * tmpvar_27)) + (tmpvar_28 * tmpvar_28));
  highp vec4 tmpvar_30;
  tmpvar_30 = (max (vec4(0.0, 0.0, 0.0, 0.0), ((((tmpvar_26 * tmpvar_8.x) + (tmpvar_27 * tmpvar_8.y)) + (tmpvar_28 * tmpvar_8.z)) * inversesqrt (tmpvar_29))) * (1.0/((1.0 + (tmpvar_29 * unity_4LightAtten0)))));
  highp vec3 tmpvar_31;
  tmpvar_31 = (tmpvar_4 + ((((unity_LightColor[0].xyz * tmpvar_30.x) + (unity_LightColor[1].xyz * tmpvar_30.y)) + (unity_LightColor[2].xyz * tmpvar_30.z)) + (unity_LightColor[3].xyz * tmpvar_30.w)));
  tmpvar_4 = tmpvar_31;
  highp vec4 o_i0;
  highp vec4 tmpvar_32;
  tmpvar_32 = (tmpvar_6 * 0.5);
  o_i0 = tmpvar_32;
  highp vec2 tmpvar_33;
  tmpvar_33.x = tmpvar_32.x;
  tmpvar_33.y = (tmpvar_32.y * _ProjectionParams.x);
  o_i0.xy = (tmpvar_33 + tmpvar_32.w);
  o_i0.zw = tmpvar_6.zw;
  gl_Position = tmpvar_6;
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = tmpvar_3;
  xlv_TEXCOORD2 = tmpvar_4;
  xlv_TEXCOORD3 = tmpvar_5;
  xlv_TEXCOORD4 = o_i0;
}



#endif
#ifdef FRAGMENT

varying highp vec4 xlv_TEXCOORD4;
varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform mediump float _Shininess;
uniform sampler2D _ShadowMapTexture;
uniform sampler2D _MainTex;
uniform lowp vec4 _LightColor0;
uniform sampler2D _BumpMap;
void main ()
{
  lowp vec4 c;
  lowp vec4 tmpvar_1;
  tmpvar_1 = texture2D (_MainTex, xlv_TEXCOORD0);
  lowp vec3 tmpvar_2;
  tmpvar_2 = ((texture2D (_BumpMap, xlv_TEXCOORD0).xyz * 2.0) - 1.0);
  lowp vec4 c_i0;
  lowp float spec;
  lowp float tmpvar_3;
  tmpvar_3 = max (0.0, dot (tmpvar_2, xlv_TEXCOORD3));
  mediump float tmpvar_4;
  tmpvar_4 = (pow (tmpvar_3, (_Shininess * 128.0)) * tmpvar_1.w);
  spec = tmpvar_4;
  c_i0.xyz = ((((tmpvar_1.xyz * _LightColor0.xyz) * max (0.0, dot (tmpvar_2, xlv_TEXCOORD1))) + (_LightColor0.xyz * spec)) * (texture2DProj (_ShadowMapTexture, xlv_TEXCOORD4).x * 2.0));
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_1.xyz * xlv_TEXCOORD2));
  gl_FragData[0] = c;
}



#endif"
}

SubProgram "glesdesktop " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" "VERTEXLIGHT_ON" }
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying highp vec4 xlv_TEXCOORD4;
varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;
uniform highp vec4 unity_SHC;
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
uniform highp vec4 unity_LightColor[4];
uniform highp vec4 unity_4LightPosZ0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightAtten0;

uniform lowp vec4 _WorldSpaceLightPos0;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _World2Object;
uniform highp vec4 _ProjectionParams;
uniform highp mat4 _Object2World;
uniform highp vec4 _MainTex_ST;
attribute vec4 _glesTANGENT;
attribute vec4 _glesMultiTexCoord0;
attribute vec3 _glesNormal;
attribute vec4 _glesVertex;
void main ()
{
  vec4 tmpvar_1;
  tmpvar_1.xyz = normalize (_glesTANGENT.xyz);
  tmpvar_1.w = _glesTANGENT.w;
  vec3 tmpvar_2;
  tmpvar_2 = normalize (_glesNormal);
  highp vec3 shlight;
  lowp vec3 tmpvar_3;
  lowp vec3 tmpvar_4;
  lowp vec3 tmpvar_5;
  highp vec4 tmpvar_6;
  tmpvar_6 = (gl_ModelViewProjectionMatrix * _glesVertex);
  mat3 tmpvar_7;
  tmpvar_7[0] = _Object2World[0].xyz;
  tmpvar_7[1] = _Object2World[1].xyz;
  tmpvar_7[2] = _Object2World[2].xyz;
  highp vec3 tmpvar_8;
  tmpvar_8 = (tmpvar_7 * (tmpvar_2 * unity_Scale.w));
  highp mat3 tmpvar_9;
  tmpvar_9[0] = tmpvar_1.xyz;
  tmpvar_9[1] = (cross (tmpvar_2, tmpvar_1.xyz) * _glesTANGENT.w);
  tmpvar_9[2] = tmpvar_2;
  mat3 tmpvar_10;
  tmpvar_10[0].x = tmpvar_9[0].x;
  tmpvar_10[0].y = tmpvar_9[1].x;
  tmpvar_10[0].z = tmpvar_9[2].x;
  tmpvar_10[1].x = tmpvar_9[0].y;
  tmpvar_10[1].y = tmpvar_9[1].y;
  tmpvar_10[1].z = tmpvar_9[2].y;
  tmpvar_10[2].x = tmpvar_9[0].z;
  tmpvar_10[2].y = tmpvar_9[1].z;
  tmpvar_10[2].z = tmpvar_9[2].z;
  highp vec3 tmpvar_11;
  tmpvar_11 = (tmpvar_10 * (_World2Object * _WorldSpaceLightPos0).xyz);
  tmpvar_3 = tmpvar_11;
  highp vec4 tmpvar_12;
  tmpvar_12.w = 1.0;
  tmpvar_12.xyz = _WorldSpaceCameraPos;
  highp vec3 tmpvar_13;
  tmpvar_13 = normalize ((tmpvar_11 + normalize ((tmpvar_10 * (((_World2Object * tmpvar_12).xyz * unity_Scale.w) - _glesVertex.xyz)))));
  tmpvar_5 = tmpvar_13;
  highp vec4 tmpvar_14;
  tmpvar_14.w = 1.0;
  tmpvar_14.xyz = tmpvar_8;
  mediump vec3 tmpvar_15;
  mediump vec4 normal;
  normal = tmpvar_14;
  mediump vec3 x3;
  highp float vC;
  mediump vec3 x2;
  mediump vec3 x1;
  highp float tmpvar_16;
  tmpvar_16 = dot (unity_SHAr, normal);
  x1.x = tmpvar_16;
  highp float tmpvar_17;
  tmpvar_17 = dot (unity_SHAg, normal);
  x1.y = tmpvar_17;
  highp float tmpvar_18;
  tmpvar_18 = dot (unity_SHAb, normal);
  x1.z = tmpvar_18;
  mediump vec4 tmpvar_19;
  tmpvar_19 = (normal.xyzz * normal.yzzx);
  highp float tmpvar_20;
  tmpvar_20 = dot (unity_SHBr, tmpvar_19);
  x2.x = tmpvar_20;
  highp float tmpvar_21;
  tmpvar_21 = dot (unity_SHBg, tmpvar_19);
  x2.y = tmpvar_21;
  highp float tmpvar_22;
  tmpvar_22 = dot (unity_SHBb, tmpvar_19);
  x2.z = tmpvar_22;
  mediump float tmpvar_23;
  tmpvar_23 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_23;
  highp vec3 tmpvar_24;
  tmpvar_24 = (unity_SHC.xyz * vC);
  x3 = tmpvar_24;
  tmpvar_15 = ((x1 + x2) + x3);
  shlight = tmpvar_15;
  tmpvar_4 = shlight;
  highp vec3 tmpvar_25;
  tmpvar_25 = (_Object2World * _glesVertex).xyz;
  highp vec4 tmpvar_26;
  tmpvar_26 = (unity_4LightPosX0 - tmpvar_25.x);
  highp vec4 tmpvar_27;
  tmpvar_27 = (unity_4LightPosY0 - tmpvar_25.y);
  highp vec4 tmpvar_28;
  tmpvar_28 = (unity_4LightPosZ0 - tmpvar_25.z);
  highp vec4 tmpvar_29;
  tmpvar_29 = (((tmpvar_26 * tmpvar_26) + (tmpvar_27 * tmpvar_27)) + (tmpvar_28 * tmpvar_28));
  highp vec4 tmpvar_30;
  tmpvar_30 = (max (vec4(0.0, 0.0, 0.0, 0.0), ((((tmpvar_26 * tmpvar_8.x) + (tmpvar_27 * tmpvar_8.y)) + (tmpvar_28 * tmpvar_8.z)) * inversesqrt (tmpvar_29))) * (1.0/((1.0 + (tmpvar_29 * unity_4LightAtten0)))));
  highp vec3 tmpvar_31;
  tmpvar_31 = (tmpvar_4 + ((((unity_LightColor[0].xyz * tmpvar_30.x) + (unity_LightColor[1].xyz * tmpvar_30.y)) + (unity_LightColor[2].xyz * tmpvar_30.z)) + (unity_LightColor[3].xyz * tmpvar_30.w)));
  tmpvar_4 = tmpvar_31;
  highp vec4 o_i0;
  highp vec4 tmpvar_32;
  tmpvar_32 = (tmpvar_6 * 0.5);
  o_i0 = tmpvar_32;
  highp vec2 tmpvar_33;
  tmpvar_33.x = tmpvar_32.x;
  tmpvar_33.y = (tmpvar_32.y * _ProjectionParams.x);
  o_i0.xy = (tmpvar_33 + tmpvar_32.w);
  o_i0.zw = tmpvar_6.zw;
  gl_Position = tmpvar_6;
  xlv_TEXCOORD0 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = tmpvar_3;
  xlv_TEXCOORD2 = tmpvar_4;
  xlv_TEXCOORD3 = tmpvar_5;
  xlv_TEXCOORD4 = o_i0;
}



#endif
#ifdef FRAGMENT

varying highp vec4 xlv_TEXCOORD4;
varying lowp vec3 xlv_TEXCOORD3;
varying lowp vec3 xlv_TEXCOORD2;
varying lowp vec3 xlv_TEXCOORD1;
varying highp vec2 xlv_TEXCOORD0;
uniform mediump float _Shininess;
uniform sampler2D _ShadowMapTexture;
uniform sampler2D _MainTex;
uniform lowp vec4 _LightColor0;
uniform sampler2D _BumpMap;
void main ()
{
  lowp vec4 c;
  lowp vec4 tmpvar_1;
  tmpvar_1 = texture2D (_MainTex, xlv_TEXCOORD0);
  lowp vec3 normal;
  normal.xy = ((texture2D (_BumpMap, xlv_TEXCOORD0).wy * 2.0) - 1.0);
  normal.z = sqrt (((1.0 - (normal.x * normal.x)) - (normal.y * normal.y)));
  lowp vec4 c_i0;
  lowp float spec;
  lowp float tmpvar_2;
  tmpvar_2 = max (0.0, dot (normal, xlv_TEXCOORD3));
  mediump float tmpvar_3;
  tmpvar_3 = (pow (tmpvar_2, (_Shininess * 128.0)) * tmpvar_1.w);
  spec = tmpvar_3;
  c_i0.xyz = ((((tmpvar_1.xyz * _LightColor0.xyz) * max (0.0, dot (normal, xlv_TEXCOORD1))) + (_LightColor0.xyz * spec)) * (texture2DProj (_ShadowMapTexture, xlv_TEXCOORD4).x * 2.0));
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_1.xyz * xlv_TEXCOORD2));
  gl_FragData[0] = c;
}



#endif"
}

SubProgram "flash " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "tangent" TexCoord2
Bind "normal" Normal
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Vector 12 [_ProjectionParams]
Vector 13 [unity_NPOTScale]
Vector 14 [unity_Scale]
Vector 15 [_WorldSpaceCameraPos]
Vector 16 [_WorldSpaceLightPos0]
Matrix 4 [_Object2World]
Matrix 8 [_World2Object]
Vector 17 [unity_4LightPosX0]
Vector 18 [unity_4LightPosY0]
Vector 19 [unity_4LightPosZ0]
Vector 20 [unity_4LightAtten0]
Vector 21 [unity_LightColor0]
Vector 22 [unity_LightColor1]
Vector 23 [unity_LightColor2]
Vector 24 [unity_LightColor3]
Vector 25 [unity_SHAr]
Vector 26 [unity_SHAg]
Vector 27 [unity_SHAb]
Vector 28 [unity_SHBr]
Vector 29 [unity_SHBg]
Vector 30 [unity_SHBb]
Vector 31 [unity_SHC]
Vector 32 [_MainTex_ST]
"agal_vs
c33 1.0 0.0 0.5 0.0
[bc]
adaaaaaaadaaahacabaaaaoeaaaaaaaaaoaaaappabaaaaaa mul r3.xyz, a1, c14.w
bdaaaaaaaaaaabacaaaaaaoeaaaaaaaaafaaaaoeabaaaaaa dp4 r0.x, a0, c5
bfaaaaaaabaaabacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa neg r1.x, r0.x
abaaaaaaabaaapacabaaaaaaacaaaaaabcaaaaoeabaaaaaa add r1, r1.x, c18
bcaaaaaaadaaaiacadaaaakeacaaaaaaafaaaaoeabaaaaaa dp3 r3.w, r3.xyzz, c5
adaaaaaaacaaapacadaaaappacaaaaaaabaaaaoeacaaaaaa mul r2, r3.w, r1
bdaaaaaaaaaaabacaaaaaaoeaaaaaaaaaeaaaaoeabaaaaaa dp4 r0.x, a0, c4
bcaaaaaaaeaaabacadaaaakeacaaaaaaaeaaaaoeabaaaaaa dp3 r4.x, r3.xyzz, c4
bfaaaaaaaaaaabacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa neg r0.x, r0.x
abaaaaaaaaaaapacaaaaaaaaacaaaaaabbaaaaoeabaaaaaa add r0, r0.x, c17
adaaaaaaabaaapacabaaaaoeacaaaaaaabaaaaoeacaaaaaa mul r1, r1, r1
adaaaaaaafaaapacaeaaaaaaacaaaaaaaaaaaaoeacaaaaaa mul r5, r4.x, r0
abaaaaaaacaaapacafaaaaoeacaaaaaaacaaaaoeacaaaaaa add r2, r5, r2
bcaaaaaaafaaaiacadaaaakeacaaaaaaagaaaaoeabaaaaaa dp3 r5.w, r3.xyzz, c6
bdaaaaaaaeaaacacaaaaaaoeaaaaaaaaagaaaaoeabaaaaaa dp4 r4.y, a0, c6
adaaaaaaagaaapacaaaaaaoeacaaaaaaaaaaaaoeacaaaaaa mul r6, r0, r0
abaaaaaaabaaapacagaaaaoeacaaaaaaabaaaaoeacaaaaaa add r1, r6, r1
bfaaaaaaaaaaacacaeaaaaffacaaaaaaaaaaaaaaaaaaaaaa neg r0.y, r4.y
abaaaaaaaaaaapacaaaaaaffacaaaaaabdaaaaoeabaaaaaa add r0, r0.y, c19
adaaaaaaahaaapacaaaaaaoeacaaaaaaaaaaaaoeacaaaaaa mul r7, r0, r0
abaaaaaaabaaapacahaaaaoeacaaaaaaabaaaaoeacaaaaaa add r1, r7, r1
adaaaaaaaaaaapacafaaaappacaaaaaaaaaaaaoeacaaaaaa mul r0, r5.w, r0
abaaaaaaaaaaapacaaaaaaoeacaaaaaaacaaaaoeacaaaaaa add r0, r0, r2
adaaaaaaacaaapacabaaaaoeacaaaaaabeaaaaoeabaaaaaa mul r2, r1, c20
akaaaaaaabaaabacabaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rsq r1.x, r1.x
akaaaaaaabaaacacabaaaaffacaaaaaaaaaaaaaaaaaaaaaa rsq r1.y, r1.y
akaaaaaaabaaaiacabaaaappacaaaaaaaaaaaaaaaaaaaaaa rsq r1.w, r1.w
akaaaaaaabaaaeacabaaaakkacaaaaaaaaaaaaaaaaaaaaaa rsq r1.z, r1.z
adaaaaaaaaaaapacaaaaaaoeacaaaaaaabaaaaoeacaaaaaa mul r0, r0, r1
abaaaaaaabaaapacacaaaaoeacaaaaaacbaaaaaaabaaaaaa add r1, r2, c33.x
ahaaaaaaaaaaapacaaaaaaoeacaaaaaacbaaaaffabaaaaaa max r0, r0, c33.y
afaaaaaaabaaabacabaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rcp r1.x, r1.x
afaaaaaaabaaacacabaaaaffacaaaaaaaaaaaaaaaaaaaaaa rcp r1.y, r1.y
afaaaaaaabaaaiacabaaaappacaaaaaaaaaaaaaaaaaaaaaa rcp r1.w, r1.w
afaaaaaaabaaaeacabaaaakkacaaaaaaaaaaaaaaaaaaaaaa rcp r1.z, r1.z
adaaaaaaagaaapacaaaaaaoeacaaaaaaabaaaaoeacaaaaaa mul r6, r0, r1
adaaaaaaabaaahacagaaaaffacaaaaaabgaaaaoeabaaaaaa mul r1.xyz, r6.y, c22
adaaaaaaafaaahacagaaaaaaacaaaaaabfaaaaoeabaaaaaa mul r5.xyz, r6.x, c21
abaaaaaaafaaahacafaaaakeacaaaaaaabaaaakeacaaaaaa add r5.xyz, r5.xyzz, r1.xyzz
aaaaaaaaaaaaahacafaaaaoeaaaaaaaaaaaaaaaaaaaaaaaa mov r0.xyz, a5
adaaaaaaabaaahacabaaaancaaaaaaaaaaaaaaajacaaaaaa mul r1.xyz, a1.zxyw, r0.yzxx
aaaaaaaaaaaaahacafaaaaoeaaaaaaaaaaaaaaaaaaaaaaaa mov r0.xyz, a5
adaaaaaaahaaahacabaaaamjaaaaaaaaaaaaaafcacaaaaaa mul r7.xyz, a1.yzxw, r0.zxyy
acaaaaaaabaaahacahaaaakeacaaaaaaabaaaakeacaaaaaa sub r1.xyz, r7.xyzz, r1.xyzz
adaaaaaaadaaahacabaaaakeacaaaaaaafaaaappaaaaaaaa mul r3.xyz, r1.xyzz, a5.w
aaaaaaaaaaaaaiaccbaaaaaaabaaaaaaaaaaaaaaaaaaaaaa mov r0.w, c33.x
aaaaaaaaaaaaahacapaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0.xyz, c15
bdaaaaaaacaaaeacaaaaaaoeacaaaaaaakaaaaoeabaaaaaa dp4 r2.z, r0, c10
bdaaaaaaacaaabacaaaaaaoeacaaaaaaaiaaaaoeabaaaaaa dp4 r2.x, r0, c8
bdaaaaaaacaaacacaaaaaaoeacaaaaaaajaaaaoeabaaaaaa dp4 r2.y, r0, c9
adaaaaaaahaaahacacaaaakeacaaaaaaaoaaaappabaaaaaa mul r7.xyz, r2.xyzz, c14.w
acaaaaaaaaaaahacahaaaakeacaaaaaaaaaaaaoeaaaaaaaa sub r0.xyz, r7.xyzz, a0
bcaaaaaaacaaacacadaaaakeacaaaaaaaaaaaakeacaaaaaa dp3 r2.y, r3.xyzz, r0.xyzz
bcaaaaaaacaaabacafaaaaoeaaaaaaaaaaaaaakeacaaaaaa dp3 r2.x, a5, r0.xyzz
bcaaaaaaacaaaeacabaaaaoeaaaaaaaaaaaaaakeacaaaaaa dp3 r2.z, a1, r0.xyzz
bcaaaaaaabaaabacacaaaakeacaaaaaaacaaaakeacaaaaaa dp3 r1.x, r2.xyzz, r2.xyzz
akaaaaaaacaaaiacabaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rsq r2.w, r1.x
aaaaaaaaabaaapacaiaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r1, c8
bdaaaaaaaeaaacacbaaaaaoeabaaaaaaabaaaaoeacaaaaaa dp4 r4.y, c16, r1
aaaaaaaaaaaaapacakaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0, c10
bdaaaaaaaeaaaiacbaaaaaoeabaaaaaaaaaaaaoeacaaaaaa dp4 r4.w, c16, r0
aaaaaaaaaaaaapacajaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0, c9
bdaaaaaaaeaaaeacbaaaaaoeabaaaaaaaaaaaaoeacaaaaaa dp4 r4.z, c16, r0
adaaaaaaabaaaiacadaaaappacaaaaaaadaaaappacaaaaaa mul r1.w, r3.w, r3.w
bcaaaaaaaaaaacacaeaaaapjacaaaaaaadaaaakeacaaaaaa dp3 r0.y, r4.yzww, r3.xyzz
bcaaaaaaaaaaabacaeaaaapjacaaaaaaafaaaaoeaaaaaaaa dp3 r0.x, r4.yzww, a5
bcaaaaaaaaaaaeacabaaaaoeaaaaaaaaaeaaaapjacaaaaaa dp3 r0.z, a1, r4.yzww
adaaaaaaabaaahacacaaaappacaaaaaaacaaaakeacaaaaaa mul r1.xyz, r2.w, r2.xyzz
abaaaaaaabaaahacabaaaakeacaaaaaaaaaaaakeacaaaaaa add r1.xyz, r1.xyzz, r0.xyzz
bcaaaaaaaaaaaiacabaaaakeacaaaaaaabaaaakeacaaaaaa dp3 r0.w, r1.xyzz, r1.xyzz
adaaaaaaacaaahacagaaaakkacaaaaaabhaaaaoeabaaaaaa mul r2.xyz, r6.z, c23
abaaaaaaacaaahacacaaaakeacaaaaaaafaaaakeacaaaaaa add r2.xyz, r2.xyzz, r5.xyzz
akaaaaaaaaaaaiacaaaaaappacaaaaaaaaaaaaaaaaaaaaaa rsq r0.w, r0.w
adaaaaaaadaaahaeaaaaaappacaaaaaaabaaaakeacaaaaaa mul v3.xyz, r0.w, r1.xyzz
aaaaaaaaaeaaacacadaaaappacaaaaaaaaaaaaaaaaaaaaaa mov r4.y, r3.w
aaaaaaaaaeaaaeacafaaaappacaaaaaaaaaaaaaaaaaaaaaa mov r4.z, r5.w
aaaaaaaaaeaaaiaccbaaaaaaabaaaaaaaaaaaaaaaaaaaaaa mov r4.w, c33.x
adaaaaaaadaaahacagaaaappacaaaaaabiaaaaoeabaaaaaa mul r3.xyz, r6.w, c24
abaaaaaaadaaahacadaaaakeacaaaaaaacaaaakeacaaaaaa add r3.xyz, r3.xyzz, r2.xyzz
adaaaaaaacaaapacaeaaaakeacaaaaaaaeaaaacjacaaaaaa mul r2, r4.xyzz, r4.yzzx
bdaaaaaaafaaaeacaeaaaaoeacaaaaaablaaaaoeabaaaaaa dp4 r5.z, r4, c27
bdaaaaaaafaaacacaeaaaaoeacaaaaaabkaaaaoeabaaaaaa dp4 r5.y, r4, c26
bdaaaaaaafaaabacaeaaaaoeacaaaaaabjaaaaoeabaaaaaa dp4 r5.x, r4, c25
adaaaaaaahaaaiacaeaaaaaaacaaaaaaaeaaaaaaacaaaaaa mul r7.w, r4.x, r4.x
acaaaaaaabaaaiacahaaaappacaaaaaaabaaaappacaaaaaa sub r1.w, r7.w, r1.w
bdaaaaaaaeaaaeacacaaaaoeacaaaaaaboaaaaoeabaaaaaa dp4 r4.z, r2, c30
bdaaaaaaaeaaacacacaaaaoeacaaaaaabnaaaaoeabaaaaaa dp4 r4.y, r2, c29
bdaaaaaaaeaaabacacaaaaoeacaaaaaabmaaaaoeabaaaaaa dp4 r4.x, r2, c28
adaaaaaaacaaahacabaaaappacaaaaaabpaaaaoeabaaaaaa mul r2.xyz, r1.w, c31
abaaaaaaaeaaahacafaaaakeacaaaaaaaeaaaakeacaaaaaa add r4.xyz, r5.xyzz, r4.xyzz
abaaaaaaacaaahacaeaaaakeacaaaaaaacaaaakeacaaaaaa add r2.xyz, r4.xyzz, r2.xyzz
bdaaaaaaabaaaiacaaaaaaoeaaaaaaaaadaaaaoeabaaaaaa dp4 r1.w, a0, c3
bdaaaaaaabaaaeacaaaaaaoeaaaaaaaaacaaaaoeabaaaaaa dp4 r1.z, a0, c2
bdaaaaaaabaaabacaaaaaaoeaaaaaaaaaaaaaaoeabaaaaaa dp4 r1.x, a0, c0
bdaaaaaaabaaacacaaaaaaoeaaaaaaaaabaaaaoeabaaaaaa dp4 r1.y, a0, c1
abaaaaaaacaaahaeacaaaakeacaaaaaaadaaaakeacaaaaaa add v2.xyz, r2.xyzz, r3.xyzz
adaaaaaaacaaahacabaaaapeacaaaaaacbaaaakkabaaaaaa mul r2.xyz, r1.xyww, c33.z
aaaaaaaaabaaahaeaaaaaakeacaaaaaaaaaaaaaaaaaaaaaa mov v1.xyz, r0.xyzz
adaaaaaaaaaaacacacaaaaffacaaaaaaamaaaaaaabaaaaaa mul r0.y, r2.y, c12.x
aaaaaaaaaaaaabacacaaaaaaacaaaaaaaaaaaaaaaaaaaaaa mov r0.x, r2.x
abaaaaaaaaaaadacaaaaaafeacaaaaaaacaaaakkacaaaaaa add r0.xy, r0.xyyy, r2.z
adaaaaaaaeaaadaeaaaaaafeacaaaaaaanaaaaoeabaaaaaa mul v4.xy, r0.xyyy, c13
aaaaaaaaaaaaapadabaaaaoeacaaaaaaaaaaaaaaaaaaaaaa mov o0, r1
aaaaaaaaaeaaamaeabaaaaopacaaaaaaaaaaaaaaaaaaaaaa mov v4.zw, r1.wwzw
adaaaaaaahaaadacadaaaaoeaaaaaaaacaaaaaoeabaaaaaa mul r7.xy, a3, c32
abaaaaaaaaaaadaeahaaaafeacaaaaaacaaaaaooabaaaaaa add v0.xy, r7.xyyy, c32.zwzw
aaaaaaaaaaaaamaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v0.zw, c0
aaaaaaaaabaaaiaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v1.w, c0
aaaaaaaaacaaaiaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v2.w, c0
aaaaaaaaadaaaiaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v3.w, c0
"
}

}
Program "fp" {
// Fragment combos: 2
//   opengl - ALU: 22 to 24, TEX: 2 to 3
//   d3d9 - ALU: 25 to 26, TEX: 2 to 3
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Vector 0 [_LightColor0]
Float 1 [_Shininess]
SetTexture 0 [_MainTex] 2D
SetTexture 1 [_BumpMap] 2D
"!!ARBfp1.0
OPTION ARB_precision_hint_fastest;
# 22 ALU, 2 TEX
PARAM c[3] = { program.local[0..1],
		{ 2, 1, 0, 128 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEX R0, fragment.texcoord[0], texture[0], 2D;
TEX R1.yw, fragment.texcoord[0], texture[1], 2D;
MAD R1.xy, R1.wyzw, c[2].x, -c[2].y;
MUL R1.z, R1.y, R1.y;
MAD R1.z, -R1.x, R1.x, -R1;
MUL R2.xyz, R0, fragment.texcoord[2];
ADD R1.z, R1, c[2].y;
RSQ R1.z, R1.z;
RCP R1.z, R1.z;
DP3 R1.w, R1, fragment.texcoord[3];
MOV R2.w, c[2];
MUL R2.w, R2, c[1].x;
MAX R1.w, R1, c[2].z;
POW R1.w, R1.w, R2.w;
MUL R1.w, R0, R1;
DP3 R0.w, R1, fragment.texcoord[1];
MUL R0.xyz, R0, c[0];
MUL R1.xyz, R1.w, c[0];
MAX R0.w, R0, c[2].z;
MAD R0.xyz, R0, R0.w, R1;
MAD result.color.xyz, R0, c[2].x, R2;
MOV result.color.w, c[2].z;
END
# 22 instructions, 3 R-regs
"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Vector 0 [_LightColor0]
Float 1 [_Shininess]
SetTexture 0 [_MainTex] 2D
SetTexture 1 [_BumpMap] 2D
"ps_2_0
; 25 ALU, 2 TEX
dcl_2d s0
dcl_2d s1
def c2, 2.00000000, -1.00000000, 1.00000000, 0.00000000
def c3, 128.00000000, 0, 0, 0
dcl t0.xy
dcl t1.xyz
dcl t2.xyz
dcl t3.xyz
texld r0, t0, s1
texld r3, t0, s0
mov r0.x, r0.w
mad_pp r4.xy, r0, c2.x, c2.y
mul_pp r0.x, r4.y, r4.y
mad_pp r0.x, -r4, r4, -r0
add_pp r0.x, r0, c2.z
rsq_pp r0.x, r0.x
rcp_pp r4.z, r0.x
mov_pp r0.x, c1
dp3_pp r1.x, r4, t3
mul_pp r0.x, c3, r0
max_pp r1.x, r1, c2.w
pow_pp r2.w, r1.x, r0.x
mov_pp r1.x, r2.w
mul_pp r1.x, r3.w, r1
mul_pp r2.xyz, r1.x, c0
dp3_pp r0.x, r4, t1
mul_pp r1.xyz, r3, c0
max_pp r0.x, r0, c2.w
mad_pp r0.xyz, r1, r0.x, r2
mul_pp r1.xyz, r3, t2
mov_pp r0.w, c2
mad_pp r0.xyz, r0, c2.x, r1
mov_pp oC0, r0
"
}

SubProgram "xbox360 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Vector 0 [_LightColor0]
Float 1 [_Shininess]
SetTexture 0 [_MainTex] 2D
SetTexture 1 [_BumpMap] 2D
// Shader Timing Estimate, in Cycles/64 pixel vector:
// ALU: 18.67 (14 instructions), vertex: 0, texture: 8,
//   sequencer: 10, interpolator: 16;    5 GPRs, 36 threads,
// Performance (if enough threads): ~18 cycles per vector
// * Texture cycle estimates are assuming an 8bit/component texture with no
//     aniso or trilinear filtering.

"ps_360
backbbaaaaaaabgaaaaaabdaaaaaaaaaaaaaaaceaaaaabaiaaaaabdaaaaaaaaa
aaaaaaaaaaaaaaoaaaaaaabmaaaaaandppppadaaaaaaaaaeaaaaaabmaaaaaaaa
aaaaaammaaaaaagmaaadaaabaaabaaaaaaaaaahiaaaaaaaaaaaaaaiiaaacaaaa
aaabaaaaaaaaaajiaaaaaaaaaaaaaakiaaadaaaaaaabaaaaaaaaaahiaaaaaaaa
aaaaaalbaaacaaabaaabaaaaaaaaaalmaaaaaaaafpechfgnhaengbhaaaklklkl
aaaeaaamaaabaaabaaabaaaaaaaaaaaafpemgjghgiheedgpgmgphcdaaaklklkl
aaabaaadaaabaaaeaaabaaaaaaaaaaaafpengbgjgofegfhiaafpfdgigjgogjgo
gfhdhdaaaaaaaaadaaabaaabaaabaaaaaaaaaaaahahdfpddfpdaaadccodacodc
dadddfddcodaaaklaaaaaaaaaaaaaaabaaaaaaaaaaaaaaaaaaaaaabeabpmaaba
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaeaaaaaaapabaaaaeaaaaaaaaae
aaaaaaaaaaaacmieaaapaaapaaaaaaabaaaadafaaaaahbfbaaaahcfcaaaahdfd
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
eaaaaaaaaaaaaaaalpiaaaaadpiaaaaaedaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaafcaadaaaabcaameaaaaaaaaaagaafgaalbcaabcaaaaaaaaaacabbaaaaccaa
aaaaaaaabaaieaabbpbppgiiaaaaeaaababiaaabbpbpppnjaaaaeaaamiagaaaa
aagbgmmgilaapopomiabaaaaaelclcblnbaaaapokaiaaaaaaaaaaagmocaaaaia
miabaaaaaamdloaapaaaabaamiacaaaaaamdloaapaaaadaamiafaaaaaalalbaa
kcaapoaaeaciaaaaaagmgmmgcbabppiamiacaaaaaabllbaaobaaaaaadicaaaaa
aaaaaalbocaaaaaabeapaaabaajeielbkbaeaaaaamipaaabaapikmblobabaaae
aaehabaaaamamamlobaeacabmiadaaabaabllalaklaaaaabmiahmaaaaamagmma
klabpoaaaaaaaaaaaaaaaaaaaaaaaaaa"
}

SubProgram "ps3 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Vector 0 [_LightColor0]
Float 1 [_Shininess]
SetTexture 0 [_MainTex] 2D
SetTexture 1 [_BumpMap] 2D
"sce_fp_rsx // 33 instructions using 3 registers
[Configuration]
24
ffffffff0003c020000ffff1000000000000840003000000
[Offsets]
2
_LightColor0 2 0
000001d000000120
_Shininess 1 0
00000040
[Microcode]
528
94001702c8011c9dc8000001c8003fe106820440ce001c9daa02000054020001
00000000000040000000bf80000000001080014000021c9cc8000001c8000001
000000000000000000000000000000009e021700c8011c9dc8000001c8003fe1
02800240ab041c9cab040000c800000102800440c9041c9fc9040001c9000003
02800340c9001c9d00020000c800000100003f80000000000000000000000000
08823b4001003c9cc9000001c80000011e7e7d00c8001c9dc8000001c8000001
e4800540c9041c9dc8010001c8003fe110820900ab001c9c00020000c8000001
00000000000000000000000000000000a2800540c9041c9dc8010001c8003fe1
08801d00ff041c9dc8000001c80000010e820240c8041c9dc8020001c8000001
0000000000000000000000000000000004800240ff001c9d00020000c8000001
000043000000000000000000000000001080020055001c9dab000000c8000001
ce880140c8011c9dc8000001c8003fe104801c40ff001c9dc8000001c8000001
02800900c9001c9d00020000c800000100000000000000000000000000000000
10800240c8041c9dab000000c80000010e800240c9041c9d01000000c8000001
0e8a0440ff001c9dc8021001c900000100000000000000000000000000000000
10800140c8021c9dc8000001c800000100000000000000000000000000000000
0e810440c8041c9dc9100001c9140001
"
}

SubProgram "gles " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
"!!GLES"
}

SubProgram "glesdesktop " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
"!!GLES"
}

SubProgram "flash " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Vector 0 [_LightColor0]
Float 1 [_Shininess]
SetTexture 0 [_MainTex] 2D
SetTexture 1 [_BumpMap] 2D
"agal_ps
c2 2.0 -1.0 1.0 0.0
c3 128.0 0.0 0.0 0.0
[bc]
ciaaaaaaaaaaapacaaaaaaoeaeaaaaaaabaaaaaaafaababb tex r0, v0, s1 <2d wrap linear point>
ciaaaaaaadaaapacaaaaaaoeaeaaaaaaaaaaaaaaafaababb tex r3, v0, s0 <2d wrap linear point>
aaaaaaaaaaaaabacaaaaaappacaaaaaaaaaaaaaaaaaaaaaa mov r0.x, r0.w
adaaaaaaaeaaadacaaaaaafeacaaaaaaacaaaaaaabaaaaaa mul r4.xy, r0.xyyy, c2.x
abaaaaaaaeaaadacaeaaaafeacaaaaaaacaaaaffabaaaaaa add r4.xy, r4.xyyy, c2.y
adaaaaaaaaaaabacaeaaaaffacaaaaaaaeaaaaffacaaaaaa mul r0.x, r4.y, r4.y
bfaaaaaaabaaabacaeaaaaaaacaaaaaaaaaaaaaaaaaaaaaa neg r1.x, r4.x
adaaaaaaabaaabacabaaaaaaacaaaaaaaeaaaaaaacaaaaaa mul r1.x, r1.x, r4.x
acaaaaaaaaaaabacabaaaaaaacaaaaaaaaaaaaaaacaaaaaa sub r0.x, r1.x, r0.x
abaaaaaaaaaaabacaaaaaaaaacaaaaaaacaaaakkabaaaaaa add r0.x, r0.x, c2.z
akaaaaaaaaaaabacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rsq r0.x, r0.x
afaaaaaaaeaaaeacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rcp r4.z, r0.x
aaaaaaaaaaaaabacabaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0.x, c1
bcaaaaaaabaaabacaeaaaakeacaaaaaaadaaaaoeaeaaaaaa dp3 r1.x, r4.xyzz, v3
adaaaaaaaaaaabacadaaaaoeabaaaaaaaaaaaaaaacaaaaaa mul r0.x, c3, r0.x
ahaaaaaaabaaabacabaaaaaaacaaaaaaacaaaappabaaaaaa max r1.x, r1.x, c2.w
alaaaaaaacaaapacabaaaaaaacaaaaaaaaaaaaaaacaaaaaa pow r2, r1.x, r0.x
aaaaaaaaabaaabacacaaaaaaacaaaaaaaaaaaaaaaaaaaaaa mov r1.x, r2.x
adaaaaaaabaaabacadaaaappacaaaaaaabaaaaaaacaaaaaa mul r1.x, r3.w, r1.x
adaaaaaaacaaahacabaaaaaaacaaaaaaaaaaaaoeabaaaaaa mul r2.xyz, r1.x, c0
bcaaaaaaaaaaabacaeaaaakeacaaaaaaabaaaaoeaeaaaaaa dp3 r0.x, r4.xyzz, v1
adaaaaaaabaaahacadaaaakeacaaaaaaaaaaaaoeabaaaaaa mul r1.xyz, r3.xyzz, c0
ahaaaaaaaaaaabacaaaaaaaaacaaaaaaacaaaappabaaaaaa max r0.x, r0.x, c2.w
adaaaaaaaaaaahacabaaaakeacaaaaaaaaaaaaaaacaaaaaa mul r0.xyz, r1.xyzz, r0.x
abaaaaaaaaaaahacaaaaaakeacaaaaaaacaaaakeacaaaaaa add r0.xyz, r0.xyzz, r2.xyzz
adaaaaaaabaaahacadaaaakeacaaaaaaacaaaaoeaeaaaaaa mul r1.xyz, r3.xyzz, v2
aaaaaaaaaaaaaiacacaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0.w, c2
adaaaaaaaaaaahacaaaaaakeacaaaaaaacaaaaaaabaaaaaa mul r0.xyz, r0.xyzz, c2.x
abaaaaaaaaaaahacaaaaaakeacaaaaaaabaaaakeacaaaaaa add r0.xyz, r0.xyzz, r1.xyzz
aaaaaaaaaaaaapadaaaaaaoeacaaaaaaaaaaaaaaaaaaaaaa mov o0, r0
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Vector 0 [_LightColor0]
Float 1 [_Shininess]
SetTexture 0 [_MainTex] 2D
SetTexture 1 [_BumpMap] 2D
SetTexture 2 [_ShadowMapTexture] 2D
"!!ARBfp1.0
OPTION ARB_precision_hint_fastest;
# 24 ALU, 3 TEX
PARAM c[3] = { program.local[0..1],
		{ 2, 1, 0, 128 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEX R1.yw, fragment.texcoord[0], texture[1], 2D;
TEX R0, fragment.texcoord[0], texture[0], 2D;
TXP R1.x, fragment.texcoord[4], texture[2], 2D;
MAD R1.yz, R1.xwyw, c[2].x, -c[2].y;
MUL R1.w, R1.z, R1.z;
MAD R1.w, -R1.y, R1.y, -R1;
MOV R2.x, c[2].w;
ADD R1.w, R1, c[2].y;
RSQ R1.w, R1.w;
RCP R1.w, R1.w;
DP3 R2.y, R1.yzww, fragment.texcoord[3];
MUL R2.z, R2.x, c[1].x;
MAX R2.x, R2.y, c[2].z;
POW R2.w, R2.x, R2.z;
MUL R2.xyz, R0, c[0];
MUL R2.w, R0, R2;
DP3 R0.w, R1.yzww, fragment.texcoord[1];
MUL R1.yzw, R2.w, c[0].xxyz;
MAX R0.w, R0, c[2].z;
MAD R2.xyz, R2, R0.w, R1.yzww;
MUL R0.xyz, R0, fragment.texcoord[2];
MUL R1.xyz, R1.x, R2;
MAD result.color.xyz, R1, c[2].x, R0;
MOV result.color.w, c[2].z;
END
# 24 instructions, 3 R-regs
"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Vector 0 [_LightColor0]
Float 1 [_Shininess]
SetTexture 0 [_MainTex] 2D
SetTexture 1 [_BumpMap] 2D
SetTexture 2 [_ShadowMapTexture] 2D
"ps_2_0
; 26 ALU, 3 TEX
dcl_2d s0
dcl_2d s1
dcl_2d s2
def c2, 2.00000000, -1.00000000, 1.00000000, 0.00000000
def c3, 128.00000000, 0, 0, 0
dcl t0.xy
dcl t1.xyz
dcl t2.xyz
dcl t3.xyz
dcl t4
texld r0, t0, s1
texldp r5, t4, s2
texld r2, t0, s0
mov r0.x, r0.w
mad_pp r4.xy, r0, c2.x, c2.y
mul_pp r0.x, r4.y, r4.y
mad_pp r0.x, -r4, r4, -r0
add_pp r0.x, r0, c2.z
rsq_pp r0.x, r0.x
rcp_pp r4.z, r0.x
mov_pp r0.x, c1
dp3_pp r1.x, r4, t3
mul_pp r0.x, c3, r0
max_pp r1.x, r1, c2.w
pow_pp r3.w, r1.x, r0.x
mov_pp r1.x, r3.w
mul_pp r1.x, r2.w, r1
mul_pp r3.xyz, r1.x, c0
dp3_pp r0.x, r4, t1
mul_pp r1.xyz, r2, c0
max_pp r0.x, r0, c2.w
mad_pp r0.xyz, r1, r0.x, r3
mul_pp r0.xyz, r5.x, r0
mul_pp r1.xyz, r2, t2
mov_pp r0.w, c2
mad_pp r0.xyz, r0, c2.x, r1
mov_pp oC0, r0
"
}

SubProgram "xbox360 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Vector 0 [_LightColor0]
Float 1 [_Shininess]
SetTexture 0 [_ShadowMapTexture] 2D
SetTexture 1 [_MainTex] 2D
SetTexture 2 [_BumpMap] 2D
// Shader Timing Estimate, in Cycles/64 pixel vector:
// ALU: 21.33 (16 instructions), vertex: 0, texture: 12,
//   sequencer: 10, interpolator: 20;    6 GPRs, 30 threads,
// Performance (if enough threads): ~21 cycles per vector
// * Texture cycle estimates are assuming an 8bit/component texture with no
//     aniso or trilinear filtering.

"ps_360
backbbaaaaaaabimaaaaabfeaaaaaaaaaaaaaaceaaaaabdaaaaaabfiaaaaaaaa
aaaaaaaaaaaaabaiaaaaaabmaaaaaaplppppadaaaaaaaaafaaaaaabmaaaaaaaa
aaaaaapeaaaaaaiaaaadaaacaaabaaaaaaaaaaimaaaaaaaaaaaaaajmaaacaaaa
aaabaaaaaaaaaakmaaaaaaaaaaaaaalmaaadaaabaaabaaaaaaaaaaimaaaaaaaa
aaaaaamfaaadaaaaaaabaaaaaaaaaaimaaaaaaaaaaaaaanhaaacaaabaaabaaaa
aaaaaaoeaaaaaaaafpechfgnhaengbhaaaklklklaaaeaaamaaabaaabaaabaaaa
aaaaaaaafpemgjghgiheedgpgmgphcdaaaklklklaaabaaadaaabaaaeaaabaaaa
aaaaaaaafpengbgjgofegfhiaafpfdgigbgegphhengbhafegfhihehfhcgfaafp
fdgigjgogjgogfhdhdaaklklaaaaaaadaaabaaabaaabaaaaaaaaaaaahahdfpdd
fpdaaadccodacodcdadddfddcodaaaklaaaaaaaaaaaaaaabaaaaaaaaaaaaaaaa
aaaaaabeabpmaabaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaeaaaaaabbe
baaaafaaaaaaaaaeaaaaaaaaaaaadmkfaabpaabpaaaaaaabaaaadafaaaaahbfb
aaaahcfcaaaahdfdaaaapefeaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaalpiaaaaa
aaaaaaaadpiaaaaaedaaaaaaabfafaadaaaabcaameaaaaaaaaaagaaigaaobcaa
bcaaaaaaaaaacabeaaaaccaaaaaaaaaaemeaaaaaaaaaaablocaaaaaemiamaaaa
aamgkmaaobaaaeaababifaabbpbppgiiaaaaeaaaliaieaabbpbppppiaaaaeaaa
bacieaabbpbppompaaaaeaaamialaaaaaagcgcaaoaaeaeaamiadaaaeaalagmaa
kaaappaamiaeaaaaaegngnmgnbaeaeppkaeiaeabaagmblmgcbabppiamiabaaad
aaloloaapaaeadaamiacaaadaaloloaapaaeabaamiadaaadaalalbaakcadppaa
eaepaaaeaaaamagmkbafaaidmiabaaabaablmgaaobabaaaadiehabaaaamamagm
obafacabbeabaaabaamgblblobabafaeamedababaagmlamgkbabaaabmiahaaab
aamalbmaolaeadabmiahmaaaaamablmaolabaaaaaaaaaaaaaaaaaaaaaaaaaaaa
"
}

SubProgram "ps3 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Vector 0 [_LightColor0]
Float 1 [_Shininess]
SetTexture 0 [_MainTex] 2D
SetTexture 1 [_BumpMap] 2D
SetTexture 2 [_ShadowMapTexture] 2D
"sce_fp_rsx // 33 instructions using 3 registers
[Configuration]
24
ffffffff0007c020001fffe1000000000000840003000000
[Offsets]
2
_LightColor0 2 0
000001d0000000a0
_Shininess 1 0
00000120
[Microcode]
528
94001702c8011c9dc8000001c8003fe106800440ce001c9d00020000aa020000
000040000000bf80000000000000000008800240ab001c9cab000000c8000001
0880044001001c9e01000000c90000039e021700c8011c9dc8000001c8003fe1
08800340c9001c9d00020000c800000100003f80000000000000000000000000
08803b40c9003c9d55000001c80000010e820240c8041c9dc8020001c8000001
00000000000000000000000000000000b0820540c9001c9dc8010001c8003fe1
10820900c9041c9d00020000c800000100000000000000000000000000000000
f0800540c9001c9dc8010001c8003fe102880900ff001c9daa020000c8000001
000000000000000000000000000000001080014000021c9cc8000001c8000001
00000000000000000000000000000000028a0240ff001c9d00020000c8000001
0000430000000000000000000000000002881d00c9101c9dc8000001c8000001
1080020001101c9c01140000c80000010e820240c9041c9dff040001c8000001
02801c40ff001c9dc8000001c80000011080024001001c9cc8040001c8000001
ce800240c8041c9dc8015001c8003fe102021805c8011c9dc8000001c8003fe1
0e820440ff001c9dc8020001c904000100000000000000000000000000000000
10800140c8021c9dc8000001c800000100000000000000000000000000000000
0e81044000041c9cc9041001c9000001
"
}

SubProgram "gles " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
"!!GLES"
}

SubProgram "glesdesktop " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
"!!GLES"
}

SubProgram "flash " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Vector 0 [_LightColor0]
Float 1 [_Shininess]
SetTexture 0 [_MainTex] 2D
SetTexture 1 [_BumpMap] 2D
SetTexture 2 [_ShadowMapTexture] 2D
"agal_ps
c2 2.0 -1.0 1.0 0.0
c3 128.0 0.0 0.0 0.0
[bc]
ciaaaaaaaaaaapacaaaaaaoeaeaaaaaaabaaaaaaafaababb tex r0, v0, s1 <2d wrap linear point>
aeaaaaaaabaaapacaeaaaaoeaeaaaaaaaeaaaappaeaaaaaa div r1, v4, v4.w
ciaaaaaaafaaapacabaaaafeacaaaaaaacaaaaaaafaababb tex r5, r1.xyyy, s2 <2d wrap linear point>
ciaaaaaaacaaapacaaaaaaoeaeaaaaaaaaaaaaaaafaababb tex r2, v0, s0 <2d wrap linear point>
aaaaaaaaaaaaabacaaaaaappacaaaaaaaaaaaaaaaaaaaaaa mov r0.x, r0.w
adaaaaaaaeaaadacaaaaaafeacaaaaaaacaaaaaaabaaaaaa mul r4.xy, r0.xyyy, c2.x
abaaaaaaaeaaadacaeaaaafeacaaaaaaacaaaaffabaaaaaa add r4.xy, r4.xyyy, c2.y
adaaaaaaaaaaabacaeaaaaffacaaaaaaaeaaaaffacaaaaaa mul r0.x, r4.y, r4.y
bfaaaaaaabaaaiacaeaaaaaaacaaaaaaaaaaaaaaaaaaaaaa neg r1.w, r4.x
adaaaaaaabaaaiacabaaaappacaaaaaaaeaaaaaaacaaaaaa mul r1.w, r1.w, r4.x
acaaaaaaaaaaabacabaaaappacaaaaaaaaaaaaaaacaaaaaa sub r0.x, r1.w, r0.x
abaaaaaaaaaaabacaaaaaaaaacaaaaaaacaaaakkabaaaaaa add r0.x, r0.x, c2.z
akaaaaaaaaaaabacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rsq r0.x, r0.x
afaaaaaaaeaaaeacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rcp r4.z, r0.x
aaaaaaaaaaaaabacabaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0.x, c1
bcaaaaaaabaaabacaeaaaakeacaaaaaaadaaaaoeaeaaaaaa dp3 r1.x, r4.xyzz, v3
adaaaaaaaaaaabacadaaaaoeabaaaaaaaaaaaaaaacaaaaaa mul r0.x, c3, r0.x
ahaaaaaaabaaabacabaaaaaaacaaaaaaacaaaappabaaaaaa max r1.x, r1.x, c2.w
alaaaaaaadaaapacabaaaaaaacaaaaaaaaaaaaaaacaaaaaa pow r3, r1.x, r0.x
aaaaaaaaabaaabacadaaaaaaacaaaaaaaaaaaaaaaaaaaaaa mov r1.x, r3.x
adaaaaaaabaaabacacaaaappacaaaaaaabaaaaaaacaaaaaa mul r1.x, r2.w, r1.x
adaaaaaaadaaahacabaaaaaaacaaaaaaaaaaaaoeabaaaaaa mul r3.xyz, r1.x, c0
bcaaaaaaaaaaabacaeaaaakeacaaaaaaabaaaaoeaeaaaaaa dp3 r0.x, r4.xyzz, v1
adaaaaaaabaaahacacaaaakeacaaaaaaaaaaaaoeabaaaaaa mul r1.xyz, r2.xyzz, c0
ahaaaaaaaaaaabacaaaaaaaaacaaaaaaacaaaappabaaaaaa max r0.x, r0.x, c2.w
adaaaaaaaaaaahacabaaaakeacaaaaaaaaaaaaaaacaaaaaa mul r0.xyz, r1.xyzz, r0.x
abaaaaaaaaaaahacaaaaaakeacaaaaaaadaaaakeacaaaaaa add r0.xyz, r0.xyzz, r3.xyzz
adaaaaaaaaaaahacafaaaaaaacaaaaaaaaaaaakeacaaaaaa mul r0.xyz, r5.x, r0.xyzz
adaaaaaaabaaahacacaaaakeacaaaaaaacaaaaoeaeaaaaaa mul r1.xyz, r2.xyzz, v2
aaaaaaaaaaaaaiacacaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0.w, c2
adaaaaaaaaaaahacaaaaaakeacaaaaaaacaaaaaaabaaaaaa mul r0.xyz, r0.xyzz, c2.x
abaaaaaaaaaaahacaaaaaakeacaaaaaaabaaaakeacaaaaaa add r0.xyz, r0.xyzz, r1.xyzz
aaaaaaaaaaaaapadaaaaaaoeacaaaaaaaaaaaaaaaaaaaaaa mov o0, r0
"
}

}
	}

#LINE 51

}

FallBack "Mobile/VertexLit"
}
