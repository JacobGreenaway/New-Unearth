using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Provide a game wide teacher control via the keyboard
// 1 - Toggle contour lines
// 2 - Toggle animal spawns
// 3 - Toggle plant spawns
public class GameController : MonoBehaviour {

    //Moving vector
    private Vector3 offset;

    //Camera move speed
    private const int _Speed = 10;

    //Camera zoom speed
    private const int _Zoom = 30;

    // Use this for initialization
    void Start () {
        transform.position = SettingsController.Instance.Current.CamPos;
    }
	
	// Update is called once per frame
	void Update () {
        //Input for moving the camera left right up down
        var x = Input.GetAxis("Vertical") * Time.deltaTime * _Speed;
        var y = Input.GetAxis("Horizontal") * Time.deltaTime * _Speed;
        transform.Translate(0, x, 0);
        transform.Translate(y, 0, 0);

        //Input for zooming the camera
        var z = Input.GetAxis("Zoom") * Time.deltaTime * _Zoom;
        transform.Translate(0, 0, z);

        if(x > 0f || y > 0f || z > 0f)
        {
            SettingsController.Instance.Current.CamPos = transform.position;
        }
    }
}
