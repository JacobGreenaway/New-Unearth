using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class TextureManager : MonoBehaviour 
{
	public int ColorWidth { get; private set; }
	public int ColorHeight { get; private set; }

	public LayerManager layerManager;

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
		DepthMatrix depthMatrix = depthFeed.GetDepthMatrix ();
		Debug.Log (depthMatrix);

		// Create _Data
		for (int i = 0; i < depthMatrix.m_matrix.GetLength(0) - 1; i++) {
			for (int j = 0; j < depthMatrix.m_matrix.GetLength(1) - 1; j++) {

				DepthPoint currentDepthPoint = depthMatrix.m_matrix [i, j];

				if (currentDepthPoint == null) {
					return;
				}

				Layer layer = layerManager.DetermineLayer (currentDepthPoint.GetValue ());


				if (layer != null) {
					byte r = (byte)layer.color.r;
					byte g = (byte)layer.color.b;
					byte b = (byte)layer.color.b;
					byte a = (byte)layer.color.a;

					_Data [i * j + 0] = r;
					_Data [i * j + 1] = g;
					_Data [i * j + 2] = b;
					_Data [i * j + 3] = a;
				}
					

//				} else {
//					_Data [i * j + 0] = (byte)0;
//					_Data [i * j + 1] = (byte)0;
//					_Data [i * j + 2] = (byte)0;
//					_Data [i * j + 3] = (byte)0;
//				}


			}
		}

		_Texture.LoadRawTextureData(_Data);
		_Texture.Apply();

	}
}
