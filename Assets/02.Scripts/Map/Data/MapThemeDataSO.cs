using UnityEngine;

[CreateAssetMenu(fileName = "MapThemeDataSO", menuName = "Scriptable Objects/MapThemeDataSO")]
public class MapThemeDataSO : ScriptableObject
{
    public EMapTheme MapTheme;
    public string ThemeName;
    public Sprite MapIcon;
}
