using UnityEngine;

[CreateAssetMenu(fileName = "MapDataSO", menuName = "Scriptable Objects/MapDataSO")]
public class MapDataSO : ScriptableObject
{
    public ESceneList MapSceneList;
    public string MapName;
    public Sprite MapIcon;
    public Sprite MapSprite;
}
