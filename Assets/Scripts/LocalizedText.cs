using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필수

// TextMeshProUGUI 컴포넌트가 없으면 유니티가 알아서 추가해주는 안전장치입니다.
[RequireComponent(typeof(TextMeshProUGUI))] 
public class LocalizedText : MonoBehaviour
{
    [Header("엑셀에 적은 ID를 여기에 입력하세요")]
    public string textID; 

    private TextMeshProUGUI textMesh;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();

        // 1. 매니저의 방송국(OnLanguageChanged)에 '새로고침(Refresh)' 기능을 구독합니다.
        LanguageManager.Instance.OnLanguageChanged += Refresh;
        
        // 2. 처음 켜졌을 때도 글자를 맞춰야 하니 한 번 실행해줍니다.
        Refresh(); 
    }

    // 매니저에게 내 ID를 주고 글자를 받아와서 화면에 찍는 함수
    public void Refresh()
    {
        if (LanguageManager.Instance != null)
        {
            textMesh.text = LanguageManager.Instance.GetText(textID);
        }
    }

    // 🌟 실전 필수: 이 오브젝트가 파괴될 때 방송국 구독을 취소해야 메모리 누수(에러)가 안 납니다!
    void OnDestroy()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= Refresh;
        }
    }
}