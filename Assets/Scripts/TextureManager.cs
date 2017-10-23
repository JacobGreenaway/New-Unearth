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

		if (depthMatrix == null) {
			return;
		}
		
		int iterationCount = 0;
//
//			// Create _Data
		for (int i = depthMatrix.matrix.GetLength (0) - 1; i > -1 ; i--) {
			for (int j = depthMatrix.matrix.GetLength (1) -1; j > -1; j--) {
				iterationCount++;

				DepthPoint currentDepthPoint = depthMatrix.matrix [i, j];

				if (currentDepthPoint == null) {
					throw new UnityException ("depth point is null");
				}

				ushort value = currentDepthPoint.GetValue ();

				if (value == null) {
					throw new UnityException ("value of depth point is null");
				}

				Layer layer = layerManager.DetermineLayer(value);

				int baseIndex =  (i * depthMatrix.matrix.GetLength (1) + j) * 4;

				if (layer != null) {
					_Data [baseIndex + 0] = (byte) (layer.color.r * 255);
					_Data [baseIndex + 1] = (byte) (layer.color.g * 255);
					_Data [baseIndex + 2] = (byte) (layer.color.b * 255);
					_Data [baseIndex + 3] = (byte) (layer.color.a);
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
