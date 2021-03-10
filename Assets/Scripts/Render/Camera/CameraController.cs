using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    /// <summary>
    /// Objects with this layer won't cancel camera control
    /// when mouse pointer is over them
    /// </summary>
    private int DontDisableCameraControlLayer;
    [SerializeField]
    private Canvas CanvasComponent;

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
            Vector3 cameraTranslation = -mouseMovement * Time.unscaledDeltaTime * (1f / CanvasComponent.scaleFactor);
            CameraComponent.transform.Translate(cameraTranslation);
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
        float zoom = Input.GetAxis("Mouse ScrollWheel") * Time.unscaledDeltaTime * CameraZoomSpeed;
        CameraComponent.orthographicSize -= zoom;
        CameraComponent.orthographicSize = Mathf.Clamp(CameraComponent.orthographicSize, CAMERA_MAX_ZOOM, CAMERA_MIN_ZOOM);
    }

    private void Awake()
    {
        CameraComponent = GetComponent<Camera>();
    }

    private void Start()
    {
        CameraDefaultZoom = CameraComponent.orthographicSize;
        string layerName = "DontDisableCameraControl";
        DontDisableCameraControlLayer = LayerMask.NameToLayer(layerName);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (-1 == DontDisableCameraControlLayer)
        {
            string debugMsg = string.Format(
                "[{0}] Could not get layer \"{1}\" by name. Holding mouse pointer over all UI elements will disable camera control",
                this.GetType().Name, layerName);
            Debug.Log(debugMsg);
        }
#endif
    }

    /// <summary>
    /// Checks if camera control should be active this frame
    /// </summary>
    private bool GetCameraControlActive()
    {
        bool result = true;

        if (-1 != DontDisableCameraControlLayer)
        {
            PointerEventData data = new PointerEventData(EventSystem.current);
            data.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);

            //Dont change camera position when mouse is over UI element unless it has
            //specific layer. It is use to prevent situation like zooming when user is scrolling list
            if (results.Count > 0)
            {
                foreach (RaycastResult rayResult in results)
                {
                    if (rayResult.gameObject.layer != DontDisableCameraControlLayer)
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
            {
                result = false;
            }
        }
        else
        {
            result = false;
        }

        return result;
    }

    private void Update()
    {
        //Whether camera control should be enabled this frame
        bool cameraControlActive = GetCameraControlActive();

        if (true == cameraControlActive)
        {
            SetCameraPosition();
            SetCameraZoom();
        }

        LastMousePosition = Input.mousePosition;
    }

    /*Public methods*/
}
