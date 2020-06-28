# RhinoInsideGame

これは2020/07/04に行われた[Tokyo AEC Industry Dev Group](https://www.meetup.com/ja-JP/Tokyo-AEC-Industry-Dev-Group/) でのハンズオンの資料になります。
RhinoInside と Unity を使ったボールをゴールへ運ぶゲームのつくり方のハンズオンになります。

# 完成品のイメージ

<img src=./images/RUIGame.gif width=500>

# ハンズオン

## 0. 環境構築

+ RhinoInsideのリポをクローンしておく（いくつかのファイルを使う）
  + [ここ](https://github.com/mcneel/rhino.inside)からクローンorダウンロード
+ RhinoWIP
  + [ここ](https://www.rhino3d.com/download/rhino/wip)からダウンロード
+ Unity2019.4.1.f1
  + [ここ](https://unity3d.com/jp/get-unity/download)からダウンロード

+ Rider2020.1
  + コードを書くエディタです。VisualStudioやVSCodeなどでもよいです。
  + Unity関連のデータをダウンロードをしておいてください。
    + Riderの場合
      + 特に追加でダウンロードするものはないです
    + Visual Studioの場合
      + Visual Studio Installerから以下のUnityに関するものをインストール
      <img src=./images/VS_Unity.png width=500>
    + VS Codeの場合
      + ExtensionsからDebugger for Unity をインストール
      <img src=./images/VSC_Unity.png width=500>
    + エディタの設定はUnityの以下から設定
    <img src=./images/EditorSettings.png width=500>

## 1. UnityでRhinoを使う

+ Asset下にScriptsという名前のフォルダを作成してそこに"Convert.cs"を入れる。"LoftSurface.cs"を作る
+ Asset下にPluginsという名前のフォルダを作成してそこに"RhinoCommon.dll"を入れる。
  + Convert.cs、RhinoCommon.dllはクローンしたリポの中に入っています。
+ Asset下にEditorというフォルダを作成して、"RhinoInsideUI.cs"を作る
  + Editorという名前のフォルダ名は特殊な扱いを受けるのでフォルダ名は間違えないで入れてください。
+ 以下を書いて、エディタからRhinoを起動してみる

```cs
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
        GC.SuppressFinalize(new RhinoCore(new string[] { "/scheme=Unity", "/nosplash" }, WindowStyle.Minimized));
    }
}
```

+ ロフトサーフェスを作る
  + まずはRhino内で作ってみる。
  + 次にRhinoInside を使って作ってみる。

```cs
public class RhinoInsideUI : MonoBehaviour
{
    public static void StartRhinoInside()
    {
        // 省略
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
                var controlSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                controlSphere.transform.parent = surface.transform;
                controlSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                controlSphere.transform.position = new Vector3( i * 5f, 0, j * 5f);
                controlSphere.GetComponent<MeshRenderer>().material = cpMaterial;
            }
        }
    }
}
```

**これで RhinoInside はほぼ終わり**