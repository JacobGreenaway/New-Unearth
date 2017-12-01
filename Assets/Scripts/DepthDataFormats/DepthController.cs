using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System;

public class DepthController : MonoBehaviour
{
    [Serializable]
    public class DepthLayer
    {
        public string Name;
        public float LayerMax;
        public Color LayerColor = Color.black;
        public Texture2D LayerTexture;
    }
    private const int MaxLayers = 10;

    [SerializeField]
    private MultiSourceManager multiSourceManager;
    [SerializeField]
    private int frameBufferCount = 5;
    private int currentBuffer = 0;

    private KinectSensor sensor;
    private DepthFrameReader depthFrameReader;

    private ushort[][] arrDepth;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public event Action NewDepthInfoAvailable;
    
    private Material m_BlitMat;
    [SerializeField]
    private RenderTexture m_RenderTex;
    [SerializeField]
    private Material m_TargetMaterial;

    [SerializeField]
    private float m_RangeMin = 0.8f;
    [SerializeField]
    private float m_RangeMax = 0.9f;


    [SerializeField]
    private List<DepthLayer> m_DepthLayers = new List<DepthLayer>();

    private void Start()
    {
        m_BlitMat = new Material(Shader.Find("Hidden/LayersConversion"));
        sensor = KinectSensor.GetDefault();
        Debug.Log("Float : " + sizeof(float));

        if (sensor != null)
        {
            Debug.Log("Kinect sensor found");
            arrDepth = new ushort[frameBufferCount][];
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

        var depthTex = multiSourceManager.GetDepthTexture();
        if (depthTex == null)
        {
            return;
        }
        if (m_RenderTex == null)
        {
            m_RenderTex = new RenderTexture(depthTex.width, depthTex.height, 16);
            
            m_TargetMaterial.mainTexture = m_RenderTex;
        }
        m_BlitMat.SetTexture("_DepthTex", depthTex);
        m_BlitMat.SetFloat("_RangeMin", m_RangeMin);
        m_BlitMat.SetFloat("_RangeMax", m_RangeMax);

        for(int i = 0; i < MaxLayers; i++)
        {
            if (i < m_DepthLayers.Count)
            {
                m_BlitMat.SetColor(string.Format("_Layer{0}Color", (i + 1).ToString()), m_DepthLayers[i].LayerColor);
                m_BlitMat.SetFloat(string.Format("_Layer{0}Max", (i + 1).ToString()), GetRangedMax(m_DepthLayers[i].LayerMax));
            }
            else
            {
                m_BlitMat.SetColor(string.Format("_Layer{0}Color", (i + 1).ToString()), Color.black);
                m_BlitMat.SetFloat(string.Format("_Layer{0}Max", (i + 1).ToString()), -1f);
            }
        }

        Graphics.Blit(m_RenderTex, m_RenderTex, m_BlitMat);
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
}
