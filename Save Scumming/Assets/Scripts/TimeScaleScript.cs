using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleScript : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)]
    private float timeScale;

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale != timeScale)
        {
            Time.timeScale = timeScale;
        }
    }
}
