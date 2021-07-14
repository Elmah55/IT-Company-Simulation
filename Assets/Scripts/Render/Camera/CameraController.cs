using ITCompanySimulation.Utilities;
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
    /// <summary>
    /// Stored last direction that camera was moving to continue camera movement
    /// in that direction for some time after player stopped moving mouse (smooth
    /// camera effect)
    /// </summary>
    private Vector2 LastCameraDirection;
    /// <summary>
    /// Controls how smooth camera movement should. The bigger the value
    /// the longer camera will continue movement
    /// </summary>
    private float LastCameraDirectionMovementFactor;
    private float LastCameraZoom;
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

    [Range(1f, 10f)]
    public float CameraMovementSpeed;
    [Range(0f, 10f)]
    public float CameraMovementSmoothness;
    [Range(1f, 10f)]
    public float CameraZoomSpeed;
    [Range(0f, 10f)]
    public float CameraZoomSmoothness;
    [Tooltip("Camera distance limit will be measured from this point")]
    public Vector2 CameraCenterPoint;
    [Tooltip("Indicates how far camera can move from center point")]
    public float CameraDistanceLimit;

    /*Private methods*/

    /// <summary>
    /// Checks for camera position position and returns amount by how much limit distance was exceeded.
    /// Returns 0 if distance limit was not exceeded, otherwise returns amount that limit was exceeded by.
    /// </summary>
    private float CheckCameraDistanceLimit()
    {
        float result = 0f;
        Vector2 camerPos = CameraComponent.transform.position;
        /// <summary>
        /// Value used to calcute distance limit based on camera zoom.
        /// Used camera is ortographic. That means when zoom of camera
        /// changes size of tilemap and sprites will change but distance
        /// between unity game objects will remain the same. To have same
        /// distance limit regardless of zoom this value needs to be taking
        /// into consideration.
        /// </summary>
        float zoomFactor = CAMERA_MIN_ZOOM / CameraComponent.orthographicSize;
        float distanceFromCenterPoint = Vector2.Distance(CameraCenterPoint, camerPos);

        if (distanceFromCenterPoint > CameraDistanceLimit * zoomFactor)
        {
            //Vector from camera position to center point
            Vector2 centerPointVector = CameraCenterPoint - camerPos;
            result = centerPointVector.magnitude - CameraDistanceLimit;
            result /= zoomFactor;
        }

        return result;
    }

    /// <summary>
    /// Moves camera based on user input
    /// </summary>
    private void SetCameraPosition()
    {
        float overLimitAmount = CheckCameraDistanceLimit();

        if (0f != CameraMovementSmoothness)
        {
            LastCameraDirectionMovementFactor -= (2f / CameraMovementSmoothness) * Time.unscaledDeltaTime;
            LastCameraDirectionMovementFactor = Mathf.Clamp(LastCameraDirectionMovementFactor, 0f, 1f);
            LastCameraDirection *= LastCameraDirectionMovementFactor;
            CameraComponent.transform.Translate(LastCameraDirection);
        }

        if (true == Input.GetMouseButton(RIGHT_MOUSE_BUTTON))
        {
            Vector2 mouseMovement = (Vector2)Input.mousePosition - LastMousePosition;
            mouseMovement *= (CameraMovementSpeed / 100f) * CameraComponent.orthographicSize;
            LastCameraDirection = -mouseMovement * Time.unscaledDeltaTime * (1f / CanvasComponent.scaleFactor);
            LastCameraDirectionMovementFactor = 1f - (overLimitAmount / 3f);

            if (0f == CameraMovementSmoothness)
            {
                CameraComponent.transform.Translate(LastCameraDirection);
            }
        }
        else if (overLimitAmount > 0f)
        {
            //Vector from camera position to center point
            Vector2 centerPointVector = CameraCenterPoint - (Vector2)CameraComponent.transform.position;
            LastCameraDirection = centerPointVector.normalized;
            LastCameraDirectionMovementFactor = overLimitAmount / 100f;
        }

        if (CameraPreviousZoom != CameraComponent.orthographicSize)
        {
            CameraPreviousZoom = CameraComponent.orthographicSize;
        }
    }

    /// <summary>
    /// Sets camera zoom based on user input
    /// </summary>
    private void SetCameraZoom()
    {
        float cameraZoom = Input.GetAxis("Mouse ScrollWheel") * Time.unscaledDeltaTime * CameraZoomSpeed * 10f;

        if (0f != cameraZoom)
        {
            LastCameraZoom = cameraZoom;
        }

        CameraComponent.orthographicSize -= 0f != CameraZoomSmoothness ? LastCameraZoom : cameraZoom;
        CameraComponent.orthographicSize = Mathf.Clamp(CameraComponent.orthographicSize, CAMERA_MAX_ZOOM, CAMERA_MIN_ZOOM);

        //This calculations are needed only when camera zoom smoothness is enabled
        if (0f != CameraZoomSmoothness)
        {
            float cameraZoomChange = (1f / CameraZoomSmoothness) * Time.unscaledDeltaTime;

            if (0f != LastCameraZoom)
            {
                LastCameraZoom = LastCameraZoom > 0 ? (LastCameraZoom - cameraZoomChange) : (LastCameraZoom + cameraZoomChange);
            }

            //Take floating point error into consideration
            if (Mathf.Abs(LastCameraZoom) <= 0.02f)
            {
                LastCameraZoom = 0f;
            }

            LastCameraZoom = Mathf.Clamp(LastCameraZoom, -1f, 1f);
        }
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
        bool result = Utils.MouseInsideScreen();

        if (-1 != DontDisableCameraControlLayer && true == result)
        {
            PointerEventData data = new PointerEventData(EventSystem.current);
            data.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);

            //Dont change camera position when mouse is over UI element unless it has
            //specific layer. It is use to prevent situation like zooming when user is scrolling list
            foreach (RaycastResult rayResult in results)
            {
                if (rayResult.gameObject.layer != DontDisableCameraControlLayer)
                {
                    result = false;
                    break;
                }
            }
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
