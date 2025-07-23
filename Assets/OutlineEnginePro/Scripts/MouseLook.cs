using UnityEngine;

namespace OutlineEnginePro
{

    public class MouseLook : MonoBehaviour
    {
        [Tooltip("Mouse sensitivity.")]
        public float mouseSensitivity = 100f;

        [Tooltip("The player's body Transform. The camera will rotate up/down, the body left/right.")]
        public Transform playerBody;

        private float xRotation = 0f;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * mouseX);
            }
            else
            {
                transform.parent.Rotate(Vector3.up * mouseX);
            }
        }
    }
}
