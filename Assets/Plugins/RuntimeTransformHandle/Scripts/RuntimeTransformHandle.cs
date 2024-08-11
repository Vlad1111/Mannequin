using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RuntimeHandle
{
    /**
     * Created by Peter @sHTiF Stefcek 21.10.2020
     */
    public class RuntimeTransformHandle : MonoBehaviour
    {
        public static RuntimeTransformHandle Instance;
        private void Awake()
        {
            Instance = this;
        }
        public HandleAxes axes = HandleAxes.XYZ;
        public HandleSpace space = HandleSpace.LOCAL;
        public HandleType type = HandleType.POSITION;
        public HandleSnappingType snappingType = HandleSnappingType.RELATIVE;

        public Vector3 positionSnap = Vector3.zero;
        public float rotationSnap = 0;
        public Vector3 scaleSnap = Vector3.zero;

        public bool autoScale = false;
        public float autoScaleFactor = 1;
        public Camera handleCamera;
        private float initialCameraFOV;

        private Vector3 _previousMousePosition;
        private HandleBase _previousAxis;

        private HandleBase _draggingHandle;

        private HandleType _previousType;
        private HandleAxes _previousAxes;

        private PositionHandle _positionHandle;
        private RotationHandle _rotationHandle;
        private ScaleHandle _scaleHandle;

        public Transform target;

        public UnityEvent startedDraggingHandle = new UnityEvent();
        public UnityEvent isDraggingHandle = new UnityEvent();
        public UnityEvent endedDraggingHandle = new UnityEvent();

        [SerializeField] private bool disableWhenNoTarget;

        void Start()
        {
            if (handleCamera == null)
                handleCamera = Camera.main;
            initialCameraFOV = handleCamera.fieldOfView;

            _previousType = type;

            if (target == null)
                target = transform;

            if (disableWhenNoTarget && target == transform)
                gameObject.SetActive(false);

            CreateHandles();
            gameObject.SetActive(false);
        }

        void CreateHandles()
        {
            switch (type)
            {
                case HandleType.POSITION:
                    _positionHandle = gameObject.AddComponent<PositionHandle>().Initialize(this);
                    break;
                case HandleType.ROTATION:
                case HandleType.ROTATION_Y:
                    _rotationHandle = gameObject.AddComponent<RotationHandle>().Initialize(this, type);
                    break;
                case HandleType.SCALE:
                    _scaleHandle = gameObject.AddComponent<ScaleHandle>().Initialize(this);
                    break;
            }
            ApplyLayerToChildren();
        }

        void Clear()
        {
            _draggingHandle = null;

            if (_positionHandle) _positionHandle.Destroy();
            if (_rotationHandle) _rotationHandle.Destroy();
            if (_scaleHandle) _scaleHandle.Destroy();
        }

        public static void ApplyLayerToChildren(Transform parentGameObj = null, int parentCount = 0)
        {
            if (parentGameObj == null) parentGameObj = Instance.transform;

            foreach (Transform transform1 in parentGameObj.transform)
            {
                int layer = parentGameObj.gameObject.layer;
                transform1.gameObject.layer = layer;
                ApplyLayerToChildren(transform1, parentCount + 1);
            }
        }

        void Update()
        {
            if (autoScale)
            {
                transform.localScale =
                     (handleCamera.fieldOfView / initialCameraFOV) * Vector3.one * (Vector3.Distance(handleCamera.transform.position, transform.position) * autoScaleFactor) / 15;
            }
            if (_previousType != type || _previousAxes != axes)
            {
                Clear();
                CreateHandles();
                _previousType = type;
                _previousAxes = axes;
                transform.rotation = target.transform.rotation;
            }
            if(target == null)
            {
                gameObject.SetActive(false);
                return;
            }
            transform.position = target.transform.position;

            if (space == HandleSpace.LOCAL || type == HandleType.SCALE)
            {
                if(space != HandleSpace.LOCAL || type != HandleType.POSITION || GetPointerUp())
                    transform.rotation = target.transform.rotation;
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }

            if (EventSystem.current.IsPointerOverGameObject() || Input.touches.Length > 1)
                return;

            HandleBase handle = null;
            Vector3 hitPoint = Vector3.zero;
            GetHandle(ref handle, ref hitPoint);

            HandleOverEffect(handle, hitPoint);

            if (GetPointerDown() && handle != null && handle.CanInteract(hitPoint))
            {
                _draggingHandle = handle;
                _draggingHandle.StartInteraction(hitPoint);
                startedDraggingHandle.Invoke();
            }


            if (PointerIsDown() && _draggingHandle != null)
            {
                _draggingHandle.Interact(_previousMousePosition);
                isDraggingHandle.Invoke();
            }

            if (GetPointerUp())
            {
                if (_draggingHandle != null)
                {
                    _draggingHandle.EndInteraction();
                    _draggingHandle = null;
                    endedDraggingHandle.Invoke();
                }
                _draggingHandle = null;
            }

            _previousMousePosition = GetMousePosition();
        }

        public static bool GetPointerDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        public static bool PointerIsDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.leftButton.isPressed;
#else
            return Input.GetMouseButton(0);
#endif
        }

        public static bool GetPointerUp()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.leftButton.wasReleasedThisFrame;
#else
            return Input.GetMouseButtonUp(0);
#endif
        }

        public static Vector3 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.position.ReadValue();
#else
            return Input.mousePosition;
#endif
        }

        void HandleOverEffect(HandleBase p_axis, Vector3 p_hitPoint)
        {
            if (_draggingHandle == null && _previousAxis != null && (_previousAxis != p_axis || !_previousAxis.CanInteract(p_hitPoint)))
            {
                _previousAxis.SetDefaultColor();
            }

            if (p_axis != null && _draggingHandle == null && p_axis.CanInteract(p_hitPoint))
            {
                var yellow = Color.yellow;
                yellow.a = 0.1f;
                p_axis.SetColor(yellow);
            }

            _previousAxis = p_axis;
        }

        private void GetHandle(ref HandleBase p_handle, ref Vector3 p_hitPoint)
        {
            Ray ray = Camera.main.ScreenPointToRay(GetMousePosition());
            RaycastHit[] hits = Physics.RaycastAll(ray);
            if (hits.Length == 0)
                return;

            Vector3 lastHitPoint = Vector3.zero;
            HandleBase lastHandleBase = null;
            foreach (RaycastHit hit in hits)
            {
                //p_handle = hit.collider.gameObject.GetComponentInParent<HandleBase>();
                //
                //if (p_handle != null)
                //{
                //    p_hitPoint = hit.point;
                //    return;
                //}
                var handle = hit.collider.gameObject.GetComponentInParent<HandleBase>();
                if(handle != null)
                {
                    if(lastHandleBase == null ||
                        (lastHitPoint - Camera.main.transform.position).magnitude >
                        (hit.point - Camera.main.transform.position).magnitude)
                    {
                        lastHandleBase = handle;
                        lastHitPoint = hit.point;
                    }
                }
            }
            p_handle = lastHandleBase;
            if(p_handle != null)
                p_hitPoint = lastHitPoint;
        }

        static public RuntimeTransformHandle Create(Transform p_target, HandleType p_handleType)
        {
            RuntimeTransformHandle runtimeTransformHandle = new GameObject().AddComponent<RuntimeTransformHandle>();
            runtimeTransformHandle.target = p_target;
            runtimeTransformHandle.type = p_handleType;

            return runtimeTransformHandle;
        }

        #region public methods to control handles
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetTarget(GameObject newTarget)
        {
            target = newTarget.transform;

            if (target == null)
                target = transform;

            if (disableWhenNoTarget && target == transform)
                gameObject.SetActive(false);
            else if(disableWhenNoTarget && target != transform)
                gameObject.SetActive(true);
        }

        public void SetHandleMode(int mode)
        {
            SetHandleMode((HandleType)mode);
        }

        public void SetHandleMode(HandleType mode)
        {
            type = mode;
        }

        public void EnableXAxis(bool enable)
        {
            if (enable)
                axes |= HandleAxes.X;
            else
                axes &= ~HandleAxes.X;
        }

        public void EnableYAxis(bool enable)
        {
            if (enable)
                axes |= HandleAxes.Y;
            else
                axes &= ~HandleAxes.Y;
        }

        public void EnableZAxis(bool enable)
        {
            if (enable)
                axes |= HandleAxes.Z;
            else
                axes &= ~HandleAxes.Z;
        }

        public void SetAxis(HandleAxes newAxes)
        {
            axes = newAxes;
        }
        #endregion
    }
}