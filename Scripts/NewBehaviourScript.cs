using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCylinderRender : MonoBehaviour
{
    [Range(0, 20f)]
    public float circleRadius = 10f;
    [Range(3, 50)]
    public int circleSegement = 20;
    public Transform startTrans;            //from
    public Transform endTrans;              //to
    [Range(0, 90f)]
    public float rotateAngle = 30f;
    private MeshRenderer meshRender;
    private MeshFilter meshFilter;

    private float lastCcRadius;
    private int lastCcSege;
    private Vector3 lastStartPos;
    private Vector3 lastEndPos;
    private Mesh mesh;

    void Start()
    {
        meshRender = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
    }

    void Update()
    {
        if (CheckParamsChanged())
        {
            UpdateCylinderMesh();
        }
    }
    /// <summary>
    /// 检查绘制参数改变
    /// 改变才重绘制
    /// </summary>
    /// <returns></returns>
    private bool CheckParamsChanged()
    {
        if (lastCcRadius != circleRadius
            || lastCcSege != circleSegement
            || lastStartPos != startTrans.position
            || lastEndPos != endTrans.position)
        {
            lastCcRadius = circleRadius;
            lastCcSege = circleSegement;
            lastStartPos = startTrans.position;
            lastEndPos = endTrans.position;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 绘制圆柱体网格
    /// </summary>
    private void UpdateCylinderMesh()
    {
        Vector3 start = startTrans.position;
        Vector3 end = endTrans.position;
        Vector3 p2 = RotateAroundXAxis(start, end, rotateAngle * Mathf.Deg2Rad);
        Vector3 p1 = RayLineCrossPanel(start, end, p2);
        Vector3 p = start + (p1 - start).normalized * circleRadius;
        Vector3[] startposarr = CalculateCirclePoints(start, end, p, circleSegement);
        Vector3[] endposarr = CalculateBiasPoints(start, end, startposarr);
        //构建网格数据
        if (mesh != null)
        {
            mesh.Clear();
        }
        //构建顶点列表
        List<Vector3> vertlist = new List<Vector3>();
        vertlist.Add(start);
        vertlist.AddRange(startposarr);
        vertlist.AddRange(endposarr);
        vertlist.Add(end);
        //构建拓扑三角列表（逆时针）
        //014 043 032 021
        List<int> trilist = new List<int>();
        for (int i = 0; i < circleSegement; i++)
        {
            int[] tris = new int[]
            {
                0,
                i+2>circleSegement?(i+2)%circleSegement:i+2,
                i+1
            };
            trilist.AddRange(tris);
        }
        //165 126
        //276 237
        //387 348
        //458 415
        for (int i = 0; i < circleSegement; i++)
        {
            int[] tris = new int[]
            {
                i+1,
                i+circleSegement+2>circleSegement*2?i+2:i+circleSegement+2,
                i+circleSegement+1
            };
            trilist.AddRange(tris);
            tris = new int[]
            {
                i+1,
                i+2>circleSegement?(i+2)%circleSegement:i+2,
                i+circleSegement+2>circleSegement*2?i+2:i+circleSegement+2
            };
            trilist.AddRange(tris);
        }
        //956 967 978 985
        for (int i = 0; i < circleSegement; i++)
        {
            int[] tris = new int[]
            {
                circleSegement*2+1,
                i+circleSegement+1,
                i+circleSegement+2>circleSegement*2?i+2:i+circleSegement+2
            };
            trilist.AddRange(tris);
        }
        //构建网格
        mesh.vertices = vertlist.ToArray();
        mesh.triangles = trilist.ToArray();
        meshFilter.sharedMesh = mesh;
    }
    /// <summary>
    /// 绘制圆柱体网线图
    /// </summary>
    private void DrawCylinderGizmos()
    {
        Vector3 start = startTrans.position;
        Vector3 end = endTrans.position;
        Vector3 p2 = RotateAroundXAxis(start, end, rotateAngle * Mathf.Deg2Rad);
        Vector3 p1 = RayLineCrossPanel(start, end, p2);
        Vector3 p = start + (p1 - start).normalized * circleRadius;
        Vector3[] startposarr = CalculateCirclePoints(start, end, p, circleSegement);
        Vector3[] endposarr = CalculateBiasPoints(start, end, startposarr);
        for (int i = 0; i < circleSegement; i++)
        {
            Vector3 ccspos = startposarr[i];
            Vector3 ccsposp1 = startposarr[(i + 1) % circleSegement];
            Vector3 ccepos = endposarr[i];
            Vector3 cceposp1 = endposarr[(i + 1) % circleSegement];
            //构建start平面圆形
            Debug.DrawLine(ccspos, ccsposp1, Color.black);
            //构建start圆形的辐条
            Debug.DrawLine(start, ccspos, Color.black);
            //构建start->end界面矩形
            Debug.DrawLine(ccspos, ccepos, Color.black);
            //构建end平面圆形
            Debug.DrawLine(ccepos, cceposp1, Color.black);
            //构建end圆形的辐条
            Debug.DrawLine(end, ccepos, Color.black);
        }
    }

    /// <summary>
    /// 已知start为圆心r半径圆环上所有离散坐标sposarr
    /// 只需要通过sposarr+=(end-start)即可得到tposarr
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="sposarr"></param>
    /// <returns></returns>
    private Vector3[] CalculateBiasPoints(Vector3 start, Vector3 end, Vector3[] sposarr)
    {
        Vector3[] eposarr = new Vector3[sposarr.Length];
        Vector3 offset = end - start;
        for (int i = 0; i < sposarr.Length; i++)
        {
            Vector3 spos = sposarr[i];
            Vector3 epos = spos + offset;
            eposarr[i] = epos;
        }
        return eposarr;
    }

    /// <summary>
    /// 算出圆环上所有离散坐标点
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="p"></param>
    /// <param name="sege"></param>
    /// <returns></returns>
    private Vector3[] CalculateCirclePoints(Vector3 start, Vector3 end, Vector3 p, int sege)
    {
        Vector3[] posarr = new Vector3[sege];
        posarr[0] = p;
        Vector3 naxis = (end - start).normalized;
        float segerad = 2f * Mathf.PI / (float)sege;
        for (int i = 1; i < sege; i++)
        {
            float rad = segerad * i;
            Vector3 segepos = RotateAroundAnyAxis(start, p, naxis, rad);
            posarr[i] = segepos;
        }
        return posarr;
    }

    /// <summary>
    /// p(x,y,z)点绕start为起点的任意坐标轴旋转后的坐标
    /// </summary>
    /// <param name="start"></param>
    /// <param name="naxis"></param>
    /// <param name="rad"></param>
    /// <returns></returns>
    private Vector3 RotateAroundAnyAxis(Vector3 start, Vector3 p, Vector3 naxis, float rad)
    {
        float n1 = naxis.x;
        float n2 = naxis.y;
        float n3 = naxis.z;

        //获取p相对start的本地坐标
        p -= start;

        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);

        float m00 = n1 * n1 * (1 - cos) + cos;
        float m01 = n1 * n2 * (1 - cos) - n3 * sin;
        float m02 = n1 * n3 * (1 - cos) + n2 * sin;

        float m10 = n1 * n2 * (1 - cos) + n3 * sin;
        float m11 = n2 * n2 * (1 - cos) + cos;
        float m12 = n2 * n3 * (1 - cos) - n1 * sin;

        float m20 = n1 * n3 * (1 - cos) - n2 * sin;
        float m21 = n2 * n3 * (1 - cos) + n1 * sin;
        float m22 = n3 * n3 * (1 - cos) + cos;

        Vector3 mat_p = new Vector3();
        mat_p[0] = m00 * p[0] + m01 * p[1] + m02 * p[2];
        mat_p[1] = m10 * p[0] + m11 * p[1] + m12 * p[2];
        mat_p[2] = m20 * p[0] + m21 * p[1] + m22 * p[2];

        //绕轴旋转后，处理成世界坐标
        Vector3 px = mat_p + start;

        return px;
    }

    /// <summary>
    /// 通过start end计算start所处平面F方程
    /// 通过end p2计算射线与平面F交点p1
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    private Vector3 RayLineCrossPanel(Vector3 start, Vector3 end, Vector3 p2)
    {
        //start = from
        //end = to
        //构建平面F方程参数
        Vector3 ft = end - start;
        float u = ft.x, v = ft.y, w = ft.z;
        float a = start.x, b = start.y, c = start.z;
        //构建射线tp2参数
        float sx = end.x;
        float sy = end.y;
        float sz = end.z;
        Vector3 ntp2 = (p2 - end).normalized;
        float dx = ntp2.x;
        float dy = ntp2.y;
        float dz = ntp2.z;
        //计算p1
        float n = ((u * a + v * b + w * c) - (u * sx + v * sy + w * sz)) / (u * dx + v * dy + w * dz);
        Vector3 p1 = end + n * ntp2;
        return p1;
    }

    /// <summary>
    /// 空间任意起终点
    /// 终点绕x轴旋转rad弧度
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="rad"></param>
    /// <returns></returns>
    private Vector3 RotateAroundXAxis(Vector3 start, Vector3 end, float rad)
    {

        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        float m00 = 1;
        float m01 = 0;
        float m02 = 0;
        float m10 = 0;
        float m11 = cos;
        float m12 = -sin;
        float m20 = 0;
        float m21 = sin;
        float m22 = cos;

        Vector3 p = (start - end);
        Vector3 mat_p = new Vector3();
        mat_p[0] = m00 * p[0] + m01 * p[1] + m02 * p[2];
        mat_p[1] = m10 * p[0] + m11 * p[1] + m12 * p[2];
        mat_p[2] = m20 * p[0] + m21 * p[1] + m22 * p[2];

        Vector3 ret = mat_p + end;
        return ret;
    }
}