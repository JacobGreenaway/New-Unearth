using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthPoint {
    
    private int x;

    private int y;

    // The depth value at this point
	private ushort value;

    // The point in the matrix to this point's left
    private DepthPoint left = null;

     // The point in the matrix to this point's left
    private DepthPoint right = null;

     // The point in the matrix to this point's left
    private DepthPoint top = null;

     // The point in the matrix to this point's left
    private DepthPoint bottom = null;

	private DepthPoint[,] matrix;

	public DepthPoint(int x, int y, ushort value, DepthPoint[,] matrix)
	{
        this.x = x;
        this.y = y;
        this.value = value;
        this.matrix = matrix;
	}

    public ushort GetValue()
    {
        return this.value;
    }

    public DepthPoint GetLeft()
    {
        if (this.left != null)
        {
            return this.left;
        }

        if (this.x - 1 >= 0 && matrix[this.x -1, this.y] != null)
        {
            this.left = matrix[this.x - 1, this.y];
            return this.left;
        }

        return null;
    }

}
