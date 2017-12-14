using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnearthLayersController : MonoBehaviour
{
    // Set up as flags so that items can use multiple layers at once
    [Flags]
    public enum Layers
    {
        None = 1 << 0,
        DeepWater = 1 << 1,
        Water = 1 << 2,
        Shallows = 1 << 3,
        Sand = 1 << 4,
        Grass = 1 << 5,
        Forest = 1 << 6,
        Rock = 1 << 7,
        Snow = 1 << 8,
        Lava = 1 << 9
    }

    private struct LayerRange
    {
        public uint Min;
        public uint Max;
    }

    /// <summary>
    /// Depth Layer is a serializable class for setting up layers in the Unity Editor.
    /// </summary>
    [Serializable]
    public class DepthLayer
    {
        public Layers Layer;
        public Color LayerColor = Color.black;
        public float AnimationSpeed = 25f;
        private float m_FrameTime = 0f;
        public Texture2D[] LayerTextures;
        public Texture2D LayerTexture
        {
            get
            {
                if(LayerTextures != null)
                {
                    if(LayerTextures.Length == 0)
                    {
                        return null;
                    }
                    if(LayerTextures.Length ==1)
                    {
                        // Not animated
                        return LayerTextures[0];
                    }
                    // Greater than 1, animate
                    return LayerTextures[(int)Mathf.Abs(m_FrameTime % LayerTextures.Length)];

                }
                return null;
            }
        }

        /// <summary>
        /// Wrapper function for accessing settings stored Layer Max values
        /// </summary>
        public float LayerMax
        {
            get
            {
                var settings = SettingsController.Instance.Current;
                switch (Layer)
                {
                    case Layers.None:
                        return 0f;
                    case Layers.DeepWater:
                        return settings.DeepWaterMax;
                    case Layers.Water:
                        return settings.WaterMax;
                    case Layers.Shallows:
                        return settings.ShallowsMax;
                    case Layers.Sand:
                        return settings.SandMax;
                    case Layers.Grass:
                        return settings.GrassMax;
                    case Layers.Forest:
                        return settings.ForestMax;
                    case Layers.Rock:
                        return settings.RockMax;
                    case Layers.Snow:
                        return settings.SnowMax;
                    case Layers.Lava:
                        return settings.LavaMax;
                    default:
                        return 0f;
                }
            }
        }

        public void UpdateTick(float deltaTime)
        {
            m_FrameTime += Time.deltaTime * AnimationSpeed;
        }
    }

    public Vector3 DefaultPos { get { return new Vector3(66f, 66f, 66f); } }
    private LayerRange[] m_Ranges = new LayerRange[9];
    private int m_RangeCount = 0;
    private int[] m_SpawnableIndices;

    [SerializeField]
    private UnearthDepthController m_DepthController;
    [SerializeField]
    private Transform m_TargetTransform;

    [SerializeField]
    private List<DepthLayer> m_DepthLayers = new List<DepthLayer>();
    public List<DepthLayer> GetDepthLayers()
    {
        return m_DepthLayers;
    }

    private void Start()
    {
        // Sort depth layers
        m_DepthLayers.Sort((dl1, dl2) => dl1.LayerMax.CompareTo(dl2.LayerMax));
    }

    private void Update()
    {
        for(int i = 0; i < m_DepthLayers.Count; i++)
        {
            m_DepthLayers[i].UpdateTick(Time.deltaTime);
        }
    }

    public Vector3 GetRandomPointOnLayers(Layers layers)
    {
        var selectedLayers = m_DepthLayers.Where((dl) => layers.HasFlag(dl.Layer));
        if (selectedLayers == null || selectedLayers.Count() == 0)
        {
            return DefaultPos;
        }

        m_RangeCount = 0;
        var rangeMin = SettingsController.Instance.Current.RangeMin;
        var rangeMax = SettingsController.Instance.Current.RangeMax;
        var uintRange = (uint)(rangeMax - rangeMin);
        foreach (var sl in selectedLayers)
        {
            var layerBelowIndex = m_DepthLayers.IndexOf(sl) - 1;
            var r = new LayerRange();
            r.Min = (uint)(rangeMax - (sl.LayerMax * uintRange));
            r.Max = (uint)rangeMax;
            if (layerBelowIndex >= 0)
            {
                r.Max = (uint)(rangeMax - (m_DepthLayers[layerBelowIndex].LayerMax * uintRange));
            }
            m_Ranges[m_RangeCount] = r;
            m_RangeCount++;
        }
        var renderTex = m_DepthController.GetDepthRenderTexture();
        var width = renderTex.width;
        var height = renderTex.height;
        var settings = SettingsController.Instance.Current;
        var quadWidth = m_TargetTransform.localScale.x;
        var quadHeight = m_TargetTransform.localScale.y;
        int xMin = 0;
        int xMax = 0;
        int yMin = 0;
        int yMax = 0;
        if (settings.FlipHorizontal)
        {
            xMax = Mathf.FloorToInt((1f - ((settings.ClipLeft + (quadWidth * 0.5f)) / quadWidth)) * width);
            xMin = Mathf.CeilToInt((1f - ((settings.ClipRight + (quadWidth * 0.5f)) / quadWidth)) * width);
        }
        else
        {
            xMin = Mathf.FloorToInt(((settings.ClipLeft + (quadWidth * 0.5f)) / quadWidth) * width);
            xMax = Mathf.CeilToInt(((settings.ClipRight + (quadWidth * 0.5f)) / quadWidth) * width);
        }
        if (settings.FlipVertical)
        {
            yMax = Mathf.FloorToInt((1f - ((settings.ClipBottom + (quadHeight * 0.5f)) / quadHeight)) * height);
            yMin = Mathf.CeilToInt((1f - ((settings.ClipTop + (quadHeight * 0.5f)) / quadHeight)) * height);
        }
        else
        {
            yMin = Mathf.FloorToInt(((settings.ClipBottom + (quadHeight * 0.5f)) / quadHeight) * height);
            yMax = Mathf.CeilToInt(((settings.ClipTop + (quadHeight * 0.5f)) / quadHeight) * height);
        }

        int count = 0;
        if (m_SpawnableIndices == null)
        {
            m_SpawnableIndices = new int[renderTex.width * renderTex.height];
        }
        // Clear all values
        for (int i = 0; i < m_SpawnableIndices.Length; i++)
        {
            m_SpawnableIndices[i] = int.MaxValue;
        }
        var depthBuffer = m_DepthController.GetDepthBuffer();
        for (int i = 0; i < m_SpawnableIndices.Length; i++)
        {
            var depth = depthBuffer[i];
            for (int r = 0; r < m_RangeCount; r++)
            {
                if (depth > m_Ranges[r].Min && depth <= m_Ranges[r].Max && CoordInClipRange(i % width, Mathf.FloorToInt(i / (float)width), xMin, xMax, yMin, yMax))
                {
                    m_SpawnableIndices[count] = i;
                    count++;
                    break;
                }
            }
        }

        if (count == 0)
        {
            return DefaultPos;
        }

        var index = m_SpawnableIndices[UnityEngine.Random.Range(0, count)];

        int x = index % width;
        int y = Mathf.FloorToInt(index / (float)width);

        var quadScale = m_TargetTransform.localScale;
        // We add the half pixel size to place the item at the centre of the sampled pixel.
        var halfPixelWorldSize = (quadScale.x / width) * 0.5f;

        var pos = new Vector3((x / (float)width) * quadScale.x - (quadScale.x * 0.5f) + halfPixelWorldSize, 0f, (y / (float)height) * quadScale.y - (quadScale.y * 0.5f) + halfPixelWorldSize);
        if (SettingsController.Instance.Current.FlipHorizontal)
        {
            pos.x = -pos.x;
        }
        if (SettingsController.Instance.Current.FlipVertical)
        {
            pos.z = -pos.z;
        }
        return pos;
    }

    public Layers GetLayerAtPosition(Vector3 pos)
    {
        var quadScale = m_TargetTransform.localScale;
        // If we're currently in a flipped mode the positions will be flipped,
        // so flip them back for lookup.
        if (SettingsController.Instance.Current.FlipHorizontal)
        {
            pos.x = -pos.x;
        }
        if (SettingsController.Instance.Current.FlipVertical)
        {
            pos.z = -pos.z;
        }
        var renderTex = m_DepthController.GetDepthRenderTexture();
        var width = renderTex.width;
        var height = renderTex.height;
        var x = Mathf.FloorToInt(((pos.x + (quadScale.x * 0.5f)) / quadScale.x) * width);
        var y = Mathf.FloorToInt(((pos.z + (quadScale.y * 0.5f)) / quadScale.y) * height);
        var settings = SettingsController.Instance.Current;
        var uintDepth = m_DepthController.GetDepthBuffer()[y * width + x];
        var offsetDepth = uintDepth - settings.RangeMin;
        var depth = 1f - ((m_DepthController.GetDepthBuffer()[y * width + x] - settings.RangeMin) / (float)(settings.RangeMax - settings.RangeMin));

        // Depth layers are sorted in ascending order.
        bool done = false;
        int i = 0;
        while (!done && i < m_DepthLayers.Count)
        {
            if (depth > m_DepthLayers[i].LayerMax)
            {
                i++;
                continue;
            }
            // found highest layer
            return m_DepthLayers[i].Layer;
        }

        return Layers.None;
    }

    private bool CoordInClipRange(int x, int y, int xMin, int xMax, int yMin, int yMax)
    {
        return x >= xMin && x <= xMax && y >= yMin && y <= yMax;
    }
}
