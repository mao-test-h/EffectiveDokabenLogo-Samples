#ifndef logo_h_
#define logo_h_

struct appdata
{
    uint vid : SV_VertexID;
};

struct v2g  // vert → geom
{
    // 通常 : float4(0, 0, SV_VertexID, 0)
    // DrawMeshInstancedIndirect版 : float4(0, 0, SV_VertexID, SV_InstanceID)
    // ※uv.xyにはUV座標が入る想定ではあるが...今回の実装ではテーブルで持っているので無用
    float4 uv : TEXCOORD0;
};

struct g2f  // geom → frag
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
};

// ------------------------------------------------------------
// 頂点シェーダー
v2g logo_vert(appdata v)
{
    v2g o;
    // geom側にSV_VertexIDを渡せないので代わりに使っていないuv.zに入れておく
    o.uv = float4(0, 0, v.vid, 0);
    return o;
}

// ------------------------------------------------------------
// ジオメトリシェーダー
// ※引数には1頂点のみを受け取る
//      → 中で4頂点に増やすことで板ポリにしてTextureを貼るイメージ
[maxvertexcount(4)]
void logo_geom(point v2g input[1], inout TriangleStream<g2f> outStream)
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

        o.vertex = UnityObjectToClipPos(o.vertex);
        o.uv = UVs[i];
        o.uv = TRANSFORM_TEX(o.uv, _MainTex);
        outStream.Append(o);
    }
    outStream.RestartStrip();
}

// ------------------------------------------------------------
// フラグメントシェーダ―
fixed4 logo_frag (g2f i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv);
    clip(col.a - 0.5);
    return col;
}

#endif  // logo_h_
