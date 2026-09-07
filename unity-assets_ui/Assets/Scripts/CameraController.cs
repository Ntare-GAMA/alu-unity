using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public float distance = 5f;
    public float height = 2f;
    public float mouseSensitivity = 2f;

    private float currentAngleY = 0f;
    private float currentAngleX = 0f;

    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            UnlockCursor();
            return;
        }

        LockCursor();

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentAngleY += mouseX;

        if (SettingsData.invertY)
        {
            currentAngleX += mouseY;
        }
        else
        {
            currentAngleX -= mouseY;
        }

        currentAngleX = Mathf.Clamp(currentAngleX, -20f, 80f);
    }

    void LateUpdate()
    {
        if (player == null) return;
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 offset = new Vector3(0, height, -distance);
        Quaternion rotation = Quaternion.Euler(currentAngleX, currentAngleY, 0);

        transform.position = player.position + rotation * offset;
        transform.LookAt(player.position);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}