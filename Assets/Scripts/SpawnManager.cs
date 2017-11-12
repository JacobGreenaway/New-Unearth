using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {

    // DepthFeed to use for spawning position
    public DepthFeedManager depthFeed;

	public GameObject spawnObjectParent;

	// Array of spawnable GameObjects
	public GameObject[] spawnableGameObjects;

	public LayerManager contourManager;

	public LayerManager layerManager;

	private Spawnable[] spawnables;

	private DepthMatrix currentDepthMatrix;

    public bool _toggleAnimal;
    public bool _togglePlant;

	// Use this for initialization
	void Start () {
        _toggleAnimal = true;
        _togglePlant = true;

        StartSpawning();
	}
	
	// Update is called once per frame
	void Update () {
        //Check to see if animals and plant modes have been toggled
        if (Input.GetButtonUp("Toggle plants") == true)
        {
            //Debug.Log("Plants Toggled!");
            _togglePlant = !_togglePlant;
        }
        else if (Input.GetButtonUp("Toggle animals") == true)
        {
            //Debug.Log("Animals Toggled");
            _toggleAnimal = !_toggleAnimal;
        }

        if (Input.GetButtonUp("Reset all") == true)
        {
            StartSpawning();
        }

        SpawnAll();
	}

    void SpawnAll ()
    {
        // Get the latest depth data
        currentDepthMatrix = depthFeed.GetDepthMatrix();
		if (currentDepthMatrix == null) {
			return;
		}

        for (int i = 0; i < spawnables.Length; i++)
        {
            Spawnable spawnable = spawnables[i];

            if (SpawnChance(spawnable.spawnFrequency))
			{
				DepthPoint depthPoint = GetDepthPointInLayer(currentDepthMatrix, layerManager.GetLayer(spawnable.strLayer));

				// Manually setting the position in the matrix before spawning - should be able to control position from within the spawnable after that
				// DepthPoints have references to left right top bottom points - can refreence these for positions 
				spawnable.positionInMatrix = depthPoint;

				// Can get the layer for a depthpoint by calling 'getLayer()'
				//Debug.Log(depthPoint.getLayer().strName);

                if (spawnable.maxNum > spawnable.currentNum)
                {
                    spawnable.currentNum++;
					GameObject fish = spawnable.Spawn(depthPoint);
                    fish.transform.SetParent(spawnObjectParent.transform);
                }
                else
                {
                    //Debug.Log("Max reached");

                }

            }

        }
    }

    // Select a random point from the depth matrix which is inside the desired layer
    DepthPoint GetDepthPointInLayer(DepthMatrix depthMatrix, Layer layer)
	{

		if (depthMatrix == null) {
			return null;
		}
		// Select a random point from the depth matrix which is inside the desired layer

		List<DepthPoint> depthPointsOnLayer = depthMatrix.GetAllOnLayer(layer);

		int length = depthPointsOnLayer.Count;
		int randomIndex = Random.Range (0, length - 1);

		return depthPointsOnLayer [randomIndex];
	}

	bool SpawnChance(int spawnFrequency)
	{
		int rand = Random.Range (0, 1000);
		return rand < spawnFrequency;
	}

    //Resets the current number of spawnables to start spawning again
    void StartSpawning()
    {
        spawnables = new Spawnable[spawnableGameObjects.Length];

        for (int i = 0; i < spawnableGameObjects.Length; i++)
        {
            GameObject spawnobj = spawnableGameObjects[i];
            Spawnable spawnable = spawnobj.GetComponent<Spawnable>();
            spawnables[i] = spawnable;
            spawnables[i].currentNum = 0;
        }
    }
}
