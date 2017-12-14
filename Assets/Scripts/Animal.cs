using UnityEngine;

/// <summary>
/// Controls and animates a sprite based animal
/// </summary>
public class Animal : MonoBehaviour
{
    // Speed the animal moves at
    [SerializeField]
    private float speed = 0.5f;

    // The current direction the animal is engaged in (1, 2, 3, 4, 5, 6, 7, 8 With 5 been standing still)
    private int _direction;

    // The amount of time that lapses before it changes direction
    public int restless;

    private SimpleTimer timer;

    private SpawnedObject spawnedObject;
    
    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {
        spawnedObject = GetComponent<SpawnedObject>();
        timer = GetComponent<SimpleTimer>();
        //timer.targetTime = 
        _direction = 1;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // If the timer has finised, change direction
        if (timer.Count() == 0f)
        {
            ChangeDirection();
        }

        // If we're on a valid layer
        if (spawnedObject != null && spawnedObject.CheckDepth())
        {
            Move();
        }
    }

    // Changes the direction the animal is moving.
    private void ChangeDirection()
    {
        _direction = Random.Range(1, 10);
    }

    /// <summary>
    /// Moves and orients the animal
    /// </summary>
    private void Move()
    {
        Vector3 dir = new Vector3(0f, 0f, 1f);
        switch (_direction)
        {
            case 1:
                dir = Quaternion.AngleAxis(225f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, -135, 0);
                break;
            case 2:
                dir = Quaternion.AngleAxis(270f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, 180, 0);
                break;
            case 3:
                dir = Quaternion.AngleAxis(135f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, -225, 0);
                break;
            case 4:
                dir = Quaternion.AngleAxis(270f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, -90, 0);
                break;
            case 5:
                //Stand still
                dir = Vector3.zero;
                break;
            case 6:
                dir = Quaternion.AngleAxis(90f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, 90, 0);
                break;
            case 7:
                dir = Quaternion.AngleAxis(315f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, -45, 0);
                break;
            case 8:
                // This is our default dir vector
                transform.eulerAngles = new Vector3(90, 0, 0);
                break;
            case 9:
                dir = Quaternion.AngleAxis(45f, Vector3.up) * dir;
                transform.eulerAngles = new Vector3(90, 45, 0);
                break;
        }
        transform.position += dir * Time.deltaTime * speed;
    }
}
