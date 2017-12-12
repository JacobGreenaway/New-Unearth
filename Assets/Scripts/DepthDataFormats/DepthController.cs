using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DepthController : MonoBehaviour
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
        public float Top;
        public float Bottom;
    }

    [Serializable]
    public class DepthLayer
    {
        public Layers Layer;
        public Color LayerColor = Color.black;
        public Texture2D LayerTexture;

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
    private MultiSourceManager multiSourceManager;
    [SerializeField]
    private int frameBufferCount = 5;
    private int currentBuffer = 0;

    private KinectSensor sensor;
    private DepthFrameReader depthFrameReader;

    private ushort[][] arrDepth;

    [SerializeField]
    private int m_SpawnMapMipLevel = 0;
    [SerializeField]
    private float m_SpawnableHeight = 0.5f;

    [SerializeField]
    private float m_SmoothingWeight;
    [SerializeField]
    private Vector2 m_Tiling = Vector2.one;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public event Action NewDepthInfoAvailable;

    public event Action<bool> HorizontalFlipped;
    public event Action<bool> VerticalFlipped;
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

    private Color[] m_SpawnableValues;

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
    private Color[] m_DepthColorData;

    [SerializeField]
    private bool m_Parallel = false;

    private void Start()
    {
        m_BlitMat = new Material(m_LayerConversionShader);
        m_TexturedBlitMat = new Material(m_TexturedLayersConversionShader);
        m_ContourBlitMat = new Material(m_ContoursShader);
        
        sensor = KinectSensor.GetDefault();

        // Sort depth layers
        m_DepthLayers.Sort((dl1, dl2) => dl1.LayerMax.CompareTo(dl2.LayerMax));

        if (sensor != null)
        {
            Debug.Log("Kinect sensor found");
            arrDepth = new ushort[frameBufferCount][];
            var depthFrameDesc = sensor.DepthFrameSource.FrameDescription;
           // m_DepthTex = new Texture2D(depthFrameDesc.Width, depthFrameDesc.Height, TextureFormat.RGBAFloat, true);
            m_DepthColorData = new Color[depthFrameDesc.LengthInPixels];
            for (int i = 0; i < m_DepthColorData.Length; i++)
            {
                m_DepthColorData[i] = Color.black;
            }
            for (int i = 0; i < frameBufferCount; i++)
            {
                Width = sensor.DepthFrameSource.FrameDescription.Width;
                Height = sensor.DepthFrameSource.FrameDescription.Height;
                arrDepth[i] = new ushort[sensor.DepthFrameSource.FrameDescription.LengthInPixels];
            }
        }
    }

    // The multi source manager prepares data during update, so we wait for LateUpdate to get this frame's data
    private void LateUpdate()
    {
        if (Input.GetButtonDown("Flip Vertical"))
        {
            SettingsController.Instance.Current.FlipVertical = !SettingsController.Instance.Current.FlipVertical;
            VerticalFlipped?.Invoke(SettingsController.Instance.Current.FlipVertical);
        }
        if (Input.GetButtonDown("Flip Horizontal"))
        {
            SettingsController.Instance.Current.FlipHorizontal = !SettingsController.Instance.Current.FlipHorizontal;
            HorizontalFlipped?.Invoke(SettingsController.Instance.Current.FlipHorizontal);
        }
        if(Input.GetButtonDown("Toggle contour line"))
        {
            m_ContoursEnabled = !m_ContoursEnabled;
        }
        if(Input.GetButtonDown("Toggle Textured"))
        {
            m_TexturedToggle = !m_TexturedToggle;
        }
        
        if (arrDepth == null)
        {
            return;
        }
        ++currentBuffer;
        if (currentBuffer >= frameBufferCount)
        {
            // Loop
            currentBuffer = 0;
        }
        arrDepth[currentBuffer] = multiSourceManager.GetDepthData();
        NewDepthInfoAvailable?.Invoke();

        // For each depth pixel
        //if (m_Parallel)
        //{
        //    Parallel.For(0, m_DepthColorData.Length, UpdateDepth);
        //}
        //else
        //{
        //    for (int i = 0; i < m_DepthColorData.Length; i++)
        //    {
        //        UpdateDepth(i);
        //    }
        //}

        //for (int dcd = 0; dcd < m_DepthColorData.Length; dcd++)
        //{
        //    var val = arrDepth[currentBuffer][dcd];
        //    if(val < m_RangeMin)
        //    {
        //        // If it's rejected above the range, we use the previous frame's data.
        //        // Do Nothing

        //    }
        //    else if(val < m_RangeMax)
        //    {
        //        var ranged = (val - m_RangeMin) / (float)(m_RangeMax - m_RangeMin);
        //        m_DepthColorData[dcd] = (m_DepthColorData[dcd] + (new Color(ranged, ranged, ranged, 1f) * m_SmoothingWeight)) / (m_SmoothingWeight + 1);
        //    }
        //    else
        //    {
        //        m_DepthColorData[dcd] = new Color(1f, 1f, 1f, 1f);
        //    }
        //}

        //m_DepthTex.SetPixels(m_DepthColorData);
        //m_DepthTex.Apply();
        m_DepthTex = multiSourceManager.GetDepthRenderTexture();

        //m_SpawnableValues = m_DepthTex.GetPixels(m_SpawnMapMipLevel);

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

    //private void UpdateDepth(int dcd)
    //{
    //    int denominator = 0;
    //    int count = 1;
    //    float val = 0f;
    //    // For every frame in the frame buffer
    //    for (int frame = 1; frame < frameBufferCount; frame++)
    //    {
    //        // Calculate the offset from the newest frame
    //        var offsetFrame = currentBuffer + frame;
    //        if (offsetFrame >= frameBufferCount)
    //        {
    //            offsetFrame -= frameBufferCount;
    //        }
    //        // If the target pixel is within the correct range
    //        if (arrDepth[offsetFrame][dcd] >= RangeMin && arrDepth[offsetFrame][dcd] < RangeMax)
    //        {
    //            // Add the value multiplied by the count
    //            val += arrDepth[offsetFrame][dcd] * count;
    //            // Add the count to the denominator (give more priority to more recent frames
    //            denominator += count;
    //            count++;
    //        }
    //    }
    //    // If we got any values inside the range
    //    if (denominator > 0)
    //    {
    //        // Find the scaled value average and store in the depth data array
    //        var avg = ((val / denominator) - RangeMin) / (float)(RangeMax - RangeMin);
    //        // Smooth this with the current value in the buffer
    //        avg = (avg + (m_DepthColorData[dcd].r * m_SmoothingWeight)) / (m_SmoothingWeight + 1f);
    //        m_DepthColorData[dcd] = new Color(avg, avg, avg, 1f);
    //    }
    //    else
    //    {
    //        // Force Black
    //        m_DepthColorData[dcd] = new Color(1f, 1f, 1f, 1f);
    //    }
    //}


    public ushort GetDepthValue(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            return arrDepth[currentBuffer][y * Width + x];
        }
        return 0;
    }

    private float GetRangedMax(float maxValue)
    {
        return ((RangeMax - RangeMin) * maxValue) + RangeMin;
    }

    private List<LayerRange> m_Ranges = new List<LayerRange>();

    public Vector3 GetRandomPointOnLayers(Layers layers)
    {
        var selectedLayers = m_DepthLayers.Where((dl) => layers.HasFlag(dl.Layer));

        if (selectedLayers == null || selectedLayers.Count() == 0)
        {
            return DefaultPos;
        }

        m_Ranges.Clear();
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
            m_Ranges.Add(r);
        }

        var width = GetMipMapWidth();
        var height = GetMipMapHeight();
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

        var matches = m_SpawnableValues.Select((c, i) => new { c = c, i = i })
            .Where(v => IndexInClipRange(v.i, xMin, xMax, yMin, yMax) && m_Ranges.Any((r) => 1f - v.c.r > r.Bottom && 1f - v.c.r < r.Top));
       if (matches == null || matches.Count() == 0)
        {
            return DefaultPos;
        }
        var selection = matches
            .OrderBy(v => v.GetHashCode())
            .FirstOrDefault();
        
        
        var index = selection.i;//testX + testY * width; 
        int x = index % width;
        int y = Mathf.FloorToInt(index / (float)width);
        
        var quadScale = m_TargetQuad.transform.localScale;
        // We add the hald mipped pixel size to place the item at the centre of the sampled pixel.
        var halfMippedPixelWorldSize = (quadScale.x / width) * 0.5f;

        var pos = new Vector3((x / (float)width) * quadScale.x - (quadScale.x * 0.5f) + halfMippedPixelWorldSize, m_SpawnableHeight, (y / (float)height) * quadScale.y - (quadScale.y * 0.5f) + halfMippedPixelWorldSize);
        //Debug.Log("Got Random Point : " + x + " : " + y + " : " + pos.x + " : " + pos.z + " : " + selection.c.r + " : " + selection.i);
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
        var x = Mathf.FloorToInt(((pos.x + (quadScale.x * 0.5f)) / quadScale.x) * GetMipMapWidth());
        var y = Mathf.FloorToInt(((pos.z + (quadScale.y * 0.5f)) / quadScale.y) * GetMipMapHeight());

        var depth = 1f - m_SpawnableValues[y * GetMipMapWidth() + x].r;
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

    private int GetMipMapWidth()
    {
        var width = Width;
        for (int i = 0; i < m_SpawnMapMipLevel; i++)
        {
            width /= 2;
        }
        return width;
    }

    private int GetMipMapHeight()
    {
        var height = Height;
        for (int i = 0; i < m_SpawnMapMipLevel; i++)
        {
            height /= 2;
        }
        return height;
    }

    private float GetScaledRange(float range)
    {
        return ((RangeMax - RangeMin) * range) + RangeMin;
    }

    private bool IndexInClipRange(int index, int xMin, int xMax, int yMin, int yMax)
    {
        int x = index % GetMipMapWidth();
        int y = Mathf.FloorToInt(index / (float)GetMipMapWidth());
        return x >= xMin && x <= xMax && y >= yMin && y <= yMax;
    }
}
