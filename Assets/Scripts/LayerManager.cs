using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Provides public management for Layers - Depth values that map to particular colours
public class LayerManager : MonoBehaviour {

	// The distance apart of each layer
	public ushort LayerHeight;

	public ushort layerGap;

	// Repersents the upper bound of the layers (Smaller Depth is higher)
	public ushort MinDepth;

	public string[] arrLayerNames;

	public Color[] arrColours;

	public Layer[] arrLayers;

	public ushort Max;

	public ushort Min;

	public Dictionary<ushort, Layer> layerMap = new Dictionary<ushort, Layer>();


	// Create a LayerManager
	public void Start ()
	{
		if (layerGap == null) 
		{
			layerGap = (ushort)0;
		}

		// Initialise layer array
		arrLayers = new Layer[arrLayerNames.Length];

		// Create the layers based on the LayerHeight
		for (int layerCount = 0; layerCount < arrLayerNames.Length; layerCount++) 
		{
			ushort upperBound = layerCount == 0 ? (ushort)(MinDepth) : (ushort) (MinDepth + ((LayerHeight + layerGap) * layerCount));
			ushort lowerBound = (ushort)(upperBound + LayerHeight);

			string name = arrLayerNames [layerCount];

			Layer layer = new Layer (name, upperBound, lowerBound, arrColours[layerCount]);

			arrLayers [layerCount] = layer;
		}

		// Map every valid value to its layer for fast access
		for (ushort i = MinDepth; i < arrLayers [arrLayers.Length - 1].lowerBound; i++) {
			layerMap [(ushort)i] = DetermineLayer ((ushort)i);
		}

		Max = arrLayers [arrLayers.Length - 1].lowerBound;
		Debug.Log("Max is" + Max);
		Min = MinDepth;
		Debug.Log("Min is" + Min);
			
	}

	//
	public Layer GetLayer(string strLayerName)
	{
		Debug.Log (arrLayers);
		for (int i = 0; i < arrLayers.Length; i++) 
		{
			Layer layer = arrLayers [i];

			if (layer.strName == strLayerName) {
				return layer;
			}
		}

		return null;
	}

	// Given a depth value, return the layer that this depth belongs to
	public Layer DetermineLayer(ushort value)
	{
		foreach (Layer layer in arrLayers) 
		{
			if (layer.WithinBounds(value))
			{
//				Debug.Log ("Layer is: " + layer.strName);
				return layer; 
			}
		}
		return arrLayers[arrLayers.Length - 1];
//		Debug.Log("Returning default layer: " + arrLayers[0].strName);
	}
}
