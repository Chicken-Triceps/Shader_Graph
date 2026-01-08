using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WeaponUpgradeManager : MonoBehaviour
{
    [Header("=== UI Connection ===")]
    public Image badgeImage;          // 배지 UI
    public TextMeshProUGUI msgText;
    public GameObject uiGroup;

    [Header("=== Object Connection ===")]
    public GameObject weaponObject;
    public MeshRenderer gunRenderer;

    [Header("=== Data Settings ===")]
    public Sprite[] gradeBadges;      // 배지 그림 (Normal, Rare, Legend, Unique)
    public Material[] gradeMaterials; // 총 스킨 (Normal, Rare, Legend, Unique)

    // ★ 새로 추가된 부분: 배지용 쉐이더 머티리얼 리스트
    public Material[] badgeEffectMaterials;

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
        // 1. 배지 그림(Sprite) 변경
        if (badgeImage != null && currentLevel < gradeBadges.Length)
        {
            badgeImage.sprite = gradeBadges[currentLevel];
        }

        // ★ 2. 배지 쉐이더(Material) 변경 (핵심!)
        if (badgeImage != null && currentLevel < badgeEffectMaterials.Length)
        {
            // 해당 등급의 머티리얼을 적용 (없으면 기본값)
            badgeImage.material = badgeEffectMaterials[currentLevel];
        }

        // 3. 총 스킨 변경
        if (gunRenderer != null && currentLevel < gradeMaterials.Length)
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

    IEnumerator DestroySequence()
    {
        isDestroyed = true;
        ShowMessage("Upgrade Failed... Weapon Destroyed.", Color.red);
        yield return new WaitForSeconds(1.0f);

        if (weaponObject != null) weaponObject.SetActive(false);
        if (uiGroup != null) uiGroup.SetActive(false);
    }
}