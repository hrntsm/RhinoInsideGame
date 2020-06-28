using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private GameObject ball;
    private void OnTriggerEnter(Collider other)
    {
        ball.SetActive(false);
    }
}
