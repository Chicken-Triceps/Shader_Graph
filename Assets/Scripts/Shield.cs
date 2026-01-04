using UnityEngine;
using System.Collections;

// print("여기까지 정상 작동");

public class Shield : MonoBehaviour
{
    Material material; // 쉐이더가 포함된 Material 로드
    public float speed;

    private void Start()
    {
        material = GetComponent<Renderer>().material; // 게임 시작될 시 Material 로드
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 좌클릭 시
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 카메라를 기준으로 마우스 위치로 Ray(광선) 발사

            if (Physics.Raycast(ray, out RaycastHit hit)) // 만약 광선에 닿은 물체가 해당하는 물체라면
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    StartCoroutine(GetPowerUp()); // 코루틴 실행
                }
            }
        }
    }

    IEnumerator GetPowerUp()
    {
        float start = material.GetFloat("_FresnelEffectPower");
        float value = start;

        while (value > 1) // 1이 될 때까지 값이 줄어듦
        {
            value -= Time.deltaTime * speed;
            material.SetFloat("_FresnelEffectPower", value);
            yield return null;
        }

        yield return new  WaitForSeconds(0.1f);

        while (value < start)
        {
            value += Time.deltaTime * speed;
            material.SetFloat("_FresnelEffectPower", value);
            yield return null;
        }
    }

}
