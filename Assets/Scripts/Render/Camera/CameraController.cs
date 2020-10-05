using UnityEngine;

public class CameraController : MonoBehaviour
{
    /*Private consts fields*/

    private const int RIGHT_MOUSE_BUTTON = 1;
    //Smaller value means bigger zoom
    private const float CAMERA_MAX_ZOOM = 1.5f;
    private const float CAMERA_MIN_ZOOM = 6f;

    /*Private fields*/

    private Camera CameraComponent;
    private Vector2 LastMousePosition;

    /*Public consts fields*/

    /*Public fields*/

    public float CameraMovementSpeed;
    public float CameraZoomSpeed;
    /// <summary>
    /// Bounds defining possible camera movment
    /// </summary>
    public Bounds CameraPostionBounds;

    /*Private methods*/

    private void SetCameraPosition()
    {
        if (true == Input.GetMouseButton(RIGHT_MOUSE_BUTTON))
        {
            Vector2 mouseMovement = (Vector2)Input.mousePosition - LastMousePosition;
            mouseMovement *= CameraMovementSpeed * CameraComponent.orthographicSize;
            CameraComponent.transform.Translate(-mouseMovement * Time.deltaTime);
            Vector2 camerPos = CameraComponent.transform.position;
            camerPos.x = Mathf.Clamp(camerPos.x, CameraPostionBounds.min.x, CameraPostionBounds.max.x);
            camerPos.y = Mathf.Clamp(camerPos.y, CameraPostionBounds.min.y, CameraPostionBounds.max.y);
            //Setting position like that makes camera not render tilemap
            //CameraComponent.transform.position = camerPos;
        }
    }

    private void SetCameraZoom()
    {
        float zoom = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * CameraZoomSpeed;
        CameraComponent.orthographicSize -= zoom;
        CameraComponent.orthographicSize = Mathf.Clamp(CameraComponent.orthographicSize, CAMERA_MAX_ZOOM, CAMERA_MIN_ZOOM);
    }

    private void Start()
    {
        CameraComponent = GetComponent<Camera>();
    }

    private void Update()
    {
        SetCameraPosition();
        SetCameraZoom();
        LastMousePosition = Input.mousePosition;
    }

    /*Public methods*/
}
