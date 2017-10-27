using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnable : MonoBehaviour {

	public LayerManager layerManager;

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
        return fish;
	}

    private void Update()
    {
        if(Input.GetButtonUp("Reset all") == true)
        {
            Debug.Log("Die!");
            Die();
        }
    }

    private void Die ()
    {
       Destroy(this.gameObject);
    }


}
