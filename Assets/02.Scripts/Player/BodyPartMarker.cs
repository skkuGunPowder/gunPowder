using UnityEngine;

public enum BodyPartType
{
    Head,
    Body,
    LeftArm,
    LeftLeg,
    RightArm,
    RightLeg
}

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class BodyPartMarker : MonoBehaviour
{
    public BodyPartType PartType;
}


