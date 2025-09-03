using System;
using System.Collections.Generic;
using UnityEngine;

public class MapThemeData
{
    // 맵 테마가 가지고 있는 모든 맵 리스트
    public readonly EMapTheme MapTheme;
    public readonly Sprite MapIcon;
    public readonly string ThemeName;
    public List<MapData> MapDataList;

    public MapThemeData(EMapTheme mapTheme, string themeName ,Sprite mapIcon)
    {
        if (themeName == null)
        {
            throw new Exception("테마의 이름 없습니다.");
        }
        if (mapIcon == null)
        {
            throw new Exception($"{mapTheme}에 맞는 맵 아이콘이 존재하지 않습니다.");
        }
        
        MapTheme = mapTheme;
        ThemeName = themeName;
        MapIcon = mapIcon;
        MapDataList = new List<MapData>();
    }

    public void AddMapData(MapData mapData)
    {
        MapDataList.Add(mapData);
    }

    public bool MapCheck(EMap map)
    {
        foreach (MapData data in MapDataList)
        {
            if (data.Map == map)
            {
                return true;
            }
        }
        
        return false;
    }
    public MapData GetMapData(string mapName)
    {
        foreach (var data in MapDataList)
        {
            if (data.MapName == mapName)
            {
                return data;
            }
        }
        
        return null;
    }
}
