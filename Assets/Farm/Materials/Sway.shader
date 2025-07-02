Shader "Unlit/AnimateSprite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _SwayFlip ("Sway Flip", Range(0,1)) = 0
        _SwayAmount ("Sway Amount", Range(0,0.2)) = 0.05
        _SwaySpeed ("Sway Speed", Range(0,10)) = 2.0
        [Toggle] _EnableRainbow ("Enable Shiny Rainbow", Float) = 0
        _RainbowSpeed ("Rainbow Speed", Range(0,10)) = 2.0
        _RainbowIntensity ("Rainbow Intensity", Range(0,2)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _SwayAmount;
            float _SwaySpeed;
            float _SwayFlip;
            float _EnableRainbow;
            float _RainbowSpeed;
            float _RainbowIntensity;
            v2f vert (appdata v)
            {
                v2f o;
                float sway = sin(_Time.y * _SwaySpeed*10 + v.vertex.x * 2.0) * _SwayAmount;
                float4 pos = v.vertex;
                float swayMask = saturate((v.uv.x - 0.8) * 2.0); // 0 for uv.y <= 0.5, 1 for uv.y == 1
                if (_SwayFlip == 1)
                {
                    pos.x += sway * swayMask; // Changed from pos.x to pos.y for vertical sway
                }
                else
                {
                    pos.x -= sway * swayMask; // Changed from pos.x to pos.y for vertical sway
                }
                o.vertex = UnityObjectToClipPos(pos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Rainbow effect
                if (_EnableRainbow > 0.5)
                {
                    // Create smoother rainbow colors using improved HSV to RGB conversion
                    float time = _Time.y * _RainbowSpeed;
                    float hue = frac(time + i.uv.x * -0.5 + i.uv.y * 0); // Smoother UV distribution
                    
                    // Improved HSV to RGB conversion for smoother transitions
                    float3 rainbow;
                    hue = hue * 6.0;
                    float chroma = 1.0;
                    float x = chroma * (1.0 - abs(fmod(hue, 2.0) - 1.0));
                    float m = 0.0;
                    
                    if (hue >= 0.0 && hue < 1.0) {
                        rainbow = float3(chroma, x, 0.0);
                    } else if (hue >= 1.0 && hue < 2.0) {
                        rainbow = float3(x, chroma, 0.0);
                    } else if (hue >= 2.0 && hue < 3.0) {
                        rainbow = float3(0.0, chroma, x);
                    } else if (hue >= 3.0 && hue < 4.0) {
                        rainbow = float3(0.0, x, chroma);
                    } else if (hue >= 4.0 && hue < 5.0) {
                        rainbow = float3(x, 0.0, chroma);
                    } else {
                        rainbow = float3(chroma, 0.0, x);
                    }
                    
                    rainbow += m;
                    
                    // Smoother blending with original color
                    float rainbowMask = col.a; // Use alpha for better blending
                    col.rgb = lerp(col.rgb, col.rgb + rainbow * _RainbowIntensity * rainbowMask, 0.7);
                }
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                // output with alpha
                return col;
            }
            ENDCG
        }
    }
}
