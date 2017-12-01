using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureFormatTesting : MonoBehaviour {

    public Material m_Mat;

	// Use this for initialization
	void Start () {
        var tex = new Texture2D(4, 4, TextureFormat.RGBAHalf, false);
        m_Mat.mainTexture = tex;
        var bytes =  tex.GetRawTextureData();
	}
	
	
}
