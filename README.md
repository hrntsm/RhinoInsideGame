# RhinoInsideGame

これは2020/07/04に行われた[Tokyo AEC Industry Dev Group](https://www.meetup.com/ja-JP/Tokyo-AEC-Industry-Dev-Group/) でのハンズオンの資料になります。
RhinoInside と Unity を使ったボールをゴールへ運ぶゲームのつくり方のハンズオンになります。

## 完成品のイメージ

<img src=https://github.com/hrntsm/RhinoInsideGame/blob/master/images/RIUGame.gif width=500>

## ハンズオン

### 0. 環境構築

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

### 1. UnityでRhinoを使う

#### 1.1 RhinoInsideを起動できるようにする

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

#### 1.2 RhinoでSurfaceを作る

+ ロフトサーフェスを作る
  + まずはRhino内で作ってみる。
  <img src=./images/loftsurf.png width=500>

#### 1.3 RhinoInside でSurfaceを作る

+ 次にRhinoInside を使って作ってみる。
  + コントロールポイントをまず作る

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

+ 作ったコントロールポイントを使ってロフトサーフェスを作る

```cs
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
    void Update()
    {
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
            controlPointsRow.Add(controlSphere.position);
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
```

これで"Rhino/Start RhinoInside" をした後に、"Rhino/Create Loft Surface"を押すとロフトサーフェスが作成されるはずです。
ここまでの内容は、part1のフォルダのデータになっています。
<img src=./images/LoftSurface.png width=500>

**これで RhinoInside は終わり。あとはUnityのみになります。**

---

### 2. ゲーム化する

#### 2.1 ボールを弾ませる

1. Ballを作成する
2. play▶してみる
   + 何も行らない…
3. RigidBodyをアタッチする
   + 重力で落ちていくが貫通する…
4. LoftSurfaceにMeshColliderをアタッチする
   + Ballが弾まない…
5. Materialsフォルダを作成してそこにPhysicMaterialを作成する。
   + Bouncesを任意の値にして、BallとLoftSurfaceにアタッチする。
6. ボールが弾む！！
7. コントロールポイントを動かしてみる
   + コライダーが反映されない…
8. 動的にMeshColliderをアタッチできるようにする。
   + LoftSurface.csに以下を追記
   + アタッチされているゲームオブジェクトにMeshColliderがあれば削除し、新しいMeshColliderを設定する SetMeshCollider メソッドを追加している

```cs
public class LoftSurface : MonoBehaviour
{
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

    void Update()
    {
        SetMeshCollider(gameObject);
        var controlPoints  = new List<List<Vector3>>();
        // 以下省略...
```

#### 2.2 ゲームオーバー時にゲームを再スタートできるようにする

1. SampleScene の名前を GameScene に変える
2. Cube を作成する
   + 名前を Respawn にする
3. LoftSurfaceの下の方に適当な距離をとって、X と Z の Scale を100にする
   + ここに当たらないと再スタートしないので、位置に注意
4. リスポンの判定に使うのみで、レンダーする必要なので MeshRenderer を非アクティブにする
5. Add Component で Respawn.cs を追加する。
   + コライダーに入ってきたら実行するメソッドOnCollisionEnterを使う
   + シーンを読み込む形で再スタートを実装する

```cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class Respawn : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        SceneManager.LoadScene("GameScene");
    }
}
```

#### 2.3 ゴールを作る

1. Cube で作成する
   + 名前を Goal にする
2. ゴールにしたい個所に配置する
   + スケールも好きな値に設定する
   + 単純にこれがゲームの難しさになるので注意
3. Add Component で Goal.cs を追加する。
4. クリア時の画面を作成（次のところでまとめて作成するので後回し）
5. クリアなのでBallを消す
   + SerializeField をつけるとエディタ上から値を設定できるようになる
   + Ballをエディタ上で設定する

```cs
public class Goal : MonoBehaviour
{
  [SerializeField] private GameObject ball;
  private void OnTriggerEnter(Collider other)
  {
    ball.SetActive(false);
  }
}
```

#### 2.4 現状の確認

+ ここまで作るとUnityはこんなになっているはずです

<img src=./images/GameScene.png width=500>

+ ここまでのデータは part2 に入っているものになっています

---

### 3. UIを作っていく

