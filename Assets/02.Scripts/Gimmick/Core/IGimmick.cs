public interface IGimmick
{
    GimmickType Type { get; }
    GimmickGroupType GroupType { get; }
    void Activate();
    void Deactivate();
    bool IsActive { get; }
}
