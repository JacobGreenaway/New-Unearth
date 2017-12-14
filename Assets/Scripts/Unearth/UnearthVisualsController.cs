using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

/// <summary>
/// Controls all colouring and rendering onto main plane
/// </summary>
public class UnearthVisualsController : MonoBehaviour
{
    // Number of layers the shaders support
    private const int MaxLayers = 10;
    // Default position, used for rejecting positions.
    public Vector3 DefaultPos { get { return new Vector3(66f, 66f, 66f); } }

    [SerializeField]
    private UnearthDepthController m_DepthController;
    [SerializeField]
    private UnearthLayersController m_LayersController;

    [Header("Colours and Textures")]
    // Material for Blitting Coloured mode to render texture
    private Material m_BlitMat;
    // Material for Blitting the Textures mode to render texture
    private Material m_TexturedBlitMat;
    // Render texture applied to our main rendering material
    private RenderTexture m_RenderTex;
    // Material applied to the main Quad in the scene
    [SerializeField]
    private Material m_TargetMaterial;
    // The target Quad being rendered to
    [SerializeField]
    private GameObject m_TargetQuad;
    // Range between layers to blend
    [SerializeField]
    private float m_FadeRange = 0.05f;
    // Shader for generating main coloured visuals
    [SerializeField]
    private Shader m_LayerConversionShader;
    // Shader for generating textured visuals
    [SerializeField]
    private Shader m_TexturedLayersConversionShader;
    // Toggle for Textured mode vs coloured mode
    [SerializeField]
    private bool m_TexturedToggle = false;
    // Tiling scale applied to Textures in Textured mode.
    [SerializeField]
    private Vector2 m_Tiling = Vector2.one;

    [Header("Contours")]
    // Shader for generating Contours
    [SerializeField]
    private Shader m_ContoursShader;
    // Material used for rendering generated contours texture
    [SerializeField]
    private Material m_ContourTargetMaterial;
    // Target Qaud for rendering contours
    [SerializeField]
    private GameObject m_ContourTargetQuad;
    // Blitting material for rendering contours
    private Material m_ContourBlitMat;
    // Render texture for storing rendererd contours.
    private RenderTexture m_ContourRenderTex;
    // Turns contours on and off.
    [SerializeField]
    private bool m_ContoursEnabled = false;
    // Sea level used as starting point for contours
    [SerializeField]
    private float m_SeaLevel = 0.25f;
    // How much distance there is between contour levels
    [SerializeField]
    private float m_HeightDivision = 0.1f;
    // Colour of contour lines
    [SerializeField]
    private Color m_LineColor = Color.black;
    // How far should contour lines sample?  Increasing this will increase line thickness
    [SerializeField]
    private float m_SampleDistance = 1f;

    private List<UnearthLayersController.DepthLayer> m_DepthLayers;
    private RenderTexture m_DepthTex;
    private int m_Width;
    private int m_Height;

    // Multiplier for render texture resolution vs depth texture size.
    private const int OutputResolutionScale = 3;

    /// <summary>
    /// Called by Unity once during startup
    /// </summary>
    private void Start()
    {
        // Create our blitting materials
        m_BlitMat = new Material(m_LayerConversionShader);
        m_TexturedBlitMat = new Material(m_TexturedLayersConversionShader);
        m_ContourBlitMat = new Material(m_ContoursShader);

        m_DepthLayers = m_LayersController.GetDepthLayers();
        
        var sensor = KinectSensor.GetDefault();

        if (sensor != null)
        {
            var depthFrameDesc = sensor.DepthFrameSource.FrameDescription;
            m_Width = depthFrameDesc.Width;
            m_Height = depthFrameDesc.Height;
        }
    }

    /// <summary>
    /// Called once per frame by Unity after all Update functions
    /// Use Late Update so that the depth texture is always the most recent from the Depth Controller.
    /// </summary>
    private void LateUpdate()
    {
        // Calculate changes based on Input.
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
        
        // Get the current rendertexture
        m_DepthTex = m_DepthController.GetDepthRenderTexture();

        // Init our render texture
        if (m_RenderTex == null)
        {
            m_RenderTex = new RenderTexture(m_Width * OutputResolutionScale, m_Height * OutputResolutionScale, 16);
            // Set the main texture on our target material to our render texture
            m_TargetMaterial.mainTexture = m_RenderTex;
        }
        // If we're in Textured mode
        if(m_TexturedToggle)
        {
            // Run Textured Shader
            m_TexturedBlitMat.SetTexture("_DepthTex", m_DepthTex);
            m_TexturedBlitMat.SetFloat("_FadeRange", m_FadeRange);
            m_TexturedBlitMat.SetFloat("_UOffset", SettingsController.Instance.Current.FlipHorizontal ? 1f : 0f);
            m_TexturedBlitMat.SetFloat("_VOffset", SettingsController.Instance.Current.FlipVertical ? 1f : 0f);
            
            // For each layer, set the texture, texture tiling and layer max information
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

            // Apply the shader to the Render Texture
            Graphics.Blit(m_RenderTex, m_RenderTex, m_TexturedBlitMat);
        }
        else
        {
            // We're in coloured mode.
            m_BlitMat.SetTexture("_DepthTex", m_DepthTex);
            m_BlitMat.SetFloat("_FadeRange", m_FadeRange);
            m_BlitMat.SetFloat("_UOffset", SettingsController.Instance.Current.FlipHorizontal ? 1f : 0f);
            m_BlitMat.SetFloat("_VOffset", SettingsController.Instance.Current.FlipVertical ? 1f : 0f);

            // For each layer, set the colour and layer max
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

            // Apply the shader to the Render Texture
            Graphics.Blit(m_RenderTex, m_RenderTex, m_BlitMat);
        }

        // If contours are enabled
        if(m_ContoursEnabled)
        {
            // Activate the Rendering Quad.
            m_ContourTargetQuad.SetActive(true);
            // Init our render texture
            if (m_ContourRenderTex == null)
            {
                m_ContourRenderTex = new RenderTexture(m_DepthTex.width * OutputResolutionScale, m_DepthTex.height * OutputResolutionScale, 16);
                // Set the main texture on our target material to our render texture
                m_ContourTargetMaterial.mainTexture = m_ContourRenderTex;
            }

            m_ContourBlitMat.SetTexture("_DepthTex", m_DepthTex);
            m_ContourBlitMat.SetFloat("_SeaLevel", m_SeaLevel);
            m_ContourBlitMat.SetFloat("_HeightDivision", m_HeightDivision);
            m_ContourBlitMat.SetColor("_LineColor", m_LineColor);
            m_ContourBlitMat.SetFloat("_SampleDistance", m_SampleDistance);
            m_ContourBlitMat.SetFloat("_UOffset", SettingsController.Instance.Current.FlipHorizontal ? 1f : 0f);
            m_ContourBlitMat.SetFloat("_VOffset", SettingsController.Instance.Current.FlipVertical ? 1f : 0f);

            // Apply the shader to the Render Texture
            Graphics.Blit(m_ContourRenderTex, m_ContourRenderTex, m_ContourBlitMat);
        }
        else
        {
            // Deactivate the rendering quad.
            m_ContourTargetQuad.SetActive(false);
        }
    }
}
