using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour {
    private Spawnable spawnable;
    private SpawnedObject spawnedObject;
    
    //Determin the max size of the object
    public float MaxSize;

    //Life
    [SerializeField]
    private float life = 100f;
    private float decayRate = 10f;

	// Use this for initialization
	void Start () {
        spawnable = this.GetComponent<Spawnable>();
        spawnedObject = GetComponent<SpawnedObject>();
    }

    public void Reset()
    {
        life = 100f;
    }

    // Update is called once per frame
    void Update()
    {   
        if (spawnable != null)
        {
            if (!spawnable.CheckTerrain())
            {
                life -= decayRate * Time.deltaTime;
            }
            else if (life < 100)
            {
                life += decayRate * Time.deltaTime;
            }

            if (life < 0)
            {
                //Debug.Log("kill plant");
                spawnable.Die();
            }
        }
        if(spawnedObject != null)
        {
            if(!spawnedObject.CheckDepth())
            {
                life -= decayRate * Time.deltaTime;
            }
            else if (life < 100)
            {
                life += decayRate * Time.deltaTime;
            }
            life = Mathf.Clamp(life, 0f, 100f);

            if(life <= 0)
            {
                spawnedObject.Despawn();
            }
        }
        var scale = MaxSize * life / 100f;
        transform.localScale = new Vector3(scale, scale, scale);
    }
    
	void Die()
	{
		Destroy (gameObject);
	}
}
