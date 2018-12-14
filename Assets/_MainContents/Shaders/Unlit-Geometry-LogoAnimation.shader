// ロゴアニメーションサンプル(ジオメトリシェーダ―を用いて形状関係なしに強制的にロゴに塗り替える)
Shader "Custom/Unlit-Geometry-LogoAnimation"
{
    Properties
    {
        // 表示Texture
        _MainTex ("Texture", 2D) = "white" {}

        // オブジェクトのスケール
        // ※こちらはunity_ObjectToWorldから取得せずにPropertiesから設定
        _Scale ("Scale", Vector) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Geometry"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
        }
        Cull Off
        Lighting Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            // Logo.cgincに実装
            #pragma vertex logo_vert
            #pragma geometry logo_geom
            #pragma fragment logo_frag

            // Propertiesに定義した値
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Scale;

            #include "UnityCG.cginc"
            #include "Logo.cginc"
            ENDCG
        }
    }
}
