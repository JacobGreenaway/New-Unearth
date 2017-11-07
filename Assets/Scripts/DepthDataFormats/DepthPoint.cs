using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthPoint {
    
	public int x;
	public int y;


	public DepthPoint left;
	public DepthPoint right;
	public DepthPoint top;
	public DepthPoint bottom;

    public Vector3 position;


    // The depth value at this point
	private ushort value;

	public DepthPoint(int xPos, int yPos, ushort value)
	{
		this.x = xPos;
        this.y = yPos;
        this.value = value;


        //Converts position on the matrix into world position vector
        int posX = y;
        int posZ = x;
        int xModifier = (512 / 2) * 1;
        int zModifier = (424 / 2) * 1;
        this.position = new Vector3(xModifier - posX, -10, zModifier - posZ);
    }

	public void AverageWithSurroundingPoints()
	{
		if (this.left != null) {
			this.AverageWithOtherPoint (this.left);
		}

		if (this.right != null) {
			this.AverageWithOtherPoint (this.right);
		}

		if (this.top != null) {
			this.AverageWithOtherPoint (this.top);
		}

		if (this.bottom != null) {
			this.AverageWithOtherPoint (this.bottom);
		}
	}

	public void AverageWithOtherPoint(DepthPoint otherPoint)
	{
		ushort otherValue = otherPoint.GetValue ();

//		if (otherValue > this.value) {
//			this.value = (ushort)Mathf.Min (this.value + (ushort)100, otherValue);
//		}
//		if (otherValue < this.value) {
//			this.value = (ushort)Mathf.Max (this.value - (ushort)100, otherValue);
//		}
		if (otherValue == 0) {
			return;
		} else if (this.value == 0) {
			this.value = otherValue;
			return;
		}
					
		this.value = (ushort)((this.value + otherValue) / (ushort)2);
	}

    public ushort GetValue()
    {
//		Debug.Log ("point get value: " + value);
        return this.value;
    }

	public void setSurroundingPoints(DepthPoint left, DepthPoint right, DepthPoint top, DepthPoint bottom) {
		this.left = left;
		this.right = right;
		this.top = top;
		this.bottom = bottom;
	}

	// Possible extension for easy recursive traversing through DepthMatrix

}
