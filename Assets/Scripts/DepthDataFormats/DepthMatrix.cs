using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthMatrix {
    // The 2d array storing the depth points
    public DepthPoint[,] matrix;

    // The layerManager giving depth value layer definition.
	public LayerManager layerManager;

	/**
	 * Constructor
	 * 
	 * @param ushort[] arrDepthData - An array of ushorts representing all the data for a single kinect frame
	 */
	public DepthMatrix(ref ushort[] arrDepthData, int iHeight, int iWidth, LayerManager layerManager)
	{
		this.layerManager = layerManager;
		this.matrix = Make2DArray(ref arrDepthData, iHeight, iWidth);
	}

	/**
	 * @param DepthMatrix otherMatrix - A matrix to average the values of this matrix with
	 */
	public void AverageWith(DepthMatrix otherMatrix)
	{
		for (int height = 0; height < this.matrix.GetLength (0); height++) 
		{
			for (int width = 0; width < this.matrix.GetLength(1); width++)
			{
				DepthPoint currentPoint = this.matrix [height, width];
				DepthPoint otherPoint = otherMatrix.matrix [height, width];
				
				currentPoint.AverageWithOtherPoint (otherPoint);
			}
		}
	}

	/**
	 * @param DepthMatrix otherMatrix - A matrix to average the values of this matrix with
	 */
	public void AverageSmoothPoints()
	{
		for (int height = 0; height < this.matrix.GetLength (0); height++) 
		{
			for (int width = 0; width < this.matrix.GetLength(1); width++)
			{
				DepthPoint currentPoint = this.matrix [height, width];
				currentPoint.AverageWithSurroundingPoints ();
			}
		}
	}

	public List<Vector3> GetAllOnLayer(Layer layer)
	{
		// List to store points that belong to the specified layer
		List<Vector3> pointsInLayer = new List<Vector3> ();

		for (int height = 0; height < this.matrix.GetLength (0); height++) 
		{
			for (int width = 0; width < this.matrix.GetLength(1); width++)
			{
				DepthPoint currentPoint = this.matrix [height, width];

				//Debug.Log (layer);

                //If it has the height of the layer AND inside the camera's view then add point
				if(layer.WithinBounds(currentPoint.GetValue()) && CheckCamera(currentPoint.position))
				{
					pointsInLayer.Add(currentPoint.position);
				}
			}
		}

		return pointsInLayer;
	}

	/*
	 * Takes a 1D array and maps it to a 2D array with the specified height and width
	 * 
	 * @param ushort[] input - the 1D array of depth data
	 * @param int height - the height of the matrix
	 * @param int width - the width of the matrix
	 */
	private DepthPoint[,] Make2DArray(ref ushort[] input, int height, int width)
	{
		DepthPoint[,] output = new DepthPoint[height, width];
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				output[i, j] = new DepthPoint(i, j, (ushort)input[i * width + j], this.layerManager);
			}
		}

		// Set surrounding points for each depth point
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				DepthPoint left = null;
				DepthPoint right = null;
				DepthPoint top = null;
				DepthPoint bottom = null;

				if (0 < i && i < height -1) {
					//Debug.Log (i);
					if (output [i - 1, j] != null) {
						top = output [i - 1, j];
					}
					if (output [i + 1, j] != null) {
						bottom = output [i + 1, j];
					}
				}
				if (0 < j && j < width -1) {
					if (output [i, j - 1] != null) {
						left = output [i, j - 1];
					}
					if (output [i, j + 1] != null) {
						right = output [i, j + 1];
					}
				}

				output [i, j].setSurroundingPoints (left, right, top, bottom);
			}
		}

		return output;
	}


    //Returns true or false depending on if it is in camera view
    private bool CheckCamera(Vector3 position)
    {
        //Get the main camera
        Camera mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();
        Vector3 screenPoint = mainCam.WorldToViewportPoint(position);
        if (screenPoint.y > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y < 1)
        {
            return true;
        }
        return false;

    }

    /*
     * Get the depth value of a given Depthpoint in the matrix
     **/
     public Layer GetLayer(int height, int width)
    {
        return this.matrix[height, width].getLayer();
    }
}
