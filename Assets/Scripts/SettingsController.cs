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
                m_FlipHorizontal = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private bool m_FlipVertical;
        public bool FlipVertical
        {
            get { return m_FlipVertical; }
            set
            {
                m_FlipVertical = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private float m_DeepWaterMax = 0.1f;
        public float DeepWaterMax
        {
            get { return m_DeepWaterMax; }
            set
            {
                m_DeepWaterMax = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private float m_WaterMax = 0.2f;
        public float WaterMax
        {
            get { return m_WaterMax; }
            set
            {
                m_WaterMax = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private float m_ShallowsMax = 0.25f; 
        public float ShallowsMax
        {
            get { return m_ShallowsMax; }
            set
            {
                m_ShallowsMax = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private float m_SandMax = 0.3f;
        public float SandMax
        {
            get { return m_SandMax; }
            set
            {
                m_SandMax = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private float m_GrassMax = 0.4f;
        public float GrassMax
        {
            get { return m_GrassMax; }
            set
            {
                m_GrassMax = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private float m_ForestMax = 0.6f;
        public float ForestMax
        {
            get { return m_ForestMax; }
            set
            {
                m_ForestMax = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private float m_RockMax = 0.75f;
        public float RockMax
        {
            get { return m_RockMax; }
            set
            {
                m_RockMax = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private float m_SnowMax = 0.9f;
        public float SnowMax
        {
            get { return m_SnowMax; }
            set
            {
                m_SnowMax = value;
                Dirty = true;
            }
        }

        [SerializeField]
        private float m_LavaMax = 1f;
        public float LavaMax
        {
            get { return m_LavaMax; }
            set
            {
                m_LavaMax = value;
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
