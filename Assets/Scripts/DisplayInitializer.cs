using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayInitializer : MonoBehaviour
{
    [SerializeField]
    private Camera m_HeightmapCamera;
    [SerializeField]
    private Camera m_UICamera;


    private void Awake()
    {
        Display.displays[0].SetRenderingResolution(Display.displays[0].systemWidth, Display.displays[0].systemHeight);
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Display.displays[1].SetRenderingResolution(Display.displays[1].systemWidth, Display.displays[1].systemHeight);
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Camera Swap"))
        {
            var target = m_HeightmapCamera.targetDisplay;
            m_HeightmapCamera.targetDisplay = m_UICamera.targetDisplay;
            m_UICamera.targetDisplay = target;
        }
    }
}
