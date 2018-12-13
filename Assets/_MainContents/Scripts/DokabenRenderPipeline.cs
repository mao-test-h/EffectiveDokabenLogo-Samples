namespace MainContents
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Experimental.Rendering;

    [ExecuteInEditMode]
    [CreateAssetMenu(fileName = "DokabenRenderPipeline", menuName = "RenderPipelineAsset/DokabenRenderPipeline")]
    public sealed class DokabenRenderPipeline : RenderPipelineAsset
    {
        protected override IRenderPipeline InternalCreatePipeline()
        {
            return new DokabenRenderPipelineInstance();
        }
    }

    public sealed class DokabenRenderPipelineInstance : RenderPipeline
    {
        CullResults _cull;
        ScriptableCullingParameters _cullingParams;
        CommandBuffer _cmd;

        ShaderPassName _logoPass = new ShaderPassName("LogoPass");

        private static DokabenRenderPipelineInstance instance;
        public static DokabenRenderPipelineInstance Instance { get { return instance; } }

        public DokabenRenderPipelineInstance() => instance = this;


        public override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            base.Render(context, cameras);

            if (this._cmd == null)
            {
                this._cmd = new CommandBuffer();
            }

            foreach (var camera in cameras)
            {
                // Culling
                if (!CullResults.GetCullingParameters(camera, out this._cullingParams))
                {
                    continue;
                }
                CullResults.Cull(ref this._cullingParams, context, ref this._cull);

                // カメラのセットアップ
                // ※レンダーターゲット、ビュープロジェクション行列、カメラ毎のBuilt-in ShaderのVariantのセットなど
                context.SetupCameraProperties(camera);

                // クリアバッファ
                this._cmd.Clear();
                this._cmd.ClearRenderTarget(true, true, Color.black, 1.0f);
                context.ExecuteCommandBuffer(this._cmd);

                // ロゴを描画するための設定
                // 今回の例だと"LogoPass"と言う名前が指定されたShaderPassを使用.
                // ソート順は幾つかのオプションから組み合わせで指定可能であり、
                // ドカベンは透過オブジェクトなのでCommonTransparentでソートする。
                // https://docs.unity3d.com/ScriptReference/Experimental.Rendering.SortFlags.html
                var settings = new DrawRendererSettings(camera, this._logoPass);
                settings.sorting.flags = SortFlags.CommonTransparent;

                // どのオブジェクトを描画するかを記述
                var filterSettings = new FilterRenderersSettings(true)
                {
                    renderQueueRange = RenderQueueRange.opaque,
                };

                // カリングの結果見えている不透明オブジェクトを BasicPass パスで描画
                // カリング結果の見えている"LogoPass"の透過オブジェクトを描画
                context.DrawRenderers(this._cull.visibleRenderers, ref settings, filterSettings);

                // 描画内容のコミット
                context.Submit();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            instance = null;

            if (this._cmd != null)
            {
                this._cmd.Dispose();
                this._cmd = null;
            }
        }
    }
}
