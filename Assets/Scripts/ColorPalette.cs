/// <summary>
/// TextMeshPro(TMP)에서 사용할 리치 텍스트(<color>) 태그 모음 클래스입니다.
/// </summary>
/// <example>
/// <code>
/// string message = $"{ColorPalette.Red}경고!{ColorPalette.End} 몬스터가 나타났습니다.";
/// </code>
/// </example>
public static class ColorPalette
{
    // ==========================================
    // 기본 색상 리스트
    // ==========================================

    /// <summary> 기본 빨간색 태그 시작 신호 <br/>실제 색상: <color=#FF0000>■■■</color></summary>
    public const string Red = "<color=#FF0000>";

    /// <summary> 기본 노란색 태그 시작 신호 <br/>실제 색상: <color=#FFFF00>■■■</color></summary>
    public const string Yellow = "<color=#FFFF00>";

    /// <summary> 기본 초록색 태그 시작 신호 <br/>실제 색상: <color=#00FF00>■■■</color></summary>
    public const string Green = "<color=#00FF00>";

    /// <summary> 기본 청록색(시안) 태그 시작 신호 <br/>실제 색상: <color=#00FFFF>■■■</color></summary>
    public const string Cyan = "<color=#00FFFF>";
    
    // ==========================================
    // 게임 아이템/등급별 색상 리스트
    // ==========================================

    /// <summary> 레어(Rare) 등급 파란색 태그 시작 신호 <br/>실제 색상: <color=#00A2FF>■■■</color></summary>
    public const string Rare = "<color=#00A2FF>";

    /// <summary> 에픽(Epic) 등급 보라색 태그 시작 신호 <br/>실제 색상: <color=#A600FF>■■■</color></summary>
    public const string Epic = "<color=#A600FF>";

    /// <summary> 레전드(Legend) 등급 주황색 태그 시작 신호 <br/>실제 색상: <color=#FF6A00>■■■</color></summary>
    public const string Legend = "<color=#FF6A00>";
    
    // ==========================================
    // 공용 종료 태그
    // ==========================================

    /// <summary> 
    /// 모든 <color> 태그를 닫아주는 공용 종료 신호입니다. <br/>
    /// <b>주의:</b> 색상 태그를 연 후에는 반드시 이 태그로 닫아주어야 뒤의 텍스트가 정상 출력됩니다.
    /// </summary>
    public const string End = "</color>";
}
