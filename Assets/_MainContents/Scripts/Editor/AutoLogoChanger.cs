namespace MainContents.Editor
{
    using UnityEngine;
    using UnityEditor;

    using UnityObject = UnityEngine.Object;

    [InitializeOnLoad]
    public static class AutoLogoChanger
    {
        static Material matRef = null;
        static Mesh fallbackMesh = null;

        static AutoLogoChanger()
        {
            // ロゴのマテリアルを取得
            matRef = Resources.Load<Material>("Materials/Geometry-LogoAnimation");

            // フォールバック用のMeshを取得(とりあえずCubeで)
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallbackMesh = obj.GetComponent<MeshFilter>().sharedMesh;
            UnityObject.DestroyImmediate(obj);

            // 機能の有効/無効の設定はPreferenceにて可能
            if (EditorPrefs.GetBool(EnabledAutoLogoChangerKey, false))
            {
                // Hierarchyの変更を検知
                EditorApplication.hierarchyChanged += OnHierarchyChanged;
            }
        }

        // -------------------------------------------------------------
        #region // HierarchyChanged

        /// <summary>
        /// Hierarchyの変更検知イベント
        /// </summary>
        static void OnHierarchyChanged()
        {
            foreach (var obj in UnityObject.FindObjectsOfType<GameObject>())
            {
                var renderers = obj.GetComponentsInChildren<Renderer>(includeInactive: true);
                for (int i = 0; i < renderers.Length; i++)
                {
                    var renderer = renderers[i];
                    var rendererObj = renderer.gameObject;

                    // MeshRenderer以外の派生クラスであればMeshRendererに塗り替えておく
                    if (!(renderer is MeshRenderer))
                    {
                        UnityObject.DestroyImmediate(renderer);
                        renderer = AddMeshRenderer(rendererObj);
                    }

                    renderer.sharedMaterial = matRef;
                }
            }
        }

        static MeshRenderer AddMeshRenderer(GameObject obj)
        {
            var meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = fallbackMesh;
            return obj.AddComponent<MeshRenderer>();
        }

        #endregion // HierarchyChanged

        // -------------------------------------------------------------
        #region // Preference Menu

        const string EnabledAutoLogoChangerKey = "EnabledAutoLogoChanger";

        /// <summary>
        /// Preferenceメニュー
        /// </summary>
        [PreferenceItem("Logo Settings")]
        private static void OnPreferenceGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Enabled AutoLogoChanger", EditorStyles.boldLabel);
            bool isAutoLogoChanger = EditorPrefs.GetBool(EnabledAutoLogoChangerKey, false);
            isAutoLogoChanger = EditorGUILayout.Toggle(isAutoLogoChanger);
            if (EditorGUI.EndChangeCheck())
            {
                if (isAutoLogoChanger)
                {
                    EditorApplication.hierarchyChanged += OnHierarchyChanged;
                }
                else
                {
                    EditorApplication.hierarchyChanged -= OnHierarchyChanged;
                }
                EditorPrefs.SetBool(EnabledAutoLogoChangerKey, isAutoLogoChanger);
            }
        }

        #endregion // Preference Menu
    }
}
