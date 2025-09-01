using System;
using UnityEngine;

public class MapData
{
    public readonly EMap Map;
    public readonly string MapName;
    public Sprite MapSprite;

    public MapData(EMap map, string mapName, Sprite mapSprite)
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
        MapName = mapName;
        MapSprite = mapSprite;
    }
    
    
}
