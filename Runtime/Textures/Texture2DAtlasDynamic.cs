using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Rendering
{
    class AtlasAllocatorDynamic
    {
        private class AtlasNodePool
        {
            internal AtlasNode[] m_Nodes;
            Int16 m_Next;
            Int16 m_FreelistHead;

            public AtlasNodePool(Int16 capacity)
            {
                m_Nodes = new AtlasNode[capacity];
                m_Next = 0;
                m_FreelistHead = -1;
            }

            public void Dispose()
            {
                Clear();
                m_Nodes = null;
            }

            public void Clear()
            {
                m_Next = 0;
                m_FreelistHead = -1;
            }

            public Int16 AtlasNodeCreate(Int16 parent)
            {
                if (m_FreelistHead != -1)
                {
                    Int16 freelistHeadNext = m_Nodes[m_FreelistHead].m_FreelistNext;
                    m_Nodes[m_FreelistHead] = new AtlasNode(m_FreelistHead, parent);
                    Int16 res = m_FreelistHead;
                    m_FreelistHead = freelistHeadNext;
                    return res;
                }

                m_Nodes[m_Next] = new AtlasNode(m_Next, parent);
                return m_Next++;
            }

            public void AtlasNodeFree(Int16 index)
            {
                m_Nodes[index].m_FreelistNext = m_FreelistHead;
                m_FreelistHead = index;
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 32)]
        private struct AtlasNode
        {
            private enum AtlasNodeFlags : uint
            {
                IsOccupied = 1 << 0
            }

            [FieldOffset(0)] public Int16 m_Self;
            [FieldOffset(2)] public Int16 m_Parent;
            [FieldOffset(4)] public Int16 m_LeftChild;
            [FieldOffset(6)] public Int16 m_RightChild;
            [FieldOffset(8)] public Int16 m_FreelistNext;
            [FieldOffset(10)] public UInt16 m_Flags;
            // [15:12] bytes are padding
            [FieldOffset(16)] public Vector4 m_Rect;

            public AtlasNode(Int16 self, Int16 parent)
            {
                m_Self = self;
                m_Parent = parent;
                m_LeftChild = -1;
                m_RightChild = -1;
                m_Flags = 0;
                m_FreelistNext = -1;
                m_Rect = Vector4.zero; // x,y is width and height (scale) z,w offset into atlas (bias)
            }

            public bool IsOccupied()
            {
                return (m_Flags & (UInt16)AtlasNodeFlags.IsOccupied) > 0;
            }

            public void SetIsOccupied()
            {
                UInt16 isOccupiedMask = (UInt16)AtlasNodeFlags.IsOccupied;
                m_Flags |= isOccupiedMask;
            }

            public void ClearIsOccupied()
            {
                UInt16 isOccupiedMask = (UInt16)AtlasNodeFlags.IsOccupied;
                m_Flags &= (UInt16)~isOccupiedMask;
            }

            public bool IsLeafNode()
            {
                // Note: Only need to check if m_LeftChild == null, as either both are allocated (split), or none are allocated (leaf).
                return m_LeftChild == -1;
            }

            public Int16 Allocate(AtlasNodePool pool, int width, int height)
            {
                if (Mathf.Min(width, height) < 1)
                {
                    return -1;
                }

                // not a leaf node, try children
                // TODO: Rather than always going left, then right, we might want to always attempt to allocate in the smaller child, then larger.
                if (!IsLeafNode())
                {
                    Int16 node = pool.m_Nodes[m_LeftChild].Allocate(pool, width, height);
                    if (node == -1)
                    {
                        node = pool.m_Nodes[m_RightChild].Allocate(pool, width, height);
                    }
                    return node;
                }

                // leaf node, check for fit
                if (IsOccupied()) { return -1; }
                if (width > m_Rect.x || height > m_Rect.y) { return -1; }

                m_LeftChild = pool.AtlasNodeCreate(m_Self);
                m_RightChild = pool.AtlasNodeCreate(m_Self);

                float deltaX = m_Rect.x - width;
                float deltaY = m_Rect.y - height;

                if (deltaX >= deltaY)
                {
                    pool.m_Nodes[m_LeftChild].m_Rect.x = width;
                    pool.m_Nodes[m_LeftChild].m_Rect.y = m_Rect.y;
                    pool.m_Nodes[m_LeftChild].m_Rect.z = m_Rect.z;
                    pool.m_Nodes[m_LeftChild].m_Rect.w = m_Rect.w;

                    pool.m_Nodes[m_RightChild].m_Rect.x = deltaX;
                    pool.m_Nodes[m_RightChild].m_Rect.y = m_Rect.y;
                    pool.m_Nodes[m_RightChild].m_Rect.z = m_Rect.z + width;
                    pool.m_Nodes[m_RightChild].m_Rect.w = m_Rect.w;

                    if (deltaY < 1)
                    {
                        pool.m_Nodes[m_LeftChild].SetIsOccupied();
                        return m_LeftChild;
                    }
                    else
                    {
                        Int16 node = pool.m_Nodes[m_LeftChild].Allocate(pool, width, height);
                        if (node >= 0) { pool.m_Nodes[node].SetIsOccupied(); }
                        return node;
                    }
                }
                else
                {
                    pool.m_Nodes[m_LeftChild].m_Rect.x = m_Rect.x;
                    pool.m_Nodes[m_LeftChild].m_Rect.y = height;
                    pool.m_Nodes[m_LeftChild].m_Rect.z = m_Rect.z;
                    pool.m_Nodes[m_LeftChild].m_Rect.w = m_Rect.w;

                    pool.m_Nodes[m_RightChild].m_Rect.x = m_Rect.x;
                    pool.m_Nodes[m_RightChild].m_Rect.y = deltaY;
                    pool.m_Nodes[m_RightChild].m_Rect.z = m_Rect.z;
                    pool.m_Nodes[m_RightChild].m_Rect.w = m_Rect.w + height;

                    if (deltaX < 1)
                    {
                        pool.m_Nodes[m_LeftChild].SetIsOccupied();
                        return m_LeftChild;
                    }
                    else
                    {
                        Int16 node = pool.m_Nodes[m_LeftChild].Allocate(pool, width, height);
                        if (node >= 0) { pool.m_Nodes[node].SetIsOccupied(); }
                        return node;
                    }
                }
            }

            public void ReleaseChildren(AtlasNodePool pool)
            {
                if (IsLeafNode()) { return; }
                pool.m_Nodes[m_LeftChild].ReleaseChildren(pool);
                pool.m_Nodes[m_RightChild].ReleaseChildren(pool);

                pool.AtlasNodeFree(m_LeftChild);
                pool.AtlasNodeFree(m_RightChild);
                m_LeftChild = -1;
                m_RightChild = -1;
            }

            public void ReleaseAndMerge(AtlasNodePool pool)
            {
                Int16 n = m_Self;
                do
                {
                    pool.m_Nodes[n].ReleaseChildren(pool);
                    pool.m_Nodes[n].ClearIsOccupied();
                    n = pool.m_Nodes[n].m_Parent;
                }
                while (n >= 0 && pool.m_Nodes[n].IsMergeNeeded(pool));
            }

            public bool IsMergeNeeded(AtlasNodePool pool)
            {
                return pool.m_Nodes[m_LeftChild].IsLeafNode() && (!pool.m_Nodes[m_LeftChild].IsOccupied())
                    && pool.m_Nodes[m_RightChild].IsLeafNode() && (!pool.m_Nodes[m_RightChild].IsOccupied());
            }
        }

        private int m_Width;
        private int m_Height;
        private AtlasNodePool m_Pool;
        private Int16 m_Root;
        private Dictionary<int, Int16> m_NodeFromID;

        public AtlasAllocatorDynamic(int width, int height, int capacityAllocations)
        {
            // In an evenly split binary tree, the nodeCount == leafNodeCount * 2
            int capacityNodes = capacityAllocations * 2;
            m_Pool = new AtlasNodePool((Int16)capacityNodes);

            m_NodeFromID = new Dictionary<int, Int16>(capacityAllocations);

            Int16 rootParent = -1;
            m_Root = m_Pool.AtlasNodeCreate(rootParent);
            m_Pool.m_Nodes[m_Root].m_Rect.Set(width, height, 0, 0);
            m_Width = width;
            m_Height = height;
        }

        public bool Allocate(out Vector4 result, int key, int width, int height)
        {
            Int16 node = m_Pool.m_Nodes[m_Root].Allocate(m_Pool, width, height);
            if (node >= 0)
            {
                result = m_Pool.m_Nodes[node].m_Rect;
                m_NodeFromID.Add(key, node);
                return true;
            }
            else
            {
                result = Vector4.zero;
                return false;
            }
        }

        public void Release(int key)
        {
            if (m_NodeFromID.TryGetValue(key, out Int16 node))
            {
                m_Pool.m_Nodes[node].ReleaseAndMerge(m_Pool);
                m_NodeFromID.Remove(key);
                return;
            }
        }

        public void Release()
        {
            m_Pool.Clear();
            m_Root = m_Pool.AtlasNodeCreate(-1);
            m_Pool.m_Nodes[m_Root].m_Rect.Set(m_Width, m_Height, 0, 0);
            m_NodeFromID.Clear();
        }

        public string DebugStringFromRoot(int depthMax = -1)
        {
            string res = "";
            DebugStringFromNode(ref res, m_Root, 0, depthMax);
            return res;
        }

        private void DebugStringFromNode(ref string res, Int16 n, int depthCurrent = 0, int depthMax = -1)
        {
            res += "{[" + depthCurrent + "], isOccupied = " + (m_Pool.m_Nodes[n].IsOccupied() ? "true" : "false") + ", self = " + m_Pool.m_Nodes[n].m_Self + ", " + m_Pool.m_Nodes[n].m_Rect.x + "," + m_Pool.m_Nodes[n].m_Rect.y + ", " + m_Pool.m_Nodes[n].m_Rect.z + ", " + m_Pool.m_Nodes[n].m_Rect.w + "}\n";

            if (depthMax == -1 || depthCurrent < depthMax)
            {
                if (m_Pool.m_Nodes[n].m_LeftChild >= 0)
                {
                    DebugStringFromNode(ref res, m_Pool.m_Nodes[n].m_LeftChild, depthCurrent + 1, depthMax);
                }

                if (m_Pool.m_Nodes[n].m_RightChild >= 0)
                {
                    DebugStringFromNode(ref res, m_Pool.m_Nodes[n].m_RightChild, depthCurrent + 1, depthMax);
                }
            }
        }
    }

    /// <summary>
    /// A generic Atlas texture of 2D textures.
    /// An atlas texture is a texture collection that collects multiple sub-textures into a single big texture.
    /// Sub-texture allocation for Texture2DAtlasDynamic is dynamic.
    /// </summary>
    class Texture2DAtlasDynamic
    {
        private RTHandle m_AtlasTexture = null;
        private bool isAtlasTextureOwner = false;
        private int m_Width;
        private int m_Height;
        private GraphicsFormat m_Format;
        private AtlasAllocatorDynamic m_AtlasAllocator = null;
        private Dictionary<int, Vector4> m_AllocationCache;

        /// <summary>
        /// Handle to the texture of the atlas.
        /// </summary>
        public RTHandle AtlasTexture
        {
            get
            {
                return m_AtlasTexture;
            }
        }

        /// <summary>
        /// Creates a new empty texture atlas.
        /// </summary>
        public Texture2DAtlasDynamic(int width, int height, int capacity, GraphicsFormat format)
        {
            m_Width = width;
            m_Height = height;
            m_Format = format;
            m_AtlasTexture = RTHandles.Alloc(
                m_Width,
                m_Height,
                1,
                DepthBits.None,
                m_Format,
                FilterMode.Point,
                TextureWrapMode.Clamp,
                TextureDimension.Tex2D,
                false,
                true,
                false,
                false,
                1,
                0,
                MSAASamples.None,
                false,
                false
            );
            isAtlasTextureOwner = true;

            m_AtlasAllocator = new AtlasAllocatorDynamic(width, height, capacity);
            m_AllocationCache = new Dictionary<int, Vector4>(capacity);
        }

        /// <summary>
        /// Creates a new empty texture atlas. Use external atlas texture.
        /// </summary>
        public Texture2DAtlasDynamic(int width, int height, int capacity, RTHandle atlasTexture)
        {
            m_Width = width;
            m_Height = height;
            m_Format = atlasTexture.rt.graphicsFormat;
            m_AtlasTexture = atlasTexture;
            isAtlasTextureOwner = false;

            m_AtlasAllocator = new AtlasAllocatorDynamic(width, height, capacity);
            m_AllocationCache = new Dictionary<int, Vector4>(capacity);
        }

        /// <summary>
        /// Release atlas resources.
        /// </summary>
        public void Release()
        {
            ResetAllocator();
            if (isAtlasTextureOwner) { RTHandles.Release(m_AtlasTexture); }
        }

        /// <summary>
        /// Clear atlas sub-texture allocations.
        /// </summary>
        public void ResetAllocator()
        {
            m_AtlasAllocator.Release();
            m_AllocationCache.Clear();
        }

        /// <summary>
        /// Add a texture into the atlas.
        /// </summary>
        public bool AddTexture(CommandBuffer cmd, out Vector4 scaleOffset, Texture texture)
        {
            int key = texture.GetInstanceID();
            if (!m_AllocationCache.TryGetValue(key, out scaleOffset))
            {
                int width = texture.width;
                int height = texture.height;
                if (m_AtlasAllocator.Allocate(out scaleOffset, key, width, height))
                {
                    scaleOffset.Scale(new Vector4(1.0f / m_Width, 1.0f / m_Height, 1.0f / m_Width, 1.0f / m_Height));
                    for (int mipLevel = 0; mipLevel < (texture as Texture2D).mipmapCount; mipLevel++)
                    {
                        cmd.SetRenderTarget(m_AtlasTexture, mipLevel);
                        Blitter.BlitQuad(cmd, texture, new Vector4(1, 1, 0, 0), scaleOffset, mipLevel, false);
                    }
                    m_AllocationCache.Add(key, scaleOffset);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if the atlas contains the texture.
        /// </summary>
        public bool IsCached(out Vector4 scaleOffset, int key)
        {
            return m_AllocationCache.TryGetValue(key, out scaleOffset);
        }

        /// <summary>
        /// Allocate space from the atlas.
        /// </summary>
        public bool EnsureTextureSlot(out bool isUploadNeeded, out Vector4 scaleOffset, int key, int width, int height)
        {
            isUploadNeeded = false;
            if (m_AllocationCache.TryGetValue(key, out scaleOffset)) { return true; }
            if (!m_AtlasAllocator.Allocate(out scaleOffset, key, width, height)) { return false; }
            
            isUploadNeeded = true;
            scaleOffset.Scale(new Vector4(1.0f / m_Width, 1.0f / m_Height, 1.0f / m_Width, 1.0f / m_Height));
            m_AllocationCache.Add(key, scaleOffset);
            return true;
        }

        /// <summary>
        /// Release allocated space from the atlas.
        /// </summary>
        public void ReleaseTextureSlot(int key)
        {
            m_AtlasAllocator.Release(key);
            m_AllocationCache.Remove(key);
        }
    }
}
