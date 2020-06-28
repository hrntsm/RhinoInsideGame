using System;
using System.IO;

using UnityEngine;
using UnityEditor;

using Rhino.Runtime.InProcess;

[ExecuteInEditMode]
public class RhinoInsideUI : MonoBehaviour
{
    [MenuItem("Rhino/Start RhinoInside")]
    public static void StartRhinoInside()
    {
        string rhinoSystemDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Rhino WIP", "System");
        var path = Environment.GetEnvironmentVariable("PATH");
        Environment.SetEnvironmentVariable("PATH", path + ";" + rhinoSystemDir);
        // Finalizerが起動して、Rhinoが終了しないようにしている
        GC.SuppressFinalize(new RhinoCore(new string[] { "/scheme=Unity", "/nosplash" }, WindowStyle.Minimized));
    }

    [MenuItem("Rhino/Create Loft Surface")]
    public static void Create()
    {
        var surface = new GameObject("Loft Surface");
        surface.AddComponent<LoftSurface>();
        CreateLoft(surface);
    }

    private static void CreateLoft(GameObject surface)
    {
        surface.AddComponent<MeshFilter>();
        
        // Surfaceの色の設定
        var material = new Material(Shader.Find("Standard"))
        {
            color = new Color(1.0f, 0.0f, 0.0f, 1.0f)
        };
        surface.AddComponent<MeshRenderer>().material = material;
        // 影を落とさないようにする
        surface.GetComponent<MeshRenderer>().receiveShadows = false;

        // コントロールポイントの色の設定
        var cpMaterial = new Material(Shader.Find("Standard"))
        {
            color = new Color(0.2f, 0.2f, 0.8f, 1f)
        };
        
        // コントロールポイントの作成
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int num = 4 * i + j;
                var controlSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                controlSphere.name = "Sphere" + num;
                controlSphere.transform.parent = surface.transform;
                controlSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                controlSphere.transform.position = new Vector3( i * 5f, 0, j * 5f);
                controlSphere.GetComponent<MeshRenderer>().material = cpMaterial;
            }
        }
    }
}