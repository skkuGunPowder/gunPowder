using UnityEngine;

public interface ISlot
{
    public void Refresh(ItemDTO item);

    public void Select();

    public void Deselect();

    public void OnClick();
}
