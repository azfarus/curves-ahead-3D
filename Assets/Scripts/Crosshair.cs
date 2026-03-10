using System;
using UnityEngine;

public class CrosshairDetection : MonoBehaviour
{
    public float detectionDistance = 100f;
    public int threshold = 10;
    
    private bool firstTimeDetected = false;
    private Transform cameraTransform;
    private ParametricRopeMesh ropeMesh;
    private int curveIndex = 0;

    public ParametricRopeMesh RopeMesh => ropeMesh;
    public int CurveIndex => curveIndex;

    Vector3 targetPoint = new Vector3(1f, 0f, 0f);


    private void Start()
    {
        cameraTransform = Camera.main.transform;
        ropeMesh = this.GetComponent<ParametricRopeMesh>();
        
        ropeMesh.SegmentMeshes[curveIndex].enabled = true;
        if (curveIndex > 0) ropeMesh.SegmentMeshes[curveIndex-1].enabled = false;
        curveIndex++;
    }

    void Update()
    {
        if (RopeUIController.isMenuOpen)
        {
            curveIndex=0;
            firstTimeDetected = false;
            return;
        }
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        // 2. Perform the Raycast
        if (Physics.Raycast(ray, out hit))
        {
            // 3. Get the closest point on the mesh/collider

            for (int start = curveIndex; start < curveIndex + threshold; start++)
            {
                if (hit.collider.name.Equals($"segment_{start}"))
                {   
                    
                    if (start >= ropeMesh.SegmentMeshes.Count) start = ropeMesh.SegmentMeshes.Count - 1;
                    ropeMesh.SegmentMeshes[start].enabled = true;
                    
                    for (int erase = start; erase >= curveIndex ; erase--) if (erase > 0) ropeMesh.SegmentMeshes[erase-1].enabled = false;
               
                    
                    curveIndex=start;
                    Debug.Log($"{curveIndex} is hit");
                    break;
                }
                
                
            }
        }

        
    }
}
