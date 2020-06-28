using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        var pos = sphere.transform.position;
        pos.y = _slider.value;
        sphere.transform.position = pos;
    }
}
