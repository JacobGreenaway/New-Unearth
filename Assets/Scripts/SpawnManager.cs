using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {

	// DepthFeed to use for spawning position
	public DepthFeedManager depthFeed;

	public GameObject spawnObjectParent;

	// Array of spawnable GameObjects
	public GameObject[] spawnableGameObjects;

	public LayerManager layerManager;

	private Spawnable[] spawnables;

	private DepthMatrix currentDepthMatrix;

	// Use this for initialization
	void Start () {

		spawnables = new Spawnable[spawnableGameObjects.Length];

		for (int i = 0; i < spawnableGameObjects.Length; i++) {
			GameObject spawnobj = spawnableGameObjects [i];

			Spawnable spawnable = spawnobj.GetComponent<Spawnable> ();
			spawnables [i] = spawnable;
		}
	}
	
	// Update is called once per frame
	void Update () {

		// Get the latest depth data
		currentDepthMatrix = depthFeed.GetDepthMatrix();

		for (int i = 0; i < spawnables.Length; i++)
		{
			Spawnable spawnable = spawnables [i];

			if (SpawnChance (spawnable.spawnFrequency)) {
				DepthPoint depthPoint = GetDepthPointInLayer (currentDepthMatrix, layerManager.GetLayer(spawnable.strLayer));

				Vector3 position = ConvertDepthPointToVector (depthPoint);
				Debug.Log("spawning at position");
				GameObject fish = spawnable.Spawn (position);
				fish.transform.parent = spawnObjectParent.transform;
			}

		}
	}

	DepthPoint GetDepthPointInLayer(DepthMatrix depthMatrix, Layer layer)
	{
		// Select a random point from the depth matrix which is inside the desired layer
		List<DepthPoint> depthPointsOnLayer = depthMatrix.GetAllOnLayer(layer);

		int length = depthPointsOnLayer.Count;
		int randomIndex = Random.Range (0, length - 1);

		return depthPointsOnLayer [randomIndex];
	}

	Vector3 ConvertDepthPointToVector(DepthPoint depthPoint)
	{
		int x = depthPoint.y;
		int z = depthPoint.x;

		int xModifier = (512 / 2) * -1;

		Vector3 position = new Vector3 (x + xModifier, 1, -z);

		return position;
	}

	bool SpawnChance(int spawnFrequency)
	{
		int rand = Random.Range (0, 100);
		return rand < spawnFrequency;
	}
}
