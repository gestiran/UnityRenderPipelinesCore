using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering {
    using UnityObject = UnityEngine.Object;
    public static class CoreUtils {
        public static class Sections {
            /// <summary>Menu section 1</summary>
            public const int section1 = 10000;
            /// <summary>Menu section 2</summary>
            public const int section2 = 20000;
            /// <summary>Menu section 3</summary>
            public const int section3 = 30000;
            /// <summary>Menu section 4</summary>
            public const int section4 = 40000;
            /// <summary>Menu section 5</summary>
            public const int section5 = 50000;
            /// <summary>Menu section 6</summary>
            public const int section6 = 60000;
            /// <summary>Menu section 7</summary>
            public const int section7 = 70000;
            /// <summary>Menu section 8</summary>
            public const int section8 = 80000;
        }

        public static class Priorities {
            /// <summary>Assets > Create > Shader priority</summary>
            public const int assetsCreateShaderMenuPriority = 83;
            /// <summary>Assets > Create > Rendering priority</summary>
            public const int assetsCreateRenderingMenuPriority = 308;
            /// <summary>Edit Menu base priority</summary>
            public const int editMenuPriority = 320;
            /// <summary>Game Object Menu priority</summary>
            public const int gameObjectMenuPriority = 10;
            /// <summary>Lens Flare Priority</summary>
            public const int srpLensFlareMenuPriority = 303;
        }

        static Cubemap m_BlackCubeTexture;
        static Cubemap m_MagentaCubeTexture;
        static CubemapArray m_MagentaCubeTextureArray;
        static Cubemap m_WhiteCubeTexture;
        static RenderTexture m_EmptyUAV;
        static Texture3D m_BlackVolumeTexture;
        
        public static void ClearRenderTarget(CommandBuffer cmd, ClearFlag clearFlag, Color clearColor) {
            if (clearFlag != ClearFlag.None)
                cmd.ClearRenderTarget((RTClearFlags)clearFlag, clearColor, 1.0f, 0x00);
        }

        private static int FixupDepthSlice(int depthSlice, RTHandle buffer) {
            if (depthSlice == -1 && buffer.rt?.dimension == TextureDimension.Cube)
                depthSlice = 0;

            return depthSlice;
        }

        private static int FixupDepthSlice(int depthSlice, CubemapFace cubemapFace) {
            if (depthSlice == -1 && cubemapFace != CubemapFace.Unknown)
                depthSlice = 0;

            return depthSlice;
        }

        public static void SetRenderTarget(CommandBuffer cmd, RenderTargetIdentifier buffer, ClearFlag clearFlag, Color clearColor, int miplevel = 0,
                                           CubemapFace cubemapFace = CubemapFace.Unknown, int depthSlice = -1) {
            depthSlice = FixupDepthSlice(depthSlice, cubemapFace);
            cmd.SetRenderTarget(buffer, miplevel, cubemapFace, depthSlice);
            ClearRenderTarget(cmd, clearFlag, clearColor);
        }

        public static void SetRenderTarget(CommandBuffer cmd, RenderTargetIdentifier buffer, ClearFlag clearFlag = ClearFlag.None, int miplevel = 0,
                                           CubemapFace cubemapFace = CubemapFace.Unknown, int depthSlice = -1) {
            SetRenderTarget(cmd, buffer, clearFlag, Color.clear, miplevel, cubemapFace, depthSlice);
        }

        public static void SetRenderTarget(CommandBuffer cmd, RenderTargetIdentifier colorBuffer, RenderTargetIdentifier depthBuffer, int miplevel = 0,
                                           CubemapFace cubemapFace = CubemapFace.Unknown, int depthSlice = -1) {
            SetRenderTarget(cmd, colorBuffer, depthBuffer, ClearFlag.None, Color.clear, miplevel, cubemapFace, depthSlice);
        }

        public static void SetRenderTarget(CommandBuffer cmd, RenderTargetIdentifier colorBuffer, RenderTargetIdentifier depthBuffer, ClearFlag clearFlag, Color clearColor,
                                           int miplevel = 0, CubemapFace cubemapFace = CubemapFace.Unknown, int depthSlice = -1) {
            depthSlice = FixupDepthSlice(depthSlice, cubemapFace);
            cmd.SetRenderTarget(colorBuffer, depthBuffer, miplevel, cubemapFace, depthSlice);
            ClearRenderTarget(cmd, clearFlag, clearColor);
        }

        public static void SetRenderTarget(CommandBuffer cmd, RenderTargetIdentifier[] colorBuffers, RenderTargetIdentifier depthBuffer, ClearFlag clearFlag, Color clearColor) {
            cmd.SetRenderTarget(colorBuffers, depthBuffer, 0, CubemapFace.Unknown, -1);
            ClearRenderTarget(cmd, clearFlag, clearColor);
        }

        public static void SetRenderTarget(CommandBuffer cmd, RenderTargetIdentifier buffer, RenderBufferLoadAction loadAction, RenderBufferStoreAction storeAction,
                                           ClearFlag clearFlag, Color clearColor) {
            cmd.SetRenderTarget(buffer, loadAction, storeAction);
            ClearRenderTarget(cmd, clearFlag, clearColor);
        }

        public static void SetRenderTarget(CommandBuffer cmd, RenderTargetIdentifier colorBuffer, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction,
                                           RenderTargetIdentifier depthBuffer, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction,
                                           ClearFlag clearFlag, Color clearColor) {
            cmd.SetRenderTarget(colorBuffer, colorLoadAction, colorStoreAction, depthBuffer, depthLoadAction, depthStoreAction);
            ClearRenderTarget(cmd, clearFlag, clearColor);
        }

        public static void SetRenderTarget(CommandBuffer cmd, RenderTargetIdentifier buffer, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction,
                                           RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction, ClearFlag clearFlag, Color clearColor) {
            cmd.SetRenderTarget(buffer, colorLoadAction, colorStoreAction, depthLoadAction, depthStoreAction);
            ClearRenderTarget(cmd, clearFlag, clearColor);
        }

        private static void SetViewportAndClear(CommandBuffer cmd, RTHandle buffer, ClearFlag clearFlag, Color clearColor) {
        #if !UNITY_EDITOR
            SetViewport(cmd, buffer);
        #endif
            CoreUtils.ClearRenderTarget(cmd, clearFlag, clearColor);
        #if UNITY_EDITOR
            SetViewport(cmd, buffer);
        #endif
        }

        public static void SetRenderTarget(CommandBuffer cmd, RTHandle buffer, ClearFlag clearFlag, Color clearColor, int miplevel = 0,
                                           CubemapFace cubemapFace = CubemapFace.Unknown, int depthSlice = -1) {
            depthSlice = FixupDepthSlice(depthSlice, buffer);
            cmd.SetRenderTarget(buffer, miplevel, cubemapFace, depthSlice);
            SetViewportAndClear(cmd, buffer, clearFlag, clearColor);
        }

        public static void SetRenderTarget(CommandBuffer cmd, RTHandle buffer, ClearFlag clearFlag = ClearFlag.None, int miplevel = 0,
                                           CubemapFace cubemapFace = CubemapFace.Unknown, int depthSlice = -1) =>
                SetRenderTarget(cmd, buffer, clearFlag, Color.clear, miplevel, cubemapFace, depthSlice);

        public static void SetRenderTarget(CommandBuffer cmd, RTHandle colorBuffer, RTHandle depthBuffer, int miplevel = 0, CubemapFace cubemapFace = CubemapFace.Unknown,
                                           int depthSlice = -1) {
            SetRenderTarget(cmd, colorBuffer, depthBuffer, ClearFlag.None, Color.clear, miplevel, cubemapFace, depthSlice);
        }

        public static void SetRenderTarget(CommandBuffer cmd, RTHandle colorBuffer, RTHandle depthBuffer, ClearFlag clearFlag, Color clearColor, int miplevel = 0,
                                           CubemapFace cubemapFace = CubemapFace.Unknown, int depthSlice = -1) {
            CoreUtils.SetRenderTarget(cmd, colorBuffer.rt, depthBuffer.rt, miplevel, cubemapFace, depthSlice);
            SetViewportAndClear(cmd, colorBuffer, clearFlag, clearColor);
        }

        public static void SetRenderTarget(CommandBuffer cmd, RenderTargetIdentifier[] colorBuffers, RTHandle depthBuffer) {
            CoreUtils.SetRenderTarget(cmd, colorBuffers, depthBuffer.rt, ClearFlag.None, Color.clear);
            SetViewport(cmd, depthBuffer);
        }

        public static void SetViewport(CommandBuffer cmd, RTHandle target) {
            if (target.useScaling) {
                Vector2Int scaledViewportSize = target.GetScaledSize(target.rtHandleProperties.currentViewportSize);
                cmd.SetViewport(new Rect(0.0f, 0.0f, scaledViewportSize.x, scaledViewportSize.y));
            }
        }

        public static string GetRenderTargetAutoName(int width, int height, int depth, RenderTextureFormat format, string name, bool mips = false, bool enableMSAA = false,
                                                     MSAASamples msaaSamples = MSAASamples.None) => GetRenderTargetAutoName(width, height, depth, format.ToString(),
                TextureDimension.None, name, mips, enableMSAA, msaaSamples, dynamicRes: false);

        public static string GetRenderTargetAutoName(int width, int height, int depth, GraphicsFormat format, TextureDimension dim, string name, bool mips = false,
                                                     bool enableMSAA = false, MSAASamples msaaSamples = MSAASamples.None, bool dynamicRes = false) => GetRenderTargetAutoName(width,
                height, depth, format.ToString(), dim, name, mips, enableMSAA, msaaSamples, dynamicRes);

        static string GetRenderTargetAutoName(int width, int height, int depth, string format, TextureDimension dim, string name, bool mips, bool enableMSAA,
                                              MSAASamples msaaSamples, bool dynamicRes) {
            string result = string.Format("{0}_{1}x{2}", name, width, height);

            if (depth > 1)
                result = string.Format("{0}x{1}", result, depth);

            if (mips)
                result = string.Format("{0}_{1}", result, "Mips");

            result = string.Format("{0}_{1}", result, format);

            if (dim != TextureDimension.None)
                result = string.Format("{0}_{1}", result, dim);

            if (enableMSAA)
                result = string.Format("{0}_{1}", result, msaaSamples.ToString());

            if (dynamicRes)
                result = string.Format("{0}_{1}", result, "dynamic");

            return result;
        }

        public static Color ConvertSRGBToActiveColorSpace(Color color) {
            return (QualitySettings.activeColorSpace == ColorSpace.Linear) ? color.linear : color;
        }

        public static Color ConvertLinearToActiveColorSpace(Color color) {
            return (QualitySettings.activeColorSpace == ColorSpace.Linear) ? color : color.gamma;
        }

        public static Material CreateEngineMaterial(Shader shader) {
            if (shader == null) {
                return null;
            }

            var mat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };

            return mat;
        }

        public static void Swap<T>(ref T a, ref T b) {
            var tmp = a;
            a = b;
            b = tmp;
        }

        public static void SetKeyword(CommandBuffer cmd, string keyword, bool state) {
            if (state)
                cmd.EnableShaderKeyword(keyword);
            else
                cmd.DisableShaderKeyword(keyword);
        }

        // Caution: such a call should not be use interlaced with command buffer command, as it is immediate
        /// <summary>
        /// Set a keyword immediatly on a Material.
        /// </summary>
        /// <param name="material">Material on which to set the keyword.</param>
        /// <param name="keyword">Keyword to set on the material.</param>
        /// <param name="state">Value of the keyword to set on the material.</param>
        public static void SetKeyword(Material material, string keyword, bool state) {
            if (state)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);
        }

        /// <summary>
        /// Destroys a UnityObject safely.
        /// </summary>
        /// <param name="obj">Object to be destroyed.</param>
        public static void Destroy(UnityObject obj) {
            if (obj != null) {
            #if UNITY_EDITOR
                if (Application.isPlaying && !UnityEditor.EditorApplication.isPaused)
                    UnityObject.Destroy(obj);
                else
                    UnityObject.DestroyImmediate(obj);
            #else
                UnityObject.Destroy(obj);
            #endif
            }
        }

        static IEnumerable<Type> m_AssemblyTypes;

        public static IEnumerable<Type> GetAllTypesDerivedFrom<T>() {
        #if UNITY_EDITOR
            return UnityEditor.TypeCache.GetTypesDerivedFrom<T>();
        #else
            return GetAllAssemblyTypes().Where(t => t.IsSubclassOf(typeof(T)));
        #endif
        }

        public static IEnumerable<Type> GetAllAssemblyTypes()
        {
            if (m_AssemblyTypes == null)
            {
                m_AssemblyTypes = AppDomain.CurrentDomain.GetAssemblies()
                                           .SelectMany(t =>
                                                       {
                                                           // Ugly hack to handle mis-versioned dlls
                                                           var innerTypes = new Type[0];
                                                           try
                                                           {
                                                               innerTypes = t.GetTypes();
                                                           }
                                                           catch { }
                                                           return innerTypes;
                                                       });
            }

            return m_AssemblyTypes;
        }
        
        /// <summary>
        /// Safely release a Compute Buffer.
        /// </summary>
        /// <param name="buffer">Compute Buffer that needs to be released.</param>
        public static void SafeRelease(ComputeBuffer buffer) {
            if (buffer != null)
                buffer.Release();
        }

        /// <summary>
        /// Creates a cube mesh.
        /// </summary>
        /// <param name="min">Minimum corner coordinates in local space.</param>
        /// <param name="max">Maximum corner coordinates in local space.</param>
        /// <returns>A new instance of a cube Mesh.</returns>
        public static Mesh CreateCubeMesh(Vector3 min, Vector3 max) {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[8];

            vertices[0] = new Vector3(min.x, min.y, min.z);
            vertices[1] = new Vector3(max.x, min.y, min.z);
            vertices[2] = new Vector3(max.x, max.y, min.z);
            vertices[3] = new Vector3(min.x, max.y, min.z);
            vertices[4] = new Vector3(min.x, min.y, max.z);
            vertices[5] = new Vector3(max.x, min.y, max.z);
            vertices[6] = new Vector3(max.x, max.y, max.z);
            vertices[7] = new Vector3(min.x, max.y, max.z);

            mesh.vertices = vertices;

            int[] triangles = new int[36];

            triangles[0] = 0;
            triangles[1] = 2;
            triangles[2] = 1;
            triangles[3] = 0;
            triangles[4] = 3;
            triangles[5] = 2;
            triangles[6] = 1;
            triangles[7] = 6;
            triangles[8] = 5;
            triangles[9] = 1;
            triangles[10] = 2;
            triangles[11] = 6;
            triangles[12] = 5;
            triangles[13] = 7;
            triangles[14] = 4;
            triangles[15] = 5;
            triangles[16] = 6;
            triangles[17] = 7;
            triangles[18] = 4;
            triangles[19] = 3;
            triangles[20] = 0;
            triangles[21] = 4;
            triangles[22] = 7;
            triangles[23] = 3;
            triangles[24] = 3;
            triangles[25] = 6;
            triangles[26] = 2;
            triangles[27] = 3;
            triangles[28] = 7;
            triangles[29] = 6;
            triangles[30] = 4;
            triangles[31] = 1;
            triangles[32] = 5;
            triangles[33] = 4;
            triangles[34] = 0;
            triangles[35] = 1;

            mesh.triangles = triangles;

            return mesh;
        }

        /// <summary>
        /// Returns true if "Post Processes" are enabled for the view associated with the given camera.
        /// </summary>
        /// <param name="camera">Input camera.</param>
        /// <returns>True if "Post Processes" are enabled for the view associated with the given camera.</returns>
        public static bool ArePostProcessesEnabled(Camera camera) {
            bool enabled = true;

        #if UNITY_EDITOR
            if (camera.cameraType == CameraType.SceneView) {
                enabled = false;

                // Determine whether the "Post Processes" checkbox is checked for the current view.
                for (int i = 0; i < UnityEditor.SceneView.sceneViews.Count; i++) {
                    var sv = UnityEditor.SceneView.sceneViews[i] as UnityEditor.SceneView;

                    // Post-processing is disabled in scene view if either showImageEffects is disabled or we are
                    // rendering in wireframe mode.
                    if (sv.camera == camera && (sv.sceneViewState.imageEffectsEnabled && sv.cameraMode.drawMode != UnityEditor.DrawCameraMode.Wireframe)) {
                        enabled = true;

                        break;
                    }
                }
            }
        #endif

            return enabled;
        }

    #if UNITY_EDITOR
        static Func<List<UnityEditor.MaterialEditor>> materialEditors;

        static CoreUtils() {
            //quicker than standard reflection as it is compiled
            System.Reflection.FieldInfo field =
                    typeof(UnityEditor.MaterialEditor).GetField("s_MaterialEditors", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var fieldExpression = System.Linq.Expressions.Expression.Field(null, field);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<List<UnityEditor.MaterialEditor>>>(fieldExpression);
            materialEditors = lambda.Compile();
            LoadSceneViewMethods();
        }

    #endif

        /// <summary>
        /// Returns true if any Scene view is using the Scene filtering.
        /// </summary>
        /// <returns>True if any Scene view is using the Scene filtering.</returns>
        public static bool IsSceneFilteringEnabled() {
        #if UNITY_EDITOR && UNITY_2021_2_OR_NEWER
            for (int i = 0; i < UnityEditor.SceneView.sceneViews.Count; i++) {
                var sv = UnityEditor.SceneView.sceneViews[i] as UnityEditor.SceneView;

                if (sv.isUsingSceneFiltering) return true;
            }
        #endif
            return false;
        }

    #if UNITY_EDITOR
        static void LoadSceneViewMethods() {
            var stageNavigatorManager = typeof(UnityEditor.SceneManagement.PrefabStage).Assembly.GetType("UnityEditor.SceneManagement.StageNavigationManager");
            var instance = stageNavigatorManager.GetProperty("instance",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);

            var renderMode = stageNavigatorManager.GetProperty("contextRenderMode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            var renderModeAccessor = System.Linq.Expressions.Expression.Property(System.Linq.Expressions.Expression.Property(null, instance), renderMode);
            var internalRenderModeLambda = System.Linq.Expressions.Expression.Lambda<Func<int>>(System.Linq.Expressions.Expression.Convert(renderModeAccessor, typeof(int)));
            internalRenderModeLambda.Compile();
        }
    #endif

        /// <summary>
        /// Compute a hash of texture properties.
        /// </summary>
        /// <param name="texture"> Source texture.</param>
        /// <returns>Returns hash of texture properties.</returns>
        public static int GetTextureHash(Texture texture) {
            int hash = texture.GetHashCode();

            unchecked {
            #if UNITY_EDITOR
                hash = 23 * hash + texture.imageContentsHash.GetHashCode();
            #endif
                hash = 23 * hash + texture.GetInstanceID().GetHashCode();
                hash = 23 * hash + texture.graphicsFormat.GetHashCode();
                hash = 23 * hash + texture.wrapMode.GetHashCode();
                hash = 23 * hash + texture.width.GetHashCode();
                hash = 23 * hash + texture.height.GetHashCode();
                hash = 23 * hash + texture.filterMode.GetHashCode();
                hash = 23 * hash + texture.anisoLevel.GetHashCode();
                hash = 23 * hash + texture.mipmapCount.GetHashCode();
                hash = 23 * hash + texture.updateCount.GetHashCode();
            }

            return hash;
        }

        // Hacker’s Delight, Second Edition page 66
        /// <summary>
        /// Branchless previous power of two.
        /// </summary>
        /// <param name="size">Starting size or number.</param>
        /// <returns>Previous power of two.</returns>
        public static int PreviousPowerOfTwo(int size) {
            if (size <= 0)
                return 0;

            size |= (size >> 1);
            size |= (size >> 2);
            size |= (size >> 4);
            size |= (size >> 8);
            size |= (size >> 16);

            return size - (size >> 1);
        }

        /// <summary>
        /// Get the last declared value from an enum Type
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <returns>Last value of the enum</returns>
        public static T GetLastEnumValue<T>() where T : Enum => typeof(T).GetEnumValues().Cast<T>().Last();

        internal static string GetCorePath() => "Packages/com.unity.render-pipelines.core/";

    #if UNITY_EDITOR

        public static void EnsureFolderTreeInAssetFilePath(string filePath) {
            void Recurse(string _folderPath) {
                int lastSeparator = _folderPath.LastIndexOf('/');

                if (lastSeparator == -1)
                    return;

                string rootPath = _folderPath.Substring(0, lastSeparator);

                Recurse(rootPath);

                string folder = _folderPath.Substring(lastSeparator + 1);
                if (!UnityEditor.AssetDatabase.IsValidFolder(_folderPath))
                    UnityEditor.AssetDatabase.CreateFolder(rootPath, folder);
            }

            if (!filePath.StartsWith("assets/", System.StringComparison.CurrentCultureIgnoreCase))
                throw new System.ArgumentException($"Path should start with \"Assets/\". Got {filePath}.", filePath);

            Recurse(filePath.Substring(0, filePath.LastIndexOf('/')));
        }

    #endif
    }
}