using UnityEngine;

public class Result
{
    public readonly bool IsSuccess;
    public readonly string Message;

    public Result(bool isSuccess, string message = "")
    {
        IsSuccess = isSuccess;
        Message = message;
    }
}