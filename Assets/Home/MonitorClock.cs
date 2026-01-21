using UnityEngine;
using TMPro; // TextMeshPro 필수
using System; // 날짜 시간 필수

public class MonitorClock : MonoBehaviour
{
    [Header("텍스트 연결")]
    public TextMeshProUGUI timeText; // 시간 (예: 12:34)
    public TextMeshProUGUI dateText; // 날짜 (예: 11월 23일 금요일)

    void Update()
    {
        // 1. 시간 업데이트 (HH:mm = 24시간제, h:mm = 12시간제)
        if (timeText != null)
            timeText.text = DateTime.Now.ToString("HH:mm");

        // 2. 날짜 업데이트 (M월 d일 dddd = 1월 1일 월요일)
        if (dateText != null)
            dateText.text = DateTime.Now.ToString("M월 d일 dddd");
    }
}