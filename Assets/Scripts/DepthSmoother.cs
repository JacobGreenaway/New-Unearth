using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthSmoother {

	private Queue<DepthMatrix> depthQueue;

	public int frameCount;

	/**
	 * Class used for smoothing out depth data.
	 * 
	 * @param int frameCount - The number of frames that the DepthSmoother should be performing smoothing on.
	 */
	public DepthSmoother(int frameCount)
	{
		this.frameCount = frameCount;
	}

	/**
	 * Add a DepthMatrix to the DepthSmoother Queue.
	 */
	public void AddDepth (DepthMatrix depthMatrix)
	{
		depthQueue.Enqueue (depthMatrix);
	}

	/**
	 * If the DepthSmoother has enough data frames, it takes an average of the frames it has and returns a smoothed DepthMatrix.
	 * 
	 * @returns Null if (this.depthQueue.Count < this.frameCount)
	 */
	public DepthMatrix GetSmoothDepthMatrix()
	{
		if (this.depthQueue.Count >= this.frameCount) 
		{
			DepthMatrix initialMatrix = this.depthQueue.Dequeue ();

			foreach (DepthMatrix matrix in this.depthQueue) 
			{
				initialMatrix.AverageWith (matrix);
			}
			return initialMatrix;
		} 

		else 
		{
			return null;
		}
	}
}
