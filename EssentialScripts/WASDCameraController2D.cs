using UnityEngine;

/// <summary>
/// Simple camera controller for 2D strategy games.
/// Allows panning using WASD or arrow keys and
/// zooming in/out with the mouse scroll wheel.
/// </summary>
[RequireComponent(typeof(Camera))]
public class WASDCameraController2D : MonoBehaviour
{
    [Tooltip("Speed of camera movement in units per second")]
    public float panSpeed = 10f;

    [Tooltip("Speed multiplier for zooming with the scroll wheel")]
    public float zoomSpeed = 5f;

    [Tooltip("Minimum orthographic size for zooming in")]
    public float minZoom = 2f;

    [Tooltip("Maximum orthographic size for zooming out")]
    public float maxZoom = 20f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector3 delta = new Vector3(moveX, moveY, 0f) * panSpeed * Time.deltaTime;
        transform.position += delta;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(scroll, 0f))
            return;

        float size = cam.orthographicSize - scroll * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(size, minZoom, maxZoom);
    }

    private void OnValidate()
    {
        if (maxZoom < minZoom)
            maxZoom = minZoom;
    }
}
