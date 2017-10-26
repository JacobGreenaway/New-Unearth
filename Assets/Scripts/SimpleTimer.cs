using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTimer : MonoBehaviour
{

    public float targetTime = 60.0f;
    public float currentTime;

    void Start()
    {
        currentTime = targetTime;
    }
    //Returns time left or zero
    //Upon returning zero, will reset
    public float Count()
    {

        currentTime -= Time.deltaTime;

        if (currentTime <= 0.0f)
        {
            currentTime = targetTime;
            return 0f;
        }
        return currentTime;
    }

    void timerEnded()
    {
        //do your stuff here.
    }

}
