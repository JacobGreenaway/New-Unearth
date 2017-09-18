using UnityEngine;
using System.Collections;
using System.IO;
using System;


public class FileReader : MonoBehaviour {

	// Use this for initialization

    public DepthPoint[,] depth;

	//Change ME to make it work on a new machine!!!
	private const string path = "/Users/Jacob/Desktop/NEW UNEARTH/NEW UNEARTH/ushort.txt"; 

	public DepthPoint[,] GetData()
    {
        depth = new DepthPoint[512,424];
        string text = System.IO.File.ReadAllText(path);
		Debug.Log(text);
        string[] lineValues = text.Split(',');
        //Debug.Log(lineValues.Length);
        for (int i = 0; i < 511; i++)
        {
            try
            {
                depth[i,0] = new DepthPoint(i, 0, ushort.Parse(lineValues[i]), depth);
            }
            catch (Exception ex)
            {
                Debug.Log((i - 1) + ": " + lineValues[i - 1]);
                Debug.Log(i + ": " + lineValues[i]);
                break; //throw new Exception();
            }
            // depth[i] = Convert.ToUInt16(lineValues[i]);
        }
		for (int c = 0; c < depth.GetLength(0); c++)
		{
			DepthPoint point = depth [c+1, 0];
			Debug.Log(point.GetLeft());
		}
        return depth;
    }

	public void Start()
	{
		this.GetData();
	}



}

