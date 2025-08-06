using UnityEngine;
using BackEnd;
using System.Threading.Tasks;


public class BackendLogin
{
    public Result CustomSignUp(string id, string pw)
    {
        var bro = Backend.BMember.CustomSignUp(id, pw);

        if (bro.IsSuccess())
        {
            Debug.Log($"[BackendLogin] 회원가입 성공 : {bro}");
            return new Result(true, "회원가입에 성공하였습니다.");
        }
        else
        {
            Debug.Log($"[BackendLogin] 회원가입 실패 : {bro.ErrorCode} | {bro.Message}");
            
            if (bro.GetStatusCode() == "400")
            {
                return new Result(false, "디바이스 정보가 없습니다.");
            }

            if(bro.GetStatusCode() == "401")
            {
                return new Result(false, "죄송합니다. 지금은 점검중입니다.");
            }

            if (bro.GetStatusCode() == "403")
            {
                return new Result(false, "차단한 디바이스입니다.");
            }

            if (bro.GetStatusCode() == "409")
            {
                return new Result(false, "이미 가입한 이메일입니다.");
            }
            
            return new Result(true, "회원가입에 실패하였습니다.");
        }
    }

    public Result CustomLogin(string id, string pw)
    {
        var bro = Backend.BMember.CustomLogin(id, pw);

        if (bro.IsSuccess())
        {
            Debug.Log($"[BackendLogin] 로그인 성공 : {bro}");
            return new Result(true, "로그인에 성공하였습니다.");
        }
        else
        {
            Debug.Log($"[BackendLogin] 로그인 실패 : {bro.ErrorCode} | {bro.Message}");
            return new Result(false, "로그인에 실패하였습니다.");
        }
    }

    public Result UpdateNickName(string nickname)
    {
        var bro = Backend.BMember.UpdateNickname(nickname);

        if (bro.IsSuccess())
        {
            Debug.Log($"[BackendLogin] 닉네임 변경 성공 : {bro}");
            return new Result(true, "닉네임 변경에 성공하였습니다.");
        }
        else
        {
            Debug.LogError($"[BackendLogin] 닉네임 변경 실패 : {bro.ErrorCode} | {bro.Message}");
            return new Result(false, "닉네임 변경에 실패하였습니다.");
        }
    }
}
