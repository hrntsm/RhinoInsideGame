using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Respawn : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        // コライダーに入ったらシーンをロードする
        SceneManager.LoadScene("GameScene");
    }
}
