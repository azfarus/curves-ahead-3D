
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ParametricRopeMesh : MonoBehaviour
{
    [Header("Rope Shape")]
    public float ropeRadius = 0.1f;
    public int curveSegments = 100;
    public int radialSegments = 12;
    public int segmentOffset = 2;

    [Header("Curve Parameters")]
    public float curveLength = 1f;
    public List<float> coeffA = new List<float>{1,0,0};
    public List<float> coeffB = new List<float>{1,0,0};
    
    [Header("Visuals")]
    [SerializeField] private Material segmentMaterial; // Serialized field for the material
    

    private Mesh _mesh;
    private GameObject colliderContainer;
    private List<MeshRenderer> segmentMeshes = new List<MeshRenderer>(); // List to store meshes
    private  List<CapsuleCollider> segmentColliders = new List<CapsuleCollider>();
    



    public List<CapsuleCollider> SegmentColliders => segmentColliders;
    public List<MeshRenderer> SegmentMeshes => segmentMeshes;

    void Start()
    {
        GenerateRope();
    }

    public void GenerateRope()
    {
        GenerateMesh();
        GenerateSegmentColliders();
        
    }



    // =========================
    // PARAMETRIC CURVE
    // =========================
    Vector3 Curve(float t,float radius = 10)
    {   
        float x, y, z;
        z = 0;
        x = radius*math.sin(t*Mathf.PI*2);
        y = radius*math.cos(t*Mathf.PI*2);

        for (int i = 0; i < coeffA.Count; i++)
        {
            z += math.sin(coeffA[i]*x+coeffB[i]*y);
        }

        return new Vector3(
            x,  // x(s) = s
            z,
            y
        );
    }

    Vector3 Tangent(float t)
    {
        float dt = 0.001f;
        return (Curve(t + dt) - Curve(t)).normalized;
    }

    // =========================
    // MESH GENERATION
    // =========================
    
    
    void GenerateSegmentColliders()
    {
        // 1. Cleanup or Setup Container
        if (colliderContainer == null)
        {
            colliderContainer = new GameObject("Colliders");
            colliderContainer.transform.SetParent(transform, false);
        }

        // 2. Clear old colliders to prevent stacking
        foreach (var col in segmentColliders)
        {
            if (col != null) DestroyImmediate(col.gameObject);
        }
        segmentColliders.Clear();
        segmentMeshes.Clear();

        float dt = curveLength / curveSegments;

        // 3. Loop through segments to place colliders
        for (int i = 0 + segmentOffset; i < curveSegments + segmentOffset; i++)
        {
            float tStart = i * dt;
            float tEnd = (i + 1) * dt;

            Vector3 startPos = Curve(tStart);
            Vector3 endPos = Curve(tEnd);
            Vector3 center = (startPos + endPos) * 0.5f;
            Vector3 direction = endPos - startPos;
            float segmentHeight = direction.magnitude;

            // Create child object for this segment's collider
            GameObject colObj = new GameObject($"Segment_{i}");
            colObj.transform.SetParent(colliderContainer.transform, false);
            colObj.transform.localPosition = center;

            // Orient the collider to look from start to end
            // CapsuleColliders are Y-axis aligned by default
            if (direction != Vector3.zero)
            {
                colObj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            }
            
            GameObject tempCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Mesh mesh = tempCylinder.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(tempCylinder);

            MeshFilter mf = colObj.AddComponent<MeshFilter>();
            // ScaleMeshVertices(mesh,1f,1f,1f);
            mf.sharedMesh = mesh;

            MeshRenderer mr = colObj.AddComponent<MeshRenderer>();
            mr.sharedMaterial = segmentMaterial;
            mr.enabled = false; // Visibility turned off as requested
            
            colObj.transform.localScale = new Vector3(1f,.3f, 1f);
            segmentMeshes.Add(mr);

            CapsuleCollider cap = colObj.AddComponent<CapsuleCollider>();
            cap.radius = ropeRadius;
            cap.height = segmentHeight + (ropeRadius * 2f); // Add radius to ends to close gaps
            cap.direction = 1; // 1 = Y-axis
            cap.name = $"segment_{i}";

            segmentColliders.Add(cap);
        }

    }
    void GenerateMesh()
    {
        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.name = "Parametric Rope Mesh";
            GetComponent<MeshFilter>().mesh = _mesh;
            
        }
        else
        {
            _mesh.Clear();
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        float dt = curveLength / curveSegments;

        Vector3 prevT = Tangent(0f);

        Vector3 normal = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(prevT, normal)) > 0.99f)
            normal = Vector3.right;

        Vector3 binormal = Vector3.Cross(prevT, normal).normalized;
        normal = Vector3.Cross(binormal, prevT).normalized;

        for (int i = 0+segmentOffset; i <= curveSegments+segmentOffset; i++)
        {
            float t = i * dt;
            Vector3 center = Curve(t);
            Vector3 T = Tangent(t);

            // Parallel transport frame
            Vector3 axis = Vector3.Cross(prevT, T);
            float axisMag = axis.magnitude;

            if (axisMag > 1e-5f)
            {
                axis /= axisMag;
                float angle = Mathf.Asin(axisMag) * Mathf.Rad2Deg;
                normal = Quaternion.AngleAxis(angle, axis) * normal;
            }

            binormal = Vector3.Cross(T, normal).normalized;
            normal = Vector3.Cross(binormal, T).normalized;
            prevT = T;

            for (int j = 0; j < radialSegments; j++)
            {
                float theta = 2f * Mathf.PI * j / radialSegments;
                Vector3 offset =
                    Mathf.Cos(theta) * normal +
                    Mathf.Sin(theta) * binormal;

                vertices.Add(center + offset * ropeRadius);
                uvs.Add(new Vector2(j / (float)radialSegments, t / curveLength));
            }
        }

        for (int i = 0; i < curveSegments; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int current = i * radialSegments + j;
                int next = current + radialSegments;

                int currentNext = i * radialSegments + (j + 1) % radialSegments;
                int nextNext = currentNext + radialSegments;

                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(currentNext);

                triangles.Add(currentNext);
                triangles.Add(next);
                triangles.Add(nextNext);
            }
        }

        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(triangles, 0);
        _mesh.SetUVs(0, uvs);
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if(meshCollider == null)
            meshCollider = gameObject.AddComponent<MeshCollider>();

        meshCollider.sharedMesh = _mesh;
    }
}

