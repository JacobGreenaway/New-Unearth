﻿using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class DepthFeedManager : MonoBehaviour
{   


	/*
	 * GameObject with a multisourcemanager component
	 */
	public GameObject multiSourceGameObject;

	public LayerManager layerManager;

	public bool bSmoothFrames = true;
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

    /**
     * Get the exact matrix at a given world coordinate
     *
     **/
    public Layer GetLayerAt(int posY, int posX)
    {
        //Converts position on the matrix into world position vector
        int xModifier = (512 / 2) * 1;
        int yModifier = (424 / 2) * 1;
        //Debug.Log(posY);

        return depthMatrix.GetLayer(yModifier - posY, xModifier - posX);
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
//		if (Input.GetButtonUp ("Toggle contour line") == true ) {
//			bSmoothFrames = !bSmoothFrames;
//		}
		
			// Get the multi source manager
			multiSourceManager = multiSourceGameObject.GetComponent<MultiSourceManager> ();

			// If we are unable to get the multi source manager, read from the text file
			if (multiSourceManager == null) {
				return;
			}


			ushort[] depthData = multiSourceManager.GetDepthData ();
			DepthMatrix newMatrix = new DepthMatrix (ref depthData, Height, Width, layerManager);

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
