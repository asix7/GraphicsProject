// Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Adapted for COMP30019 by Jeremy Nicholson, 10 Sep 2012
// Adapted further by Chris Ewin, 23 Sep 2013

// these won't change in a given iteration of the shader
float4x4 World;
float4x4 View;
float4x4 Projection;
float4 cameraPos;
float4 lightAmbCol = float4(1.0f, 1.0f, 1.0f, 1.0f);
float4 lightPntPos = float4(0.0f, 0.0f, -2.0f, 1.0f);
float4 lightPntCol = float4(1.0f, 1.0f, 1.0f, 1.0f);
float3 lightDirPos = float3(-1.0f, -1.0f, 0.0f);
float4 lightDirCol = float4(1.0f, 1.0f, 1.0f, 1.0f);

float Ka = 0.5; // Albedo
float Kd = 1; 
float Ks = 1;
float fAtt = 1;
float specN = 5; // Numbers>>1 give more mirror-like highlights
float transparency = 1.0f;

float4x4 worldInvTrp;
//

texture2D ModelTexture;
SamplerState SampleType;


struct VS_IN
{
	float4 pos : SV_POSITION;
	float4 nrm : NORMAL;
	float2 tex : TEXCOORD0;
	// Other vertex properties, e.g. texture co-ords, surface Kd, Ks, etc
};

struct PS_IN
{
	float4 pos : SV_POSITION; //Position in camera co-ords
	float2 tex : TEXCOORD1;
	float4 wpos : TEXCOORD2; //Position in world co-ords
	float3 wnrm : TEXCOORD3; //Normal in world co-ords 
};


PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.wpos = mul(input.pos, World);
	output.wnrm = mul(input.nrm.xyz, (float3x3)worldInvTrp);

	float4 viewPos = mul(output.wpos, View);
	output.pos = mul(viewPos, Projection);

	output.tex = input.tex;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	// Our interpolated normal might not be of length 1
	float3 interpNormal = normalize(input.wnrm);

	float4 textureColor = ModelTexture.Sample(SampleType, input.tex);
	textureColor.a = transparency;

	// Calculate ambient RGB intensities
	float3 amb = textureColor.rgb*lightAmbCol.rgb*Ka;

	// Calculate diffuse RBG reflections
	float3 L = normalize(lightPntPos.xyz - input.wpos.xyz);
	float LdotN = saturate(dot(L, interpNormal.xyz));
	float3 dif = fAtt * lightPntCol.rgb * Kd * textureColor.rgb * LdotN;

	// Calculate specular reflections	
	float3 V = normalize(cameraPos.xyz - input.wpos.xyz);
	float3 R = normalize(2 * LdotN*interpNormal.xyz - L.xyz);
	//float3 R = normalize(0.5*(L.xyz+V.xyz)); //Blinn-Phong equivalent
	float3 spe = fAtt * lightPntCol.rgb * Ks * pow(saturate(dot(V, R)), specN);

	// Combine reflection components
	float4 returnCol = float4(0.0f, 0.0f, 0.0f, 0.0f);
	returnCol.rgb = amb.rgb + dif.rgb + spe.rgb;
	returnCol.a = textureColor.a;

	return returnCol;
}



technique Lighting
{
	pass Pass1
	{
		Profile = 10;
		VertexShader = VS;
		PixelShader = PS;
	}
}