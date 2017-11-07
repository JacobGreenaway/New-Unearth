using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class TextureManager : MonoBehaviour 
{
	public int ColorWidth { get; private set; }
	public int ColorHeight { get; private set; }

	public LayerManager contourManager;

	public LayerManager layerManager;

	public bool bContour = false;

	public DepthFeedManager depthFeed;

	private Texture2D _Texture;
	private byte[] _Data;

	public Texture2D GetColorTexture()
	{
		return _Texture;
	}

	void Start()
	{
		_Texture = new Texture2D(512, 424, TextureFormat.RGBA32, false);
		_Data = new byte[512 * 424 * 4];
	}

	void Update () 
	{
		
	
		if (Input.GetButtonUp ("Toggle contour line") == true ) {
			bContour = !bContour;
		}

		LayerManager lManager;

		if (bContour) {
			lManager = contourManager;
		} else {
			lManager = layerManager;
		}
			
		DepthMatrix depthMatrix = depthFeed.GetDepthMatrix ();

		if (depthMatrix == null) {
			return;
		}

//
//		// Create _Data
		int d0 = depthMatrix.matrix.GetLength (0);
		int d1 = depthMatrix.matrix.GetLength (1);

		for (int i = 0; i < d0; i++) {
			for (int j = 0; j < d1; j++) {

				ushort value = depthMatrix.matrix [i, j].GetValue ();

				int baseIndex =  (i * d1 + j) * 4;

				if (value > lManager.Min && value < lManager.Max) {
					//Debug.Log (value);
					Layer layer = lManager.layerMap [value];

					_Data [baseIndex + 0] = (byte)(layer.color.r * 255);
					_Data [baseIndex + 1] = (byte)(layer.color.g * 255);
					_Data [baseIndex + 2] = (byte)(layer.color.b * 255);
					_Data [baseIndex + 3] = (byte)(layer.color.a);
				} else {
					_Data [baseIndex + 0] = (byte) 0;
					_Data [baseIndex + 1] = (byte) 0;
					_Data [baseIndex + 2] = (byte) 0;
					_Data [baseIndex + 3] = (byte) 0;
				}
			}
		}

		_Texture.LoadRawTextureData(_Data);
		_Texture.Apply();
	

	}
}
