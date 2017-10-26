using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnable : MonoBehaviour {

	public LayerManager layerManager;

	public string strLayer;

	public Layer layer;

	public int spawnFrequency;


	public GameObject Spawn(Vector3 position)
	{
		GameObject fish = Instantiate (gameObject, position, transform.rotation);
		//Debug.Log (fish);
		return fish;
	}

    private void Update()
    {
        if(Input.GetAxis("Reset all") != 0)
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
