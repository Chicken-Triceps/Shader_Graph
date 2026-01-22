using UnityEngine;

public class KitchenLightControl : MonoBehaviour
{
    [Header("연결 대상")]
    [Tooltip("주방등 3D 모델 2개를 여기에 넣으세요 (Mesh Renderer가 있는 것)")]
    public Renderer[] lampRenderers;

    [Tooltip("실제 빛을 내는 Point Light 2개를 여기에 넣으세요")]
    public Light[] realLights;

    [Header("머티리얼 설정")]
    public Material onMaterial;  // 불 켜진 재질
    public Material offMaterial; // 불 꺼진 재질

    // 현재 불이 켜져있는지 확인하는 변수
    private bool isOn = false;

    void Start()
    {
        // 게임 시작 시, 현재 상태(isOn)에 맞춰서 불을 셋팅함
        UpdateLights();
    }

    // 버튼을 누르면 이 함수가 실행됨
    public void ToggleLights()
    {
        isOn = !isOn; // 켜짐 <-> 꺼짐 상태 반전
        UpdateLights();
    }

    void UpdateLights()
    {
        // 1. 머티리얼 교체 (겉모습)
        foreach (Renderer rend in lampRenderers)
        {
            if (rend != null)
                rend.material = isOn ? onMaterial : offMaterial;
        }

        // 2. 실제 조명 끄기/켜기 (Point Light)
        foreach (Light light in realLights)
        {
            if (light != null)
                light.enabled = isOn;
        }
    }
}