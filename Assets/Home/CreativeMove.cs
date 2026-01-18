using UnityEngine;

public class CreativeMove : MonoBehaviour
{
    public float moveSpeed = 5.0f;      // 이동 속도
    public float mouseSensitivity = 2.0f; // 마우스 감도

    public Transform cameraTransform;   // 카메라를 담을 변수
    private CharacterController controller; // 충돌 감지 이동 컴포넌트
    private float xRotation = 0f;       // 위아래 회전값 저장

    void Start()
    {
        // 1. 플레이어에 붙어있는 CharacterController 가져오기
        controller = GetComponent<CharacterController>();

        // 2. 마우스 커서를 화면 중앙에 고정하고 숨기기
        Cursor.lockState = CursorLockMode.Locked;

        // 3. 카메라가 할당 안 됐으면 자식 오브젝트에서 찾기
        if (cameraTransform == null)
        {
            cameraTransform = GetComponentInChildren<Camera>().transform;
        }
    }

    void Update()
    {
        // --- [1] 시점 회전 (마우스) ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 위아래 회전 (카메라만 회전)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 고개를 90도 이상 못 꺾게 제한
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 좌우 회전 (몸통 전체 회전)
        transform.Rotate(Vector3.up * mouseX);


        // --- [2] 이동 (WASD + Space/Shift) ---
        float x = Input.GetAxis("Horizontal"); // A, D
        float z = Input.GetAxis("Vertical");   // W, S
        float y = 0f;

        // 스페이스바 = 상승, 쉬프트 = 하강 (마인크래프트 크리에이티브 방식)
        if (Input.GetKey(KeyCode.Space)) y = 1f;
        if (Input.GetKey(KeyCode.LeftShift)) y = -1f;

        // 내가 바라보는 방향 기준으로 이동 벡터 계산
        Vector3 move = transform.right * x + transform.up * y + transform.forward * z;

        // CharacterController를 통해 이동 (충돌 처리 포함)
        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}