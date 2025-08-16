Shader "Unlit/outlineShader"
{
    Properties
    {
        _Texture("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {
            "RenderPipeline" ="UniversalPipeline"
            "RenderType" ="Opaque"
        }
        LOD 100
        BlendOp Add 		// <-- this is done to add the texture back to the screen texture
        Blend SrcAlpha One

        Pass
        {               
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl" // needed to sample scene color/luminance


            sampler2D _Texture;
            float _WidthMultiplier;
            float _HeightMultiplier;

            float3 _Color;

        float4 BlurVertical (Varyings input) : SV_Target
        {
            const float BLUR_SAMPLES = 64;
            const float BLUR_SAMPLES_RANGE = BLUR_SAMPLES / 2;
            
            float3 color = 0;
            float blurPixels = _HeightMultiplier * _ScreenParams.y;
            
            for(float i = -BLUR_SAMPLES_RANGE; i <= BLUR_SAMPLES_RANGE; i++)
            {
                float2 sampleOffset = float2 (0, (blurPixels / _BlitTexture_TexelSize.w) * (i / BLUR_SAMPLES_RANGE));
                color += tex2D(_Texture,input.texcoord + sampleOffset);
            }
            
            return float4(color.rgb / (BLUR_SAMPLES + 1), 1);
        }

        float4 BlurHorizontal (Varyings input) : SV_Target
        {
            const float BLUR_SAMPLES = 64;
            const float BLUR_SAMPLES_RANGE = BLUR_SAMPLES / 2;
            
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float3 color = 0;
            float blurPixels = _WidthMultiplier * _ScreenParams.x;
            for(float i = -BLUR_SAMPLES_RANGE; i <= BLUR_SAMPLES_RANGE; i++)
            {
                float2 sampleOffset =
                    float2 ((blurPixels / _BlitTexture_TexelSize.z) * (i / BLUR_SAMPLES_RANGE), 0);
                color += tex2D(_Texture,input.texcoord + sampleOffset);
            }
            return float4(color / (BLUR_SAMPLES + 1), 1);
        }

           half4 frag (Varyings IN) : SV_Target
	   {
                // sample the texture
                float2 uv = IN.texcoord;
                float3 col = tex2D(_Texture,uv);
                // apply fog
                
                float3 horiz = BlurHorizontal(IN);
                float3 verti = BlurVertical(IN);

                float3 outLine = min((horiz+verti),1.0) - col;

                //return float4(min((horiz+verti),1.0)-col,1.0);
                float3 sceneCol = SampleSceneColor(IN.texcoord);

                //return float4(step(0.1,outLine.r) * _Color,1.0);// <-- for solid outline
                return float4(outLine.r * _Color,1.0);		  // <-- for gradient outline
            }
            ENDHLSL
        }
    }
}
