using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WeaponUpgradeManager : MonoBehaviour
{
    [Header("=== UI Connection ===")]
    public Image badgeImage;
    public TextMeshProUGUI msgText;
    public GameObject uiGroup;

    [Header("=== Object Connection ===")]
    public GameObject weaponObject;
    public MeshRenderer gunRenderer;

    [Header("=== Data Settings ===")]
    public Sprite[] gradeBadges;
    public Material[] gradeMaterials;
    public Material[] badgeEffectMaterials;

    [Header("=== Dissolve Settings (New!) ===")]
    public Material dissolveMaterial; // ★ 만드신 'Dissolve' 머티리얼을 여기에 넣으세요
    public string dissolvePropertyName = "_DissolveAmount"; // 쉐이더 프로퍼티 이름 (아래 설명 참고)
    public float dissolveSpeed = 2.0f; // 사라지는 속도

    [Header("=== Game Settings ===")]
    [Range(0f, 1f)]
    public float successChance = 0.7f;

    private int currentLevel = 0;
    private bool isDestroyed = false;
    private Coroutine currentMsgCoroutine;

    void Start()
    {
        if (msgText != null) msgText.gameObject.SetActive(false);
        ResetWeapon();
    }

    public void OnClickUpgrade()
    {
        if (isDestroyed) return;

        if (currentLevel >= 3)
        {
            ShowMessage("Max Level Reached!", Color.red);
            return;
        }

        if (Random.value <= successChance)
        {
            currentLevel++;
            UpdateVisuals();
            ShowMessage("Upgrade Successful!", Color.green);
        }
        else
        {
            StartCoroutine(DestroySequence());
        }
    }

    public void OnClickReset()
    {
        ResetWeapon();
        ShowMessage("Weapon Reset.", Color.white);
    }

    void ShowMessage(string message, Color color)
    {
        if (currentMsgCoroutine != null) StopCoroutine(currentMsgCoroutine);
        currentMsgCoroutine = StartCoroutine(ShowMessageRoutine(message, color));
    }

    IEnumerator ShowMessageRoutine(string message, Color color)
    {
        if (msgText == null) yield break;

        msgText.text = message;
        msgText.color = color;
        msgText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.0f);

        msgText.gameObject.SetActive(false);
        currentMsgCoroutine = null;
    }

    void UpdateVisuals()
    {
        // 배지 이미지 변경
        if (badgeImage != null && currentLevel < gradeBadges.Length)
            badgeImage.sprite = gradeBadges[currentLevel];

        // 배지 쉐이더 변경
        if (badgeImage != null && currentLevel < badgeEffectMaterials.Length)
            badgeImage.material = badgeEffectMaterials[currentLevel];

        // 총 스킨 변경 (파괴 상태가 아닐 때만)
        if (gunRenderer != null && currentLevel < gradeMaterials.Length && !isDestroyed)
        {
            gunRenderer.material = gradeMaterials[currentLevel];
        }

        if (uiGroup != null) uiGroup.SetActive(true);
        if (weaponObject != null) weaponObject.SetActive(true);
    }

    void ResetWeapon()
    {
        currentLevel = 0;
        isDestroyed = false;
        UpdateVisuals();
    }

    // ★ 핵심: 디졸브 애니메이션 코루틴
    IEnumerator DestroySequence()
    {
        isDestroyed = true;
        ShowMessage("Upgrade Failed... Weapon Destroyed.", Color.red);

        // 1. 디졸브 머티리얼로 교체
        // (기존 스킨 대신, 타들어가는 효과가 있는 머티리얼을 씌웁니다)
        if (gunRenderer != null && dissolveMaterial != null)
        {
            gunRenderer.material = dissolveMaterial;
        }

        // 2. 수치 애니메이션 (-1 ~ 1 범위로 가정)
        float currentVal = -1f; // 시작값 (완전히 보임)

        while (currentVal < 1f) // 목표값 (완전히 사라짐)
        {
            currentVal += Time.deltaTime * dissolveSpeed;

            // 쉐이더에 수치 전달
            if (gunRenderer != null)
            {
                gunRenderer.material.SetFloat(dissolvePropertyName, currentVal);
            }

            yield return null;
        }

        // 3. 완전히 사라진 후 오브젝트 끄기
        if (weaponObject != null) weaponObject.SetActive(false);
        if (uiGroup != null) uiGroup.SetActive(false);
    }
}