using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaController : MonoBehaviour {
    //Settings controller
    private SettingsController.Settings m_Current;

    //Simple timer
    public float targetTime = 20.0f;
    public float currentTime;

    private bool m_lavaStart;

    // Use this for initialization
    void Start () {
        //setInitial();
        m_Current = SettingsController.Instance.Current;
        m_lavaStart = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Time.deltaTime/targetTime);
        if (Input.GetButtonUp("Lava"))
        {
            lavaToggle();
        }

        lavaFlow();
    }

    private void lavaToggle()
    {
        
        if (m_lavaStart)
        {
            m_lavaStart = false;
        } else
        {
            m_lavaStart = true;
        }

        setInitial();
    }

    private void lavaFlow()
    {
        if (!m_lavaStart)
        {
            return;
        }

        if (m_Current.LavaMax > m_Current.SnowMax)
        {
            m_Current.LavaMax -= (Time.deltaTime / targetTime);
        }
        else if (m_Current.SnowMax > m_Current.RockMax)
        {
            m_Current.LavaMax -= (Time.deltaTime / targetTime);
            m_Current.SnowMax -= (Time.deltaTime / targetTime);
        }
        else if (m_Current.RockMax > m_Current.ForestMax)
        {
            m_Current.LavaMax -= (Time.deltaTime / targetTime);
            m_Current.SnowMax -= (Time.deltaTime / targetTime);
            m_Current.RockMax -= (Time.deltaTime / targetTime);
        }
        else if (m_Current.ForestMax > m_Current.GrassMax)
        {
            m_Current.LavaMax -= (Time.deltaTime / targetTime);
            m_Current.SnowMax -= (Time.deltaTime / targetTime);
            m_Current.RockMax -= (Time.deltaTime / targetTime);
            m_Current.ForestMax -= (Time.deltaTime / targetTime);
        }
        else if (m_Current.GrassMax > m_Current.SandMax)
        {
            m_Current.LavaMax -= (Time.deltaTime / targetTime);
            m_Current.SnowMax -= (Time.deltaTime / targetTime);
            m_Current.RockMax -= (Time.deltaTime / targetTime);
            m_Current.ForestMax -= (Time.deltaTime / targetTime);
            m_Current.GrassMax -= (Time.deltaTime / targetTime);
        }
        else
        {

        }
    }

    //Returns time left or zero
    //Upon returning zero, will reset
    public float Count()
    {

        currentTime -= Time.deltaTime;
        Debug.Log(currentTime);
        if (currentTime <= 0.0f)
        {
            Debug.Log(currentTime);
            setInitial();
            return 0f;
        }
        return currentTime;
    }


    private void setInitial ()
    {
        currentTime = targetTime;
        m_Current.DeepWaterMax = 0.1f;
        m_Current.WaterMax = 0.2f;
        m_Current.ShallowsMax = 0.25f;
        m_Current.SandMax = 0.3f;
        m_Current.GrassMax = 0.4f;
        m_Current.ForestMax = 0.6f;
        m_Current.RockMax = 0.75f;
        m_Current.SnowMax = 0.9f;
        m_Current.LavaMax = 1f;
    }
}
