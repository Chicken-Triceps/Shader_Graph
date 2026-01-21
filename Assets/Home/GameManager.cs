using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI & Player Settings")]
    public GameObject settingsPanel;
    public CreativeMove playerScript;

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

    [Header("Computer System")]
    public MeshRenderer monitorScreen;
    public GameObject monitorCanvasObject;
    public Material screenOffMat;
    public Material screenBlueMat;
    public Material screenNoiseMat;

    // --- [추가됨] 스피커 시스템 ---
    [Header("Speaker System")]
    public MeshRenderer speakerMesh; // 스피커 본체(혹은 불 들어오는 부분)
    public Material speakerOnMat;    // 켜졌을 때 (불빛 O)
    public Material speakerOffMat;   // 꺼졌을 때 (불빛 X)

    [Header("UI Button Images")]
    public Image btnDay;
    public Image btnNight;
    public Image btnBed;
    public Image btnDesk;
    public Image btnWave;
    public Image btnComputer;
    public Image btnMonitor;
    public Image btnSpeaker; // [추가됨] 스피커 버튼

    public Color activeBtnColor = new Color(1f, 1f, 0f);
    public Color inactiveBtnColor = new Color(1f, 1f, 1f);

    // State Variables
    private bool isMonitorOn = false;
    private bool isComputerOn = false;
    private bool isSpeakerOn = false; // [추가됨] 스피커 스위치 상태

    private float noiseOffset = 0f;
    private bool isMenuOpen = false;

    void Start()
    {
        if (daySkybox == null) daySkybox = RenderSettings.skybox;

        if (bedMesh != null) bedOriginColor = bedMesh.material.GetColor("_EmissionColor");
        if (deskMesh != null) deskOriginColor = deskMesh.material.GetColor("_EmissionColor");
        if (waveMesh != null) waveOriginColor = waveMesh.material.GetColor("_EmissionColor");

        UpdateMonitorScreen();
        UpdateSpeakerState(); // 시작 시 스피커 상태 초기화
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
    }

    // --- Button Actions ---

    public void ToggleMonitorBtn()
    {
        isMonitorOn = !isMonitorOn;
        UpdateMonitorScreen();
        UpdateButtonColors();
    }

    public void ToggleComputerBtn()
    {
        isComputerOn = !isComputerOn;
        UpdateMonitorScreen();
        UpdateSpeakerState(); // [중요] 컴퓨터를 끄면 스피커도 영향을 받음
        UpdateButtonColors();
    }

    // [추가됨] 스피커 버튼 기능
    public void ToggleSpeakerBtn()
    {
        isSpeakerOn = !isSpeakerOn;
        UpdateSpeakerState();
        UpdateButtonColors();
    }

    // --- State Update Logic ---

    // 스피커 상태 결정 함수 (핵심 로직)
    void UpdateSpeakerState()
    {
        if (speakerMesh == null) return;

        // 조건: 스피커 스위치가 켜져 있고(AND) 컴퓨터 전원도 켜져 있어야 함
        if (isSpeakerOn && isComputerOn)
        {
            speakerMesh.material = speakerOnMat; // 불 들어옴
        }
        else
        {
            speakerMesh.material = speakerOffMat; // 꺼짐 (컴퓨터가 꺼지면 스피커도 자동 꺼짐)
        }
    }

    void UpdateMonitorScreen()
    {
        if (monitorScreen == null) return;

        if (!isMonitorOn)
        {
            monitorScreen.material = screenOffMat;
            if (monitorCanvasObject != null) monitorCanvasObject.SetActive(false);
        }
        else
        {
            if (!isComputerOn)
            {
                monitorScreen.material = screenBlueMat;
                if (monitorCanvasObject != null) monitorCanvasObject.SetActive(false);
            }
            else
            {
                monitorScreen.material = screenNoiseMat;
                if (monitorCanvasObject != null) monitorCanvasObject.SetActive(true);
            }
        }
    }

    void UpdateButtonColors()
    {
        bool isDay = (sunLight != null && sunLight.enabled);
        SetBtnColor(btnDay, isDay);
        SetBtnColor(btnNight, !isDay);

        if (bedLight != null) SetBtnColor(btnBed, bedLight.enabled);
        if (deskLight != null) SetBtnColor(btnDesk, deskLight.enabled);
        if (waveLight != null) SetBtnColor(btnWave, waveLight.enabled);

        SetBtnColor(btnComputer, isComputerOn);
        SetBtnColor(btnMonitor, isMonitorOn);

        // 스피커 버튼은 '스위치' 상태를 보여줄지, '실제 작동' 상태를 보여줄지 결정해야 함.
        // 여기서는 "스위치가 눌려있다"는 걸 보여주기 위해 isSpeakerOn 변수를 따라갑니다.
        // (즉, 컴퓨터가 꺼져서 소리가 안 나도, 스피커 버튼 자체는 켜져(노란색) 있을 수 있음 -> 이게 더 현실적)
        SetBtnColor(btnSpeaker, isSpeakerOn);
    }

    void SetBtnColor(Image btnImg, bool isActive)
    {
        if (btnImg != null)
        {
            btnImg.color = isActive ? activeBtnColor : inactiveBtnColor;
        }
    }

    // --- Light Control ---

    public void SetDay()
    {
        RenderSettings.skybox = daySkybox;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = dayAmbientColor;
        if (sunLight != null) sunLight.enabled = true;
        UpdateButtonColors();
    }

    public void SetNight()
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = nightAmbientColor;
        if (sunLight != null) sunLight.enabled = false;
        UpdateButtonColors();
    }

    public void ToggleBedLight() { ToggleLight(bedLight, bedMesh, bedOriginColor); UpdateButtonColors(); }
    public void ToggleDeskLight() { ToggleLight(deskLight, deskMesh, deskOriginColor); UpdateButtonColors(); }
    public void ToggleWaveLight() { ToggleLight(waveLight, waveMesh, waveOriginColor); UpdateButtonColors(); }

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