using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rhino.Runtime.InProcess;

public class TitleSceneScript : MonoBehaviour
{
    private void Start()
    {
        string RhinoSystemDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Rhino WIP", "System");
        var PATH = Environment.GetEnvironmentVariable("PATH");
        Environment.SetEnvironmentVariable("PATH", PATH + ";" + RhinoSystemDir);
        GC.SuppressFinalize(new RhinoCore(new string[] { "/scheme=Unity", "/nosplash" }, WindowStyle.Minimized));
    }

    void Update ()
    {
        if (Input.GetMouseButtonDown (0))
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}