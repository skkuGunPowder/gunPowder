using System;
using UnityEngine;

public class MapData
{
    public readonly EMap Map;
    public readonly string ThemeCode;
    public readonly string MapName;
    public Sprite MapSprite;

    public MapData(EMap map, string themeCode, string mapName, Sprite mapSprite)
    {
        if (mapName == null)
        {
            throw new Exception("맵 이름이 없습니다.");
        }
        if (mapSprite == null)
        {
            throw new Exception($"{mapName}의 맵 스프라이트가 존재하지 않습니다.");
        }
        
        Map = map;
        ThemeCode = themeCode;
        MapName = mapName;
        MapSprite = mapSprite;
    }


    public string GetMapName()
    {
        string  mapName = TextManager.Instance.GetText(ThemeCode);

        if (MapName == "X")
        {
            return mapName;
        }
        
        // 한칸 공백
        return mapName + " " + MapName;;
    }
}
