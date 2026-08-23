public static class ColorPalette
{
    // 자주 쓰는 색상 태그의 '시작'과 '끝'을 미리 정의합니다.
    public const string Red = "<color=#FF0000>";
    public const string Yellow = "<color=#FFFF00>";
    public const string Green = "<color=#00FF00>";
    public const string Cyan = "<color=#00FFFF>";
    
    // 게임 등급별 색상도 이렇게 이름 지어두면 편합니다.
    public const string Rare = "<color=#00A2FF>";    // 파란색
    public const string Epic = "<color=#A600FF>";    // 보라색
    public const string Legend = "<color=#FF6A00>";  // 주황색
    
    public const string End = "</color>"; // 태그를 닫아주는 공용 종료 신호
}
