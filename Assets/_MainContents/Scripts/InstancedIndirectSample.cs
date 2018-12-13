namespace MainContents.Samples
{
    using System.Runtime.InteropServices;
    using UnityEngine;
    using Unity.Collections;

    using UnityRandom = UnityEngine.Random;

    /// <summary>
    /// SRP(DLRP) + DrawMeshInstancedIndirectのサンプル
    /// </summary>
    public sealed class InstancedIndirectSample : MonoBehaviour
    {
        // 表示領域
        [SerializeField] float _boundSize = 512;
        // インスタンス数
        [SerializeField] int _instanceCount = 1000000;
        // 適用するマテリアル
        [SerializeField] Material _material = null;

        // 表示メッシュ
        Mesh _mesh = null;
        // GPU(Shader/Material)に渡すデータ
        ComputeBuffer _buffer = null;
        // Graphics.DrawMeshInstancedIndirectで必要な引数
        ComputeBuffer _bufferWithArgs = null;

        // ランダムな位置を取得
        Vector3 GetRandomPosition
        {
            get
            {
                var halfX = this._boundSize / 2;
                var halfY = this._boundSize / 2;
                var halfZ = this._boundSize / 2;
                return new Vector3(
                    UnityRandom.Range(-halfX, halfX),
                    UnityRandom.Range(-halfY, halfY),
                    UnityRandom.Range(-halfZ, halfZ));
            }
        }


        /// <summary>
        /// MonoBehaviour.Start
        /// </summary>
        void Start()
        {
            // とりあえずはCubeを取得しているが、Geometry Shaderで形状を変えているので正直何でも良い。
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            this._mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            Destroy(obj);


            // 「Graphics.DrawMeshInstancedIndirect -> bufferWithArgs」の設定
            // ※Buffer with arguments, bufferWithArgs, has to have five integer numbers at given argsOffset offset: index count per instance, instance count, start index location, base vertex location, start instance location.
            uint[] GPUInstancingArgs = new uint[5] { 0, 0, 0, 0, 0 };
            this._bufferWithArgs = new ComputeBuffer(1, GPUInstancingArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            GPUInstancingArgs[0] = (uint)this._mesh.GetIndexCount(0);
            GPUInstancingArgs[1] = (uint)this._instanceCount;
            GPUInstancingArgs[2] = (uint)this._mesh.GetIndexStart(0);
            GPUInstancingArgs[3] = (uint)this._mesh.GetBaseVertex(0);
            this._bufferWithArgs.SetData(GPUInstancingArgs);


            // インスタンスの座標の設定
            this._buffer = new ComputeBuffer(this._instanceCount, Marshal.SizeOf(typeof(Vector3)));
            var copyArray = new NativeArray<Vector3>(this._instanceCount, Allocator.Temp);
            for (int i = 0; i < this._instanceCount; i++)
            {
                copyArray[i] = this.GetRandomPosition;
            }
            this._buffer.SetData(copyArray);
            var playDataBufferID = Shader.PropertyToID("_Positions");
            this._material.SetBuffer(playDataBufferID, this._buffer);
            copyArray.Dispose();
        }

        /// <summary>
        /// MonoBehaviour.Update
        /// </summary>
        void Update()
        {
            // 描画
            Graphics.DrawMeshInstancedIndirect(
                this._mesh,
                0,
                this._material,
                new Bounds(Vector3.zero, 1000000 * Vector3.one),
                this._bufferWithArgs);
        }

        /// <summary>
        /// MonoBehaviour.OnDestroy
        /// </summary>
        void OnDestroy()
        {
            this._buffer.Dispose();
            this._bufferWithArgs.Dispose();
        }
    }
}
