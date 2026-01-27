using UnityEngine;
using UnityEngine.UI;
using System.Collections; // [필수] 코루틴 사용을 위해 추가

public class GameManager : MonoBehaviour
{
    [Header("UI & Player Settings")]
    public GameObject settingsPanel;
    public CreativeMove playerScript;
    public Camera mainCamera;

    [Header("Day/Night Ambient Colors")]
    public Color dayAmbientColor = new Color(0.8f, 0.8f, 0.8f);
    public Color nightAmbientColor = new Color(0.1f, 0.1f, 0.1f);

    [Header("Main Light (Sun)")]
    public Light sunLight;
    public Material daySkybox;

    [Header("Main Room Light (Living Room)")]
    public Renderer[] mainRoomRenderers; // 네모난 등기구 모델 (Mesh Renderer)
    public Light[] mainRoomLights;       // 그 밑에 달린 Point Light
    public Material mainRoomOnMat;       // 켜진 재질 (Emission On)
    public Material mainRoomOffMat;      // 꺼진 재질 (Emission Off)
    private bool isMainRoomOn = false;   // 상태 변수

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

    // [추가됨] 뇌우 시스템 ====================================================
    [Header("Thunder System")]
    public AudioSource thunderAudio; // 천둥 소리 재생기
    public AudioClip thunderClip;    // 천둥 소리 파일
    private bool isThunderOn = false;
    private float thunderTimer = 0f; // 번개 칠 타이밍 계산
    // =======================================================================

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
    public Image btnMainLight;

    // [추가됨] 뇌우 버튼 이미지
    public Image btnThunder;

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
        UpdateMainRoomState();
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

        // 뇌우 로직: 랜덤한 시간마다 Lightning
        if (isWeatherOn && isThunderOn)
        {
            thunderTimer -= Time.deltaTime;
            if (thunderTimer <= 0f)
            {
                StartCoroutine(DoLightningEffect());
                // 다음 번개는 3초 ~ 10초 사이 랜덤한 시간에 침
                thunderTimer = Random.Range(3f, 10f);
            }
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

        // [중요] 날씨를 끄면 뇌우도 강제로 같이 꺼짐
        if (!isWeatherOn) isThunderOn = false;

        UpdateWeatherState();
        UpdateSeoulLight();
        UpdateButtonColors();
    }

    // [추가됨] 뇌우 버튼 함수
    public void ToggleThunderBtn()
    {
        // [핵심] 날씨(강수)가 꺼져있으면 아예 함수 종료 (아무 반응 없음)
        if (!isWeatherOn) return;

        isThunderOn = !isThunderOn;

        // 뇌우를 켜자마자 바로 한번 번쩍! (피드백)
        if (isThunderOn)
        {
            thunderTimer = Random.Range(0.5f, 2f); // 곧 번개 침
        }

        UpdateButtonColors();
    }

    // [추가됨] 번개 효과 코루틴 (번쩍임 + 소리)
    IEnumerator DoLightningEffect()
    {
        // 1. 천둥 소리 재생
        if (thunderAudio != null && thunderClip != null)
        {
            thunderAudio.PlayOneShot(thunderClip);
        }

        // 2. 번개 번쩍 (어두운 배경 -> 흰색 배경 -> 다시 어두움)
        // 기존 상태 저장
        Color originalBG = mainCamera.backgroundColor;

        // 번개 (흰색 배경)
        mainCamera.backgroundColor = new Color(0.8f, 0.8f, 0.9f);
        sunLight.enabled = true; // 태양 잠깐 켜서 그림자 만들기
        sunLight.intensity = 5f; // 아주 밝게

        yield return new WaitForSeconds(0.1f); // 0.1초 유지

        // 원상 복구 (어두움)
        mainCamera.backgroundColor = originalBG;
        sunLight.enabled = false; // 태양 다시 끄기

        yield return new WaitForSeconds(0.05f); // 잠깐 쉬고

        // 한번 더 짧게 번쩍 (리얼함 추가)
        mainCamera.backgroundColor = new Color(0.6f, 0.6f, 0.7f);
        yield return new WaitForSeconds(0.05f);

        // 최종 복구
        mainCamera.backgroundColor = originalBG;
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

    // [추가됨] 메인 조명 토글 버튼
    public void ToggleMainRoomBtn()
    {
        isMainRoomOn = !isMainRoomOn;
        UpdateMainRoomState();
        UpdateButtonColors();
    }

    void UpdateMainRoomState()
    {
        // 1. 등기구(판) 색상 변경 (눈에 보이는 것)
        if (mainRoomRenderers != null)
        {
            foreach (Renderer rend in mainRoomRenderers)
            {
                if (rend != null) rend.material = isMainRoomOn ? mainRoomOnMat : mainRoomOffMat;
            }
        }

        // 2. 실제 조명 켜기/끄기 (방 밝기)
        if (mainRoomLights != null)
        {
            foreach (Light light in mainRoomLights)
            {
                if (light != null) light.enabled = isMainRoomOn;
            }
        }
    }

    void UpdateComputerLight() { if (computerLight != null) { computerLight.enabled = isComputerOn; if (isComputerOn && !isRGBMode) computerLight.color = Color.white; } }

    void UpdateKitchenState()
    {
        if (kitchenRenderers != null) foreach (Renderer rend in kitchenRenderers) if (rend != null) rend.material = isKitchenOn ? kitchenOnMat : kitchenOffMat;
        if (kitchenLights != null) foreach (Light light in kitchenLights) if (light != null) light.enabled = isKitchenOn;
    }

    void UpdateSeoulLight()
    {
        if (sunLight == null) return;
        float baseSouthY = -90.1f;
        float rotX = 0f; float rotY = 0f; float intensity = 1f; float shadowStrength = 1f;
        Color lightColor = Color.white; Color ambientColor = Color.black;

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

        if (isWeatherOn && currentTime != TimeSlot.Night_00)
        {
            sunLight.enabled = false;
            if (mainCamera != null) { mainCamera.clearFlags = CameraClearFlags.SolidColor; mainCamera.backgroundColor = new Color(0.25f, 0.25f, 0.28f); }
            ambientColor *= 0.3f;
            RenderSettings.fog = true; RenderSettings.fogDensity = 0.08f; RenderSettings.fogColor = new Color(0.25f, 0.25f, 0.28f);
        }
        else
        {
            sunLight.enabled = true;
            if (mainCamera != null) { mainCamera.clearFlags = CameraClearFlags.Skybox; }
            RenderSettings.fog = false;
        }

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
        SetBtnColor(btnMainLight, isMainRoomOn);

        // [추가됨] 뇌우 버튼 색상
        SetBtnColor(btnThunder, isThunderOn);
    }

    void SetBtnColor(Image btnImg, bool isActive) { if (btnImg != null) btnImg.color = isActive ? activeBtnColor : inactiveBtnColor; }
    void UpdateSizeHighlights() { if (highlightM_Mesh != null) highlightM_Mesh.material = isSizeM_On ? highlightOnMat : highlightOffMat; if (highlightL_Mesh != null) highlightL_Mesh.material = isSizeL_On ? highlightOnMat : highlightOffMat; }
    void UpdateSpeakerState() { if (speakerMesh == null) return; if (isSpeakerOn && isComputerOn) speakerMesh.material = speakerOnMat; else speakerMesh.material = speakerOffMat; }
    void UpdateMonitorScreen() { if (monitorScreen == null) return; if (!isMonitorOn) { monitorScreen.material = screenOffMat; if (monitorCanvasObject != null) monitorCanvasObject.SetActive(false); } else { if (!isComputerOn) { monitorScreen.material = screenBlueMat; if (monitorCanvasObject != null) monitorCanvasObject.SetActive(false); } else { monitorScreen.material = screenNoiseMat; if (monitorCanvasObject != null) monitorCanvasObject.SetActive(true); } } }
    void ToggleLight(Light lightObj, MeshRenderer meshObj, Color originColor) { if (lightObj != null) { lightObj.enabled = !lightObj.enabled; UpdateEmission(meshObj, originColor, lightObj.enabled); } }
    void UpdateEmission(MeshRenderer targetMesh, Color onColor, bool isOn) { if (targetMesh != null) { targetMesh.material.SetColor("_EmissionColor", isOn ? onColor : Color.black); DynamicGI.UpdateEnvironment(); } }
}