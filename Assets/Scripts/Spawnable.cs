using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnable : MonoBehaviour {

    //If this is true then don't get killed
    public bool IsOriginal;

    public SpawnManager Manager;

    public LayerManager layerManager;

	public Layer[] arrAcceptedLayers;

    public bool isPlant;

	public string strLayer;

	public int spawnFrequency;

    public int maxNum;
    public int currentNum;

    public int spawnID;

    private DepthFeedManager depthFeed;

    void Start()
    {
        //Link spawn manager to Manager
        GameObject spawnManager = GameObject.Find("SpawnManager");
        Manager = spawnManager.GetComponent<SpawnManager>();

        //Set starting number as one, only effects original
        currentNum = 0;

        //link to DepthManager
        depthFeed = GameObject.Find("DepthFeedManager").GetComponent<DepthFeedManager>();
        
    }

	public GameObject Spawn(Vector3 position)
	{
		//this.positionInMatrix = Matrix;
        //Debug.Log(positionInMatrix.x);
        //Vector3 position = Matrix.position;
        GameObject fish = Instantiate(gameObject, position, transform.rotation);
        fish.transform.eulerAngles = new Vector3(90, Random.Range(0, 360), 0);
        return fish;
	}

    private void Update()
    {
        if (!IsOriginal)
        {
            CheckVisible();
        }

        
        if (Input.GetButtonUp("Reset all") == true)
        {
            //Debug.Log("Die!");
            Die();
        }
    }

    //Used to 
    private void CheckVisible ()
    {
        //

        //

		float x = transform.position.x;
		float z = transform.position.z;

        if (isPlant && !Manager._togglePlant)
        {
            //Hides things under the display plain with y=-1
            transform.position = new Vector3(x, -1, z);
        } else if (!isPlant && !Manager._toggleAnimal)
        {
            //Hides things under the display plain with y=-1
            transform.position = new Vector3(x, -1, z);
        } else
        {
            transform.position = new Vector3(x, 1, z);
        }


    }

    public void Die()
    {
        if (!IsOriginal)
        {
            GameObject origin = Manager.spawnableGameObjects[spawnID];
            origin.GetComponent<Spawnable>().currentNum--;
            Destroy(this.gameObject);
        }

    }

    public bool CheckTerrain()
    {
        if (IsOriginal)
        {
            return true;
        }
        //Debug.Log("Found matrix!");
       // DepthFeedManager depthFeedManager = GameObject.Find("DepthFeedManager").GetComponent<DepthFeedManager>();

        //Layer layer = positionInMatrix.getLayer();

        Layer layer = depthFeed.GetLayerAt((int)this.transform.position.z, (int)this.transform.position.x);
        //Debug.Log(layer.ToString());
        //Debug.Log("Found matrix");
        for (int i = 0; i < arrAcceptedLayers.Length; i++) {
			Layer accLayer = arrAcceptedLayers [i];
			if (accLayer == layer) {
                //Debug.Log(accLayer); 
				return true;
			}
		}
        //Debug.Log("False!");
        return false;
    }


}
