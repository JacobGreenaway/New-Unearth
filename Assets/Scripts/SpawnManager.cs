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
            //StartSpawning();
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
				
                // Can get the layer for a depthpoint by calling 'getLayer()'
                //Debug.Log(depthPoint.getLayer().strName);
                
                if (spawnable.maxNum > spawnable.currentNum)
                {
                    //DepthPoint depthPoint = GetDepthPointInLayer(currentDepthMatrix, layerManager.GetLayer(spawnable.strLayer));


                    //Get a random position in a given layer 3 for forest, 2 for grass, 1 for sand, 0 for deep water


                    spawnable.currentNum++;
                    //spawnable.Manager = this.GetComponent<SpawnManager>();
                    spawnable.spawnID = i;
                    Vector3 position = depthFeed.GetPointOnLayer(spawnable.strLayer);
                    if (position != new Vector3(-66, -66, -66))
                    {
                        GameObject fish = spawnable.Spawn(position);
                        fish.GetComponent<Spawnable>().IsOriginal = false;
                        fish.transform.SetParent(spawnObjectParent.transform);
                    }
                    
                    // Manually setting the position in the matrix before spawning - should be able to control position from within the spawnable after that
                    // DepthPoints have references to left right top bottom points - can refreence these for positions 
                    
                }
                else
                {
                    //Debug.Log("Max reached");

                }

            }

        }
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
