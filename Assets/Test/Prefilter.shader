Shader "UnityMLStableDiffusion/Prefilter"
{
    Properties
    {
        _MainTex("", 2D) = "white" {}
    }
    CGINCLUDE

#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_TexelSize;

void Vertex(float4 inPos : POSITION, float2 inUV : TEXCOORD0,
            out float4 outPos : SV_Position, out float2 outUV : TEXCOORD0)
{
    outPos = UnityObjectToClipPos(inPos);
    outUV = inUV;
}

float4 FragmentBypass(float4 pos : SV_Position, float2 uv : TEXCOORD0) : SV_Target
{
    return tex2D(_MainTex, uv);
}

float4 FragmentMonochrome(float4 pos : SV_Position, float2 uv : TEXCOORD0) : SV_Target
{
    float4 col = tex2D(_MainTex, uv);
    return float4((float3)Luminance(col.rgb), 1);
}

float4 FragmentInvert(float4 pos : SV_Position, float2 uv : TEXCOORD0) : SV_Target
{
    float4 col = tex2D(_MainTex, uv);
    return float4(1 - col.rgb, 1);
}

float4 FragmentContours(float4 pos : SV_Position, float2 uv : TEXCOORD0) : SV_Target
{
    float2 ddxy = float2(ddx(uv.x), ddy(uv.y));
    float4 delta = ddxy.xyxy * float4(1, 1, -1, 0);
    float l0 = Luminance(tex2D(_MainTex, uv - delta.xy).rgb);
    float l1 = Luminance(tex2D(_MainTex, uv - delta.wy).rgb);
    float l2 = Luminance(tex2D(_MainTex, uv - delta.zy).rgb);
    float l3 = Luminance(tex2D(_MainTex, uv - delta.xw).rgb);
    float l4 = Luminance(tex2D(_MainTex, uv           ).rgb);
    float l5 = Luminance(tex2D(_MainTex, uv + delta.xw).rgb);
    float l6 = Luminance(tex2D(_MainTex, uv + delta.zy).rgb);
    float l7 = Luminance(tex2D(_MainTex, uv + delta.wy).rgb);
    float l8 = Luminance(tex2D(_MainTex, uv + delta.xy).rgb);
    float gx = l2 - l0 + (l5 - l3) * 2 + l8 - l6;
    float gy = l6 - l0 + (l7 - l1) * 2 + l8 - l2;
    float g = 1 - smoothstep(0, 0.1, sqrt(gx * gx + gy * gy));
    return float4(g, g, g, 1);
}

float4 FragmentVerticalSplit(float4 pos : SV_Position, float2 uv : TEXCOORD0) : SV_Target
{
    uv = uv.x < 0.5 ? uv : float2(uv.x - 0.5, 1 - uv.y);
    return tex2D(_MainTex, uv);
}

float4 FragmentHorizontalSplit(float4 pos : SV_Position, float2 uv : TEXCOORD0) : SV_Target
{
    uv = uv.y < 0.5 ? uv : float2(1 - uv.x, uv.y - 0.5);
    return tex2D(_MainTex, uv);
}

    ENDCG
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentBypass
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentMonochrome
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentInvert
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentContours
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentVerticalSplit
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentHorizontalSplit
            ENDCG
        }
    }
}
