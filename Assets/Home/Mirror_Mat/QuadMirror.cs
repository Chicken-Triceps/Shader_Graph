using UnityEngine;

public class QuadMirror : MonoBehaviour
{
    [Header("Connections")]
    public Transform mainCamera;   // 플레이어 카메라
    public Transform mirrorQuad;   // MirrorScreen (쿼드)
    public Camera mirrorCamera;    // MirrorCamera

    [Header("Mirror Settings")]
    [Tooltip("체크하면 거울이 기울어져 있어도 정면을 비춥니다.")]
    public bool forceVertical = true;

    [Tooltip("거울 밝기 (0.1 ~ 1.0)")]
    [Range(0.1f, 1.0f)]
    public float brightness = 0.9f;

    [Tooltip("좌우 반전 여부 (거울 효과 필수)")]
    public bool flipHorizontal = true; // [추가됨] 기본값 True

    private Material mirrorMaterial;

    void Start()
    {
        Renderer rend = mirrorQuad.GetComponent<Renderer>();
        if (rend != null)
        {
            mirrorMaterial = rend.material;
        }

        // [핵심] 시작할 때 텍스처를 좌우로 뒤집어버림
        if (mirrorMaterial != null && flipHorizontal)
        {
            // Tiling X를 -1로 해서 뒤집고, 
            // Offset X를 1로 해서 위치를 보정합니다.
            mirrorMaterial.mainTextureScale = new Vector2(-1, 1);
            mirrorMaterial.mainTextureOffset = new Vector2(1, 0);
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null || mirrorQuad == null || mirrorCamera == null) return;

        mirrorCamera.fieldOfView = mainCamera.GetComponent<Camera>().fieldOfView;

        // 1. 거울의 방향(Normal) 계산
        Vector3 reflectionPlaneNormal = mirrorQuad.forward;

        if (forceVertical)
        {
            reflectionPlaneNormal.y = 0;
            reflectionPlaneNormal.Normalize();
        }

        // 2. 물리 반사 계산
        Vector3 camPos = mainCamera.position;
        Vector3 planePos = mirrorQuad.position;

        // 위치 반사
        float d = -Vector3.Dot(reflectionPlaneNormal, planePos);
        float dist = Vector3.Dot(reflectionPlaneNormal, camPos) + d;
        Vector3 reflectedPos = camPos - 2 * dist * reflectionPlaneNormal;

        mirrorCamera.transform.position = reflectedPos;

        // 회전 반사
        Vector3 reflectedForward = Vector3.Reflect(mainCamera.forward, reflectionPlaneNormal);
        Vector3 reflectedUp = Vector3.Reflect(mainCamera.up, reflectionPlaneNormal);

        mirrorCamera.transform.rotation = Quaternion.LookRotation(reflectedForward, reflectedUp);

        // 3. 밝기 적용
        if (mirrorMaterial != null)
        {
            // 밝기는 유지하되 투명도는 1로 고정
            mirrorMaterial.color = new Color(brightness, brightness, brightness, 1f);
        }
    }

    // 게임 종료시 텍스처 설정 원상복구 (에디터 꼬임 방지)
    void OnDisable()
    {
        if (mirrorMaterial != null)
        {
            mirrorMaterial.mainTextureScale = new Vector2(1, 1);
            mirrorMaterial.mainTextureOffset = new Vector2(0, 0);
        }
    }
}