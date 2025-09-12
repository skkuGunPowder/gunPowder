using System;
using System.Collections.Generic;
using UnityEngine;

public class MapDataManager : Singleton<MapDataManager>
{
    [Header("모든 맵")]
    [SerializeField] private List<MapDataSO> _mapDataSOList;
    [Header("모든 테마")]
    [SerializeField] private List<MapThemeDataSO> _mapThemeDataSOList;
    
    // 맵 이름에 맞춰 맵 데이터 불러오기
    private List<MapData> _mapDataList;
    public List<MapData> MapDataList => _mapDataList;
    
    // 테마에 맞게 맵 리스트 불러오기
    private Dictionary<EMapTheme, MapThemeData> _mapThemeDataDict;
    public Dictionary<EMapTheme, MapThemeData> MapThemeDataDict => _mapThemeDataDict;

    public event Action<EMap,EMapTheme> OnMapDataLoad;
    
    protected override void Awake()
    {
        base.Awake();
        MapDataSetting();
        ThemeSetting();
    }

    private void MapDataSetting()
    {
        _mapDataList = new List<MapData>();
        
        foreach (MapDataSO dataSO in _mapDataSOList)
        {
            if (dataSO == null)
            {
                continue;
            }
            
            MapData data = new MapData(dataSO.Map, dataSO.MapName, dataSO.MapSprite);
            _mapDataList.Add(data);
        }
    }
    
    private void ThemeSetting()
    {
        _mapThemeDataDict = new Dictionary<EMapTheme, MapThemeData>();
        
        foreach (MapThemeDataSO theme in _mapThemeDataSOList)
        {
            if (theme == null)
            {
                continue;
            }
            
            MapThemeData themeData = new MapThemeData(theme.MapTheme, theme.ThemeName, theme.MapIcon);
            
            if (themeData.MapTheme == EMapTheme.All)
            {
                _mapThemeDataDict.Add(themeData.MapTheme, themeData);
                continue;  
            }
            
            foreach (MapDataSO dataSO in _mapDataSOList)
            {
                if (dataSO == null)
                {
                    continue;
                }
                
                if (dataSO.MapTheme == themeData.MapTheme || dataSO.MapTheme == EMapTheme.Random)
                {
                    MapData data = new MapData(dataSO.Map, dataSO.MapName, dataSO.MapSprite);
                    themeData.AddMapData(data);
                }
            }
            
            _mapThemeDataDict.Add(themeData.MapTheme, themeData);
        }
    }

    public MapData GetDataLoad(EMap map)
    {
        foreach (MapData data in _mapDataList)
        {
            if(map != data.Map)
            {
                continue;
            }
            
            return data;
        }
        
        return null;
    }

    public List<MapThemeData> GetThemeDataList()
    {
        List<MapThemeData> themeDataList = new List<MapThemeData>();
        
        foreach (var mapThemeData in _mapThemeDataDict)
        {
            themeDataList.Add(mapThemeData.Value);   
        }

        return themeDataList;
    }
    // 테마에 있는 모든 맵 데이터 전달
    public List<MapData> ThemeDataLoad(EMapTheme theme)
    {
        if (theme == EMapTheme.All)
        {
            return _mapDataList;       
        }
        
        foreach (var dic in _mapThemeDataDict)
        {
            if (dic.Key != theme)
            {
                continue;
            }
            
            return dic.Value.MapDataList;
        }
        
        return null;
    }

    // public MapThemeData GetThemeDataforRoom(EMap map)
    // {
    //     foreach (var dic in _mapThemeDataDict)
    //     {
    //         if (dic.Value.MapCheck(map))
    //         {
    //             return dic.Value;
    //         }
    //     }
    //     
    //     return null;
    // }
    
    // 전달받은 맵이 어떤 테마인가?
    public MapThemeData GetThemeData(EMap map)
    {
        if (map == EMap.Random)
        {
            return _mapThemeDataDict[EMapTheme.Random];       
        }
        
        foreach (var dic in _mapThemeDataDict)
        {
            if (dic.Value.MapCheck(map))
            {
                return dic.Value;
            }
        }
        
        return null;
    }
    
    // 맵이 변경되었을 때, 그 값을 팝업에 전달
    public void LoadMapData()
    {
        EMap map = RoomManager.Instance.SelectedMap;
        EMapTheme theme = GetThemeData(map).MapTheme;
        OnMapDataLoad?.Invoke(map, theme);
    }
    
}
