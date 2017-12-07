using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class SettingsController : MonoBehaviour
{
    [Serializable]
    public class Settings
    {
        public bool Dirty;

        [Header("Camera Settings")]
        [SerializeField]
        private Vector3 m_CamPos = new Vector3(0f, 47.5f, 0f);
        public Vector3 CamPos
        {
            get { return m_CamPos; }
            set
            {
                m_CamPos = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private bool m_FlipHorizontal;
        public bool FlipHorizontal
        {
            get { return m_FlipHorizontal; }
            set
            {
                if (m_FlipHorizontal != value)
                {
                    m_FlipHorizontal = value;
                    Dirty = true;
                    FlipHorizontalChanged?.Invoke(m_FlipHorizontal);
                }
            }
        }
        public event Action<bool> FlipHorizontalChanged;

        [SerializeField]
        private bool m_FlipVertical;
        public bool FlipVertical
        {
            get { return m_FlipVertical; }
            set
            {
                if (m_FlipVertical != value)
                {
                    m_FlipVertical = value;
                    Dirty = true;
                    FlipVerticalChanged?.Invoke(m_FlipVertical);
                }
            }
        }
        public event Action<bool> FlipVerticalChanged;

        [Header("Layer Levels")]
        [SerializeField]
        private float m_DeepWaterMax = 0.1f;
        public float DeepWaterMax
        {
            get { return m_DeepWaterMax; }
            set
            {
                if (m_DeepWaterMax != value)
                {
                    m_DeepWaterMax = value;
                    Dirty = true;
                    DeepWaterMaxChanged?.Invoke(m_DeepWaterMax);
                }
            }
        }
        public event Action<float> DeepWaterMaxChanged;

        [SerializeField]
        private float m_WaterMax = 0.2f;
        public float WaterMax
        {
            get { return m_WaterMax; }
            set
            {
                if (m_WaterMax != value)
                {
                    m_WaterMax = value;
                    Dirty = true;
                    WaterMaxChanged?.Invoke(m_WaterMax);
                }
            }
        }
        public event Action<float> WaterMaxChanged;

        [SerializeField]
        private float m_ShallowsMax = 0.25f;
        public float ShallowsMax
        {
            get { return m_ShallowsMax; }
            set
            {
                if (m_ShallowsMax != value)
                {
                    m_ShallowsMax = value;
                    Dirty = true;
                    ShallowsMaxChanged?.Invoke(m_ShallowsMax);
                }
            }
        }
        public event Action<float> ShallowsMaxChanged;

        [SerializeField]
        private float m_SandMax = 0.3f;
        public float SandMax
        {
            get { return m_SandMax; }
            set
            {
                if (m_SandMax != value)
                {
                    m_SandMax = value;
                    Dirty = true;
                    SandMaxChanged?.Invoke(m_SandMax);
                }
            }
        }
        public event Action<float> SandMaxChanged;

        [SerializeField]
        private float m_GrassMax = 0.4f;
        public float GrassMax
        {
            get { return m_GrassMax; }
            set
            {
                if (m_GrassMax != value)
                {
                    m_GrassMax = value;
                    Dirty = true;
                    GrassMaxChanged?.Invoke(m_GrassMax);
                }
            }
        }
        public event Action<float> GrassMaxChanged;

        [SerializeField]
        private float m_ForestMax = 0.6f;
        public float ForestMax
        {
            get { return m_ForestMax; }
            set
            {
                if (m_ForestMax != value)
                {
                    m_ForestMax = value;
                    Dirty = true;
                    ForestMaxChanged?.Invoke(m_ForestMax);
                }
            }
        }
        public event Action<float> ForestMaxChanged;

        [SerializeField]
        private float m_RockMax = 0.75f;
        public float RockMax
        {
            get { return m_RockMax; }
            set
            {
                if (m_RockMax != value)
                {
                    m_RockMax = value;
                    Dirty = true;
                    RockMaxChanged?.Invoke(m_RockMax);
                }
            }
        }
        public event Action<float> RockMaxChanged;

        [SerializeField]
        private float m_SnowMax = 0.9f;
        public float SnowMax
        {
            get { return m_SnowMax; }
            set
            {
                if (m_SnowMax != value)
                {
                    m_SnowMax = value;
                    Dirty = true;
                    SnowMaxChanged?.Invoke(m_SnowMax);
                }
            }
        }
        public event Action<float> SnowMaxChanged;

        [SerializeField]
        private float m_LavaMax = 1f;
        public float LavaMax
        {
            get { return m_LavaMax; }
            set
            {
                if (m_LavaMax != value)
                {
                    m_LavaMax = value;
                    Dirty = true;
                    LavaMaxChanged?.Invoke(m_LavaMax);
                }
            }
        }
        public event Action<float> LavaMaxChanged;

        [Header("Depth Settings")]
        [SerializeField]
        private int m_RangeMin = 950;
        public int RangeMin
        {
            get { return m_RangeMin; }
            set
            {
                if (m_RangeMin != value)
                {
                    m_RangeMin = value;
                    Dirty = true;
                    RangeMinChanged?.Invoke(m_RangeMin);
                }
            }
        }
        public event Action<int> RangeMinChanged;

        [SerializeField]
        private int m_RangeMax = 1200;
        public int RangeMax
        {
            get { return m_RangeMax; }
            set
            {
                if (m_RangeMax != value)
                {
                    m_RangeMax = value;
                    Dirty = true;
                    RangeMaxChanged?.Invoke(m_RangeMax);
                }
            }
        }
        public event Action<int> RangeMaxChanged;

        [SerializeField]
        private float m_ClipLeft = 0;
        public float ClipLeft
        {
            get { return m_ClipLeft; }
            set
            {
                m_ClipLeft = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private float m_ClipRight = 512;
        public float ClipRight
        {
            get { return m_ClipRight; }
            set
            {
                m_ClipRight = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private float m_ClipTop = 424;
        public float ClipTop
        {
            get { return m_ClipTop; }
            set
            {
                m_ClipTop = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private float m_ClipBottom = 0;
        public float ClipBottom
        {
            get { return m_ClipBottom; }
            set
            {
                m_ClipBottom = value;
                Dirty = true;
            }
        }
    }

    // Singleton Implementation;
    private static SettingsController m_Instance;
    public static SettingsController Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType<SettingsController>();
                if (m_Instance == null)
                {
                    var go = new GameObject("Settings Controller", typeof(SettingsController));
                    m_Instance = go.GetComponent<SettingsController>();
                }
            }
            return m_Instance;
        }
    }

    private void Awake()
    {
        if (m_Instance != null && m_Instance != this)
        {
            Debug.LogError("Already an instance of Settings Controller, destroying this one...");
            Destroy(gameObject);
            return;
        }

        LoadFromDisk();
        m_Instance = this;
    }

    [SerializeField]
    private Settings m_Settings;
    public Settings Current { get { return m_Settings; } }
    private const float SaveTime = 5f;
    private float m_Timer = 0f;

    private void Update()
    {
        m_Timer += Time.deltaTime;
        if (m_Timer > SaveTime)
        {
            m_Timer -= SaveTime;
            WriteToDisk();
        }
    }

    private void WriteToDisk()
    {
        if (m_Settings.Dirty)
        {
            var json = JsonUtility.ToJson(m_Settings, true);
            File.WriteAllText(GetFilePath(), json);
            m_Settings.Dirty = false;
        }
    }

    private void LoadFromDisk()
    {
        var path = GetFilePath();
        if (File.Exists(path))
        {
            m_Settings = JsonUtility.FromJson<Settings>(File.ReadAllText(path));
        }
        else
        {
            m_Settings = new Settings();
            m_Settings.Dirty = true;
            WriteToDisk();
        }
    }

    private string GetFilePath()
    {
        string path = Path.Combine(Application.dataPath, "settings.json");
#if UNITY_EDITOR
        path = path.Replace("/Assets", "");
#endif
        return path;
    }
}
