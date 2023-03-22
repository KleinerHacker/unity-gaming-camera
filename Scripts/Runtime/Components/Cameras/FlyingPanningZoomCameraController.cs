using UnityAnimation.Runtime.animation.Scripts.Runtime.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityGamingCamera.Runtime.gaming.camera.Scripts.Runtime.Components.Cameras
{
    [AddComponentMenu(UnityGamingCameraConstants.Root + "/Flying Panning Zoom Camera Controller")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class FlyingPanningZoomCameraController : MonoBehaviour
    {
        #region Inspector Data

        [Header("Behavior")]
        [FormerlySerializedAs("maxHigh")]
        [SerializeField]
        private float relativeMaxHigh = 10f;

        [FormerlySerializedAs("minHigh")]
        [SerializeField]
        private float relativeMinHigh = 1f;

        [SerializeField]
        [Tooltip("If this field is TRUE camera uses raycast to find underlying colliders to place camera over it (relative high). If it is set to FALSE " +
                 "the given high is absolute. This option should be used on terrains")]
        private bool useHighDetection = true;

        [SerializeField]
        [Range(0f, 90f)]
        private float maxHighRotation = 45f;

        [SerializeField]
        [Range(0f, 90f)]
        private float minHighRotation = 10f;

        [Space]
        [FormerlySerializedAs("levelCount")]
        [SerializeField]
        private byte highLevelCount = 5;

        [FormerlySerializedAs("startLevel")]
        [SerializeField]
        private byte startHighLevel = 4;

        [Space]
        [Tooltip("Allow or forbid horizontal movement and rotation")]
        [SerializeField]
        private bool allowFreeMoving = true;

        [Space]
        [SerializeField]
        private CameraBorder border;

        [Space]
        [SerializeField]
        private LayerMask colliderLayerMask;

        [SerializeField]
        private bool useColliderTag;

        [SerializeField]
        private string colliderTag;

        [Header("Animation")]
        [SerializeField]
        private AnimationCurve highChangeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField]
        [Range(0.01f, 1f)]
        private float highChangeSpeed = 0.1f;

        #endregion

        private AnimationRunner _cameraHeightRunner;

        private byte _currentHighLevel;

        #region Builtin Methods

        private void Awake()
        {
            _currentHighLevel = startHighLevel;
            UpdateCameraHeight(true);
        }

        #endregion

        public void Move(float deltaX, float deltaY)
        {
            var t = transform;
            var newPos = t.rotation * new Vector3(allowFreeMoving ? deltaX : 0f, 0f, deltaY) + t.position;
            if (border != null && !border.InBox(newPos))
                return;
            transform.position = new Vector3(newPos.x, CalculateCameraHeight(newPos) ?? newPos.y, newPos.z);
        }

        public void Rotation(float delta)
        {
            if (!allowFreeMoving)
                return;

            var rot = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(rot.x, rot.y - delta, rot.z);
        }

        public void Zoom(float value)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (value < 0f && _currentHighLevel > 0)
            {
                _currentHighLevel--;
            }
            else if (value > 0f && _currentHighLevel < highLevelCount - 1)
            {
                _currentHighLevel++;
            }

            UpdateCameraHeight();
        }

        private void UpdateCameraHeight(bool immediately = false)
        {
            var rotationDif = maxHighRotation - minHighRotation;
            var rotation = minHighRotation + rotationDif / highLevelCount * _currentHighLevel;

            var t = transform;

            var startPosition = t.position;
            var targetPosition = new Vector3(startPosition.x, CalculateCameraHeight(startPosition) ?? startPosition.y, startPosition.z);

            var startRotation = t.rotation;
            var targetRotation = Quaternion.Euler(rotation, startRotation.eulerAngles.y, startRotation.eulerAngles.z);

            if (immediately)
            {
                t.position = targetPosition;
                t.rotation = targetRotation;

                return;
            }

            _cameraHeightRunner?.Stop();
            _cameraHeightRunner = AnimationBuilder.Create(this)
                .Animate(highChangeCurve, highChangeSpeed, v =>
                {
                    transform.position = Vector3.Lerp(startPosition, targetPosition, v);
                    transform.rotation = Quaternion.Lerp(startRotation, targetRotation, v);
                })
                .WithFinisher(() => _cameraHeightRunner = null)
                .Start();
        }

        private float? RaycastCollisionHeight(Vector3 pos)
        {
            if (!useHighDetection)
                return 0f;

            if (Physics.Raycast(new Vector3(pos.x, 1000f, pos.z), Vector3.down, out var hit, float.MaxValue, colliderLayerMask))
            {
                if (useColliderTag && !hit.collider.CompareTag(colliderTag))
                    return null;

                return hit.point.y;
            }

            return null;
        }

        private float? CalculateCameraHeight(Vector3 pos)
        {
            var highDif = relativeMaxHigh - relativeMinHigh;
            var high = relativeMinHigh + highDif / highLevelCount * _currentHighLevel;

            return RaycastCollisionHeight(pos) + high;
        }
    }
}