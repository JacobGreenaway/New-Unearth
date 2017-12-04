using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour {
    //Determines whether an animal can move in Land or water
    public bool landAble;
    public bool waterAble;

    [SerializeField]
    private float speed = 0.5f;

    //The current direction the animal is engaged in (1, 2, 3, 4, 5, 6, 7, 8 With 5 been standing still)
    private int _direction;

    //The amount of time that lapses before it changes direction
    public int restless;

    private SimpleTimer timer;

    private Spawnable spawnable;
    private SpawnedObject spawnedObject;

	// Use this for initialization
	void Start () {
        spawnable = GetComponent<Spawnable>();
        spawnedObject = GetComponent<SpawnedObject>();
        timer = GetComponent<SimpleTimer>();
        //timer.targetTime = 
        _direction = 1;
    }


	
	// Update is called once per frame
	void Update () {
        if (timer.Count() == 0f)
        {
            ChangeDirection();
        }


        if (spawnable != null &&  spawnable.CheckTerrain()) {
            Move();
        }

        if(spawnedObject != null && spawnedObject.CheckDepth())
        {
            Move();
        }
        
    }

    private void ChangeDirection()
    {
        _direction = Random.Range(1, 10);

        //Debug.Log(_direction);
    }

    private void Move ()
    {
        Vector3 dir = new Vector3(0f, 0f, 1f);
        switch (_direction)
        {
            case 1:
                //transform.Translate(-1, 0, -1, Space.World);
                dir = Quaternion.AngleAxis(225f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, -135, 0);
                break;
            case 2:
                //transform.Translate(0, 0, -1, Space.World);
                dir = Quaternion.AngleAxis(270f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, 180, 0);
                break;
            case 3:
                //transform.Translate(1, 0, -1, Space.World);
                dir = Quaternion.AngleAxis(135f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, -225, 0);
                break;
            case 4:
                //transform.Translate(-1, 0, 0, Space.World);
                dir = Quaternion.AngleAxis(270f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, -90, 0);
                break;
            case 5:
                //Stand still
                dir = Vector3.zero;
                break;
            case 6:
                //transform.Translate(1, 0, 0, Space.World);
                dir = Quaternion.AngleAxis(90f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, 90, 0);
                break;
            case 7:
                //transform.Translate(-1, 0, 1, Space.World);
                dir = Quaternion.AngleAxis(315f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, -45, 0);
                break;
            case 8:
                //transform.Translate(0, 0, 1, Space.World);
                // This is our default dir vector
                transform.eulerAngles = new Vector3(90, 0, 0);
                break;
            case 9:
                //transform.Translate(1, 0, 1, Space.World);
                dir = Quaternion.AngleAxis(45f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, 45, 0);
                break;
        }
        transform.position += dir * Time.deltaTime;
    }
}
