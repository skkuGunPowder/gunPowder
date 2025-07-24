using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class AccountEmailSpecification : ISpecification<string>
{
    public string ErrorMessage { get; private set; }

    // 이메일 정규식
    private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

    public bool IsSatisfiedBy(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ErrorMessage = "이메일은 비어있을 수 없습니다.";
            return false;
        }

        if (!EmailRegex.IsMatch(value))
        {
            ErrorMessage = "올바른 이메일 형식이 아닙니다.";
            return false;
        }

        return true;
    }
}
