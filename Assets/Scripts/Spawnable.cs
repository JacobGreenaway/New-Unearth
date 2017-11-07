using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnable : MonoBehaviour {


    public LayerManager layerManager;

    public bool isPlant;

	public string strLayer;

	public Layer layer;

	public int spawnFrequency;

    public int maxNum;
    public int currentNum;


    void Start()
    {



        currentNum = 0;
    }

    public GameObject Spawn(Vector3 position)
	{
        GameObject fish = Instantiate(gameObject, position, transform.rotation);
        fish.transform.eulerAngles = new Vector3(90, Random.Range(0, 360), 0);
        return fish;
	}

    private void Update()
    {
        CheckVisible();

        if(Input.GetButtonUp("Reset all") == true)
        {
            Debug.Log("Die!");
            Die();
        }
    }

    //Used to 
    private void CheckVisible ()
    {
        GameObject spawnManager = GameObject.Find("SpawnManager");

        SpawnManager manager = spawnManager.GetComponent<SpawnManager>();
        if (isPlant && !manager._togglePlant)
        {
            float x = transform.position.x;
            float z = transform.position.z;
            transform.position = new Vector3(x, -10, z);
        } else if (!isPlant && !manager._toggleAnimal)
        {
            float x = transform.position.x;
            float z = transform.position.z;
            transform.position = new Vector3(x, -10, z);
        } else
        {
            float x = transform.position.x;
            float z = transform.position.z;
            transform.position = new Vector3(x, 1, z);
        }


    }

    private void Die ()
    {
       Destroy(this.gameObject);
    }



}
