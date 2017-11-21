using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour {


    //Determin the max size of the object
    public int MaxSize;

	// Use this for initialization
	void Start () {
        
    }

    // Update is called once per frame
    void Update()
    {
        Spawnable s = this.GetComponent<Spawnable>();
        Grow();
        if (s.CheckTerrain())
        {
            s.Die();
        }

    }

    void Grow()
    {
        if (transform.localScale.x*400 < MaxSize)
        {
            transform.localScale += new Vector3(0.0001F, 0.0001F, 0);
        }

    }

	void Shrink()
	{
		if (transform.localScale.x == 0) {
			
		}
		else {
			
			transform.localScale -= new Vector3(0.0005F, 0.0005F, 0);
		}
	}

	void Die()
	{
		Destroy (gameObject);
	}


}
