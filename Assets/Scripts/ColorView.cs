using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class ColorView : MonoBehaviour
{
	public GameObject textureManager;
	private TextureManager _TextureManager;
	private int count = 0;

	void Start ()
	{
		//gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
	}

	void Update()
	{
		if (textureManager == null)
		{
			return;
		}

		_TextureManager = textureManager.GetComponent<TextureManager> ();
		if (_TextureManager == null) {
			return;
		}

		Texture tex = _TextureManager.GetColorTexture ();

		if (tex == null) {
			return;
		}

		gameObject.GetComponent<Renderer> ().material.mainTexture = tex;
		//Debug.Log (_TextureManager.GetColorTexture ());
		
	}
}