using UnityEngine;

[CreateAssetMenu(fileName = "MapDataSO", menuName = "Scriptable Objects/MapDataSO")]
public class MapDataSO : ScriptableObject
{
    public EMap MapSceneList;
    public string MapName;
    public Sprite MapIcon;
    public Sprite MapSprite;
}
