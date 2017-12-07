using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SettingsUIController : MonoBehaviour
{
    private SettingsController.Settings m_Current;

    [Serializable]
    public class ButtonSetting
    {
        public Text Text;
        public Button Button;
    }

    [Serializable]
    public class InputSetting
    {
        public Text Text;
        public InputField Input;
    }

    [SerializeField]
    private ButtonSetting m_FlipVertical;

    [SerializeField]
    private ButtonSetting m_FlipHorizontal;

    [SerializeField]
    private SettingsInputQuadButton m_DepthRangeMax;

    [SerializeField]
    private SettingsInputQuadButton m_DepthRangeMin;

    [SerializeField]
    private SettingsInputQuadButton m_DeepWater;
    [SerializeField]
    private SettingsInputQuadButton m_Water;
    [SerializeField]
    private SettingsInputQuadButton m_Shallows;
    [SerializeField]
    private SettingsInputQuadButton m_Sand;
    [SerializeField]
    private SettingsInputQuadButton m_Grass;
    [SerializeField]
    private SettingsInputQuadButton m_Forest;
    [SerializeField]
    private SettingsInputQuadButton m_Rock;
    [SerializeField]
    private SettingsInputQuadButton m_Snow;
    [SerializeField]
    private SettingsInputQuadButton m_Lava;


    private float m_TrackingRangeMin;
    private float m_TrackingRangeMax;
    private const float RangeChangeSpeed = 10f;

    private const float LayerShiftSmall = 0.001f;
    private const float LayerShiftLarge = 0.01f;

    private void Awake()
    {
        m_Current = SettingsController.Instance.Current;

        InitValues();
        InitEvents();
    }

    private void InitEvents()
    {
        m_Current.FlipVerticalChanged += HandleFlipVerticalChanged;
        m_FlipVertical.Button.onClick.AddListener(HandleFlipVerticalClick);

        m_Current.FlipHorizontalChanged += HandleFlipHorizontalChanged;
        m_FlipHorizontal.Button.onClick.AddListener(HandleFlipHorizontalClick);

        m_DepthRangeMax.SetButton1Action(() => m_TrackingRangeMax -= 10);
        m_DepthRangeMax.SetButton2Action(() => m_TrackingRangeMax -= 1);
        m_DepthRangeMax.SetButton3Action(() => m_TrackingRangeMax += 1);
        m_DepthRangeMax.SetButton4Action(() => m_TrackingRangeMax += 10);
        m_Current.RangeMaxChanged += HandleRangeMaxChanged;

        m_DepthRangeMin.SetButton1Action(() => m_TrackingRangeMin -= 10);
        m_DepthRangeMin.SetButton2Action(() => m_TrackingRangeMin -= 1);
        m_DepthRangeMin.SetButton3Action(() => m_TrackingRangeMin += 1);
        m_DepthRangeMin.SetButton4Action(() => m_TrackingRangeMin += 10);
        m_Current.RangeMinChanged += HandleRangeMinChanged;

        // Deep Water
        m_DeepWater.SetButton1Action(() => m_Current.DeepWaterMax -= LayerShiftLarge);
        m_DeepWater.SetButton2Action(() => m_Current.DeepWaterMax -= LayerShiftSmall);
        m_DeepWater.SetButton3Action(() => m_Current.DeepWaterMax += LayerShiftSmall);
        m_DeepWater.SetButton4Action(() => m_Current.DeepWaterMax += LayerShiftLarge);
        m_Current.DeepWaterMaxChanged += HandleDeepWaterMaxChanged;

        // Water
        m_Water.SetButton1Action(() => m_Current.WaterMax -= LayerShiftLarge);
        m_Water.SetButton2Action(() => m_Current.WaterMax -= LayerShiftSmall);
        m_Water.SetButton3Action(() => m_Current.WaterMax += LayerShiftSmall);
        m_Water.SetButton4Action(() => m_Current.WaterMax += LayerShiftLarge);
        m_Current.WaterMaxChanged += HandleWaterMaxChanged;

        // Shallows
        m_Shallows.SetButton1Action(() => m_Current.ShallowsMax -= LayerShiftLarge);
        m_Shallows.SetButton2Action(() => m_Current.ShallowsMax -= LayerShiftSmall);
        m_Shallows.SetButton3Action(() => m_Current.ShallowsMax += LayerShiftSmall);
        m_Shallows.SetButton4Action(() => m_Current.ShallowsMax += LayerShiftLarge);
        m_Current.ShallowsMaxChanged += HandleShallowsMaxChanged;

        // Sand
        m_Sand.SetButton1Action(() => m_Current.SandMax -= LayerShiftLarge);
        m_Sand.SetButton2Action(() => m_Current.SandMax -= LayerShiftSmall);
        m_Sand.SetButton3Action(() => m_Current.SandMax += LayerShiftSmall);
        m_Sand.SetButton4Action(() => m_Current.SandMax += LayerShiftLarge);
        m_Current.SandMaxChanged += HandleSandMaxChanged;

        // Grass
        m_Grass.SetButton1Action(() => m_Current.GrassMax -= LayerShiftLarge);
        m_Grass.SetButton2Action(() => m_Current.GrassMax -= LayerShiftSmall);
        m_Grass.SetButton3Action(() => m_Current.GrassMax += LayerShiftSmall);
        m_Grass.SetButton4Action(() => m_Current.GrassMax += LayerShiftLarge);
        m_Current.GrassMaxChanged += HandleGrassMaxChanged;

        //Forest
        m_Forest.SetButton1Action(() => m_Current.ForestMax -= LayerShiftLarge);
        m_Forest.SetButton2Action(() => m_Current.ForestMax -= LayerShiftSmall);
        m_Forest.SetButton3Action(() => m_Current.ForestMax += LayerShiftSmall);
        m_Forest.SetButton4Action(() => m_Current.ForestMax += LayerShiftLarge);
        m_Current.ForestMaxChanged += HandleForestMaxChanged; ;

        // Rock
        m_Rock.SetButton1Action(() => m_Current.RockMax -= LayerShiftLarge);
        m_Rock.SetButton2Action(() => m_Current.RockMax -= LayerShiftSmall);
        m_Rock.SetButton3Action(() => m_Current.RockMax += LayerShiftSmall);
        m_Rock.SetButton4Action(() => m_Current.RockMax += LayerShiftLarge);
        m_Current.RockMaxChanged += HandleRockMaxChanged; ;

        // Snow
        m_Snow.SetButton1Action(() => m_Current.SnowMax -= LayerShiftLarge);
        m_Snow.SetButton2Action(() => m_Current.SnowMax -= LayerShiftSmall);
        m_Snow.SetButton3Action(() => m_Current.SnowMax += LayerShiftSmall);
        m_Snow.SetButton4Action(() => m_Current.SnowMax += LayerShiftLarge);
        m_Current.SnowMaxChanged += HandleSnowMaxChanged; ;

        // Lava
        m_Lava.SetButton1Action(() => m_Current.LavaMax -= LayerShiftLarge);
        m_Lava.SetButton2Action(() => m_Current.LavaMax -= LayerShiftSmall);
        m_Lava.SetButton3Action(() => m_Current.LavaMax += LayerShiftSmall);
        m_Lava.SetButton4Action(() => m_Current.LavaMax += LayerShiftLarge);
        m_Current.LavaMaxChanged += HandleLavaMaxChanged; ;
    }

    private void HandleLavaMaxChanged(float lavaMax)
    {
        m_Lava.SetText("Lava Max : " + lavaMax);
    }

    private void HandleSnowMaxChanged(float snowMax)
    {
        m_Snow.SetText("Snow Max : " + snowMax);
    }

    private void HandleRockMaxChanged(float rockMax)
    {
        m_Rock.SetText("Rock Max : " + rockMax);
    }

    private void HandleForestMaxChanged(float forestMax)
    {
        m_Forest.SetText("Forest Max : " + forestMax);
    }

    private void HandleGrassMaxChanged(float grassMax)
    {
        m_Grass.SetText("Grass Max : " + grassMax);
    }

    private void HandleSandMaxChanged(float sandMax)
    {
        m_Sand.SetText("Sand Max : " + sandMax);
    }

    private void HandleShallowsMaxChanged(float shallowsMax)
    {
        m_Shallows.SetText("Shallows Max : " + shallowsMax);
    }

    private void HandleWaterMaxChanged(float waterMax)
    {
        m_Water.SetText("Water Max : " + waterMax);
    }

    private void HandleDeepWaterMaxChanged(float deepWaterMax)
    {
        m_DeepWater.SetText("Deep Water Max : " + deepWaterMax);
    }

    private void HandleRangeMinChanged(int rangeMin)
    {
        m_DepthRangeMin.SetText("Depth Range Min : " + ToString());
    }

    private void HandleRangeMaxChanged(int rangeMax)
    {
        m_DepthRangeMax.SetText("Depth Range Max :" + rangeMax.ToString());
    }

    private void Update()
    {
        // So that we can have smooth tracking input we track changes in float and adjust on the int.
        m_TrackingRangeMin += Input.GetAxis("Raise Lower") * RangeChangeSpeed * Time.deltaTime;
        int newRangeMin = (int)m_TrackingRangeMin;
        if (m_Current.RangeMin != newRangeMin)
        {
            m_Current.RangeMin = newRangeMin;
        }

        m_TrackingRangeMax += Input.GetAxis("Raise Upper") * RangeChangeSpeed * Time.deltaTime;
        int newRangeMax = (int)m_TrackingRangeMax;
        if (m_Current.RangeMax != newRangeMax)
        {
            m_Current.RangeMax = newRangeMax;
        }
    }

    private void HandleFlipVerticalClick()
    {
        m_Current.FlipVertical = !m_Current.FlipVertical;
    }

    private void HandleFlipHorizontalClick()
    {
        m_Current.FlipHorizontal = !m_Current.FlipHorizontal;
    }

    private void HandleFlipHorizontalChanged(bool flipped)
    {
        m_FlipHorizontal.Text.text = "Horizontal : " + (flipped ? "Flipped" : "Normal");
    }

    private void HandleFlipVerticalChanged(bool flipped)
    {
        m_FlipVertical.Text.text = "Vertical : " + (flipped ? "Flipped" : "Normal");
    }

    private void InitValues()
    {
        m_TrackingRangeMin = m_Current.RangeMin;
        m_TrackingRangeMax = m_Current.RangeMax;

        HandleFlipVerticalChanged(m_Current.FlipVertical);
        HandleFlipHorizontalChanged(m_Current.FlipHorizontal);
        HandleRangeMaxChanged(m_Current.RangeMax);
        HandleRangeMinChanged(m_Current.RangeMin);

        HandleDeepWaterMaxChanged(m_Current.DeepWaterMax);
        HandleWaterMaxChanged(m_Current.WaterMax);
        HandleShallowsMaxChanged(m_Current.ShallowsMax);
        HandleSandMaxChanged(m_Current.SandMax);
        HandleGrassMaxChanged(m_Current.GrassMax);
        HandleForestMaxChanged(m_Current.ForestMax);
        HandleRockMaxChanged(m_Current.RockMax);
        HandleSnowMaxChanged(m_Current.SnowMax);
        HandleLavaMaxChanged(m_Current.LavaMax);
    }
}


