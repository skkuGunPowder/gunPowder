using UnityEngine;
using BackEnd;
using System;


public class BackendLogin
{
    public Result CustomSignUp(string id, string pw)
    {
        bool isSuccess = false;
        string message = string.Empty;

        Backend.BMember.CustomSignUp(id, pw, bro=>
        {
            if (bro.IsSuccess())
            {
                isSuccess = true;
                message = "회원가입에 성공하였습니다.";
            }
            else
            {
                isSuccess = false;
                
                Debug.LogError($"[BackendLogin] 회원가입 실패 : {bro.ErrorCode} | {bro.Message}");
                
                if (bro.GetStatusCode() == "400")
                {
                    message = "디바이스 정보가 없습니다.";
                }

                if(bro.GetStatusCode() == "401")
                {
                    message = "죄송합니다. 지금은 점검중입니다.";
                }

                if (bro.GetStatusCode() == "403")
                {
                    message = "차단된 디바이스입니다.";
                }

                if (bro.GetStatusCode() == "409")
                {
                    message = "이미 가입한 이메일입니다.";
                }
            }
        });
        return new Result(isSuccess, message);
    }

    public Result CustomLogin(string id, string pw)
    {
        var bro = Backend.BMember.CustomLogin(id, pw);

        if (bro.IsSuccess())
        {
            try
            {
                ItemDatabase.Instance.Init();

                // 뒤끝 실시간 알림 서버 연결 (친구 접속 상태, 친구 요청 이벤트)
                FriendManager.Instance.ConnectNotification();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            return new Result(true, "로그인에 성공하였습니다.");
        }
        else
        {
            Debug.LogError($"[BackendLogin] 로그인 실패 : {bro.ErrorCode} | {bro.Message}");
            return new Result(false, $"{bro.Message}");
        }
    }

    public Result UpdateNickName(string nickname)
    {
        var bro = Backend.BMember.UpdateNickname(nickname);

        if (bro.IsSuccess())
        {
            return new Result(true, "닉네임 변경에 성공하였습니다.");
        }
        else
        {
            Debug.LogError($"[BackendLogin] 닉네임 변경 실패 : {bro.ErrorCode} | {bro.Message}");
            return new Result(false, $"{bro.Message}");
        }
    }
}
