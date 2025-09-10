using UnityEngine;

public interface IPlayerSkinManager
{
	void ApplyHead(ItemDTO item);
	void ClearHead();
	void ApplyFace(ItemDTO item);
	void ClearFace();
	void ApplyChest(ItemDTO item);
	void ClearChest();
	void ApplyCape(ItemDTO item);
	void ClearCape();

	// 슬롯 부모 (Head, Face 스킨은 이 부모 아래에 생성)
	Transform HeadSlotParent { get; }
	Transform FaceSlotParent { get; }
}


