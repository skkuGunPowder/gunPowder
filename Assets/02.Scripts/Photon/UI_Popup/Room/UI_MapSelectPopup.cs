using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_MapSelectPopup : UI_Popup
{
    // 테마를 눌렀을 때 그 테마에 맞는 것들 refresh
    public List<UI_MapSelectButton> UI_MapSelectButtonList;
    public List<UI_ThemeButton> UI_ThemeButtonList;

    public EMap Map;
    public EMapTheme Theme;
    
    private List<MapData> _mapDataList;
    // 현재 맵
    private void Awake()
    {

    }

    private void Start()
    {
        Init();
    }
    
    // 팝업 창을 켰을 때, 그 맵의 테마와 그 맵이 선택되어있도록 
    private void OnEnable()
    {
        MapDataManager.Instance.OnMapDataLoad += LoadCurrentMap;
        Refresh(Theme);
    }

    private void Init()
    {
        List<MapThemeData> themeDataList = MapDataManager.Instance.GetThemeDataList();
        
        for (int i = 0; i < UI_ThemeButtonList.Count; i++)
        {
            if (i >= themeDataList.Count)
            {
                UI_ThemeButtonList[i].gameObject.SetActive(false);
                continue;
            }
            
            MapThemeData themeData = themeDataList[i];
            if (themeData.MapTheme == EMapTheme.Random)
            {
                UI_ThemeButtonList[i].gameObject.SetActive(false);
                continue;
            }
            
            UI_ThemeButtonList[i].Refresh(themeData.MapTheme,themeData.MapIcon ,themeData.ThemeName);
        }
        
        ThemeRefresh(Theme);
    }
    
    private void LoadCurrentMap(EMap map, EMapTheme theme)
    {
        Map = map;
        
        if (theme == EMapTheme.Random)
        {
            Theme = EMapTheme.All;
            MapRefresh(_mapDataList);
            return;       
        }

        Theme = theme;
        MapRefresh(_mapDataList);
    }

    private void Refresh(EMapTheme theme)
    {
        ThemeRefresh(theme);
        List<MapData> mapList = MapDataManager.Instance.ThemeDataLoad(theme);
        SetMapList(mapList);
        MapRefresh(mapList);
    }
    
    private void ThemeRefresh(EMapTheme theme)
    {
        foreach (UI_ThemeButton themeButton in UI_ThemeButtonList)
        {
            themeButton.SelectCheck(theme);
        }
    }
    
    // 테마 변경시에만 맵 목록 refresh
    private void MapRefresh(List<MapData> mapList)
    {
        for (int i = 0; i < UI_MapSelectButtonList.Count; i++)
        {
            if (i < mapList.Count)
            {
                MapData data = mapList[i];
                bool isSelected = data.Map == Map;
                UI_MapSelectButtonList[i].gameObject.SetActive(true);
                UI_MapSelectButtonList[i].Refresh(data.Map, isSelected, data.MapSprite, data.GetMapName());
            }
            else
            {
                UI_MapSelectButtonList[i].gameObject.SetActive(false);
            }
        }
    }

    // 맵 선택했을 때만 -> refresh
    private void SetMapList(List<MapData> mapList)
    {
        _mapDataList = mapList;   
    }
    
    public void ChangeTheme(EMapTheme theme)
    {
        Refresh(theme);
    }
    private void OnDisable()
    {
        MapDataManager.Instance.OnMapDataLoad -= LoadCurrentMap;
    }
    
}
