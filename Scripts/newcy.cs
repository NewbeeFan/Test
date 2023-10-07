using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class ConnectPointsWithCylinderMesh : MonoBehaviour
{
    //Hacked code from this article:
    //https://gamedev.stackexchange.com/questions/96964/how-to-correctly-draw-a-line-in-unity/96966

    // Material used for the connecting lines
    public Material lineMat;

    public float radius = 0.5f;

    public GameObject Object1;
    public GameObject Object2;

    public Mesh cylinderMesh;

    GameObject[] ringGameObjects;
    GameObject[] connectpts_obj;


    // Use this for initialization
    void Start()
    {
        //Set this list to the number
        int total_points = 2;
        this.ringGameObjects = new GameObject[total_points];
        this.connectpts_obj = new GameObject[total_points];
        Debug.Log(ringGameObjects.Length);

        for (int i = 0; i < ringGameObjects.Length; i++)
        {
            // Make a gameobject that we will put the ring on
            // And then put it as a child on the gameobject that has this Command and Control script
            this.ringGameObjects[i] = new GameObject();
            this.ringGameObjects[i].name = "Connecting ring #" + i;
            this.ringGameObjects[i].transform.parent = this.gameObject.transform;

            connectpts_obj[i] = new GameObject();
            connectpts_obj[i].name = "Connecting points #" + i;
            connectpts_obj[i].transform.parent = this.gameObject.transform;
            Debug.Log(this.gameObject.transform.position);

            // We make a offset gameobject to counteract the default cylindermesh pivot/origin being in the middle
            GameObject ringOffsetCylinderMeshObject = new GameObject();
            ringOffsetCylinderMeshObject.transform.parent = this.ringGameObjects[i].transform;

            // Offset the cylinder so that the pivot/origin is at the bottom in relation to the outer ring gameobject.
            ringOffsetCylinderMeshObject.transform.localPosition = new Vector3(0f, 1f, 0f);
            // Set the radius
            ringOffsetCylinderMeshObject.transform.localScale = new Vector3(radius, 1f, radius);

            // Create the the Mesh and renderer to show the connecting ring
            MeshFilter ringMesh = ringOffsetCylinderMeshObject.AddComponent<MeshFilter>();
            ringMesh.mesh = this.cylinderMesh;

            MeshRenderer ringRenderer = ringOffsetCylinderMeshObject.AddComponent<MeshRenderer>();
            ringRenderer.material = lineMat;

        }
        drawconnectors();

    }

    void Update()
    {
        drawconnectors();
    }

    void drawconnectors()
    {
        //Object2 to its child
        ringGameObjects[0].transform.position = Object1.transform.position;
        ringGameObjects[0].transform.localScale = new Vector3(ringGameObjects[0].transform.localScale.x, 0.5f * Vector3.Distance(Object1.transform.position, Object2.transform.position), ringGameObjects[0].transform.localScale.z);
        ringGameObjects[0].transform.LookAt(Object2.transform, Vector3.up);
        //update the rotations all at once. Not GameObject specific..
        ringGameObjects[0].transform.rotation *= Quaternion.Euler(90, 0, 0);
        
        connectpts_obj[0].transform.position = new Vector3(1.5f,-3f,1.1f);
        connectpts_obj[1].transform.position = new Vector3(2.5f,-2.2f,1.6f);
        
        ringGameObjects[1].transform.position = Object2.transform.position;
        ringGameObjects[1].transform.localScale = new Vector3(ringGameObjects[1].transform.localScale.x, 0.5f * Vector3.Distance(Object2.transform.position, connectpts_obj[0].transform.position), ringGameObjects[1].transform.localScale.z);
        ringGameObjects[1].transform.LookAt(connectpts_obj[0].transform, Vector3.up);
        //update the rotations all at once. Not GameObject specific..
        ringGameObjects[1].transform.rotation *= Quaternion.Euler(90, 0, 0);
    }


}