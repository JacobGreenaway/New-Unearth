using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthPoint {
    
	private int x { get; }

	private int y { get; }

    // The depth value at this point
	private ushort value;

	public DepthPoint(int x, int y, ushort value)
	{
        this.x = x;
        this.y = y;
        this.value = value;

	}

	public void AverageWith(DepthPoint otherPoint)
	{
		this.value = (ushort)((this.value + otherPoint.GetValue ()) / 2);
	}

    public ushort GetValue()
    {
		Debug.Log ("point get value: " + value);
        return value;
    }

	// Possible extension for easy recursive traversing through DepthMatrix

}
