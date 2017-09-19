using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class DepthSourceManager : MonoBehaviour
{   
	private KinectSensor _Sensor;
	private DepthFrameReader _Reader;

	private DepthMatrix depthMatrix;

	private ushort[] arrDepth;

	public DepthMatrix GetDepthMatrix()
	{
		return depthMatrix;
	}

	void Start () 
	{
		_Sensor = KinectSensor.GetDefault();

		if (_Sensor != null) 
		{
			_Reader = _Sensor.DepthFrameSource.OpenReader();
			arrDepth = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];
		}
	}

	void Update () 
	{
		if (_Reader != null)
		{
			var frame = _Reader.AcquireLatestFrame();
			if (frame != null)
			{
				frame.CopyFrameDataToArray(arrDepth);
				frame.Dispose();
				frame = null;

				depthMatrix = new DepthMatrix (arrDepth);
			}
		}
	}

	public int Width
	{
		get { return _Sensor.DepthFrameSource.FrameDescription.Width; }
	}

	public int Height
	{
		get { return _Sensor.DepthFrameSource.FrameDescription.Height; }
	}

	void OnApplicationQuit()
	{
		if (_Reader != null)
		{
			_Reader.Dispose();
			_Reader = null;
		}

		if (_Sensor != null)
		{
			if (_Sensor.IsOpen)
			{
				_Sensor.Close();
			}

			_Sensor = null;
		}
	}
}
