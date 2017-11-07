using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthPoint {
    
	public int x;

	public int y;

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
