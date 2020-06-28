using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
