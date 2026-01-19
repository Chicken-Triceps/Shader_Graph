using UnityEngine;

public class CreativeMove : MonoBehaviour
{
    // 변수 이름은 그대로 canMove를 쓰지만, 실제로는 '시점 회전 가능 여부'로 쓸게요
    public bool canMove = true;

    public float moveSpeed = 5.0f;
    public float mouseSensitivity = 2.0f;

    public Transform cameraTransform;
    private CharacterController controller;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        if (cameraTransform == null)
        {
            cameraTransform = GetComponentInChildren<Camera>().transform;
        }
    }

    void Update()
    {
        // -----------------------------------------------------------
        // [1] 시점 회전 (마우스) -> ★여기에만 if문을 씌웁니다★
        // -----------------------------------------------------------
        if (canMove == true)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // 위아래 회전
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // 좌우 회전 (몸통)
            transform.Rotate(Vector3.up * mouseX);
        }

        // -----------------------------------------------------------
        // [2] 이동 (WASD) -> ★if문 밖으로 뺐습니다 (항상 작동)★
        // -----------------------------------------------------------
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float y = 0f;

        if (Input.GetKey(KeyCode.Space)) y = 1f;
        if (Input.GetKey(KeyCode.LeftShift)) y = -1f;

        Vector3 move = transform.right * x + transform.up * y + transform.forward * z;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}