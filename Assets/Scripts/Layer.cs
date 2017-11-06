using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Provides public management for Layers - Depth values that map to particular colours
public class Layer {

    public string strName { get; }
	public ushort upperBound;
	public ushort lowerBound;
	public Color color;

    public Layer (string strName, ushort upperBound, ushort lowerBound, Color color)
	{
		this.strName = strName;
		this.upperBound = upperBound;
		this.lowerBound = lowerBound;
		this.color = color;
	}

	public bool WithinBounds (ushort value)
	{
//		Debug.Log ("lowerBound: " + lowerBound + ", upperBound: " + upperBound + ", value: " + value);
//		Debug.Log ((upperBound <= value) && (value <= lowerBound));

		//( (upperBound <= value) && (value <= lowerBound));
		return ( (upperBound <= value) && (value < lowerBound));
	}
}
