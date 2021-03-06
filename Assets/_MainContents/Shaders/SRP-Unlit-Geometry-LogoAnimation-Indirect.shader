﻿// ロゴアニメーションサンプル(ジオメトリシェーダ―を用いて形状関係なしに強制的にロゴに塗り替える)
// ※DLRP + Graphics.DrawMeshInstancedIndirect 専用
Shader "Custom/SRP-Unlit-Geometry-LogoAnimation-Indirect"
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
            // ロゴ専用の描画パス
            Tags 
            {
                "LightMode" = "LogoPass"
            }

            CGPROGRAM
            #pragma vertex logo_instance_vert
            #pragma geometry logo_instance_geom
            // "logo_frag"のみLogo.cgincに実装
            #pragma fragment logo_frag

            // Propertiesに定義した値
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Scale;

            #include "UnityCG.cginc"
            #include "Logo.cginc"

            // C#側から渡すインスタンス毎の座標
            StructuredBuffer<float3> _Positions;


            // ------------------------------------------------------------
            // 頂点シェーダー
            v2g logo_instance_vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2g o;
                // geom側にSV_VertexIDを渡せないので代わりに使っていないuv.zに入れておく
                // uv.wにはSV_InstanceIDを入れておく。(StructuredBufferから座標を引っ張る時に参照)
                o.uv = float4(0, 0, v.vid, instanceID);
                return o;
            }

            // ------------------------------------------------------------
            // ジオメトリシェーダー
            // ※引数には1頂点のみを受け取る
            //      → 中で4頂点に増やすことで板ポリにしてTextureを貼るイメージ
            [maxvertexcount(4)]
            void logo_instance_geom(point v2g input[1], inout TriangleStream<g2f> outStream)
            {
                // 1頂点分のみロゴを生成(ここで塞き止めないと頂点分だけロゴが生まれる)
                uint vid = input[0].uv.z;
                if(vid != 0) return;

                // ロゴデータ(頂点、UV)
                float4 Vertices[4] = { float4(-2.4, 0.6, 0.0, 0.0), float4(2.4, 0.6, 0.0, 0.0), float4(-2.4, -0.6, 0.0, 0.0), float4(2.4, -0.6, 0.0, 0.0), };
                float2 UVs[4] = { float2(0.0, 1.0), float2(1.0, 1.0), float2(0.0, 0.0), float2(1.0, 0.0), };

                // コマ落ちアニメーション(radians)
                float Animation[16] = 
                {
                    1.5707963267948966,
                    1.4660765716752369,
                    1.3613568165555772,
                    1.2566370614359172,
                    1.1519173063162575,
                    1.0471975511965979,
                    0.9424777960769379,
                    0.8377580409572781,
                    0.7330382858376184,
                    0.6283185307179586,
                    0.5235987755982989,
                    0.4188790204786392,
                    0.31415926535897926,
                    0.2094395102393195,
                    0.10471975511965975,
                    0,
                };

                // sinで取得できる-1~1の値を0~1の範囲に正規化
                float normal = (_SinTime.w + 1) / 2;
                // SinTimeの値を0~15の範囲にスケール。
                // 値を量子化することでアニメーションテーブルのIndexとして扱う。
                float rot = Animation[round(normal*15)];

                // 回転行列
                float sinX = sin(rot);
                float cosX = cos(rot);
                float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);

                // 頂点の生成
                for(int i = 0; i < 4; ++i)
                {
                    g2f o;
                    // 原点を下端に設定する為にオフセットをずらしてから回転させる。
                    // → Yスケール半分のオフセットを上にずらしてから回転をかけ、元の位置に戻す。
                    float halfScaleY = _Scale.y / 2;
                    o.vertex = Vertices[i];
                    o.vertex.y += halfScaleY;
                    o.vertex.yz = mul(rotationMatrix, o.vertex.yz);
                    o.vertex.y -= halfScaleY;

                    // MVP変換前の頂点に対してC#側から渡した座標を適用して動かす。
                    o.vertex.xyz = o.vertex + _Positions[input[0].uv.w];
                    o.vertex = UnityObjectToClipPos(o.vertex);

                    o.uv = UVs[i];
                    o.uv = TRANSFORM_TEX(o.uv, _MainTex);
                    outStream.Append(o);
                }
                outStream.RestartStrip();
            }

            ENDCG
        }
    }
}
