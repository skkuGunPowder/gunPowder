using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class AccountPasswordSpecification : ISpecification<string>
{
    public string ErrorMessage { get; private set; }

    public bool IsSatisfiedBy(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ErrorMessage = "비밀번호는 비어있을 수 없습니다.";
            return false;
        }

        if (value.Length < 6 || value.Length > 12)
        {
            ErrorMessage = "비밀번호는 6자 이상 12자 이하 이여야 합니다.";
            return false;
        }

        return true;
    }
}
