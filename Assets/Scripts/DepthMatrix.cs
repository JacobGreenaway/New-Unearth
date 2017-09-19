using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthMatrix {

	// The 2d array storing the depth points
	public DepthPoint[,] m_matrix;

	/**
	 * Constructor
	 * 
	 * @param ushort[] arrDepthData - An array of ushorts representing all the data for a single kinect frame
	 */
	public DepthMatrix(ushort[] arrDepthData)
	{
		this.m_matrix = new DepthPoint[512, 424];

		int row = 0;
		int column = 0;

		for (int i = 0; i < arrDepthData.Length - 1; i++, column++) 
		{
			DepthPoint currentDepth = new DepthPoint (row, column, arrDepthData [i]);
			this.m_matrix [row, column] = currentDepth;

			if (i % (512 - 1) == 0) 
			{
				row++;
			}

			if (column == 424 - 1) 
			{
				column = 0;
			}
		}
	}

	/**
	 * @param DepthMatrix otherMatrix - A matrix to average the values of this matrix with
	 */
	public void AverageWith(DepthMatrix otherMatrix)
	{
		for (int row = 0; row < this.m_matrix.GetLength (0); row++) 
		{
			for (int column = 0; column < this.m_matrix.GetLength(1); column++)
			{
				DepthPoint currentPoint = this.m_matrix [row, column];
				DepthPoint otherPoint = otherMatrix.m_matrix [row, column];
				
				currentPoint.AverageWith (otherPoint);
			}
		}
	}
}
