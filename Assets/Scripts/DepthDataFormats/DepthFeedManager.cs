using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Collections.Generic;

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
    private Queue<DepthMatrix> depthMatrixPool = new Queue<DepthMatrix>();
    private MultiSourceManager multiSourceManager;

    private ushort[] arrDepth;

    //Running list of positions in a given layer (must redo)
    private List<Vector3> forestPoints;
    private List<Vector3> grassPoints;
    private List<Vector3> sandPoints;
    private List<Vector3> deepSeaPoints;

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


    void Start()
    {
        _Sensor = KinectSensor.GetDefault();


        if (_Sensor != null)
        {
            Debug.Log("Kinect sensor found");
            arrDepth = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];
        }


        smoother = new DepthSmoother(iSmoothFrameRate);

        InvokeRepeating("updateLayerForestPoints", 4.0f, 4f);
        InvokeRepeating("updateLayerGrassPoints", 5f, 4f);
        InvokeRepeating("updateLayerSandPoints", 6f, 4f);
        InvokeRepeating("updateLayerWaterPoints", 7f, 4f);

    }

    void Update()
    {
        //		if (Input.GetButtonUp ("Toggle contour line") == true ) {
        //			bSmoothFrames = !bSmoothFrames;
        //		}

        // Get the multi source manager
        if (multiSourceManager == null)
        {
            multiSourceManager = multiSourceGameObject.GetComponent<MultiSourceManager>();
        }

        // If we are unable to get the multi source manager, read from the text file
        if (multiSourceManager == null)
        {
            return;
        }

        ushort[] depthData = multiSourceManager.GetDepthData();

        if (depthMatrix != null)
        {
            depthMatrixPool.Enqueue(depthMatrix);
        }


        DepthMatrix newMatrix = null;
        if (depthMatrixPool.Count > 0)
        {
            newMatrix = depthMatrixPool.Dequeue();
            newMatrix.SetNewData(ref depthData, Height, Width, layerManager);
        }
        else
        {
            newMatrix = new DepthMatrix(ref depthData, Height, Width, layerManager);
        }

        // If we are smoothing the depth frames
        if (bSmoothFrames)
        {
            smoother.AddDepth(newMatrix);
            depthMatrix = smoother.GetSmoothDepthMatrix();
            return;
        }

        // Create a new DepthMatrix from the depth data
        depthMatrix = newMatrix;

        //After depthMatrix is completed updated, DepthFeedManager will now update list of points on each layer
        //updateLayerPoints();

    }

    //Updates running list of positions (vector 3)
    private void updateLayerForestPoints()
    {
        forestPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Forest"));
        //grassPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Grass"));
        //sandPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Sand"));
        //deepSeaPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Deep Water"));

    }

    //Updates running list of positions (vector 3)
    private void updateLayerGrassPoints()
    {
        //forestPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Forest"));
        grassPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Grass"));
        //sandPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Sand"));
        //deepSeaPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Deep Water"));

    }

    //Updates running list of positions (vector 3)
    private void updateLayerSandPoints()
    {
        //forestPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Forest"));
        //grassPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Grass"));
        sandPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Sand"));
        //deepSeaPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Deep Water"));

    }

    //Updates running list of positions (vector 3)
    private void updateLayerWaterPoints()
    {
        //forestPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Forest"));
        //grassPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Grass"));
        //sandPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Sand"));
        deepSeaPoints = depthMatrix.GetAllOnLayer(layerManager.GetLayer("Deep Water"));

    }

    public int Width
    {
        get { return _Sensor.DepthFrameSource.FrameDescription.Width; }
    }

    public int Height
    {
        get { return _Sensor.DepthFrameSource.FrameDescription.Height; }
    }

    public Vector3 GetPointOnLayer(string layer)
    {
        if (depthMatrix == null)
        {
            //signal null return
            return new Vector3(-66, -66, -66);
        }
        // Select a random point from the depth matrix which is inside the desired layer

        if (layer == "Forest")
        {
            if (forestPoints == null || forestPoints.Count == 0)
            {
                return new Vector3(-66, -66, -66);
            }
            //Then get a point from forest
            int length = forestPoints.Count;
            int randomIndex = Random.Range(0, length - 1);
            return forestPoints[randomIndex];
        }
        if (layer == "Grass")
        {
            if (grassPoints == null || grassPoints.Count == 0)
            {
                return new Vector3(-66, -66, -66);
            }
            //Then get a point from forest
            int length = grassPoints.Count;
            int randomIndex = Random.Range(0, length - 1);
            return grassPoints[randomIndex];
        }
        if (layer == "Sand")
        {
            if (sandPoints == null || sandPoints.Count == 0)
            {
                return new Vector3(-66, -66, -66);
            }
            //Then get a point from forest
            int length = sandPoints.Count;
            int randomIndex = Random.Range(0, length - 1);
            return sandPoints[randomIndex];
        }
        if (layer == "Deep Water")
        {
            if (deepSeaPoints == null || deepSeaPoints.Count == 0)
            {
                return new Vector3(-66, -66, -66);
            }
            //Then get a point from forest
            int length = deepSeaPoints.Count;
            int randomIndex = Random.Range(0, length - 1);
            return deepSeaPoints[randomIndex];
        }

        //signals null return
        return new Vector3(-66, -66, -66);
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
