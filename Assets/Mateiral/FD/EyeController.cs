using UnityEngine;

public class EyeController : MonoBehaviour
{
    [Header("Connections")]
    public Transform targetObject;  // 바라볼 대상 (초록색 실린더)
    public Renderer eyeRenderer;    // 눈알 (주황색 구체)

    [Header("Shader Property")]
    public string propertyName = "_TargetPos"; // 쉐이더에서 만든 변수 이름

    void Update()
    {
        if (targetObject == null || eyeRenderer == null) return;

        // 실린더의 현재 위치를 가져와서 쉐이더에 전달
        eyeRenderer.material.SetVector(propertyName, targetObject.position);
    }
}