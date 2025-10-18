using UnityEngine;

public class MouseDragRotate : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 800;

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            float xRotation = Input.GetAxis("Mouse X") * rotateSpeed;
            float yRotation = Input.GetAxis("Mouse Y") * rotateSpeed;
            transform.RotateAround(transform.position, Vector3.up, xRotation * Time.deltaTime);
            transform.RotateAround(transform.position, transform.right, -yRotation * Time.deltaTime);
        }
    }
}
