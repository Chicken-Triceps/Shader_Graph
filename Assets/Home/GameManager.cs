using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI & Player Settings")]
    public GameObject settingsPanel;
    public CreativeMove playerScript;
    public Camera mainCamera; // [필수 추가] 메인 카메라를 여기에 넣으세요!

    [Header("Day/Night Ambient Colors")]
    public Color dayAmbientColor = new Color(0.8f, 0.8f, 0.8f);
    public Color nightAmbientColor = new Color(0.1f, 0.1f, 0.1f);

    [Header("Main Light (Sun)")]
    public Light sunLight;
    public Material daySkybox;

    [Header("Individual Lights")]
    public Light bedLight; public MeshRenderer bedMesh; private Color bedOriginColor;
    public Light deskLight; public MeshRenderer deskMesh; private Color deskOriginColor;
    public Light waveLight; public MeshRenderer waveMesh; private Color waveOriginColor;

    [Header("Kitchen System")]
    public Renderer[] kitchenRenderers;
    public Light[] kitchenLights;
    public Material kitchenOnMat;
    public Material kitchenOffMat;
    private bool isKitchenOn = false;

    [Header("Computer System")]
    public Light computerLight;
    public MeshRenderer monitorScreen;
    public GameObject monitorCanvasObject;
    public Material screenOffMat;
    public Material screenBlueMat;
    public Material screenNoiseMat;

    [Header("Speaker System")]
    public MeshRenderer speakerMesh;
    public Material speakerOnMat;
    public Material speakerOffMat;

    [Header("Size Highlight System")]
    public MeshRenderer highlightM_Mesh;
    public MeshRenderer highlightL_Mesh;
    public Material highlightOnMat;
    public Material highlightOffMat;

    [Header("Weather System")]
    public ParticleSystem rainSystem;
    public ParticleSystem snowSystem;
    private bool isWeatherOn = false;

    [Header("UI Button Images")]
    public Color activeBtnColor = new Color(1f, 1f, 0f);
    public Color inactiveBtnColor = new Color(1f, 1f, 1f);

    public Image btnTime00, btnTime06, btnTime12, btnTime18;
    public Image btnSeason3, btnSeason6, btnSeason9, btnSeason12;
    public Image btnBed, btnDesk, btnWave;
    public Image btnComputer, btnMonitor, btnSpeaker;
    public Image btnSizeM, btnSizeL;
    public Image btnKitchen;
    public Image btnRGB;
    public Image btnWeather;

    public enum Season { Spring, Summer, Autumn, Winter }
    public enum TimeSlot { Night_00, Morning_06, Noon_12, Evening_18 }

    private Season currentSeason = Season.Winter;
    private TimeSlot currentTime = TimeSlot.Noon_12;

    private bool isMonitorOn = false;
    private bool isComputerOn = false;
    private bool isSpeakerOn = false;
    private bool isSizeM_On = false;
    private bool isSizeL_On = false;
    private bool isRGBMode = false;

    private float hueValue = 0f;
    public float rgbSpeed = 0.5f;
    private float noiseOffset = 0f;
    private bool isMenuOpen = false;

    void Start()
    {
        // 카메라 자동 찾기 (혹시 연결 안 했을 때 대비)
        if (mainCamera == null) mainCamera = Camera.main;

        if (daySkybox == null) daySkybox = RenderSettings.skybox;

        if (bedMesh != null) bedOriginColor = bedMesh.material.GetColor("_EmissionColor");
        if (deskMesh != null) deskOriginColor = deskMesh.material.GetColor("_EmissionColor");
        if (waveMesh != null) waveOriginColor = waveMesh.material.GetColor("_EmissionColor");

        UpdateSeoulLight();
        UpdateMonitorScreen();
        UpdateSpeakerState();
        UpdateComputerLight();
        UpdateSizeHighlights();
        UpdateKitchenState();
        UpdateWeatherState();
        UpdateButtonColors();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isMenuOpen = !isMenuOpen;
            settingsPanel.SetActive(isMenuOpen);
            Cursor.visible = isMenuOpen;
            Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
            if (playerScript != null) playerScript.canMove = !isMenuOpen;
            if (isMenuOpen) UpdateButtonColors();
        }

        if (isMonitorOn && isComputerOn && monitorScreen != null)
        {
            noiseOffset += Time.deltaTime * 5.0f;
            screenNoiseMat.mainTextureOffset = new Vector2(Random.value, Random.value);
        }

        if (isComputerOn && isRGBMode && computerLight != null)
        {
            hueValue += Time.deltaTime * rgbSpeed;
            if (hueValue > 1.0f) hueValue = 0f;
            computerLight.color = Color.HSVToRGB(hueValue, 1f, 1f);
        }
    }

    // --- Button Actions ---
    public void SetTime00() { currentTime = TimeSlot.Night_00; UpdateSeoulLight(); UpdateButtonColors(); }
    public void SetTime06() { currentTime = TimeSlot.Morning_06; UpdateSeoulLight(); UpdateButtonColors(); }
    public void SetTime12() { currentTime = TimeSlot.Noon_12; UpdateSeoulLight(); UpdateButtonColors(); }
    public void SetTime18() { currentTime = TimeSlot.Evening_18; UpdateSeoulLight(); UpdateButtonColors(); }

    public void SetSeason3() { currentSeason = Season.Spring; UpdateSeoulLight(); UpdateWeatherState(); UpdateButtonColors(); }
    public void SetSeason6() { currentSeason = Season.Summer; UpdateSeoulLight(); UpdateWeatherState(); UpdateButtonColors(); }
    public void SetSeason9() { currentSeason = Season.Autumn; UpdateSeoulLight(); UpdateWeatherState(); UpdateButtonColors(); }
    public void SetSeason12() { currentSeason = Season.Winter; UpdateSeoulLight(); UpdateWeatherState(); UpdateButtonColors(); }

    public void ToggleBedLight() { ToggleLight(bedLight, bedMesh, bedOriginColor); UpdateButtonColors(); }
    public void ToggleDeskLight() { ToggleLight(deskLight, deskMesh, deskOriginColor); UpdateButtonColors(); }
    public void ToggleWaveLight() { ToggleLight(waveLight, waveMesh, waveOriginColor); UpdateButtonColors(); }

    public void ToggleComputerBtn()
    {
        isComputerOn = !isComputerOn;
        UpdateComputerLight(); UpdateMonitorScreen(); UpdateSpeakerState(); UpdateButtonColors();
    }
    public void ToggleMonitorBtn() { isMonitorOn = !isMonitorOn; UpdateMonitorScreen(); UpdateButtonColors(); }
    public void ToggleSpeakerBtn() { isSpeakerOn = !isSpeakerOn; UpdateSpeakerState(); UpdateButtonColors(); }
    public void ToggleSizeM() { isSizeM_On = !isSizeM_On; UpdateSizeHighlights(); UpdateButtonColors(); }
    public void ToggleSizeL() { isSizeL_On = !isSizeL_On; UpdateSizeHighlights(); UpdateButtonColors(); }
    public void ToggleKitchenBtn() { isKitchenOn = !isKitchenOn; UpdateKitchenState(); UpdateButtonColors(); }
    public void ToggleRGBBtn() { isRGBMode = !isRGBMode; if (!isRGBMode && computerLight != null) computerLight.color = Color.white; UpdateButtonColors(); }

    public void ToggleWeatherBtn()
    {
        isWeatherOn = !isWeatherOn;
        UpdateWeatherState();
        UpdateSeoulLight();
        UpdateButtonColors();
    }

    void UpdateWeatherState()
    {
        if (rainSystem == null || snowSystem == null) return;

        if (!isWeatherOn)
        {
            rainSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            snowSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return;
        }

        if (currentSeason == Season.Winter)
        {
            rainSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (!snowSystem.isPlaying) snowSystem.Play();
        }
        else
        {
            snowSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (!rainSystem.isPlaying) rainSystem.Play();
        }
    }

    void UpdateComputerLight() { if (computerLight != null) { computerLight.enabled = isComputerOn; if (isComputerOn && !isRGBMode) computerLight.color = Color.white; } }

    void UpdateKitchenState()
    {
        if (kitchenRenderers != null) foreach (Renderer rend in kitchenRenderers) if (rend != null) rend.material = isKitchenOn ? kitchenOnMat : kitchenOffMat;
        if (kitchenLights != null) foreach (Light light in kitchenLights) if (light != null) light.enabled = isKitchenOn;
    }

    // --- Seoul Lighting Logic ---
    void UpdateSeoulLight()
    {
        if (sunLight == null) return;
        float baseSouthY = -90.1f;
        float rotX = 0f; float rotY = 0f; float intensity = 1f; float shadowStrength = 1f;
        Color lightColor = Color.white; Color ambientColor = Color.black;

        // 1. 기본 조명 계산
        switch (currentTime)
        {
            case TimeSlot.Night_00:
                rotX = -60f; intensity = 0.1f; shadowStrength = 0f;
                lightColor = new Color(0.1f, 0.1f, 0.3f); ambientColor = new Color(0.05f, 0.05f, 0.1f); sunLight.shadows = LightShadows.None; break;
            case TimeSlot.Morning_06:
                sunLight.shadows = LightShadows.Soft; rotY = baseSouthY - 80f;
                if (currentSeason == Season.Summer) { rotX = 15f; intensity = 2.5f; shadowStrength = 0.7f; lightColor = new Color(1f, 0.9f, 0.8f); ambientColor = new Color(0.5f, 0.5f, 0.55f); }
                else if (currentSeason == Season.Winter) { rotX = -5f; intensity = 0.2f; shadowStrength = 0.1f; lightColor = new Color(0.2f, 0.1f, 0.3f); ambientColor = new Color(0.1f, 0.1f, 0.15f); }
                else { rotX = 5f; intensity = 2.0f; shadowStrength = 0.5f; lightColor = new Color(1f, 0.6f, 0.3f); ambientColor = new Color(0.25f, 0.25f, 0.3f); }
                break;
            case TimeSlot.Noon_12:
                sunLight.shadows = LightShadows.Soft; rotY = baseSouthY;
                if (currentSeason == Season.Summer) { rotX = 76f; intensity = 4.0f; shadowStrength = 0.6f; lightColor = Color.white; ambientColor = new Color(0.6f, 0.6f, 0.6f); }
                else if (currentSeason == Season.Winter) { rotX = 27.2f; intensity = 4.5f; shadowStrength = 0.55f; lightColor = new Color(1f, 0.98f, 0.9f); ambientColor = new Color(0.7f, 0.7f, 0.7f); }
                else { rotX = 52f; intensity = 3.8f; shadowStrength = 0.6f; lightColor = new Color(1f, 1f, 0.9f); ambientColor = new Color(0.6f, 0.6f, 0.6f); }
                break;
            case TimeSlot.Evening_18:
                sunLight.shadows = LightShadows.Soft; rotY = baseSouthY + 80f;
                if (currentSeason == Season.Summer) { rotX = 15f; intensity = 2.5f; shadowStrength = 0.6f; lightColor = new Color(1f, 0.8f, 0.6f); ambientColor = new Color(0.45f, 0.45f, 0.45f); }
                else if (currentSeason == Season.Winter) { rotX = -10f; intensity = 0.1f; shadowStrength = 0f; lightColor = new Color(0.1f, 0.1f, 0.2f); ambientColor = new Color(0.05f, 0.05f, 0.1f); }
                else { rotX = 2f; intensity = 1.5f; shadowStrength = 0.4f; lightColor = new Color(1f, 0.4f, 0.2f); ambientColor = new Color(0.25f, 0.2f, 0.25f); }
                break;
        }

        // 2. [핵심 수정] 날씨 ON -> 하늘 색상(Skybox) 교체 및 태양 끄기
        if (isWeatherOn && currentTime != TimeSlot.Night_00)
        {
            // (1) 태양 끄기 (Sun Disk 자체가 안 보이게 됨)
            sunLight.enabled = false;

            // (2) 카메라 배경을 "단색(Solid Color)"으로 변경
            // 이렇게 해야 파란 하늘 그림이 사라지고 어두운 회색이 됩니다.
            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                // 아주 어두운 회색 (비/눈 오는 하늘색)
                mainCamera.backgroundColor = new Color(0.25f, 0.25f, 0.28f);
            }

            // (3) 환경광(방 분위기) 어둡게
            ambientColor *= 0.3f;

            // (4) 안개(Fog) 켜기 (배경색과 맞춰서 자연스럽게)
            RenderSettings.fog = true;
            RenderSettings.fogDensity = 0.08f; // 안개 더 진하게
            RenderSettings.fogColor = new Color(0.25f, 0.25f, 0.28f); // 배경색과 일치
        }
        else
        {
            // 날씨 OFF -> 원래대로 복구

            // (1) 태양 다시 켜기
            sunLight.enabled = true;

            // (2) 카메라 배경을 다시 "Skybox"로 변경
            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.Skybox;
            }

            RenderSettings.fog = false;
        }

        // 3. 최종 적용
        sunLight.transform.rotation = Quaternion.Euler(rotX, rotY, 0);
        sunLight.color = lightColor;
        sunLight.intensity = intensity;
        sunLight.shadowStrength = shadowStrength;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
    }

    void UpdateButtonColors()
    {
        SetBtnColor(btnTime00, currentTime == TimeSlot.Night_00);
        SetBtnColor(btnTime06, currentTime == TimeSlot.Morning_06);
        SetBtnColor(btnTime12, currentTime == TimeSlot.Noon_12);
        SetBtnColor(btnTime18, currentTime == TimeSlot.Evening_18);

        SetBtnColor(btnSeason3, currentSeason == Season.Spring);
        SetBtnColor(btnSeason6, currentSeason == Season.Summer);
        SetBtnColor(btnSeason9, currentSeason == Season.Autumn);
        SetBtnColor(btnSeason12, currentSeason == Season.Winter);

        if (bedLight != null) SetBtnColor(btnBed, bedLight.enabled);
        if (deskLight != null) SetBtnColor(btnDesk, deskLight.enabled);
        if (waveLight != null) SetBtnColor(btnWave, waveLight.enabled);

        SetBtnColor(btnKitchen, isKitchenOn);
        SetBtnColor(btnComputer, isComputerOn);
        SetBtnColor(btnMonitor, isMonitorOn);
        SetBtnColor(btnSpeaker, isSpeakerOn);
        SetBtnColor(btnRGB, isRGBMode);
        SetBtnColor(btnSizeM, isSizeM_On);
        SetBtnColor(btnSizeL, isSizeL_On);
        SetBtnColor(btnWeather, isWeatherOn);
    }

    void SetBtnColor(Image btnImg, bool isActive) { if (btnImg != null) btnImg.color = isActive ? activeBtnColor : inactiveBtnColor; }
    void UpdateSizeHighlights() { if (highlightM_Mesh != null) highlightM_Mesh.material = isSizeM_On ? highlightOnMat : highlightOffMat; if (highlightL_Mesh != null) highlightL_Mesh.material = isSizeL_On ? highlightOnMat : highlightOffMat; }
    void UpdateSpeakerState() { if (speakerMesh == null) return; if (isSpeakerOn && isComputerOn) speakerMesh.material = speakerOnMat; else speakerMesh.material = speakerOffMat; }
    void UpdateMonitorScreen() { if (monitorScreen == null) return; if (!isMonitorOn) { monitorScreen.material = screenOffMat; if (monitorCanvasObject != null) monitorCanvasObject.SetActive(false); } else { if (!isComputerOn) { monitorScreen.material = screenBlueMat; if (monitorCanvasObject != null) monitorCanvasObject.SetActive(false); } else { monitorScreen.material = screenNoiseMat; if (monitorCanvasObject != null) monitorCanvasObject.SetActive(true); } } }
    void ToggleLight(Light lightObj, MeshRenderer meshObj, Color originColor) { if (lightObj != null) { lightObj.enabled = !lightObj.enabled; UpdateEmission(meshObj, originColor, lightObj.enabled); } }
    void UpdateEmission(MeshRenderer targetMesh, Color onColor, bool isOn) { if (targetMesh != null) { targetMesh.material.SetColor("_EmissionColor", isOn ? onColor : Color.black); DynamicGI.UpdateEnvironment(); } }
}