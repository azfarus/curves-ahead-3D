using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Settings")]
    public float sensitivity = 200f;
    public Transform cameraTransform;
    public float yRotationOffset = 0f;

    private float yRotation = 0f; // vertical rotation

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (RopeUIController.isMenuOpen) return;
        // Get mouse movement
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // ---- Vertical rotation (Look Up/Down) ----
        yRotation -= mouseY;
        yRotation = Mathf.Clamp(yRotation, -90f, 90f);
        

        // ---- Rotation Update ----
        cameraTransform.localRotation = Quaternion.Euler(yRotation+yRotationOffset, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}