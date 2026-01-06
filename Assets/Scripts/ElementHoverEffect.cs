using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ElementHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Objects)")]
    public GameObject[] targetObjects; // 배열로 변경! 여러 개를 넣을 수 있습니다.

    [Header("Animation")]
    public float animSpeed = 5f; // 변화 속도
    public float targetScale = 1f; // 최종 크기 (보통 1)

    private Coroutine currentCoroutine;

    void Start()
    {
        // 시작할 때 등록된 모든 오브젝트를 숨깁니다.
        if (targetObjects != null)
        {
            foreach (GameObject obj in targetObjects)
            {
                if (obj != null)
                {
                    obj.transform.localScale = Vector3.zero;
                    obj.SetActive(true); // 오브젝트는 켜져 있어야 함
                }
            }
        }
    }

    // 마우스가 글자 위에 올라갔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetObjects == null || targetObjects.Length == 0) return;

        // 실행 중인 애니메이션이 있다면 멈추고 새로 시작
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(AnimateScale(Vector3.one * targetScale));
    }

    // 마우스가 글자에서 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetObjects == null || targetObjects.Length == 0) return;

        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(AnimateScale(Vector3.zero));
    }

    // 부드럽게 크기를 바꾸는 코루틴
    IEnumerator AnimateScale(Vector3 endScale)
    {
        // 첫 번째 오브젝트를 기준으로 거리를 체크합니다 (모두 같은 속도로 움직이므로)
        GameObject checkObj = targetObjects[0];

        if (checkObj != null)
        {
            while (Vector3.Distance(checkObj.transform.localScale, endScale) > 0.01f)
            {
                // 배열 안에 있는 모든 오브젝트를 동시에 조절
                foreach (GameObject obj in targetObjects)
                {
                    if (obj != null)
                    {
                        obj.transform.localScale = Vector3.Lerp(
                            obj.transform.localScale,
                            endScale,
                            Time.deltaTime * animSpeed
                        );
                    }
                }
                yield return null;
            }
        }

        // 마지막에 모든 오브젝트 값을 정확한 목표치로 고정
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null) obj.transform.localScale = endScale;
        }
    }
}