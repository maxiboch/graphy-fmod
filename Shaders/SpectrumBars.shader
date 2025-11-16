Shader "Graphy/Spectrum Bars"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        
        _BarColor("Bar Color", Color) = (0,1,0,1)
        _BackgroundColor("Background Color", Color) = (0,0,0,0.5)
        _BarCount("Bar Count", Int) = 64
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
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

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
            };

            fixed4 _Color;
            fixed4 _BarColor;
            fixed4 _BackgroundColor;
            int _BarCount;
            float _BarHeights[128]; // Max 128 bars

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                
                // Calculate which bar this pixel belongs to
                int barIndex = floor(uv.x * _BarCount);
                barIndex = clamp(barIndex, 0, _BarCount - 1);
                
                // Get the height for this bar (0-1 range)
                float barHeight = _BarHeights[barIndex];
                
                // Calculate bar width and spacing
                float barWidth = 1.0 / _BarCount;
                float barSpacing = barWidth * 0.1; // 10% spacing between bars
                
                // Calculate local position within the bar
                float localX = frac(uv.x * _BarCount);
                
                // Check if we're in the spacing between bars
                if (localX < barSpacing || localX > (1.0 - barSpacing))
                {
                    return _BackgroundColor;
                }
                
                // Check if we're below the bar height
                if (uv.y <= barHeight)
                {
                    // Color based on height (gradient from green to yellow to red)
                    fixed4 color;
                    if (barHeight < 0.5)
                    {
                        // Green to yellow
                        color = lerp(fixed4(0, 1, 0, 1), fixed4(1, 1, 0, 1), barHeight * 2.0);
                    }
                    else
                    {
                        // Yellow to red
                        color = lerp(fixed4(1, 1, 0, 1), fixed4(1, 0, 0, 1), (barHeight - 0.5) * 2.0);
                    }
                    
                    // Fade out at the top of the bar for a nice effect
                    float fadeStart = barHeight - 0.05;
                    if (uv.y > fadeStart)
                    {
                        float fade = (barHeight - uv.y) / 0.05;
                        color.a *= fade;
                    }
                    
                    return color * IN.color;
                }
                else
                {
                    return _BackgroundColor;
                }
            }
            ENDCG
        }
    }
}

