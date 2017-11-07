using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class DepthFeedManager : MonoBehaviour
{   
	public int pollRate;
	private int currentPoll = 0;

	/**
	 * Reference to a filereader in case the multi source reader doesnt work
	*/
	public FileReader fileReader;

	/*
	 * GameObject with a multisourcemanager component
	 */
	public GameObject multiSourceGameObject;

	public bool bSmoothFrames;
	public int iSmoothFrameRate = 30;
	private DepthSmoother smoother;

	// Reference to kinect sensor 
	private KinectSensor _Sensor;
	private DepthFrameReader _Reader;

	private DepthMatrix depthMatrix;
	private MultiSourceManager multiSourceManager;

	private ushort[] arrDepth;

	/**
	 * Get the current depth data as a DepthMatrix
	 */
	public DepthMatrix GetDepthMatrix()
	{
		return depthMatrix;
	}
		

	void Start () 
	{
		_Sensor = KinectSensor.GetDefault();


		if (_Sensor != null) 
		{
			Debug.Log ("Kinect sensor found");
			arrDepth = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];
		}


		smoother = new DepthSmoother (iSmoothFrameRate);

	}

	void Update () 
	{
		
			// Get the multi source manager
			multiSourceManager = multiSourceGameObject.GetComponent<MultiSourceManager> ();

			// If we are unable to get the multi source manager, read from the text file
			if (multiSourceManager == null) {
				return;
			}


			ushort[] depthData = multiSourceManager.GetDepthData ();
			DepthMatrix newMatrix = new DepthMatrix (ref depthData, Height, Width);

			// If we are smoothing the depth frames
			if (bSmoothFrames) {
				smoother.AddDepth (newMatrix);
				depthMatrix = smoother.GetSmoothDepthMatrix ();
				return;
			}

			// Create a new DepthMatrix from the depth data
			depthMatrix = newMatrix;




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
