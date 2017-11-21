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

	public DepthPoint positionInMatrix;

    public int spawnID;


    void Start()
    {
        currentNum = 0;
    }

	public GameObject Spawn(DepthPoint positionInMatrix)
	{
		this.positionInMatrix = positionInMatrix;
		Vector3 position = positionInMatrix.position;
        GameObject fish = Instantiate(gameObject, position, transform.rotation);
        fish.transform.eulerAngles = new Vector3(90, Random.Range(0, 360), 0);
        return fish;
	}

    private void Update()
    {
        CheckVisible();
        CheckTerrain();

        if(Input.GetButtonUp("Reset all") == true)
        {
            //Debug.Log("Die!");
            Die();
        }
    }

    //Used to 
    private void CheckVisible ()
    {
        //GameObject spawnManager = GameObject.Find("SpawnManager");

        //SpawnManager manager = Manager;

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
		// Do not need to check the position if we have not spawned yet
		if (positionInMatrix == null) {
			return false;
		}
			
		Layer layer = positionInMatrix.getLayer();

		for (int i = 0; i < arrAcceptedLayers.Length; i++) {
			Layer accLayer = arrAcceptedLayers [i];
			if (accLayer == layer) {
				
				return true;
			}
		}
		return false;
    }


}
