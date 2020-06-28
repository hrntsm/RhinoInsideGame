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

+ Rider2020.1 (スクリプトエディタ)
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

<img src=./images/LoftSurface.png width=500>

ここまでの内容は、part1 のフォルダのデータになっています。

#### 1.4 Unityのデバッグの仕方

+ Unityにエディタをアタッチすることでデバッグできます

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
4. ゲームクリア時の画面を作成（次のところでまとめて作成するので後回し）
5. ゲームクリアなのでBallを消す
   + SerializeField をつけるとエディタ上から値を設定できるようになる
   + Ballをエディタ上で設定する

```cs
public class Goal : MonoBehaviour
{
  [SerializeField] private GameObject ball;
  private void OnCollisionEnter(Collision other)
  {
    ball.SetActive(false);
  }
}
```

#### 2.4 現状の確認

+ ここまで作るとUnityはこんなになっているはずです

<img src=./images/GameScene.png width=500>

+ ここまでのデータは part2 のフォルダ に入っているものになっています

---

### 3. UIを作っていく

#### 3.1 クリア画面を作る

1. Hierarchyで右クリックして、UIからTextを選ぶとHierarchyにCanvasとEventSystemとCanvasの子にTextが作成される
   + CanvasのサイズはGameウインドウのサイズによるので注意してください

    <img src=./images/UI.png width=500>

2. Textにクリアを示す文字を入れる
3. Panelを使って背景を入れる
4. Panelの名前をGoalPanelにして、Textを子にする
5. GoalPanelを非アクティブにする
6. 2.3で作成したGoal.csに下記を追記して、BallがGoalに入った時にGoalPanelをアクティブにして表示されるようにする
   + エディタからGoalPanelをセットしておく

```cs
public class Goal : MonoBehaviour
{
  [SerializeField] private GameObject ball;
  [SerializeField] private GameObject goalPanel; // 追加
  private void OnTriggerEnter(Collider other)
  {
    goalPanel.gameObject.SetActive(true); // 追加
    ball.SetActive(false);
  }
}
```

#### 3.2 リスポンの確認画面を作成する

1. 3.1と同様にTextとPanelを使って確認画面を作成する
2. 2.2で作成した Respawn.cs を以下のように書き換える
   + BallがRespawnの枠内に入ったらボールを消して、リスポン確認画面を表示させる
   + Updateでは_retryがtrueかつ右クリックが押されたらGameSceneをロードさせる

```cs
public class Respawn : MonoBehaviour
{
    [SerializeField] private GameObject ball;
    [SerializeField] private GameObject respawn;
    private bool _retry = false;

    private void OnCollisionEnter(Collision other)
    {
        respawn.SetActive(true);
        ball.SetActive(false);
        _retry = true;
    }

    void Update ()
    {
        if (Input.GetMouseButtonDown (0) & _retry == true)
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}
```

#### 3.3 コントロールポイントの座標をスライダーで変更できるようにする

1. UIからSliderを作成する
2. Anchorsを左の中央にする
3. SliderのMinValueを-10、MaxValueを10にする
4. MoveSphere.csを作成してSliderにアタッチする

```cs
public class MoveSphere : MonoBehaviour
{
    [SerializeField] private GameObject sphere;
    private Slider _slider;

    private void Start()
    {
      _slider = gameObject.GetComponent<Slider>();
      _slider.value = 0;
    }

    public void Move()
    {
      // gameobject.transform.position.y は値が変えられないのでいったんposを介して値を変える
      var pos = sphere.transform.position;
      pos.y = _slider.value;
      sphere.transform.position = pos;
    }
}
```

5. sphere の部分に座標を操作したいコントロールポイントを設定する
6. SliderのOn Value Changed を設定する
   + ここで設定されたものはスライダーの値が変えられたときに呼ばれる

    <img src=./images/Slider.png>

7. 各コントロールポイントにスライダーを設定する

#### 3.4 カメラを設定する

1. MainCamera を選択するとSceneのウインドウの中にカメラのビューが表示される
2. ゲーム画面にしたい、いい感じのアングルを設定する

    <img src=./images/Camera.png width=500>

#### 3.5 ゲームのスタート画面を作る

1. Projectウインドウを右クリックしてCreateからSceneを作成する
   + 名前は TitleScene とする
2. SceneをTitleSceneに切り替える
3. Respawn画面などでやったようにTitle画面を作成する

    <img src=./images/Title.png width=500>

4. Create Empty から空のGameObjectを作り、それにTitleSceneScriptをアタッチする
   + 今はUnityエディタからRhinoInside を起動しているが、ビルドした単体のアプリとしてもRhinoInside を起動できるようにしなければいけないので、StartにRhinoInside を起動する部分をかく
   + Updateには画面をクリックしたら先程まで作っていたGameSceneがロードされるようにしている

```cs
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

  void Update () {

    if (Input.GetMouseButtonDown (0)) {
      SceneManager.LoadScene("GameScene");
    }
  }
}
```

### 4. ゲームとしてビルドする

+ File - Build Settingsを開く
+ Scene In Build で作成した2つのシーンを登録する
+ Architecture はx86_64 にする（多分デフォルトでこのあたい）
+ PlayerSettings から OtherSettingsから ScriptingBackend をMono、Api Compatibility Level を .Net 4.x にする
+ Buildする
+ 完成！！！！！

## まとめ

+ 最終版は final version のものになっています。
+ ほとんどUnityでしたが、うまく動きましたでしょうか。
+ RhinoInside の部分は、RhinoInsideのリポのUnityフォルダのsample1のものを参考にしています。
+ 質問は [Tokyo AEC Industry Dev Group](https://www.meetup.com/ja-JP/Tokyo-AEC-Industry-Dev-Group/events/gdqbsrybckbgb/) のハンズオンのページ、またはTokyo AEC Industry Dev GroupのDiscord、直接私に聞きたい場合は[Twitterアカウント](https://twitter.com/hiron_rgkr)へDMください。