using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour {
    //1 to 40 works best
    public int speed;

    public float targetScale = 20f;
    public float expandSpeed = 0.1f;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(transform.position.y < 0)
        {
            transform.Translate(Vector3.up * Time.deltaTime * speed, Space.World);
        } else if (transform.localScale.x < 10)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(targetScale, targetScale, targetScale), Time.deltaTime * expandSpeed);
        } else
        {
            //Destroy(this.gameObject);
        }

    }
}
