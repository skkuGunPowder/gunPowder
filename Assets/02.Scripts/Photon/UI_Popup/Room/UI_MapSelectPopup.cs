using System.Collections.Generic;
using UnityEngine;

public class UI_MapSelectPopup : UI_Popup
{
    // 테마를 눌렀을 때 그 테마에 맞는 것들 refresh
    public List<UI_MapSelectButton> UI_MapSelectButtonList;
    public List<UI_ThemeButton> UI_ThemeButtonList;
    
    // 현재 맵
    private void Awake()
    {
        
    }
    
    // 테마 개수에 맞춰서 SELECTBUTTON 켜기
    public void Refresh()
    {
        
    }
}
