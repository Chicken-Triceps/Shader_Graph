using UnityEngine;

public class ObjectFloater : MonoBehaviour
{
    [Header("회전 설정")]
    [Tooltip("1초에 몇 도씩 회전할지 설정합니다.")]
    public float rotationSpeed = 50f;

    [Header("둥둥 뜨는 설정 (Bobbing)")]
    [Tooltip("위아래로 움직이는 속도입니다.")]
    public float floatSpeed = 2f;

    [Tooltip("위아래로 움직이는 범위(높이)입니다.")]
    public float floatHeight = 0.2f;

    private Vector3 startPos;

    void Start()
    {
        // 게임이 시작될 때의 원래 위치를 기억해둡니다.
        startPos = transform.position;
    }

    void Update()
    {
        // 1. 회전 구현 (Y축 기준)
        // Vector3.up은 (0, 1, 0)을 의미합니다.
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // 2. 위아래 흔들림 구현 (Mathf.Sin 함수 사용)
        // Sin 함수는 -1과 1 사이를 부드럽게 오가기 때문에 흔들리는 효과에 딱입니다.
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        // X, Z는 그대로 두고 Y값만 부드럽게 갱신
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}