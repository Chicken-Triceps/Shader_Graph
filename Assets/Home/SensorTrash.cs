using UnityEngine;

public class SensorTrash : MonoBehaviour
{
    [Header("감지 설정")]
    public Transform player;
    public Transform lidObject;
    public float detectionRange = 0.5f;
    public float autoCloseDelay = 3.0f;

    [Header("회전 설정")]
    public Vector3 openAngle = new Vector3(0, 0, -90);
    public float openSpeed = 5.0f;

    [Header("LED 설정 (Mesh Renderer)")]
    public Renderer ledRenderer; // Sphere의 Mesh Renderer 연결
    public Color openColor = Color.green; // 열림~대기 색상 (인스펙터 지정)
    public Color closeColor = Color.red;  // 닫힘 색상 (인스펙터 지정)

    private Material ledMat; // 머티리얼 인스턴스 저장용
    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private float closeTimer = 0f;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (lidObject != null)
        {
            closedRotation = lidObject.localRotation;
            targetRotation = closedRotation;
        }

        // [핵심] 머티리얼 가져오기 및 초기화
        if (ledRenderer != null)
        {
            // 런타임에 머티리얼 인스턴스를 복사해서 가져옴 (원본 수정 방지)
            ledMat = ledRenderer.material;
            // Emission 키워드가 확실히 켜져있도록 설정
            ledMat.EnableKeyword("_EMISSION");
            // 시작할 때 끄기 (검은색 = 빛 안남)
            UpdateLED(Color.black);
        }
    }

    void Update()
    {
        if (player == null || lidObject == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // 현재 뚜껑이 얼마나 열려있는지 각도 차이 계산 (0에 가까우면 닫힌 것)
        float angleDifference = Quaternion.Angle(lidObject.localRotation, closedRotation);

        // 1. 플레이어 감지됨 (열림 상태 유지)
        if (distance <= detectionRange)
        {
            targetRotation = closedRotation * Quaternion.Euler(openAngle);
            closeTimer = autoCloseDelay; // 타이머 계속 리셋

            // [LED] 초록색 Emission
            UpdateLED(openColor);
        }
        // 2. 플레이어 멀어짐 (닫기 대기 or 닫는 중)
        else
        {
            if (closeTimer > 0)
            {
                // 아직 닫기 전 대기 시간 (여전히 초록색)
                closeTimer -= Time.deltaTime;
                UpdateLED(openColor);
            }
            else
            {
                // 타이머 종료 -> 닫기 시작
                targetRotation = closedRotation;

                // [LED] 닫히는 중(각도가 벌어져 있음)이면 빨간색
                if (angleDifference > 1.0f)
                {
                    UpdateLED(closeColor);
                }
                else
                {
                    // 완전히 닫힘 -> LED 끄기 (검은색)
                    UpdateLED(Color.black);
                }
            }
        }

        // 뚜껑 회전
        lidObject.localRotation = Quaternion.Slerp(lidObject.localRotation, targetRotation, Time.deltaTime * openSpeed);
    }

    // LED 상태 관리 함수 (Emission 색상 변경)
    void UpdateLED(Color targetColor)
    {
        if (ledMat != null)
        {
            // Emission 색상을 타겟 색상으로 변경
            // Color.black을 넣으면 빛이 꺼지는 효과가 남
            ledMat.SetColor("_EmissionColor", targetColor);

            // 주변 환경광 즉시 갱신 (필요한 경우)
            DynamicGI.UpdateEnvironment();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}