using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera fpsCamera;
    [SerializeField] private float lookSpeed = 0.5f;
    [SerializeField] private float lookLimit = 60.0f;
    [SerializeField] private float dashFov = 70f;
    [SerializeField] private float defaultFov = 60f; // Default FOV value


    private float rotation = 0.0f;

    public void HandleCameraRotation(Vector2 lookInput)
    {
        rotation += -lookInput.y * lookSpeed;
        rotation = Mathf.Clamp(rotation, -lookLimit, lookLimit);

        fpsCamera.transform.localRotation = Quaternion.Euler(rotation, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSpeed, 0);
    }

    public void DoFov()
    {
        fpsCamera.DOFieldOfView(dashFov, 0.25f);
    }
    public void ResetFov()
    {
        fpsCamera.DOFieldOfView(defaultFov, 0.25f);
    }
}