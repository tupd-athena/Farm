Shader "Custom/Flash"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FlashColor ("Flash Color", Color) = (1,1,1,1)
        _FlashIntensity ("Flash Intensity", Range(0,2)) = 1.0
        _LineWidth ("Line Width", Range(0.01, 0.3)) = 0.1
        _LineAngle ("Line Angle", Range(-180, 180)) = 45
        _FlashSpeed ("Flash Speed", Range(0.1, 5)) = 1.0
        _FlashPosition ("Flash Position", Range(-1, 2)) = 0
        _LineCount ("Line Count", Range(1, 5)) = 2
        _LineSpacing ("Line Spacing", Range(0.1, 1.0)) = 0.3
        
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _FlashColor;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _FlashIntensity;
            float _LineWidth;
            float _LineAngle;
            float _FlashSpeed;
            float _FlashPosition;
            float _LineCount;
            float _LineSpacing;
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                // Calculate animated flash position - moves in one direction continuously
                float time = _Time.y * _FlashSpeed;
                float animatedPosition = _FlashPosition + frac(time) * 2.0 - 1.0;
                
                // Convert angle to radians
                float angleRad = _LineAngle * 3.14159265 / 180.0;
                
                // Calculate line position based on UV coordinates and angle
                float2 uv = IN.texcoord;
                float linePos = (uv.x * cos(angleRad) + uv.y * sin(angleRad));
                
                // Create multiple flash lines
                float totalFlashLine = 0.0;
                for (int i = 0; i < _LineCount; i++)
                {
                    float lineOffset = i * _LineSpacing;
                    float currentLinePos = animatedPosition + lineOffset;
                    
                    // Create the flash line effect - thin sharp line
                    float lineDistance = abs(linePos - currentLinePos);
                    float flashLine = 1.0 - smoothstep(0.0, _LineWidth, lineDistance);
                    
                    // Add a sharper edge for thinner appearance
                    flashLine = pow(flashLine, 2.0);
                    
                    totalFlashLine += flashLine;
                }
                
                // Normalize the combined flash lines
                totalFlashLine = saturate(totalFlashLine);
                
                // Apply flash effect with constant intensity
                color.rgb = lerp(color.rgb, _FlashColor.rgb, totalFlashLine * _FlashIntensity);
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif
                
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
