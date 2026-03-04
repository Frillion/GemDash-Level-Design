Shader "Unlit/DashBlendShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _DashTex ("DashTexture", 2D) = "white" {}
        _Blend ("DashBlend", Float) = 0
        _RotationSpeed ("RotationSpeed", Float) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="True"}
        LOD 100

        Pass
        {
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha 
            ZWrite off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 dash_uv : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _DashTex;
            float4 _MainTex_ST;
            float4 _DashTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.dash_uv = v.vertex.xy / 2 + 0.5;
                o.dash_uv = TRANSFORM_TEX(o.dash_uv, _DashTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            float2 rotate_uv(float2 uv, float2 pivot, float angle)
            {
                float2x2 rotation_matrix = float2x2(
                    float2(cos(angle),sin(angle)),
                    float2(-sin(angle),cos(angle)));
                
                uv -= pivot;
                uv = mul(rotation_matrix, uv);
                return uv + pivot;
            }

            float _Blend;
            float _RotationSpeed;
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 main_col = tex2D(_MainTex, i.uv);
                float2 rotated_uv =  rotate_uv(i.dash_uv, float2(0.5, 0.5),_Time.x * _RotationSpeed);
                fixed4 dash_col = tex2D(_DashTex,rotated_uv);
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return lerp(main_col,dash_col,_Blend);
            }
            ENDCG
        }
    }
}
