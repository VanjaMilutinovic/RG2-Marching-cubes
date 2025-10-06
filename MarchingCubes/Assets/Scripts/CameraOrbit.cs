using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float distance = 35f;
    public float minDist = 8f;
    public float maxDist = 120f;
    public float zoomSpeed = 240f;
    public float rotSpeedMouse = 240f;
    public float rotSpeedKeys = 60f;
    public float moveSpeed = 50f;

    float yaw = 30f;
    float pitch = 25f;

    // Centar kamere
    Vector3 center;

    void Start()
    {
        if (target)
        {
            center = target.position;
        }
    }

    void LateUpdate()
    {
        // Kretanje kamere na WASD
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        if (Input.GetKey(KeyCode.W))
        {
            center += forward * (moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            center -= forward * (moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            center -= right * (moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            center += right * (moveSpeed * Time.deltaTime);
        }


        // Zoom in/out
        if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl))
        {
            float scroll = Input.mouseScrollDelta.y;
            distance = Mathf.Clamp(distance - scroll * zoomSpeed * Time.deltaTime, minDist, maxDist);
        }
        

        // Rotacija kamere mišem
        if (Input.GetMouseButton(2))
        {
            yaw += Input.GetAxis("Mouse X") * rotSpeedMouse * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * rotSpeedMouse * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
        }

        // Rotacija kamere strelicama
        if (Input.GetKey(KeyCode.LeftArrow)) yaw += rotSpeedKeys * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow)) yaw -= rotSpeedKeys * Time.deltaTime;
        if (Input.GetKey(KeyCode.UpArrow)) pitch += rotSpeedKeys * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) pitch -= rotSpeedKeys * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
        
        
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pos = center + rot * (Vector3.back * distance);
        transform.SetPositionAndRotation(pos, rot);
    }
}