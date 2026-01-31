using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Sensitivity")]
    public float sensitivityX = 2f;
    public float sensitivityY = 2f;

    [Header("Clamp Vertical Rotation")]
    public float minY = -80f;
    public float maxY = 80f;

    private float rotationY = 0f;

    void Update()
    {
        // Movimiento del mouse
        float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;

        // Rotación horizontal (yaw)
        transform.Rotate(Vector3.up * mouseX);

        // Rotación vertical (pitch)
        rotationY -= mouseY;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        // Aplicar rotación vertical
        transform.localEulerAngles = new Vector3(rotationY, transform.localEulerAngles.y, 0f);

        // Debugs
        //Debug.Log($"MouseX: {mouseX}, MouseY: {mouseY}, RotationY: {rotationY}");
    }
}