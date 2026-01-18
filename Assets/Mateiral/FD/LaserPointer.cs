using UnityEngine;

public class LaserPointer : MonoBehaviour
{
    [Header("Settings")]
    public float laserLength = 50f;      // 레이저 최대 사거리
    public LayerMask hitLayers;          // 레이저가 부딪힐 레이어 (벽 등)
    public Transform firePoint;          // 레이저가 나가는 총구 위치 (없으면 본체 중심)

    [Header("Visuals")]
    public LineRenderer lineRenderer;    // 레이저 선 컴포넌트
    public GameObject hitEffect;         // 벽에 닿았을 때 표시될 점 (선택사항)

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        // 총구 위치를 따로 안정했으면 자기 자신의 위치를 사용
        if (firePoint == null) firePoint = transform;

        // Line Renderer 기본 설정 (혹시 안 했을까봐)
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2; // 시작점, 끝점 2개
    }

    void Update()
    {
        AimAndFire();
    }

    void AimAndFire()
    {
        // 1. 마우스 위치를 월드 좌표로 변환 (카메라에서 레이 쏘기)
        Ray camRay = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit mouseHit;

        // 마우스가 닿은 곳이 있다면
        if (Physics.Raycast(camRay, out mouseHit, 100f, hitLayers))
        {
            // 2. 캡슐을 마우스 지점 쪽으로 회전시키기
            Vector3 targetPosition = mouseHit.point;

            // (옵션) 캡슐이 위아래로 기울지 않고 좌우로만 돌게 하려면 아래 줄 주석 해제
            targetPosition.y = transform.position.y;

            transform.LookAt(targetPosition);
        }

        // 3. 레이저 발사 로직
        ShootLaser();
    }

    void ShootLaser()
    {
        // 레이저 시작점 설정
        lineRenderer.SetPosition(0, firePoint.position);

        RaycastHit objectHit;
        Vector3 endPosition = firePoint.position + (transform.forward * laserLength);

        // 캡슐 앞쪽으로 레이를 쏴서 벽에 닿는지 확인
        if (Physics.Raycast(firePoint.position, transform.forward, out objectHit, laserLength, hitLayers))
        {
            // 벽에 닿았으면 끝점을 벽 위치로 설정
            endPosition = objectHit.point;

            // 벽에 닿은 효과(빨간 점)가 있다면 위치 이동
            if (hitEffect != null)
            {
                hitEffect.SetActive(true);
                // 텍스처가 파묻히지 않게 벽에서 아주 살짝 띄움 (+ hit.normal * 0.01f)
                hitEffect.transform.position = objectHit.point + (objectHit.normal * 0.01f);
                hitEffect.transform.rotation = Quaternion.LookRotation(objectHit.normal);
            }
        }
        else
        {
            // 허공을 쏘면 효과 끄기
            if (hitEffect != null) hitEffect.SetActive(false);
        }

        // 레이저 끝점 설정
        lineRenderer.SetPosition(1, endPosition);
    }
}