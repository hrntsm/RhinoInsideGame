using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rhino.Geometry;
#if UNITY_EDITOR
using UnityEditor;
#endif


#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class LoftSurface : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SetMeshCollider(gameObject);
    }

    private void SetMeshCollider(GameObject obj)
    {
        if (obj.GetComponent<MeshCollider>() != null)
        {
            DestroyImmediate(gameObject.GetComponent<MeshCollider>());
        }

        obj.AddComponent<MeshCollider>();
        obj.GetComponent<MeshCollider>().material = new PhysicMaterial("SurfMaterial")
        {
            bounciness = (float) 1.0
        };
    }

    // Update is called once per frame
    void Update()
    {
        SetMeshCollider(gameObject);
        
        var controlPoints  = new List<List<Vector3>>();

        int i = 0;
        List<Vector3> controlPointsRow = null;
        foreach (UnityEngine.Transform controlSphere in transform)
        {
            if ((i++ % 4) == 0)
            {
                controlPointsRow = new List<Vector3>(4);
                controlPoints.Add(controlPointsRow);
            }

            controlPointsRow.Add(controlSphere.position); // 今はスタートの座標が0なので、これでも問題ない
            // 多分surfaceがどこにあっても相対座標でとってくるのでうまく座標を取得してくる？
            // controlPointsRow.Add(gameObject.transform.worldToLocalMatrix.MultiplyPoint(controlSphere.gameObject.transform.position));
        }

        gameObject.GetComponent<MeshFilter>().mesh = CreateLoft(controlPoints);
    }

    private UnityEngine.Mesh CreateLoft(List<List<Vector3>> controlPoints)
    {
        if (controlPoints.Count > 0 )
        {
            var profileCurves = new List<Curve>();
            foreach (var controlPointsRow in controlPoints)
            {
                profileCurves.Add(Curve.CreateInterpolatedCurve(controlPointsRow.ToRhino(), 3));
            }

            Brep brep = Brep.CreateFromLoft(profileCurves, Point3d.Unset,Point3d.Unset, LoftType.Normal, false)[0];
            Rhino.Geometry.Mesh mesh = Rhino.Geometry.Mesh.CreateFromBrep(brep, MeshingParameters.Default)[0];
            return mesh.ToHost();
        }

        return null;
    }
}
