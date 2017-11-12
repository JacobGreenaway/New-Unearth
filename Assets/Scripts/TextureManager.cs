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

		float x = 30f / 100f;
		Debug.Log (x);
	}

	void Update () 
	{
		
	
		if (Input.GetButtonUp ("Toggle contour line") == true ) {
			bContour = !bContour;
			depthFeed.bSmoothFrames = true;
			depthFeed.iSmoothFrameRate = 2;
		}

		LayerManager lManager;

		if (bContour) {
			lManager = contourManager;
			Debug.Log("contour chosen");
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
					
					Layer layer = lManager.layerMap [value];

					float[] gradientColor = layer.getGradientColorForValue (value);

					_Data [baseIndex + 0] = (byte)(gradientColor[0] * 255);
					_Data [baseIndex + 1] = (byte)(gradientColor[1] * 255);
					_Data [baseIndex + 2] = (byte)(gradientColor[2] * 255);
					_Data [baseIndex + 3] = (byte)(gradientColor[3] * 255);
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

//	public int[] RGBToHSL(int r, int g, int b)
//	{
//		float fr = (float) r / 255;
//		float fg = (float) g / 255;
//		float fb = (float) b / 255;
//
//		float max = Mathf.Max(Mathf.Max(fr, fg), fb);
//		float min = Mathf.Min(Mathf.Min(fr, fg), fb);
//		float h;
//		float s;
//		var l = (max + min) / 2;
//
//		if (max == min)
//		{
//			h = 0;
//			s = 0;
//		}
//		else
//		{
//			float d = max - min;
//			s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
//
//			if (max == r) {
//				h = (fg - fb) / d + (fg < fb ? 6 : 0);
//			}
//			else if (max == g) {
//				h = (fb - fr) / d + 2;
//			}
//			else if (max == b) {
//				b: h = (fr - fg) / d + 4;
//			}
//
//			h /= 6;
//		}
//
//		int [] hsl = new int[]{h, s, l * 100};
//		return hsl;
//	}
//
//	public int[] HSLToRGB(int h, int s, int l)
//	{
//		// h stored as 0 to 360, s stored as 0% to 100%, l stored as 0% to 100%
//		h /= 360;
//		s /= 100;
//		l /= 100;
//
//		int r, g, b;
//
//		if (s == 0)
//		{
//			r = g = b = l;
//		}
//		else
//		{
//			var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
//			var p = 2 * l - q;
//			r = hToRGB(p, q, h + 1 / 3);
//			g = hToRGB(p, q, h);
//			b = hToRGB(p, q, h - 1 / 3);
//		}
//
//		return new int[]{(int)Mathf.Round(255 * r), (int)Mathf.Round(255 * g), (int)Mathf.Round(255 * b)};
//	}
//
//	private int hToRGB(int p, int q, int t)
//	{
//		if (t < 0) t += 1;
//		if (t > 1) t -= 1;
//		if (t < 1 / 6) return p + (q - p) * 6 * t;
//		if (t < 1 / 2) return q;
//		if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
//		return p;
//	}

}
