using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Provides public management for Layers - Depth values that map to particular colours
public class LayerManager : MonoBehaviour {

	// The distance apart of each layer

	public ushort layerGap;

	// Repersents the upper bound of the layers (Smaller Depth is higher)
	public ushort MinDepth;

	//public string[] arrLayerNames;

	//public Color[] arrColours;

	//public ushort[] arrIndividualRanges;

	public Layer[] arrLayers;

	public ushort Max;

	public ushort Min;

	public Dictionary<ushort, Layer> layerMap = new Dictionary<ushort, Layer>();

	public void Update(){

		if (Input.GetButtonUp ("Lift") == true ) {
			float input = Input.GetAxis ("Lift");
			if (input < 0) {
				input = -10f;
			} else {
				input = 10f;
			}

			MinDepth += (ushort)(input);
			SetupLayers ();
		}
	}

	// Create a LayerManager
	public void Start ()
	{
		SetupLayers ();
			
	}


















	public void SetupLayers(){
		if (layerGap == null) 
		{
			layerGap = (ushort)0;
		}

		// Initialise layer array
		//arrLayers = new Layer[arrLayerNames.Length];

		ushort currentUpperBound = (ushort)MinDepth;

		// Create the layers based on the LayerHeight
		for (int layerCount = 0; layerCount < arrLayers.Length; layerCount++) 
		{
			Layer layer = arrLayers [layerCount];

			ushort upperBound = (ushort)(currentUpperBound);
			ushort lowerBound = (ushort) (upperBound + layer.height);

			currentUpperBound = (ushort) (lowerBound + layerGap);

			layer.upperBound = upperBound;
			layer.lowerBound = lowerBound;

			//layer.setUpColorMap ();
		}

		// Map every valid value to its layer for fast access
		for (ushort i = MinDepth; i < arrLayers [arrLayers.Length - 1].lowerBound; i++) {
			layerMap [(ushort)i] = DetermineLayer ((ushort)i);
		}

		Max = arrLayers [arrLayers.Length - 1].lowerBound;
		Min = MinDepth;
	}

	//Returns an single layer
	public Layer GetLayer(string strLayerName)
	{
		if (strLayerName == "") {
			return arrLayers[arrLayers.Length - 1];
		}
		//Debug.Log (arrLayers);
		for (int i = 0; i < arrLayers.Length; i++) 
		{
			Layer layer = arrLayers [i];

			if (layer.strName == strLayerName) {
				return layer;
			}
		}

		return arrLayers[arrLayers.Length - 1];
	}

	// Given a depth value, return the layer that this depth belongs to
	public Layer DetermineLayer(ushort value)
	{
		for (int i = 0; i < arrLayers.Length - 1; i++) 
		{
			Layer layer = arrLayers [i];
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
