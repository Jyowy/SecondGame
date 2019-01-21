using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class CircuitBuilder : MonoBehaviour {

    public GameObject m_racePoints;
    public float m_density = 1.0f;

    private CircuitPoint[] m_circuitPoints;
    private int m_transformCount;

    private CombineInstance[] m_meshCombine;
    private MeshFilter[] m_circuitMeshFilters;
    private Mesh[] m_circuitMesh;
    private Vector3[][] m_vertices;
    private int[][] m_triangles;

    private void Awake()
    {
        if (m_racePoints != null && m_racePoints.transform.childCount > 1)
        {
            SetVariables();
            Debug.Log("MeshFilters detected: " + m_circuitMeshFilters.Length);

            GenerateCircuit();
        }
        else
        {
            Debug.Log("No RacePoints found in Circuit Builder or it has less than 2 circuit points"); ;
        }
    }

    private void SetVariables()
    {
        m_transformCount = m_racePoints.transform.childCount;
        if (m_circuitPoints == null || m_circuitPoints.Length != m_transformCount)
        {
            m_circuitPoints = null;
            m_circuitPoints = new CircuitPoint[m_transformCount];
        }
        for (int i = 0; i < m_transformCount; ++i)
        {
            m_circuitPoints[i] = m_racePoints.transform.GetChild(i).GetComponent<CircuitPoint>();
        }

        if (m_vertices != null)
        {
            for (int i = 0; i < m_vertices.Length; ++i)
            {
                m_vertices[i] = null;
            }
            m_vertices = null;
        }
        m_vertices = new Vector3[m_transformCount - 1][];
        if (m_triangles != null)
        {
            for (int i = 0; i < m_triangles.Length; ++i)
            {
                m_triangles[i] = null;
            }
        }
        m_triangles = new int[m_transformCount - 1][];
        m_circuitMesh = null;
        m_circuitMesh = new Mesh[m_transformCount - 1];
        m_circuitMeshFilters = null;
        m_circuitMeshFilters = m_racePoints.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < m_circuitMeshFilters.Length; ++i)
        {
            m_circuitMeshFilters[i].mesh = null;
        }
        m_meshCombine = null;
        m_meshCombine = new CombineInstance[m_transformCount - 1];
    }

    private void OnDestroy()
    {
        Debug.Log("CircuitBuilder - OnDestroy");
        m_circuitPoints = null;
        if (m_vertices != null)
        {
            for (int i = 0; i < m_vertices.Length; ++i)
            {
                m_vertices[i] = null;
            }
            m_vertices = null;
        }
        m_circuitMesh = null;
        m_meshCombine = null;
        m_circuitMeshFilters = null;
    }
    
   [ContextMenu("Regenerate Circuit")]
    public void RegenerateCircuit()
    {
        GenerateCircuit();
    }

    private void GenerateCircuit()
    {
        SetVariables();
        Debug.Log("CircuitBuilder - Generate Circuit");
        for (int point = 0; point < m_transformCount - 1; ++point)
        {
            GenerateStep(point);
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(m_meshCombine);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        Debug.Log("CircuitBuilder - Generated");
    }

    private void GenerateStep(int point)
    {
        Debug.Log("CircuitBuilder - Step #" + point);
        GenerateVertices(point);
        GenerateTriangles(point);
        GenerateMesh(point);
    }

    private void GenerateVertices(int point)
    {
        Vector3 left1;
        Vector3 right1;
        Vector3 left2;
        Vector3 right2;

        m_circuitPoints[point].GetPoints(out left1, out right1);
        m_circuitPoints[point + 1].GetPoints(out left2, out right2);

        int rows = (int)Mathf.Max(Mathf.Floor(2.0f * m_density), 2.0f);
        int columns = (int)Mathf.Max(Mathf.Floor(2.0f * m_density), 2.0f);

        m_vertices[point] = new Vector3[rows * columns];

        for (int row = 0, pos = 0; row < rows; ++row)
        {
            Vector3 vertex1 = Vector3.Lerp(left1, left2, row / (rows - 1.0f));
            Vector3 vertex2 = Vector3.Lerp(right1, right2, row / (rows - 1.0f));
            for (int col = 0; col < columns; ++col, ++pos)
            {
                m_vertices[point][pos] = Vector3.Lerp(vertex1, vertex2, col / (columns - 1));
            }
        }
    }

    private void GenerateTriangles(int point)
    {
        int rows = (int)Mathf.Max(Mathf.Floor(2.0f * m_density), 2.0f);
        int columns = (int)Mathf.Max(Mathf.Floor(2.0f * m_density), 2.0f);

        int ntriangles = (rows - 1) * (columns - 1) * 2;
        m_triangles[point] = new int[ntriangles * 3];

        for (int row = 0, pos = 0; row < rows - 1; ++row)
        {
            for (int col = 0; col < columns - 1; ++col, pos += 6)
            {
                // Triangle 1
                m_triangles[point][pos] = row * (columns) + col;
                m_triangles[point][pos + 1] = m_triangles[point][pos] + columns;
                m_triangles[point][pos + 2] = m_triangles[point][pos] + 1;
                // Triangle 2
                m_triangles[point][pos + 3] = m_triangles[point][pos + 2];
                m_triangles[point][pos + 4] = m_triangles[point][pos + 1];
                m_triangles[point][pos + 5] = m_triangles[point][pos + 2] + columns;
            }
        }
    }

    private void GenerateMesh(int point)
    {
        m_circuitMesh[point] = new Mesh();
        m_circuitMesh[point].vertices = m_vertices[point];
        m_circuitMesh[point].triangles = m_triangles[point];

        m_meshCombine[point].mesh = m_circuitMesh[point];
        m_meshCombine[point].transform = Matrix4x4.identity;
    }

    //

    private void OnDrawGizmos()
    {
        if (m_vertices == null)
        {
            return;
        }

        Gizmos.color = Color.black;
        for (int i = 0; i < m_vertices.Length; ++i)
        {
            for (int pos = 0; pos < m_vertices[i].Length; pos++)
            {
                Gizmos.DrawSphere(m_vertices[i][pos], 0.1f);
            }
        }
    }

    //

}
