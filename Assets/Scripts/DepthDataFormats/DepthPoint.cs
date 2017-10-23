using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthPoint {
    
	public int x;

	public int y;

    // The depth value at this point
	private ushort value;

	public DepthPoint(int xPos, int yPos, ushort value)
	{
		this.x = xPos;
        this.y = yPos;
        this.value = value;

	}

	public void AverageWith(DepthPoint otherPoint)
	{
		value = (ushort)((value + otherPoint.GetValue ()) / (ushort)2);
	}

    public ushort GetValue()
    {
//		Debug.Log ("point get value: " + value);
        return value;
    }

	// Possible extension for easy recursive traversing through DepthMatrix

}
