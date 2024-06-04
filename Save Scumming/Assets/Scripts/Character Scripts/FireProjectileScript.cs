using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireProjectileScript : MonoBehaviour
{
    [SerializeField] private GameObject projectile;
    [SerializeField] private GameObject _handslot;

    public void FireProjectile()
    {
        var proj = Instantiate(projectile, _handslot.transform.position, Quaternion.identity, transform);
        proj.transform.rotation = transform.rotation;
    }
}
