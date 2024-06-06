using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    [SerializeField] private float _destroyTime = 0f;

    private void Start()
    {
        Destroy(gameObject, _destroyTime);
    }

    void DestroyGameObject()
    {
        Destroy(gameObject, _destroyTime);
    }
}
