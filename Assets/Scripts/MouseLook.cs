using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 300f;
    public Transform playerBody;
    public bool invertY = false;

    private float xRotation = 0f;

    void Start()
    {
        LockCursor();
        Debug.Log("MouseLook started - курсор заблокирован");
    }
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (Time.timeScale > 0 && Cursor.lockState == CursorLockMode.Locked)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            if (invertY)
            {
                mouseY = -mouseY;
            }

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * mouseX);
            }
        }
    }
}