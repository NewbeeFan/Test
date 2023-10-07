using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TinyplyCSharp;
using ThreeDeeBear.Models.Ply;
using System.IO;

public class DrawCylinderUsingPoints : MonoBehaviour
{
    public Material lineMat;
    public float radius = 0.5f;
    public Mesh cylinderMesh;
    GameObject[] lineObjects;
    GameObject[] tubeGameObjects;
    GameObject[] controlPointsObjects;
    PlyResult result = PlyHandler.GetResult(File.ReadAllBytes("Fiber-ascii.ply"));

    void Start()
    {
        int total_lines = 2;
        int total_points = 355555;
        this.lineObjects = new GameObject[total_lines];
        this.tubeGameObjects = new GameObject[total_points-1];
        this.controlPointsObjects = new GameObject[total_points];

        for (int j = 0; j < lineObjects.Length; j++)
        {
            lineObjects[j] = new GameObject();
            lineObjects[j].name = "Line #" + 0;
            lineObjects[j].transform.parent = this.gameObject.transform;
        }
        
        for (int i = 0; i < tubeGameObjects.Length; i++)
        {
            tubeGameObjects[i] = new GameObject();
            tubeGameObjects[i].name = "ConnectTube #" + i;
            tubeGameObjects[i].transform.parent = lineObjects[0].transform;
            /*
            GameObject ringOffsetCylinderMeshObject = new GameObject();
            ringOffsetCylinderMeshObject.transform.parent = this.tubeGameObjects[i].transform;
            ringOffsetCylinderMeshObject.transform.localPosition = new Vector3(0f, 1f, 0f);
            ringOffsetCylinderMeshObject.transform.localScale = new Vector3(radius, 1f, radius);

            // 设定Mesh和renderer
            MeshFilter ringMesh = ringOffsetCylinderMeshObject.AddComponent<MeshFilter>();
            ringMesh.mesh = this.cylinderMesh;
            MeshRenderer ringRenderer = ringOffsetCylinderMeshObject.AddComponent<MeshRenderer>();
            ringRenderer.material = lineMat;
            */
            tubeGameObjects[i].transform.localPosition = new Vector3(0f, 1f, 0f);
            tubeGameObjects[i].transform.localScale = new Vector3(radius, 1f, radius);

            // 设定Mesh和renderer
            MeshFilter ringMesh = tubeGameObjects[i].AddComponent<MeshFilter>();
            ringMesh.mesh = this.cylinderMesh;
            MeshRenderer ringRenderer = tubeGameObjects[i].AddComponent<MeshRenderer>();
            ringRenderer.material = lineMat;
        }

        for (int i = 0; i < controlPointsObjects.Length; i++)
        {
            controlPointsObjects[i] = new GameObject();
            controlPointsObjects[i].name = "ControlPoints #" + i;
            controlPointsObjects[i].transform.parent = lineObjects[0].transform;
            if (result.vertices[i] != new Vector3(0f,0f,0f))
            {
                controlPointsObjects[i].transform.position = result.vertices[i];
            }
            else
            {
                continue;
            }
            
        }
        drawconnectors();
        Debug.Log(result.vertices[0]);
    }

    // Update is called once per frame
    void Update()
    {
        //drawconnectors();
    }

    void drawconnectors()
    {
        for (int i = 0; i < tubeGameObjects.Length; i++)
        {
            tubeGameObjects[i].transform.position = controlPointsObjects[i].transform.position;
            tubeGameObjects[i].transform.localScale = new Vector3(tubeGameObjects[i].transform.localScale.x, 0.5f * Vector3.Distance(controlPointsObjects[i].transform.position, controlPointsObjects[i+1].transform.position), tubeGameObjects[i].transform.localScale.z);
            tubeGameObjects[i].transform.LookAt(controlPointsObjects[i+1].transform, Vector3.up);
            tubeGameObjects[i].transform.rotation *= Quaternion.Euler(90, 0, 0);
        }
    }
    /*
    static public void ReadPlyFile(string filePath, bool preloadIntoMemory = true)
    {
        {
            byte[] contents = File.ReadAllBytes(filePath);
            var ms = new MemoryStream(contents);
            PlyFile file = new PlyFile();
            file.ParseHeader(ms);
            Debug.Log("[ply_header] Type: " + (file.IsBinaryFile() ? "binary" : "ascii"));
            foreach (var comment in file.GetComments())
            {
                Debug.Log("\t[ply_header] Comment: " + comment);
            }

            foreach (var info in file.GetInfo())
            {
                Debug.Log("\t[ply_header] Info: " + info);
            }

            foreach (var element in file.GetElements())
            {
                Debug.Log("\t[ply_header] element: " + element.Name + "(" + element.Size + ")");
                foreach (var p in element.Properties)
                {
                    Debug.Log("\t[ply_header] \tproperty: " + p.Name + " (type=" + PlyHelper.PropertyTable[p.PropertyType].Str + ")");
                    if (p.IsList)
                    {
                        Debug.Log(" (list_type=" + PlyHelper.PropertyTable[p.ListType].Str + ")");
                    }
                }
            }

            PlyData vertices = file.RequestPropertiesFromElement("vertex", new List<string> { "x", "y", "z" });
            //PlyData normals = file.RequestPropertiesFromElement("vertex", new List<string> { "nx", "ny", "nz" });
            //PlyData faces = file.RequestPropertiesFromElement("face", new List<string> { "vertex_indices" });
            file.Read(ms);

            if (vertices != null)
            {
                Debug.Log("\tRead " + vertices.Count + " total vertices");
            }
            Debug.Log(System.BitConverter.ToSingle(vertices.Buffer.Get(),0));
            Debug.Log(System.BitConverter.ToSingle(vertices.Buffer.Get(),1));
            Debug.Log(System.BitConverter.ToSingle(vertices.Buffer.Get(),2));
            Debug.Log(System.BitConverter.ToSingle(vertices.Buffer.Get(),3));
            Debug.Log(System.BitConverter.ToSingle(vertices.Buffer.Get(),4));
            Debug.Log(System.BitConverter.ToSingle(vertices.Buffer.Get(),5));
            /*
            if (normals != null)
            {
                Debug.Log("\tRead " + normals.Count + " total vertex normals");
            }
            if (faces != null)
            {
                Debug.Log("\tRead " + faces.Count + " total faces (triangles)");
            }
        }
    }*/

}
