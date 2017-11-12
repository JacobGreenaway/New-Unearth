using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Provides public management for Layers - Depth values that map to particular colours
public class Layer : MonoBehaviour{

	public string strName;

	public ushort height;

	public ushort upperBound;
	public ushort lowerBound;

	public Color upperColor;
	public Color lowerColor;

	public Dictionary<ushort, float[]> gradientColorMap;

//	public void setUpColorMap()
//	{
//		for (ushort i = upperBound; i < lowerBound; i++) {
//			gradientColorMap [(ushort)i] = getGradientColorForValue ((ushort)i);
//		}
//	}
//
	public bool WithinBounds (ushort value)
	{
		return ( (upperBound <= value) && (value < lowerBound));
	}

	public float[] getGradientColorForValue(ushort value) {

		if (!WithinBounds (value)) {
			return new float[] {0, 0, 0, 0};
		}

		float uR = upperColor.r;
		float uG = upperColor.g;
		float uB = upperColor.b;
		float uA = upperColor.a;

		float lR = lowerColor.r;
		float lG = lowerColor.g;
		float lB = lowerColor.b;
		float lA = lowerColor.a;

		ushort range = (ushort)(lowerBound - upperBound);
		float increment = (range / 100f);
		ushort span = (ushort)(value - upperBound);
		float percentageFilled = (((range - span) / increment) / 100f);

		float newR = lR - gradientValue (uR, lR, percentageFilled);
		float newG = lG - gradientValue (uG, lG, percentageFilled);
		float newB = lB - gradientValue (uB, lB, percentageFilled);
		float newA = lA - gradientValue (uA, lA, percentageFilled);

		return new float[] {newR, newG, newB, newA};

	}

	float gradientValue(float upper, float lower, float percentage) {
		float range = lower - upper;

		return range * percentage;
	}
}
