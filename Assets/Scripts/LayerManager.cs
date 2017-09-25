using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Provides public management for Layers - Depth values that map to particular colours
public class LayerManager : MonoBehaviour {

	// The distance apart of each layer
	public ushort LayerHeight;

	// Repersents the upper bound of the layers (Smaller Depth is higher)
	public ushort MinDepth;

	public string[] arrLayerNames = {"Snow", "Rock", "Grass", "Sand", "Water", "DeepWater" };

	public Color[] arrColours;

	public Layer[] arrLayers;


	// Create a LayerManager
	public void Start ()
	{
		// Initialise layer array
		arrLayers = new Layer[arrLayerNames.Length];

		// Create the layers based on the LayerHeight
		for (int layerCount = 0; layerCount < arrLayerNames.Length; layerCount++) 
		{
			ushort upperBound = (ushort)(MinDepth + (LayerHeight * layerCount));
			ushort lowerBound = (ushort)(upperBound + LayerHeight);

			string name = arrLayerNames [layerCount];

			Layer layer = new Layer (name, upperBound, lowerBound, arrColours[layerCount]);

			arrLayers [layerCount] = layer;
		}

		foreach (Layer layer in arrLayers) {
			Debug.Log (layer.strName);
		}
	}

	// Given a depth value, return the layer that this depth belongs to
	public Layer DetermineLayer(ushort value)
	{
		if (arrLayers.Length == 0) {
			Start ();
		}

		foreach (Layer layer in arrLayers) 
		{
			if (layer.WithinBounds(value))
			{
				Debug.Log ("Layer is: " + layer.strName);
				return layer; 
			}
		}

		return arrLayers[0];
	}
}
