using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("설정창 패널")]
    public GameObject settingsPanel;
    public CreativeMove playerScript;

    [Header("낮/밤 조명 색상 (여기서 조절하세요!)")]
    public Color dayAmbientColor = new Color(0.8f, 0.8f, 0.8f); // 낮에 쓸 화사한 색
    public Color nightAmbientColor = new Color(0.1f, 0.1f, 0.1f); // 밤에 쓸 어두운 색

    [Header("메인 조명 (태양)")]
    public Light sunLight;
    public Material daySkybox;
    
    // --- [개별 조명들] ---
    [Header("개별 조명 연결")]
    public Light bedLight;    public MeshRenderer bedMesh;    private Color bedOriginColor;
    public Light deskLight;   public MeshRenderer deskMesh;   private Color deskOriginColor;
    public Light waveLight;   public MeshRenderer waveMesh;   private Color waveOriginColor;

    private bool isMenuOpen = false;

    void Start()
    {
        if (daySkybox == null) daySkybox = RenderSettings.skybox;

        if (bedMesh != null) bedOriginColor = bedMesh.material.GetColor("_EmissionColor");
        if (deskMesh != null) deskOriginColor = deskMesh.material.GetColor("_EmissionColor");
        if (waveMesh != null) waveOriginColor = waveMesh.material.GetColor("_EmissionColor");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isMenuOpen = !isMenuOpen;
            settingsPanel.SetActive(isMenuOpen);
            Cursor.visible = isMenuOpen;
            Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
            if (playerScript != null) playerScript.canMove = !isMenuOpen; 
        }
    }

    // --- [핵심 수정] 낮 설정 ---
    public void SetDay()
    {
        // 1. 하늘 복구
        RenderSettings.skybox = daySkybox;
        
        // 2. [수정됨] 무조건 'Color' 모드로 변경 (라이팅 창 설정과 동일하게)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        
        // 3. [수정됨] 사용자가 지정한 '화사한 색' 적용
        RenderSettings.ambientLight = dayAmbientColor;

        if (sunLight != null) sunLight.enabled = true;
    }

    // --- 밤 설정 ---
    public void SetNight()
    {
        RenderSettings.skybox = null;
        
        // 밤에도 'Color' 모드 유지 (색깔만 어둡게 바꿈)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = nightAmbientColor;

        if (sunLight != null) sunLight.enabled = false;
    }

    // --- 조명 토글 (기존과 동일) ---
    public void ToggleBedLight() { ToggleLight(bedLight, bedMesh, bedOriginColor); }
    public void ToggleDeskLight() { ToggleLight(deskLight, deskMesh, deskOriginColor); }
    public void ToggleWaveLight() { ToggleLight(waveLight, waveMesh, waveOriginColor); }

    void ToggleLight(Light lightObj, MeshRenderer meshObj, Color originColor)
    {
        if (lightObj != null)
        {
            lightObj.enabled = !lightObj.enabled;
            UpdateEmission(meshObj, originColor, lightObj.enabled);
        }
    }

    void UpdateEmission(MeshRenderer targetMesh, Color onColor, bool isOn)
    {
        if (targetMesh != null)
        {
            targetMesh.material.SetColor("_EmissionColor", isOn ? onColor : Color.black);
            DynamicGI.UpdateEnvironment();
        }
    }
}