using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreeDeeBear.Models.Ply;
using System.IO;

[RequireComponent(typeof(PipeMeshGenerator))]
public class RandomPipe : MonoBehaviour {

	public int numberOfPoints;
	public float range;
	PlyResult result = PlyHandler.GetResult(File.ReadAllBytes("Fiber-ascii.ply"));
	void Start() 
	{
		/*
		Vector3[,] lines = new Vector3[500,500];
		//List<Vector3> line = new List<Vector3>();
		//Vector3[] line = new Vector3[500];
		int j = 0;
		int cnt = 0;
		for (int i = 0; i < 100000; i++) {
			if (result.vertices[i] != new Vector3(0f,0f,0f))
            {
                lines[j,cnt] = result.vertices[i];
				cnt += 1;
            }
			else
			{
				j += 1;
				cnt = 0;
			}
		}
		Debug.Log(lines);*/

/*
		PipeMeshGenerator pmg = GetComponent<PipeMeshGenerator>();
		for (int i = 0; i < 30000; i++)
		{
			if (result.vertices[i] != new Vector3(0f,0f,0f))
			{
				pmg.points.Add(result.vertices[i]);
			}
			else
			{
				pmg.RenderPipe();
				pmg = GetComponent<PipeMeshGenerator>();
			}
		}
			*/
	}
}