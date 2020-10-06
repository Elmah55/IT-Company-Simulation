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
    private float CameraDefaultZoom;
    private float CameraPreviousZoom;

    /*Public consts fields*/

    /*Public fields*/

    public float CameraMovementSpeed;
    public float CameraZoomSpeed;
    /// <summary>
    /// Bounds defining possible camera movment
    /// </summary>
    public Bounds CameraPostionBounds;

    /*Private methods*/

    /// <summary>
    /// Checks for camera position position and if it exceeded defined bounds moves it inside bounds
    /// </summary>
    private void CheckCameraBounds()
    {
        Vector2 camerPos = CameraComponent.transform.position;
        //Take zoom into consideration when calculating bounds. The bigger the zoom is the bigger bounds should be
        float zoomBoundFactor = CameraComponent.orthographicSize > CameraDefaultZoom ?
            (1f / (CameraComponent.orthographicSize - CameraDefaultZoom)) : (0.7f * (CameraDefaultZoom - CameraComponent.orthographicSize));
        camerPos.x = Mathf.Clamp(camerPos.x, CameraPostionBounds.min.x * zoomBoundFactor, CameraPostionBounds.max.x * zoomBoundFactor);
        camerPos.y = Mathf.Clamp(camerPos.y, CameraPostionBounds.min.y * zoomBoundFactor, CameraPostionBounds.max.y * zoomBoundFactor);
        //Setting position like that makes camera not render tilemap
        CameraComponent.transform.position = camerPos;
    }

    /// <summary>
    /// Moves camera based on user input
    /// </summary>
    private void SetCameraPosition()
    {
        if (true == Input.GetMouseButton(RIGHT_MOUSE_BUTTON))
        {
            Vector2 mouseMovement = (Vector2)Input.mousePosition - LastMousePosition;
            mouseMovement *= CameraMovementSpeed * CameraComponent.orthographicSize;
            CameraComponent.transform.Translate(-mouseMovement * Time.deltaTime);
            CheckCameraBounds();
        }

        if (CameraPreviousZoom != CameraComponent.orthographicSize)
        {
            CheckCameraBounds();
            CameraPreviousZoom = CameraComponent.orthographicSize;
        }
    }

    /// <summary>
    /// Sets camera zoom based on user input
    /// </summary>
    private void SetCameraZoom()
    {
        float zoom = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * CameraZoomSpeed;
        CameraComponent.orthographicSize -= zoom;
        CameraComponent.orthographicSize = Mathf.Clamp(CameraComponent.orthographicSize, CAMERA_MAX_ZOOM, CAMERA_MIN_ZOOM);
    }

    private void Start()
    {
        CameraComponent = GetComponent<Camera>();
        CameraDefaultZoom = CameraComponent.orthographicSize;
    }

    private void Update()
    {
        SetCameraPosition();
        SetCameraZoom();
        LastMousePosition = Input.mousePosition;
    }

    /*Public methods*/
}
