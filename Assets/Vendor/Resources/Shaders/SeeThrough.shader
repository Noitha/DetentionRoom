Shader "Custom/SeeThrough"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("SeeThrough Color",Color) = (255,255,255,255)
        
    }
    SubShader
    {
        
        Tags { "RenderPipeline" = "HDRenderPipeline" "Queue"="Transparent" }
        LOD 100
        Pass {
        
        
            Cull Off
            ZWrite Off
            ZTest Always
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float3 RotateAroundZInDegrees (float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                //return float3(mul(m, vertex.xz), vertex.y).xzy;
                return float3(mul(m, vertex.xy), vertex.z).zxy;
            }

            
            v2f vert (appdata v)
            {
                v2f o;
                float rot = (_SinTime.w * 360);
                o.vertex = UnityObjectToClipPos(RotateAroundZInDegrees(v.vertex,rot));
                return o;
            }
            
            float4 _Color;
            
            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float3 RotateAroundZInDegrees (float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                //return float3(mul(m, vertex.xz), vertex.y).xzy;
                return float3(mul(m, vertex.xy), vertex.z).zxy;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float rot = (_SinTime.w * 360);
                o.vertex = UnityObjectToClipPos(RotateAroundZInDegrees(v.vertex,rot));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                return col;
            }
            ENDCG
        }
    }
}
