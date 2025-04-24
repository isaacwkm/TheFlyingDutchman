using UnityEngine;

namespace mendako
{

public class CameraController : MonoBehaviour
{
    public Transform pivot; // the pivot point around which the camera will rotate
    public float rotationSpeed = 5f; // the speed at which the camera rotates
    public float zoomSpeed = 5f; // the speed at which the camera zooms in and out
    public float shiftSpeed = 5f; // the speed at which the screen shifts

    private Vector3 lastMousePosition;

    void Update()
    {
        // Rotate the camera around the pivot point when the left mouse button is held down
        if (Input.GetMouseButton(0))
        {
            Vector3 deltaMouse = Input.mousePosition - lastMousePosition;

            float horizontal = rotationSpeed * deltaMouse.x;
            float vertical = rotationSpeed * deltaMouse.y;

            transform.RotateAround(pivot.position, Vector3.up, horizontal);
            transform.RotateAround(pivot.position, transform.right, -vertical);
        }

        // Zoom in and out when the mouse wheel is scrolled
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 zoom = transform.forward * zoomSpeed * scroll;
        transform.position += zoom;

        // Shift the screen left/right/up/down when the right mouse button is held down
        if (Input.GetMouseButton(1))
        {
            Vector3 deltaMouse = Input.mousePosition - lastMousePosition;

            float horizontal = shiftSpeed * deltaMouse.x;
            float vertical = shiftSpeed * deltaMouse.y;

            transform.Translate(new Vector3(horizontal, vertical, 0));
        }

        lastMousePosition = Input.mousePosition;
    }
}
}