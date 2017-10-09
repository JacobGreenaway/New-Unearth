using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;


public class FileReader : MonoBehaviour {

	// Use this for initialization

    public DepthMatrix depthMatrix = null;

//	public GameObject prefab;

	//Change ME to make it work on a new machine!!!
	private const string path = "C:\\Users\\jacob\\Documents\\GitHub\\New-Unearth\\ushort.txt"; 

	public DepthMatrix GetData()
    {
		if (depthMatrix != null) {
			return depthMatrix;
		}

		string text = "";
		try {
			text = System.IO.File.ReadAllText(path);
		}
		catch (Exception e) {
			Debug.Log ("broke reading file");
			return null;
		}
        
		//Debug.Log(text);
        string[] lineValues = text.Split(',');
		List<ushort> depthData = new List<ushort>();

		foreach (string value in lineValues) {
			depthData.Add(ushort.Parse(value));
		}

		depthMatrix = new DepthMatrix(depthData.ToArray(), 424, 512);


		return depthMatrix;
    }
		



}

