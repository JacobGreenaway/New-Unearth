using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour {
    private Spawnable spawn;
    
    //Determin the max size of the object
    public int MaxSize;

    //Life
    private int life = 100;

	// Use this for initialization
	void Start () {
        spawn = this.GetComponent<Spawnable>();
    }

    // Update is called once per frame
    void Update()
    {   
        Grow();
        if (!spawn.CheckTerrain())
        {
            life = life - 10;
        } else if (life < 100)
        {
            life = life + 10;
        }

            if (life < 0)
        {
            //Debug.Log("kill plant");
            spawn.Die();
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
