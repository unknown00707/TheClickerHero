using System;
using System.Collections.Generic;

public static class StringExtensions
{
    // 딕셔너리를 통째로 넘겨받아 문자열 안의 {Key}들을 Value로 싹 다 바꿔주는 함수
    public static string ReplaceTagsDict(this string target, Dictionary<string, object> replacements)
    {
        if (string.IsNullOrEmpty(target) || replacements == null) 
            return target;

        foreach (var pair in replacements)
        {
            // CSV에 적어둔 {TagName}을 실제 값으로 치환
            target = target.Replace($"{{{pair.Key}}}", pair.Value?.ToString());
        }

        return target;
    }
    // [최적화] 인자가 1쌍(2개)일 때 - 배열을 만들지 않아 가비지 0!
    public static string ReplaceTags(this string target, string k1, object v1)
    {
        if (string.IsNullOrEmpty(target)) return target;
        return target.Replace($"{{{k1}}}", v1?.ToString());
    }

    // [최적화] 인자가 2쌍(4개)일 때 - 배열을 만들지 않아 가비지 0!
    public static string ReplaceTags(this string target, string k1, object v1, string k2, object v2)
    {
        if (string.IsNullOrEmpty(target)) return target;
        return target.Replace($"{{{k1}}}", v1?.ToString())
                     .Replace($"{{{k2}}}", v2?.ToString());
    }

    // [최종 무기] 3쌍보다 많을 때만 AI가 준 params 함수가 작동하게 함
    public static string ReplaceTags(this string target, params object[] args)
    {
        // (기존 AI가 짜준 ReplaceTagsParams 코드 그대로 수록)
        if (string.IsNullOrEmpty(target) || args == null || args.Length % 2 != 0) return target;
        for (int i = 0; i < args.Length; i += 2)
        {
            string key = args[i]?.ToString();
            string value = args[i + 1]?.ToString();
            if (!string.IsNullOrEmpty(key)) target = target.Replace($"{{{key}}}", value);
        }
        return target;
    }
    public static string ReplaceTagsParams(this string target, params object[] args)
    {
        if (string.IsNullOrEmpty(target) || args == null || args.Length % 2 != 0) 
        {
            UnityEngine.Debug.LogWarning("ReplaceTags의 인자 개수는 반드시 [태그 이름, 값] 짝수 쌍이어야 합니다.");
            return target;
        }

        for (int i = 0; i < args.Length; i += 2)
        {
            string key = args[i]?.ToString();
            string value = args[i + 1]?.ToString();
            
            if (!string.IsNullOrEmpty(key))
            {
                target = target.Replace($"{{{key}}}", value);
            }
        }
        return target;
    }
}
