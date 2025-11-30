Shader "ComGraphics/NetherPortal_Final_3D"
{
    Properties
    {
        _MainTex ("Swirl Texture (Noise)", 2D) = "white" {}
        _PortalColor ("Portal Color (Purple)", Color) = (0.5, 0.0, 0.5, 1)
        _ScrollSpeed ("Scroll Speed (X,Y)", Vector) = (0.1, 0.2, 0, 0)
        _NoiseScale ("Noise Scale (Tiling)", Float) = 2.0
        _DistortionStrength ("Fragment Distortion Strength", Range(0, 1.0)) = 0.5
        _EmissionIntensity ("Emission Intensity", Range(0, 100)) = 20.0 
        
        _WaveSpeed ("Wave Speed", Range(0.1, 5.0)) = 1.0
        _WaveFrequency ("Wave Frequency", Range(1.0, 10.0)) = 4.0
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.5)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend One One 
        ZWrite Off 
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma exclude_renderers srp
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float4 _PortalColor;
            float4 _ScrollSpeed;
            float _NoiseScale;
            float _DistortionStrength;
            float _EmissionIntensity;
            
            float _WaveSpeed;
            float _WaveFrequency;
            float _WaveAmplitude;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                
                float wave_offset_x = sin(v.uv.x * _WaveFrequency * 1.0 + _Time.y * _WaveSpeed) * _WaveAmplitude;
                float wave_offset_y = sin(v.uv.y * _WaveFrequency * 1.5 + _Time.y * _WaveSpeed * 1.5) * _WaveAmplitude;

                float displacement_x = wave_offset_x;
                float displacement_y = wave_offset_y;

                float3 T = UnityObjectToWorldDir(v.tangent.xyz);
                float3 N = UnityObjectToWorldNormal(v.normal);
                float3 B = cross(N, T) * v.tangent.w;

                float3 total_displacement = T * displacement_x + B * displacement_y;

                float3 displacedPosition = v.vertex.xyz + total_displacement;

                o.vertex = UnityObjectToClipPos(float4(displacedPosition, 1.0));
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 animatedUV = i.uv * _NoiseScale;
                animatedUV += _ScrollSpeed.xy * _Time.y;

                float noiseX = sin(animatedUV.y * 5.0 + _Time.y * 1.5);
                float noiseY = cos(animatedUV.x * 5.0 + _Time.y * 1.5);
                
                animatedUV.x += noiseX * _DistortionStrength * 0.1;
                animatedUV.y += noiseY * _DistortionStrength * 0.1;
                
                float noise = tex2D(_MainTex, animatedUV).r;

                float3 baseColor = _PortalColor.rgb * noise;

                float3 finalColor = baseColor * _EmissionIntensity;

                return fixed4(finalColor, 1.0); 
            }
            ENDCG
        }
    }
}