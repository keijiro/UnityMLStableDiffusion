Shader "UnityMLStableDiffusion/Prefilter"
{
    Properties
    {
        _MainTex("", 2D) = "white" {}
    }
    CGINCLUDE

#include "UnityCG.cginc"

sampler2D _MainTex;

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
    }
}
