﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;


public class FileReader : MonoBehaviour {

	// Use this for initialization

    public DepthMatrix depthMatrix;

	//Change ME to make it work on a new machine!!!
	private const string path = "C:\\Users\\jacob\\Documents\\GitHub\\New-Unearth\\ushort.txt"; 

	public DepthMatrix GetData()
    {
		string text = "";
		try {
			text = System.IO.File.ReadAllText(path);
		}
		catch (Exception e) {
			Debug.Log ("broke reading file");
			return null;
		}
        
		Debug.Log(text);
        string[] lineValues = text.Split(',');

		Debug.Log (lineValues [2]);

		List<ushort> depthData = new List<ushort>();

		foreach (string value in lineValues) {
			try {
				depthData.Add(ushort.Parse(value));
			}
			catch (Exception e){
				break;
			}

		}
		Debug.Log (depthData);

		depthMatrix = new DepthMatrix(depthData.ToArray());
		int number = 0;
//		// Print out for testing
		for (int i = 0; i < depthMatrix.m_matrix.GetLength(0) - 1; i++) {
			for (int j = 0; j < depthMatrix.m_matrix.GetLength(1) - 1; j++) {
				number++;
				// Do not put Debug.Log here, breaks unity haha
			}
		}

		Debug.Log ("this many enteries in matrix: " + number);

		return depthMatrix;
    }

	public void Start()
	{
		this.GetData();
	}



}

