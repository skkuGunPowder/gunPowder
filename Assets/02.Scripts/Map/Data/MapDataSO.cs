using UnityEngine;

[CreateAssetMenu(fileName = "MapDataSO", menuName = "Scriptable Objects/MapDataSO")]
public class MapDataSO : ScriptableObject
{
    public EMapTheme MapTheme;
    public EMap MapSceneList;
    public string MapName;
    public Sprite MapSprite;
}
