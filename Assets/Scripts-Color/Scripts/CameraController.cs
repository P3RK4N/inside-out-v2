using UnityEngine;

public class CameraController : MonoBehaviour
{
    static float moveSpeed = 5.0f;
    static float mouseSensitivity = 360.0f;
    static float upDownRange = 60.0f;

    void Update()
    {
        // Mouse rotation
        float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, horizontalRotation * Time.deltaTime, 0, Space.World);

        float verticalRotation = -Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        transform.Rotate(transform.right, verticalRotation * Time.deltaTime, Space.World);


        float forward = Input.GetAxis("Vertical") * moveSpeed;
        float right = Input.GetAxis("Horizontal") * moveSpeed;
        float y = 0;
        if(Input.GetKey(KeyCode.LeftShift)) y -= moveSpeed;
        if(Input.GetKey(KeyCode.Space)) y += moveSpeed;

        transform.Translate((forward * transform.forward + right * transform.right + new Vector3(0, y, 0)) * Time.deltaTime, Space.World);
    }
}