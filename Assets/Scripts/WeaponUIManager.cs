using UnityEngine;
using TMPro;

public class WeaponUIManager : MonoBehaviour
{
    [Header("연결: 텍스트 UI")]
    public TextMeshProUGUI infoText;

    [Header("연결: 무기 렌더러")]
    // 총의 모양(Mesh)을 그려주는 컴포넌트입니다. 총 오브젝트를 여기에 넣어야 합니다.
    public MeshRenderer weaponRenderer;

    [Header("설정: 무기 이름")]
    public string weaponName = "M249";

    [Header("설정: 등급별 데이터 (색상 & 머티리얼)")]
    // 각 등급에 맞는 색상과 머티리얼을 짝지어 줍니다.
    public Color normalColor = Color.white;
    public Material normalMat;

    public Color rareColor = new Color(0.7f, 0.3f, 1f);
    public Material rareMat;

    public Color legendColor = new Color(1f, 0.6f, 0f);
    public Material legendMat;

    public Color uniqueColor = Color.red;
    public Material uniqueMat;

    // 내부 처리 함수
    private void UpdateDisplay(string grade, Color color, Material mat)
    {
        // 1. 텍스트 변경
        infoText.text = weaponName + " [" + grade + "]";
        // 2. 글자 색 변경
        infoText.color = color;

        // 3. 총 스킨(머티리얼) 변경
        if (weaponRenderer != null && mat != null)
        {
            weaponRenderer.material = mat;
        }
    }

    // 버튼 연결용 함수들
    public void ClickNormal()
    {
        UpdateDisplay("NORMAL", normalColor, normalMat);
    }

    public void ClickRare()
    {
        UpdateDisplay("RARE", rareColor, rareMat);
    }

    public void ClickLegend()
    {
        UpdateDisplay("LEGEND", legendColor, legendMat);
    }

    public void ClickUnique()
    {
        UpdateDisplay("UNIQUE", uniqueColor, uniqueMat);
    }
}