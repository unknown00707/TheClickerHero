using System.Diagnostics;

public static class TestLogger
{
    // 에디터에서 실행하거나, 'Development Build'로 뽑았을 때만 작동!
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message)
    {
        UnityEngine.Debug.Log($"{message} by {nameof(TestLogger)}");
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(object message)
    {
        UnityEngine.Debug.LogWarning($"{message} by {nameof(TestLogger)}");
    }

    // 치명적인 에러는 스팀 정식 출시 후 유저 버그 제보 확인용으로 남겨두는 것을 추천합니다.
    public static void LogError(object message)
    {
        UnityEngine.Debug.LogError($"{message} by {nameof(TestLogger)}");
    }
}
