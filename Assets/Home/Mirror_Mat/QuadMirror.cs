using UnityEngine;

public class QuadMirror : MonoBehaviour
{
    [Header("Connections")]
    public Transform mainCamera;   // 플레이어 카메라
    public Transform mirrorQuad;   // MirrorScreen (쿼드)
    public Camera mirrorCamera;    // MirrorCamera

    [Header("Options")]
    [Tooltip("체크하면 거울이 기울어져 있어도 정면을 비춥니다.")]
    public bool forceVertical = true; // [핵심] 기본적으로 켬

    [Tooltip("거울 밝기 (0.1 ~ 1.0)")]
    [Range(0.1f, 1.0f)]
    public float brightness = 0.9f;

    private Material mirrorMaterial;

    void Start()
    {
        Renderer rend = mirrorQuad.GetComponent<Renderer>();
        if (rend != null)
        {
            mirrorMaterial = rend.material;
            mirrorMaterial.color = Color.white;
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null || mirrorQuad == null || mirrorCamera == null) return;

        mirrorCamera.fieldOfView = mainCamera.GetComponent<Camera>().fieldOfView;

        // --------------------------------------------------------
        // 1. 거울의 방향(Normal) 계산
        // --------------------------------------------------------
        Vector3 reflectionPlaneNormal = mirrorQuad.forward;

        // [핵심 수정] 수직 강제 옵션이 켜져 있으면, 기울기를 무시함
        if (forceVertical)
        {
            // 거울의 위아래 기울기(y값)를 0으로 만들고 다시 계산
            reflectionPlaneNormal.y = 0;
            reflectionPlaneNormal.Normalize();
        }

        // --------------------------------------------------------
        // 2. 물리 반사 계산
        // --------------------------------------------------------
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

        // --------------------------------------------------------
        // 3. 밝기 적용
        // --------------------------------------------------------
        if (mirrorMaterial != null)
        {
            mirrorMaterial.color = new Color(brightness, brightness, brightness, 1f);
        }
    }
}