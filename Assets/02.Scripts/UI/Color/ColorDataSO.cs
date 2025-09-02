using UnityEngine;

[CreateAssetMenu(fileName = "ColorData", menuName = "Scriptable Objects/ColorData")]
public class ColorDataSO : ScriptableObject
{
    public EColorType ColorType;
    public Color32 Color;
}
