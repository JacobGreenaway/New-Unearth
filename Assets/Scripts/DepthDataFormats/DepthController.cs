using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System;
using System.Linq;
using System.Threading.Tasks;
using Layers = UnearthLayersController.Layers;

public class DepthController : MonoBehaviour
{
    private struct LayerRange
    {
        public float Top;
        public float Bottom;
        public uint OrigTop;
        public uint OrigBottom;
    }

    /// <summary>
    /// Depth Layer is a serializable class for setting up layers in the Unity Editor.
    /// </summary>
    [Serializable]
    public class DepthLayer
    {
        public Layers Layer;
        public Color LayerColor = Color.black;
        public Texture2D LayerTexture;

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
    }

    private const int MaxLayers = 10;
    public Vector3 DefaultPos { get { return new Vector3(66f, 66f, 66f); } }

    [SerializeField]
    private MultiSourceManager m_MultiSourceManager;
    [SerializeField]
    private int m_FrameBufferCount = 5;
    private int m_CurrentBuffer = 0;
    private ushort[][] m_DepthBufferCirc;

    private KinectSensor m_Sensor;
    
    [SerializeField]
    private float m_SpawnableHeight = 0.5f;

    [SerializeField]
    private float m_SmoothingWeight;
    [SerializeField]
    private Vector2 m_Tiling = Vector2.one;

    private int m_Width;
    private int m_Height;
    
    private Material m_BlitMat;
    private Material m_TexturedBlitMat;
    private RenderTexture m_RenderTex;
    [SerializeField]
    private Material m_TargetMaterial;
    [SerializeField]
    private GameObject m_TargetQuad;

    [SerializeField]
    private int RangeMin
    {
        get
        {
            return SettingsController.Instance.Current.RangeMin;
        }
        set
        {
            SettingsController.Instance.Current.RangeMin = value;
        }
    }

    [SerializeField]
    private int RangeMax
    {
        get
        {
            return SettingsController.Instance.Current.RangeMax;
        }
        set
        {
            SettingsController.Instance.Current.RangeMax = value;
        }
    }

    [SerializeField]
    private float m_FadeRange = 0.05f;

    [SerializeField]
    private List<DepthLayer> m_DepthLayers = new List<DepthLayer>();
    
    private int[] m_SpawnableIndices;

    [SerializeField]
    private Shader m_LayerConversionShader;
    [SerializeField]
    private Shader m_TexturedLayersConversionShader;
    [SerializeField]
    private bool m_TexturedToggle = false;

    [Header("Contours")]
    [SerializeField]
    private Shader m_ContoursShader;
    [SerializeField]
    private Material m_ContourTargetMaterial;
    [SerializeField]
    private GameObject m_ContourTargetQuad;

    private Material m_ContourBlitMat;
    private RenderTexture m_ContourRenderTex;

    [SerializeField]
    private bool m_ContoursEnabled = false;
    [SerializeField]
    private float m_SeaLevel = 0.25f;
    [SerializeField]
    private float m_HeightDivision = 0.1f;
    [SerializeField]
    private Color m_LineColor = Color.black;
    [SerializeField]
    private float m_SampleDistance = 1f;

    private RenderTexture m_DepthTex;

    private void Start()
    {
        m_BlitMat = new Material(m_LayerConversionShader);
        m_TexturedBlitMat = new Material(m_TexturedLayersConversionShader);
        m_ContourBlitMat = new Material(m_ContoursShader);

        m_Sensor = KinectSensor.GetDefault();

        // Sort depth layers
        m_DepthLayers.Sort((dl1, dl2) => dl1.LayerMax.CompareTo(dl2.LayerMax));

        if (m_Sensor != null)
        {
            Debug.Log("Kinect sensor found");
            m_DepthBufferCirc = new ushort[m_FrameBufferCount][];
            var depthFrameDesc = m_Sensor.DepthFrameSource.FrameDescription;
            for (int i = 0; i < m_FrameBufferCount; i++)
            {
                m_Width = m_Sensor.DepthFrameSource.FrameDescription.Width;
                m_Height = m_Sensor.DepthFrameSource.FrameDescription.Height;
                m_DepthBufferCirc[i] = new ushort[m_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];
            }
            m_SpawnableIndices = new int[m_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];
        }
    }

    // The multi source manager prepares data during update, so we wait for LateUpdate to get this frame's data
    private void LateUpdate()
    {
        if (Input.GetButtonDown("Flip Vertical"))
        {
            SettingsController.Instance.Current.FlipVertical = !SettingsController.Instance.Current.FlipVertical;
        }
        if (Input.GetButtonDown("Flip Horizontal"))
        {
            SettingsController.Instance.Current.FlipHorizontal = !SettingsController.Instance.Current.FlipHorizontal;
        }
        if (Input.GetButtonDown("Toggle contour line"))
        {
            m_ContoursEnabled = !m_ContoursEnabled;
        }
        if (Input.GetButtonDown("Toggle Textured"))
        {
            m_TexturedToggle = !m_TexturedToggle;
        }

        if (m_DepthBufferCirc == null)
        {
            return;
        }
        ++m_CurrentBuffer;
        if (m_CurrentBuffer >= m_FrameBufferCount)
        {
            // Loop
            m_CurrentBuffer = 0;
        }
        m_DepthBufferCirc[m_CurrentBuffer] = m_MultiSourceManager.GetDepthData();
        
        m_DepthTex = m_MultiSourceManager.GetDepthRenderTexture();

        if (m_RenderTex == null)
        {
            m_RenderTex = new RenderTexture(m_DepthTex.width * 2, m_DepthTex.height * 2, 16);

            m_TargetMaterial.mainTexture = m_RenderTex;
        }
        if (m_TexturedToggle)
        {
            m_TexturedBlitMat.SetTexture("_DepthTex", m_DepthTex);
            m_TexturedBlitMat.SetFloat("_FadeRange", m_FadeRange);
            m_TexturedBlitMat.SetFloat("_UOffset", SettingsController.Instance.Current.FlipHorizontal ? 1f : 0f);
            m_TexturedBlitMat.SetFloat("_VOffset", SettingsController.Instance.Current.FlipVertical ? 1f : 0f);

            for (int i = 0; i < MaxLayers; i++)
            {
                if (i < m_DepthLayers.Count)
                {
                    m_TexturedBlitMat.SetTexture(string.Format("_Layer{0}Tex", (i + 1).ToString()), m_DepthLayers[i].LayerTexture);
                    m_TexturedBlitMat.SetTextureScale(string.Format("_Layer{0}Tex", (i + 1).ToString()), m_Tiling);
                    m_TexturedBlitMat.SetFloat(string.Format("_Layer{0}Max", (i + 1).ToString()), m_DepthLayers[i].LayerMax);
                }
                else
                {
                    m_TexturedBlitMat.SetTexture(string.Format("_Layer{0}Tex", (i + 1).ToString()), Texture2D.blackTexture);
                    m_TexturedBlitMat.SetFloat(string.Format("_Layer{0}Max", (i + 1).ToString()), -1f);
                }
            }

            Graphics.Blit(m_RenderTex, m_RenderTex, m_TexturedBlitMat);
        }
        else
        {
            m_BlitMat.SetTexture("_DepthTex", m_DepthTex);
            m_BlitMat.SetFloat("_FadeRange", m_FadeRange);
            m_BlitMat.SetFloat("_UOffset", SettingsController.Instance.Current.FlipHorizontal ? 1f : 0f);
            m_BlitMat.SetFloat("_VOffset", SettingsController.Instance.Current.FlipVertical ? 1f : 0f);

            for (int i = 0; i < MaxLayers; i++)
            {
                if (i < m_DepthLayers.Count)
                {
                    m_BlitMat.SetColor(string.Format("_Layer{0}Color", (i + 1).ToString()), m_DepthLayers[i].LayerColor);
                    m_BlitMat.SetFloat(string.Format("_Layer{0}Max", (i + 1).ToString()), m_DepthLayers[i].LayerMax);
                }
                else
                {
                    m_BlitMat.SetColor(string.Format("_Layer{0}Color", (i + 1).ToString()), Color.black);
                    m_BlitMat.SetFloat(string.Format("_Layer{0}Max", (i + 1).ToString()), -1f);
                }
            }

            Graphics.Blit(m_RenderTex, m_RenderTex, m_BlitMat);
        }

        if (m_ContoursEnabled)
        {
            m_ContourTargetQuad.SetActive(true);
            if (m_ContourRenderTex == null)
            {
                m_ContourRenderTex = new RenderTexture(m_DepthTex.width, m_DepthTex.height, 16);
                m_ContourTargetMaterial.mainTexture = m_ContourRenderTex;
            }

            m_ContourBlitMat.SetTexture("_DepthTex", m_DepthTex);
            m_ContourBlitMat.SetFloat("_SeaLevel", m_SeaLevel);
            m_ContourBlitMat.SetFloat("_HeightDivision", m_HeightDivision);
            m_ContourBlitMat.SetColor("_LineColor", m_LineColor);
            m_ContourBlitMat.SetFloat("_SampleDistance", m_SampleDistance);
            m_ContourBlitMat.SetFloat("_UOffset", SettingsController.Instance.Current.FlipHorizontal ? 1f : 0f);
            m_ContourBlitMat.SetFloat("_VOffset", SettingsController.Instance.Current.FlipVertical ? 1f : 0f);

            Graphics.Blit(m_ContourRenderTex, m_ContourRenderTex, m_ContourBlitMat);
        }
        else
        {
            m_ContourTargetQuad.SetActive(false);
        }
    }


    public ushort GetDepthValue(int x, int y)
    {
        if (x >= 0 && x < m_Width && y >= 0 && y < m_Height)
        {
            return m_DepthBufferCirc[m_CurrentBuffer][y * m_Width + x];
        }
        return 0;
    }

    private float GetRangedMax(float maxValue)
    {
        return ((RangeMax - RangeMin) * maxValue) + RangeMin;
    }

    private LayerRange[] m_Ranges = new LayerRange[9];
    private int m_RangeCount = 0;

    public Vector3 GetRandomPointOnLayers(Layers layers)
    {
        var selectedLayers = m_DepthLayers.Where((dl) => layers.HasFlag(dl.Layer));

        if (selectedLayers == null || selectedLayers.Count() == 0)
        {
            return DefaultPos;
        }
        m_RangeCount = 0;
        foreach (var sl in selectedLayers)
        {
            var layerBelowIndex = m_DepthLayers.IndexOf(sl) - 1;
            var r = new LayerRange();
            r.Bottom = 0f;
            r.Top = sl.LayerMax;
            if (layerBelowIndex >= 0)
            {
                r.Bottom = m_DepthLayers[layerBelowIndex].LayerMax;
            }
            // The original depth data goes from top to bottom so we flip the
            // range limits when calculating for original data range checks
            r.OrigBottom = (uint)(RangeMax - (r.Top * (RangeMax - RangeMin)));
            r.OrigTop = (uint)(RangeMax - (r.Bottom * (RangeMax - RangeMin)));
            m_Ranges[m_RangeCount] = r;
            m_RangeCount++;
        }

        var width = m_Width;
        var height = m_Height;
        var settings = SettingsController.Instance.Current;
        var quadWidth = m_TargetQuad.transform.localScale.x;
        var quadHeight = m_TargetQuad.transform.localScale.y;
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
        for (int i = 0; i < m_SpawnableIndices.Length; i++)
        {
            m_SpawnableIndices[i] = int.MaxValue;
        }
        for (int i = 0; i < m_SpawnableIndices.Length; i++)
        {
            var depth = m_DepthBufferCirc[m_CurrentBuffer][i];
            for (int r = 0; r < m_RangeCount; r++)
            {
                if (depth > m_Ranges[r].OrigBottom && depth <= m_Ranges[r].OrigTop && IndexInClipRange(i, xMin, xMax, yMin, yMax))
                {
                    m_SpawnableIndices[count] = i;
                    count++;
                    break;
                }
            }
        }

        if (count == 0)
        {
            // No available spots
            return DefaultPos;
        }

        var index = m_SpawnableIndices[UnityEngine.Random.Range(0, count)];

        int x = index % width;
        int y = Mathf.FloorToInt(index / (float)width);

        var quadScale = m_TargetQuad.transform.localScale;
        // We add the half pixel size to place the item at the centre of the sampled pixel.
        var halfPixelWorldSize = (quadScale.x / width) * 0.5f;

        var pos = new Vector3((x / (float)width) * quadScale.x - (quadScale.x * 0.5f) + halfPixelWorldSize, m_SpawnableHeight, (y / (float)height) * quadScale.y - (quadScale.y * 0.5f) + halfPixelWorldSize);
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
        var quadScale = m_TargetQuad.transform.localScale;
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
        var x = Mathf.FloorToInt(((pos.x + (quadScale.x * 0.5f)) / quadScale.x) * m_Width);
        var y = Mathf.FloorToInt(((pos.z + (quadScale.y * 0.5f)) / quadScale.y) * m_Height);
        var settings = SettingsController.Instance.Current;
        var depth = 1f - ((m_DepthBufferCirc[m_CurrentBuffer][y * m_Width + x] - settings.RangeMin) / (RangeMax - RangeMin));
        //var depth = 1f - m_SpawnableValues[y * GetMipMapWidth() + x].r;
        //Debug.Log("Point Calc : " + x + " : " + y);
        //Debug.Break();
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

    private float GetScaledRange(float range)
    {
        return ((RangeMax - RangeMin) * range) + RangeMin;
    }

    private bool IndexInClipRange(int index, int xMin, int xMax, int yMin, int yMax)
    {
        int x = index % m_Width;//GetMipMapWidth();
        int y = Mathf.FloorToInt(index / (float)m_Width/*GetMipMapWidth()*/);
        return x >= xMin && x <= xMax && y >= yMin && y <= yMax;
    }
}
