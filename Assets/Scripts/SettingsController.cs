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
        public bool Dirty { get; set; }

        [SerializeField]
        private Vector3 m_CamPos;
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
