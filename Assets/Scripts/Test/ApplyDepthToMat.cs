using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyDepthToMat : MonoBehaviour {

    public MultiSourceManager m_MultiSourceManager;
    public Material m_Mat;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            m_Mat.mainTexture = m_MultiSourceManager.GetDepthRenderTexture();
        }
    }
}
