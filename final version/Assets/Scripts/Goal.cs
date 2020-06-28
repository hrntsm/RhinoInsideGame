using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private GameObject ball;
    [SerializeField] private GameObject goalPanel;
    private void OnCollisionEnter(Collision other)
    {
        goalPanel.gameObject.SetActive(true);
        ball.SetActive(false);
    }
}
