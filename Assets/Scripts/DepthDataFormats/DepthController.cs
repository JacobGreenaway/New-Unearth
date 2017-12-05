using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System;
using System.Linq;

public class DepthController : MonoBehaviour
{
    public enum Layers
    {
        None,
        DeepWater,
        Water,
        Shallows,
        Sand,
        Grass,
        Forest,
        Rock,
        Snow,
        Lava
    }

    [Serializable]
    public class DepthLayer
    {
        public Layers Layer;
        public float LayerMax;
        public Color LayerColor = Color.black;
        public Texture2D LayerTexture;
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
    private float m_SpawnableScale = 100f;

    [SerializeField]
    private int m_SmoothingWeight;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public event Action NewDepthInfoAvailable;

    private Material m_BlitMat;
    private RenderTexture m_RenderTex;
    [SerializeField]
    private Material m_TargetMaterial;
    [SerializeField]
    private GameObject m_TargetQuad;

    [SerializeField]
    private int m_RangeMin = 950;
    [SerializeField]
    private int m_RangeMax = 1200;

    [SerializeField]
    private float m_FadeRange = 0.05f;

    [SerializeField]
    private List<DepthLayer> m_DepthLayers = new List<DepthLayer>();

    private Color[] m_SpawnableValues;

    [SerializeField]
    private Shader m_LayerConversionShader;

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
    private float m_LineThickness = 0.02f;
    [SerializeField]
    private Color m_LineColor = Color.black;

    private Texture2D m_DepthTex;
    private Color[] m_DepthColorData;

    private void Start()
    {
        m_BlitMat = new Material(m_LayerConversionShader);
        m_ContourBlitMat = new Material(m_ContoursShader);
        sensor = KinectSensor.GetDefault();

        // Sort depth layers
        m_DepthLayers.Sort((dl1, dl2) => dl1.LayerMax.CompareTo(dl2.LayerMax));

        if (sensor != null)
        {
            Debug.Log("Kinect sensor found");
            arrDepth = new ushort[frameBufferCount][];
            var depthFrameDesc = sensor.DepthFrameSource.FrameDescription;
            m_DepthTex = new Texture2D(depthFrameDesc.Width, depthFrameDesc.Height, TextureFormat.RGBAFloat, true);
            m_DepthColorData = new Color[depthFrameDesc.LengthInPixels];
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
        if (NewDepthInfoAvailable != null)
        {
            NewDepthInfoAvailable();
        }

        
        for(int dcd = 0; dcd < m_DepthColorData.Length; dcd++)
        {
            int denominator = 0;
            int count = 1;
            float val = 0f;
            for(int frame = 0; frame < frameBufferCount; frame++)
            {
                var offsetFrame = currentBuffer + frame;
                if(offsetFrame >= frameBufferCount)
                {
                    offsetFrame -= frameBufferCount;
                }
                if(arrDepth[offsetFrame][dcd] >= m_RangeMin && arrDepth[offsetFrame][dcd] < m_RangeMax )
                {
                    val += arrDepth[offsetFrame][dcd] * count;
                    denominator += count;
                    count++;
                }
            }
            if(denominator > 0)
            {
                var avg = ((val / denominator) - m_RangeMin) / (float)(m_RangeMax - m_RangeMin);
                avg = (avg + (m_DepthColorData[dcd].r * m_SmoothingWeight)) / (m_SmoothingWeight + 1f); 
                m_DepthColorData[dcd] = new Color(avg, avg, avg, 1f);
            }
            else
            {
                m_DepthColorData[dcd] = new Color(0f, 0f, 0f, 0f); 
            }
        }

        m_DepthTex.SetPixels(m_DepthColorData);
        m_DepthTex.Apply();

        //var depthTex = multiSourceManager.GetDepthTexture();
        //if (depthTex == null)
        //{
        //    return;
        //}

        m_SpawnableValues = m_DepthTex.GetPixels(m_SpawnMapMipLevel);

        if (m_RenderTex == null)
        {
            m_RenderTex = new RenderTexture(m_DepthTex.width, m_DepthTex.height, 16);

            m_TargetMaterial.mainTexture = m_RenderTex;
        }
        m_BlitMat.SetTexture("_DepthTex", m_DepthTex);
        m_BlitMat.SetFloat("_FadeRange", m_FadeRange);

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

        if(m_ContoursEnabled)
        {
            m_ContourTargetQuad.SetActive(true);
            if(m_ContourRenderTex == null)
            {
                m_ContourRenderTex = new RenderTexture(m_DepthTex.width, m_DepthTex.height, 16);
                m_ContourTargetMaterial.mainTexture = m_ContourRenderTex;
            }

            m_ContourBlitMat.SetTexture("_DepthTex", m_DepthTex);
            m_ContourBlitMat.SetFloat("_SeaLevel", m_SeaLevel);
            m_ContourBlitMat.SetFloat("_HeightDivision", m_HeightDivision);
            m_ContourBlitMat.SetFloat("_LineThickness", m_LineThickness);
            m_ContourBlitMat.SetColor("_LineColor", m_LineColor);

            Graphics.Blit(m_ContourRenderTex, m_ContourRenderTex, m_ContourBlitMat);
        }
        else
        {
            m_ContourTargetQuad.SetActive(false);
        }
    }

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
        return ((m_RangeMax - m_RangeMin) * maxValue) + m_RangeMin;
    }

    public Vector3 GetRandomPointOnLayer(Layers layer)
    {
        var selectedLayer = m_DepthLayers.FirstOrDefault((dl) => dl.Layer == layer);

        if (selectedLayer == null)
        {
            return DefaultPos;
        }
        var layerBelowIndex = m_DepthLayers.IndexOf(selectedLayer) - 1;
        float rangeMin = 0f;
        float rangeMax = selectedLayer.LayerMax;
        if (layerBelowIndex >= 0)
        {
            rangeMin = m_DepthLayers[layerBelowIndex].LayerMax;
        }

        // For testing
        //Color depth;
        //int index = -1;
        //for(int i = 0; i < m_SpawnableValues.Length; i++)
        //{
        //    if(1.0f - m_SpawnableValues[i].r > rangeMin && 1.0f - m_SpawnableValues[i].r < rangeMax)
        //    {
        //        index = i;
        //        break;
        //    }
        //}
        var matches = m_SpawnableValues.Select((c, i) => new { c = c, i = i })
            .Where(v => 1f - v.c.r > rangeMin && 1f - v.c.r < rangeMax);
        //var count = matches.Count();
        //Debug.Log("Matches : " + count + " : " + (count / (float)(GetMipMapWidth() * GetMipMapHeight())));
        if (matches == null || matches.Count() == 0)
        {
            return DefaultPos;
        }
        var selection = matches
            .OrderBy(v => v.GetHashCode())
            .FirstOrDefault();
        
        var width = GetMipMapWidth();
        var height = GetMipMapHeight();

        //int testX = 127;
        //int testY = 105;

        var index = selection.i;//testX + testY * width; 
        int x = index % width;
        int y = Mathf.FloorToInt(index / (float)width);
        
        var quadScale = m_TargetQuad.transform.localScale;
        // We add the hald mipped pixel size to place the item at the centre of the sampled pixel.
        var halfMippedPixelWorldSize = (quadScale.x / width) * 0.5f;

        var pos = new Vector3((x / (float)width) * quadScale.x - (quadScale.x * 0.5f) + halfMippedPixelWorldSize, m_SpawnableHeight, (y / (float)height) * quadScale.y - (quadScale.y * 0.5f) + halfMippedPixelWorldSize);
        //Debug.Log("Got Random Point : " + x + " : " + y + " : " + pos.x + " : " + pos.z + " : " + selection.c.r + " : " + selection.i);
        return pos;
    }

    public Layers GetLayerAtPosition(Vector3 pos)
    {
        var quadScale = m_TargetQuad.transform.localScale;
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
            if (depth > GetScaledRange(m_DepthLayers[i].LayerMax))
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
        return ((m_RangeMax - m_RangeMin) * range) + m_RangeMin;
    }
}
