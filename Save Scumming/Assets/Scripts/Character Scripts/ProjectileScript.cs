using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [SerializeField] private float speed = 2f;

    private Rigidbody rb;

    private void Start()
    {
        Physics.IgnoreCollision(GetComponent<Collider>(), transform.root.GetComponent<Collider>(), true);
        transform.SetParent(null, true);
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        //transform.Translate(speed * Time.deltaTime * transform.forward);
        if (rb.velocity.magnitude < speed)
        {
            rb.AddForce(5f * Time.deltaTime * transform.forward);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(GridManager.instance.WorldPositionToGrid(transform.position));
        Destroy(gameObject);
    }
}
