#if DEMO
using UnityEngine;
using UnityEngine.InputSystem;
using UnityGamingCamera.Runtime.gaming.camera.Scripts.Runtime.Components.Cameras;

namespace UnityGamingCamera.Demo.gaming.camera.Scripts.Demo
{
    [RequireComponent(typeof(FlyingPanningZoomCameraController))]
    public sealed class CameraInput : MonoBehaviour
    {
        private FlyingPanningZoomCameraController _cameraController;

        private void Awake()
        {
            _cameraController = GetComponent<FlyingPanningZoomCameraController>();
        }

        private void Update()
        {
            var f = Mouse.current.scroll.ReadValue().y;
            if (f != 0f)
            {
                _cameraController.Zoom(-f);
            }
        }
    }
}
#endif