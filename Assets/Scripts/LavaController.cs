using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaController : MonoBehaviour {
    //Settings controller
    private SettingsController.Settings m_Current;

    //Simple timer
    public float targetTime = 20.0f;
    public float currentTime;

    // Use this for initialization
    void Start () {
        setInitial();
    }

    // Update is called once per frame
    void Update()
    {
        Count();
        //Debug.Log(Time.deltaTime/targetTime);
        if (Input.GetAxis("Lava") > 0)
        {
            
            lavaStart();
        }
    }

    private void lavaStart()
    {
        setInitial();
        if (m_Current.LavaMax > m_Current.SnowMax)
        {
            m_Current.LavaMax += (targetTime / Time.deltaTime);
        }
        else if (m_Current.SnowMax > m_Current.RockMax)
        {
            m_Current.LavaMax += (targetTime / Time.deltaTime);
            m_Current.SnowMax += (targetTime / Time.deltaTime);
        }
        else if (m_Current.RockMax > m_Current.ForestMax)
        {
            m_Current.LavaMax += (targetTime / Time.deltaTime);
            m_Current.SnowMax += (targetTime / Time.deltaTime);
            m_Current.RockMax += (targetTime / Time.deltaTime);
        }
        else if (m_Current.ForestMax > m_Current.GrassMax)
        {
            m_Current.LavaMax += (targetTime / Time.deltaTime);
            m_Current.SnowMax += (targetTime / Time.deltaTime);
            m_Current.RockMax += (targetTime / Time.deltaTime);
            m_Current.ForestMax += (targetTime / Time.deltaTime);
        }
        else if (m_Current.GrassMax > m_Current.SandMax)
        {
            m_Current.LavaMax += (targetTime / Time.deltaTime);
            m_Current.SnowMax += (targetTime / Time.deltaTime);
            m_Current.RockMax += (targetTime / Time.deltaTime);
            m_Current.ForestMax += (targetTime / Time.deltaTime);
            m_Current.GrassMax += (targetTime / Time.deltaTime);
        } else
        {

        }
    }

    //Returns time left or zero
    //Upon returning zero, will reset
    public float Count()
    {

        currentTime -= Time.deltaTime;

        if (currentTime <= 0.0f)
        {
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
