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
    private const int _Zoom = 10;

    [SerializeField]
    private Camera m_Cam;

    // Use this for initialization
    void Start () {
        transform.position = SettingsController.Instance.Current.CamPos;
        UpdateClipValues();
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

        if(Mathf.Abs(x) > 0f || Mathf.Abs(y) > 0f || Mathf.Abs(z) > 0f)
        {
            SettingsController.Instance.Current.CamPos = transform.position;
            UpdateClipValues();
        }
    }

    private void UpdateClipValues()
    {
        // Recalcuate clipping
        // Bottom Left
        var bl = m_Cam.ViewportToWorldPoint(new Vector3(0f, 0f, transform.position.y));
        // Top Right
        var tr = m_Cam.ViewportToWorldPoint(new Vector3(1f, 1f, transform.position.y));

        var settings = SettingsController.Instance.Current;
        settings.ClipBottom = bl.z;
        settings.ClipTop = tr.z;
        settings.ClipLeft = bl.x;
        settings.ClipRight = tr.x;
    }
}
