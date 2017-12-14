using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class UnearthVisualsController : MonoBehaviour
{
    private struct LayerRange
    {
        public float Top;
        public float Bottom;
        public uint OrigTop;
        public uint OrigBottom;
    }
    
    private const int MaxLayers = 10;
    public Vector3 DefaultPos { get { return new Vector3(66f, 66f, 66f); } }

    [SerializeField]
    private UnearthDepthController m_DepthController;
    [SerializeField]
    private UnearthLayersController m_LayersController;
    [SerializeField]
    private int m_FrameBufferCount = 2;
    private int m_CurrentBuffer = 0;
    private ushort[][] m_DepthBufferCirc;

    private Material m_BlitMat;
    private Material m_TexturedBlitMat;
    private RenderTexture m_RenderTex;
    [SerializeField]
    private Material m_TargetMaterial;
    [SerializeField]
    private GameObject m_TargetQuad;
    [SerializeField]
    private float m_FadeRange = 0.05f;
    [SerializeField]
    private Shader m_LayerConversionShader;
    [SerializeField]
    private Shader m_TexturedLayersConversionShader;
    [SerializeField]
    private bool m_TexturedToggle = false;
    [SerializeField]
    private Vector2 m_Tiling = Vector2.one;

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

    private List<UnearthLayersController.DepthLayer> m_DepthLayers;
    private RenderTexture m_DepthTex;
    private int m_Width;
    private int m_Height;

    private const int OutputResolutionScale = 3;

    private void Start()
    {
        m_BlitMat = new Material(m_LayerConversionShader);
        m_TexturedBlitMat = new Material(m_TexturedLayersConversionShader);
        m_ContourBlitMat = new Material(m_ContoursShader);

        m_DepthLayers = m_LayersController.GetDepthLayers();
        
        var sensor = KinectSensor.GetDefault();

        if (sensor != null)
        {
            m_DepthBufferCirc = new ushort[m_FrameBufferCount][];
            var depthFrameDesc = sensor.DepthFrameSource.FrameDescription;
            m_Width = depthFrameDesc.Width;
            m_Height = depthFrameDesc.Height;
            for (int i = 0; i < m_FrameBufferCount; i++)
            {
                m_DepthBufferCirc[i] = new ushort[depthFrameDesc.LengthInPixels];
            }
        }
    }

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
        var newBuffer = m_DepthController.GetDepthBuffer();
        for (int i = 0; i < newBuffer.Length; i++)
        {
            m_DepthBufferCirc[m_CurrentBuffer][i] = newBuffer[i];
        }

        m_DepthTex = m_DepthController.GetDepthRenderTexture();

        if (m_RenderTex == null)
        {
            m_RenderTex = new RenderTexture(m_Width * OutputResolutionScale, m_Height * OutputResolutionScale, 16);

            m_TargetMaterial.mainTexture = m_RenderTex;
        }
        if(m_TexturedToggle)
        {
            // Run Textured Shader
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

        if(m_ContoursEnabled)
        {
            m_ContourTargetQuad.SetActive(true);
            if (m_ContourRenderTex == null)
            {
                m_ContourRenderTex = new RenderTexture(m_DepthTex.width * OutputResolutionScale, m_DepthTex.height * OutputResolutionScale, 16);
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
}
