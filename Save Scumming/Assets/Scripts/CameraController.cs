using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 10f; // Rotation speed of the camera

    private void Start()
    {

    }

    private void Update()
    {
        transform.Rotate(0, rotationSpeed * Input.GetAxis("Horizontal") * Time.deltaTime, 0);
    }

 
}