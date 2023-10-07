using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LineCreate : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private void SetLines()
    {
        /*Vector3[] posArr = {
            new Vector3(-0.3544925f, 0.1513804f, 0.1344109f)*8,
            new Vector3(-0.3567241f, 0.1484797f, 0.1310042f)*8, 
            new Vector3(-0.3589425f, 0.1455317f, 0.1276296f)*8, 
            new Vector3(-0.3612304f, 0.1425848f, 0.1243008f)*8};*/
        Vector3[] posArr = {new Vector3(-0.5f, 0.1f, 0), new Vector3(0.5f, 0.1f, 0), new Vector3(0.5f, 0.1f, 1), new Vector3(0.5f, 1, 1)};
        lineRenderer.positionCount = posArr.Length;
        lineRenderer.SetPositions(posArr);
        lineRenderer.SetWidth(0.03f, 0.03f);
    }

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetLines();
    }

    void Update()
    {
        
    }
}
