using LitJson;
using UnityEngine;

public class TextData
{
    private readonly string _id;
    private readonly string _krText;
    private readonly string _enText;
    private readonly string _jpText;
    
    public string ID {get => _id;}
    public string KR {get => _krText;}    
    public string EN {get => _enText;}
    public string JP {get => _jpText;}


    public TextData(JsonData jsonData)
    {
        if (jsonData == null)
        {
            Debug.LogError("TextData 생성 실패: jsonData가 null입니다.");
            return;
        }
        
        _id = jsonData["TextID"].ToString();
        _krText = jsonData["Korean"].ToString();
        _enText = jsonData["English"].ToString();
        _jpText = jsonData["Japanese"].ToString();
    }
}
