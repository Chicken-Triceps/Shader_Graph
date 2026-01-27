using UnityEngine;

public class SensorLight : MonoBehaviour
{
    [Header("설정")]
    public Transform player;        // 플레이어 (거리를 잴 대상)
    public float detectionRange = 3.0f; // 감지 거리 (미터)
    public float lightDuration = 3.0f;  // 불이 켜져있는 시간 (3초)

    [Header("연결 대상")]
    public Light pointLight;        // 실제 빛 (Point Light)
    public Renderer bulbRenderer;   // 전구 3D 모델 (Mesh Renderer)

    [Header("색상 설정")]
    public Color onEmissionColor = Color.white; // 켜졌을 때 색

    private Material bulbMat;
    private float timer = 0f;
    private bool isOn = false;

    void Start()
    {
        // 1. 플레이어를 자동으로 찾기 (태그가 Player인 경우)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // 2. 전구 머티리얼 가져오기
        if (bulbRenderer != null)
        {
            bulbMat = bulbRenderer.material;
        }

        // 3. 시작 시 끄기 (기본값 Off)
        TurnOff();
    }

    void Update()
    {
        if (player == null) return;

        // 플레이어와 센서등 사이의 거리 계산
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            // 감지 범위 내에 있으면: 불을 켜고 타이머를 계속 3초로 리셋 (계속 켜둠)
            if (!isOn) TurnOn();
            timer = lightDuration;
        }
        else
        {
            // 감지 범위를 벗어나면: 타이머를 줄임
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else if (isOn)
            {
                // 타이머가 0이 되면 끔
                TurnOff();
            }
        }
    }

    void TurnOn()
    {
        isOn = true;

        // 1. 실제 조명 켜기
        if (pointLight != null) pointLight.enabled = true;

        // 2. 전구 머티리얼 Emission 켜기 (흰색)
        if (bulbMat != null)
        {
            bulbMat.EnableKeyword("_EMISSION"); // 이 키워드를 켜야 빛이 남
            bulbMat.SetColor("_EmissionColor", onEmissionColor);
            DynamicGI.UpdateEnvironment(); // 주변 환경광 갱신
        }
    }

    void TurnOff()
    {
        isOn = false;

        // 1. 실제 조명 끄기
        if (pointLight != null) pointLight.enabled = false;

        // 2. 전구 머티리얼 Emission 끄기 (검은색)
        if (bulbMat != null)
        {
            bulbMat.SetColor("_EmissionColor", Color.black);
            DynamicGI.UpdateEnvironment();
        }
    }

    // 에디터에서 감지 범위를 눈으로 보기 위한 기능
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}