using UnityEngine;
using DG.Tweening;

public class CameraControllerRB : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;
    public Camera cam;

    float xRotation;
    float yRotation;
    public float dashFov;
    public float defaultFov;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize rotations to the current camera rotation
        Vector3 currentRotation = transform.rotation.eulerAngles;
        xRotation = currentRotation.x;
        yRotation = currentRotation.y;
    }

    private void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }


    public void DoFov()
    {
        cam.DOFieldOfView(dashFov, 0.25f);
    }
    public void ResetFov()
    {
        cam.DOFieldOfView(defaultFov, 0.25f);
    }
}