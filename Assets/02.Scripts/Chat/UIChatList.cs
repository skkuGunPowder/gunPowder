using System;

using UnityEngine;
using UnityEngine.UI;

public class UIChatList : MonoBehaviour
{
    public Toggle CheckBox = null;
    public Image Avatar = null;
    public Text Name = null;
    public Text Message = null;
    public Text Time = null;
    public Button ReportButton = null;

    private UInt64 _index = 0;
    private string _tag = string.Empty;

    public void SetData(UInt64 index, string avatar, string name, string message, string time, string tag, Action<UInt64, string> report, Action<bool, string> translate, bool is_my = false)
    {
        _index = index;
        //_tag = tag;

        // if (avatar == string.Empty || avatar == "default")
        // {
        //     Avatar.sprite = Resources.Load<Sprite>("Images/Girl_5");
        // } 
        // else
        // {
        //     Avatar.sprite = Resources.Load<Sprite>("Images/" + avatar);
        // }

        if (is_my)
        {
            Name.text = name + " (You)";
        }
        else
        {
            Name.text = name;
        }
        
        Message.text = message;
        //Time.text = time;

        // ReportButton.onClick.RemoveAllListeners();
        // ReportButton.onClick.AddListener(() =>
        // {
        //     if (_index > 0 && _tag != string.Empty)
        //     {
        //         if (report != null)
        //         {
        //             report(_index, _tag);
        //         }
        //     }
        // });
        //
        // CheckBox.onValueChanged.RemoveAllListeners();
        // CheckBox.onValueChanged.AddListener((bool isOn) =>
        // {
        //     if (_index > 0 && _tag != string.Empty)
        //     {
        //         if (translate != null)
        //         {
        //             string key = tag + "," + index.ToString();
        //             translate(isOn, key);
        //         }
        //     }
        // });
    }

    public bool IsEqual(UInt64 index, string tag)
    {
        return _index == index && _tag == tag;
    }

    public void SetMessage(string message)
    {
        Message.text = message;
    }
}
