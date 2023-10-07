using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreeDeeBear.Models.Ply;
using System.IO;
using System;
using System.Diagnostics;

public class DrawPipe : MonoBehaviour
{
    public float pipeRadius = 0.2f;
    [Range(3, 32)]
    public int pipeSegments = 8;
    public Material pipeMaterial;
    private Vector3[] line_points = new Vector3[400];
    GameObject[] lineObjects;

    void Start() {
        Stopwatch sw=new Stopwatch();
        sw.Start();
        PlyResult result = PlyHandler.GetResult(File.ReadAllBytes("Fiber-ascii.ply"));
        sw.Stop();
        UnityEngine.Debug.Log( string.Format( "plyloader total: {0} ms" , sw.ElapsedMilliseconds));
        sw.Reset();
        sw.Start();
        int count = 0;
        int line_count = 0;
        Vector3[] line_points_nonzeros;
        lineObjects = new GameObject[16000];
        Vector3 zero_vertex = new Vector3(0f,0f,0f);
        for (int i = 0; i < 550000; i++) //result.vertices.Count
		{
			if (result.vertices[i] != zero_vertex)
			{
				line_points[count] = result.vertices[i]/8;
                count += 1;
			}
			else
			{
                line_points_nonzeros = new Vector3[count];
                Array.Copy(line_points,line_points_nonzeros,count);
				//RenderPipe(line_points_nonzeros);
				count = 0;
                lineObjects[line_count] = new GameObject();
                lineObjects[line_count].name = "Line #" + line_count;
                lineObjects[line_count].transform.parent = this.gameObject.transform;
                MeshFilter ringMesh = lineObjects[line_count].AddComponent<MeshFilter>();
                Mesh calmesh = GenerateMesh(line_points_nonzeros);
                ringMesh.mesh = calmesh;
                MeshRenderer ringRenderer = lineObjects[line_count].AddComponent<MeshRenderer>();
                ringRenderer.material = pipeMaterial;
                line_count += 1;
			}
		}
        sw.Stop();
        UnityEngine.Debug.Log( string.Format( "plot total: {0} ms" , sw.ElapsedMilliseconds));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    Mesh GenerateMesh(Vector3[] points) {
        Mesh m = new Mesh();
        m.name = "UnityPlumber Pipe";
        Vector3[] vertices = new Vector3[(points.Length - 1) * pipeSegments *2];
        //int[] triangles = new int[(points.Length - 1) * pipeSegments];
        
        Vector3[] normals = new Vector3[(points.Length - 1) * pipeSegments * 2];
        int[] triangles = new int[(points.Length - 1) * pipeSegments * 6];
        // for each segment, generate a cylinder
        for (int i = 0; i < points.Length - 1; i++) {
            Vector3 initialPoint = points[i];
            Vector3 endPoint = points[i + 1];
            Vector3 direction = (points[i + 1] - points[i]).normalized;

            // generate two circles with "pipeSegments" sides each and then
            // connect them to make the cylinder
            GenerateCircleAtPoint(vertices, normals, initialPoint, direction, 2*i);
            GenerateCircleAtPoint(vertices, normals, endPoint, direction, 2*i+1);
            MakeCylinderTriangles(triangles, i);
        }
        m.SetVertices(vertices);
        m.SetTriangles(triangles, 0);
        m.SetNormals(normals);
        return m;
    }

    void GenerateCircleAtPoint(Vector3[] vertices, Vector3[] normals, Vector3 center, Vector3 direction, int index)
    {
        // 'direction' is the normal to the plane that contains the circle
        // define a couple of utility variables to build circles
        float twoPi = Mathf.PI * 2;
        float radiansPerSegment = twoPi / pipeSegments;
        // generate two axes that define the plane with normal 'direction'
        // we use a plane to determine which direction we are moving in order
        // to ensure we are always using a left-hand coordinate system
        // otherwise, the triangles will be built in the wrong order and
        // all normals will end up inverted!
        Plane p = new Plane(Vector3.forward, Vector3.zero);
        Vector3 xAxis = Vector3.up;
        Vector3 yAxis = Vector3.right;
        if (p.GetSide(direction)) {
            yAxis = Vector3.left;
        }

        // build left-hand coordinate system, with orthogonal and normalized axes
        Vector3.OrthoNormalize(ref direction, ref xAxis, ref yAxis);

        for (int i = 0; i < pipeSegments; i++) {
            Vector3 currentVertex =
                center +
                (pipeRadius * Mathf.Cos(radiansPerSegment * i) * xAxis) +
                (pipeRadius * Mathf.Sin(radiansPerSegment * i) * yAxis);
            vertices[index * pipeSegments + i] = currentVertex;
            normals[index * pipeSegments + i] = (currentVertex - center).normalized;
        }
    }

    void MakeCylinderTriangles(int[] triangles, int segmentIdx)
    {
        // connect the two circles corresponding to segment segmentIdx of the pipe
        int offset = segmentIdx * pipeSegments * 2;
        for (int i = 0; i < pipeSegments; i++)
        {
            triangles[(segmentIdx *pipeSegments+i)*6 + 0] = offset + (i + 1) % pipeSegments;
            triangles[(segmentIdx *pipeSegments+i)*6 + 1] = offset + i + pipeSegments;
            triangles[(segmentIdx *pipeSegments+i)*6 + 2] = offset + i;

            triangles[(segmentIdx *pipeSegments+i)*6 + 3] = offset + (i + 1) % pipeSegments;
            triangles[(segmentIdx *pipeSegments+i)*6 + 4] = offset + (i + 1) % pipeSegments + pipeSegments;
            triangles[(segmentIdx *pipeSegments+i)*6 + 5] = offset + i + pipeSegments;
        }
    }
}
