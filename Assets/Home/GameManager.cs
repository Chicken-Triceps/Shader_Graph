using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("설정창 패널")]
    public GameObject settingsPanel;

    [Header("조명 설정")]
    public Light sunLight;          // 태양 (Directional Light)
    public Material daySkybox;      // 낮에 쓸 하늘 재질 (설정 안 해도 자동으로 잡음)

    [Header("플레이어 스크립트")]
    public CreativeMove playerScript;

    private bool isMenuOpen = false;
    private Light[] allLights;      // 씬에 있는 모든 전구를 담을 배열

    void Start()
    {
        // 시작할 때 현재 하늘(Skybox)을 기억해둠 (낮으로 돌아올 때 쓰려고)
        if (daySkybox == null) daySkybox = RenderSettings.skybox;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isMenuOpen = !isMenuOpen;
            settingsPanel.SetActive(isMenuOpen);

            Cursor.visible = isMenuOpen;
            Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;

            if (playerScript != null)
            {
                playerScript.canMove = !isMenuOpen;
            }
        }
    }

    // [낮] 버튼
    public void SetDay()
    {
        // 1. 하늘(Skybox) 복구
        RenderSettings.skybox = daySkybox;

        // 2. 환경광(Ambient) 복구 (밝은 회색)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.0f;

        // 3. 반사광(Reflection) 복구
        RenderSettings.reflectionIntensity = 1.0f;

        // 4. 안개(Fog) 복구 (원한다면 켜기)
        RenderSettings.fog = true;

        // 5. 모든 조명 켜기
        ToggleAllLights(true);

        // 6. 카메라 배경을 다시 하늘로 설정
        Camera.main.clearFlags = CameraClearFlags.Skybox;

        Debug.Log("낮: 광명 찾음");
    }

    // [밤] 버튼 (완전 암전)
    public void SetNight()
    {
        // 1. 하늘(Skybox) 없애기
        RenderSettings.skybox = null;

        // 2. 환경광(Ambient) 완전 차단 (검은색)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;

        // 3. 반사광(Reflection) 끄기 (벽이 반짝이는 것 방지)
        RenderSettings.reflectionIntensity = 0f;

        // 4. 안개(Fog) 끄기 (안개가 있으면 회색으로 보일 수 있음)
        RenderSettings.fog = false;

        // 5. 씬에 있는 '모든' 조명 끄기 (태양 포함 전부)
        ToggleAllLights(false);

        // 6. 카메라 배경을 '단색 검정'으로 강제 변경 (가장 중요!)
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = Color.black;

        Debug.Log("밤: 칠흑 같은 어둠");
    }

    // 씬에 있는 모든 Light 컴포넌트를 찾아서 끄거나 켜는 함수
    void ToggleAllLights(bool isOn)
    {
        // 현재 씬의 모든 조명을 찾아옴
        allLights = FindObjectsOfType<Light>();

        foreach (Light light in allLights)
        {
            light.enabled = isOn;
        }
    }
}