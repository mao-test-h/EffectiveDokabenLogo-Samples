# EffectiveDokabenLogo-Samples

「[【年末だよ】Unity お・と・なのLT大会 2018](https://meetup.unity3d.jp/jp/events/1026)」 LT資料 サンプル集


-------------------------------------------------

# ▽ サンプルについて

- Geometry Shaderの例
    - "[Shaders/Unlit-Geometry-LogoAnimation.shader](https://github.com/mao-test-h/EffectiveDokabenLogo-Samples/blob/master/Assets/_MainContents/Shaders/Unlit-Geometry-LogoAnimation.shader)"を参照

- Materialの自動適用の例
    - "[Scripts/Editor/AutoLogoChanger.cs](https://github.com/mao-test-h/EffectiveDokabenLogo-Samples/blob/master/Assets/_MainContents/Scripts/Editor/AutoLogoChanger.cs)"を参照
    - ※機能の有効/無効は「Preferences -> Logo Settings」より設定可能

- ScriptableRenderPipeline(DLRP)の例
    - 自作したRenderPipelineについては"[Scripts/DokabenRenderPipeline.cs](https://github.com/mao-test-h/EffectiveDokabenLogo-Samples/blob/master/Assets/_MainContents/Scripts/DokabenRenderPipeline.cs)"を参照
    - RenderPipeline用に実装したShaderについては"[Shaders/SRP-Unlit-Geometry-LogoAnimation.shader](https://github.com/mao-test-h/EffectiveDokabenLogo-Samples/blob/master/Assets/_MainContents/Shaders/SRP-Unlit-Geometry-LogoAnimation.shader)"を参照



## ▼ おまけ

- 100万個のドカベンロゴを「Batches : 1」「SetPass calls : 1」で描画する例について
    - ソース及びシーンは以下を参照
        - Scene : **InstancedIndirectSample.unity**
        - Source : "[Scripts/InstancedIndirectSample.cs](https://github.com/mao-test-h/EffectiveDokabenLogo-Samples/blob/master/Assets/_MainContents/Scripts/InstancedIndirectSample.cs)"
        - Indirect用のShader : "[Shaders/SRP-Unlit-Geometry-LogoAnimation-Indirect.shader](https://github.com/mao-test-h/EffectiveDokabenLogo-Samples/blob/master/Assets/_MainContents/Shaders/SRP-Unlit-Geometry-LogoAnimation-Indirect.shader)"
