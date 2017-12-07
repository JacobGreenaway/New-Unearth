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
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
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
