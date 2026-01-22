using UnityEngine;

public class MirrorReflection : MonoBehaviour
{
    [Header("Connections")]
    public Camera mainCamera;
    public Transform mirrorPlane; // 'LogicalMirror' (파란 화살표가 방 안을 보는 것)
    public Camera mirrorCamera;

    [Header("Settings")]
    [Tooltip("가구가 사라진다면 체크를 해제하세요. (벽 뒤가 보일 때만 켜세요)")]
    public bool useObliqueClip = false; // [핵심] 기본값을 꺼둠 (False)

    public float clipPlaneOffset = 0.01f;

    [Header("Emergency Fix")]
    public Vector3 additionalRotation = Vector3.zero;

    void LateUpdate()
    {
        if (mainCamera == null || mirrorPlane == null || mirrorCamera == null) return;

        // 1. 설정 동기화 (해상도 충돌 방지 버전)
        mirrorCamera.fieldOfView = mainCamera.fieldOfView;
        mirrorCamera.aspect = mainCamera.aspect;
        mirrorCamera.farClipPlane = mainCamera.farClipPlane;
        mirrorCamera.backgroundColor = mainCamera.backgroundColor;
        mirrorCamera.clearFlags = mainCamera.clearFlags;

        if (mirrorCamera.targetTexture == null) return;


        // 2. 위치/회전 계산 (물리 반사)
        Vector3 planeNormal = mirrorPlane.forward;
        Vector3 planePos = mirrorPlane.position;
        Vector3 camPos = mainCamera.transform.position;

        // 위치 반사
        float d = -Vector3.Dot(planeNormal, planePos);
        float dist = Vector3.Dot(planeNormal, camPos) + d;
        Vector3 reflectedPos = camPos - 2 * dist * planeNormal;
        mirrorCamera.transform.position = reflectedPos;

        // 회전 반사
        Vector3 reflectedForward = Vector3.Reflect(mainCamera.transform.forward, planeNormal);
        Vector3 reflectedUp = Vector3.Reflect(mainCamera.transform.up, planeNormal);
        mirrorCamera.transform.rotation = Quaternion.LookRotation(reflectedForward, reflectedUp);

        // 추가 보정
        mirrorCamera.transform.Rotate(additionalRotation);


        // 3. 거울 뒤쪽 자르기 (Oblique Frustum) - 옵션
        // [수정] 체크박스가 켜져 있을 때만 작동
        if (useObliqueClip)
        {
            Matrix4x4 projection = mainCamera.projectionMatrix;

            Vector3 localNormal = mirrorCamera.transform.InverseTransformDirection(planeNormal);
            Vector3 localPos = mirrorCamera.transform.InverseTransformPoint(planePos);

            Vector4 clipPlane = new Vector4(localNormal.x, localNormal.y, localNormal.z,
                                            -Vector3.Dot(localNormal, localPos) - clipPlaneOffset);

            projection = mirrorCamera.CalculateObliqueMatrix(clipPlane);
            mirrorCamera.projectionMatrix = projection;
        }
        else
        {
            // 꺼져있으면 메인 카메라의 기본 매트릭스 사용 (가구 절대 안 사라짐)
            mirrorCamera.ResetProjectionMatrix();
        }

        // [핵심 해결] GL.invertCulling 삭제함.
        // Tiling -1 방식을 쓸 때는 이게 있으면 가구가 투명해집니다.
    }
}