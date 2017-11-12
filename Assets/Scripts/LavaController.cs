using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaController : MonoBehaviour {

	public LayerManager layerManager;

	public Layer lavaLayer;
	Layer currentLayer;

	int frameCount = 0;
	int layerCount = 0;
	bool bActivated = false;

	public Layer[] arrAffectedLayers;
	public Dictionary<Layer, ushort> origHeight = new Dictionary<Layer, ushort> ();

	// Use this for initialization
	void Start () {

		// Set the final count
		for (int i = 0; i < arrAffectedLayers.Length; i++) {
			Layer layer = arrAffectedLayers [i];

			origHeight [layer] = layer.height;
		}
	}
	
	// Update is called once per frame
	void Update () {

		//Input detection
		if (Input.GetButtonUp ("Activate Lava") == true ) {
			bActivated = true;
		}

		if (bActivated) {
			flowLavaOverLayer (arrAffectedLayers [layerCount]);

			if (arrAffectedLayers [0].height == 0) {
				flowLavaOverLayer (arrAffectedLayers [1]);

				if (arrAffectedLayers [1].height == 0) {
					flowLavaOverLayer (arrAffectedLayers [2]);

					if (arrAffectedLayers [2].height == 0) {
						flowLavaOverLayer (arrAffectedLayers [3]);

						if (arrAffectedLayers [3].height == 0) {

							for (int i = 0; i < arrAffectedLayers.Length; i++) {
								Layer layer = arrAffectedLayers [i];
								layer.height = origHeight [layer];
								lavaLayer.height = 30;

							}

							layerManager.SetupLayers ();
							bActivated = false;
						}
					}
				}
			}
		}
	}

	void flowLavaOverLayer(Layer layer) {

		frameCount++;

		if (frameCount <= layer.height) {
			layer.height--;
			lavaLayer.height++;
			layerManager.SetupLayers ();
		} else {
			frameCount = 0;
		}
	}
}
