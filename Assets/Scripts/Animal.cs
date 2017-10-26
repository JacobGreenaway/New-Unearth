using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour {
    //Determines whether an animal can move in Land or water
    public bool landAble;
    public bool waterAble;

    //The current direction the animal is engaged in (1, 2, 3, 4, 5, 6, 7, 8 With 5 been standing still)
    private byte _direction;

    //The amount of time that lapses before it changes direction
    public int restless;

	// Use this for initialization
	void Start () {
        _direction = 1;
    }
	
	// Update is called once per frame
	void Update () {
        Move();
	}

    private void Move ()
    {
        switch (_direction)
        {
            case 1:
                transform.Translate(-1, 0, -1);
                break;
            case 2:
                transform.Translate(1, 0, -1);
                break;
            case 3:
                transform.Translate(1, 0, -1);
                break;
            case 4:
                transform.Translate(-1, 0, 0);
                break;
            case 5:
                //Stand still
                break;
            case 6:
                transform.Translate(1, 0, 0);
                break;
            case 7:
                transform.Translate(-1, 0, 1);
                break;
            case 8:
                transform.Translate(0, 0, 1);
                break;
            case 9:
                transform.Translate(1, 0, 1);
                break;
        }
        
    }
}
