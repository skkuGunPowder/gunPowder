using UnityEngine;

public class PlayerSkinManagerSequence : MonoBehaviour, IPlayerSkinManager
{
	[Header("스킨")]
	[SerializeField] private GameObject _originalHeadSkin;
	[SerializeField] private GameObject _currentHeadSkin;
	[SerializeField] private GameObject _originalFaceSkin;
	[SerializeField] private GameObject _currentFaceSkin;
	[Header("슬롯 부모")]
	[SerializeField] private Transform _headSlotParent;
	[SerializeField] private Transform _faceSlotParent;
	[SerializeField] private Transform _chestSlotParent;
	[SerializeField] private GameObject _currentChestSkin;
	[SerializeField] private Transform _capeSlotParent;
	[SerializeField] private GameObject _currentCapeSkin;

	private void Awake()
	{
	}

	public void ApplyHead(ItemDTO item)
	{
		if (item == null || item.Prefab == null) { return; }
		_currentHeadSkin = ReplacePrefabInSlot(_currentHeadSkin, _originalHeadSkin, item.Prefab, _headSlotParent);
	}

	public void ClearHead()
	{
	}

	public void ApplyFace(ItemDTO item)
	{
		if (item == null || item.Prefab == null) { return; }
		_currentFaceSkin = ReplacePrefabInSlot(_currentFaceSkin, _originalFaceSkin, item.Prefab, _faceSlotParent);
	}

	public void ClearFace()
	{
	}

	public void ApplyChest(ItemDTO item)
	{
		if (item == null || item.Prefab == null) { return; }
		_currentChestSkin = AdditiveEquip(_currentChestSkin, _chestSlotParent, item.Prefab, true);
	}

	public void ClearChest()
	{
	}

	public void ApplyCape(ItemDTO item)
	{
		if (item == null || item.Prefab == null) { return; }
		_currentCapeSkin = AdditiveEquip(_currentCapeSkin, _capeSlotParent, item.Prefab, true);
	}

	public void ClearCape()
	{
	}

	private GameObject ReplacePrefabInSlot(GameObject currentInstance, GameObject originalObject, GameObject newPrefab, Transform overrideParent = null)
	{
		if (currentInstance != null)
		{
			Destroy(currentInstance);
			currentInstance = null;
		}
		if (originalObject == null || newPrefab == null) { return null; }

		if (originalObject.activeSelf)
		{
			originalObject.SetActive(false);
		}

		Transform parent = overrideParent != null ? overrideParent : (originalObject.transform.parent != null ? originalObject.transform.parent : transform);
		Vector3 localPos = originalObject.transform.localPosition;
		Quaternion localRot = originalObject.transform.localRotation;
		Vector3 localScale = originalObject.transform.localScale;

		GameObject instance = Instantiate(newPrefab, parent);
		instance.transform.localPosition = localPos;
		instance.transform.localRotation = localRot;
		instance.transform.localScale = localScale;

		instance.layer = originalObject.layer;
		SpriteRenderer srcSr = originalObject.GetComponent<SpriteRenderer>();
		SpriteRenderer dstSr = instance.GetComponent<SpriteRenderer>();
		if (srcSr != null && dstSr != null)
		{
			dstSr.sortingLayerID = srcSr.sortingLayerID;
			dstSr.sortingOrder = srcSr.sortingOrder;
		}
		
		return instance;
	}

	public Transform HeadSlotParent => _headSlotParent;
	public Transform FaceSlotParent => _faceSlotParent;

	private GameObject AdditiveEquip(GameObject currentInstance, Transform slotParent, GameObject prefab, bool manageLists)
	{
		if (slotParent == null || prefab == null) { return currentInstance; }
		if (currentInstance != null)
		{
			Destroy(currentInstance);
			currentInstance = null;
		}
		GameObject instance = Instantiate(prefab, slotParent);
		instance.transform.localPosition = Vector3.zero;
		instance.transform.localRotation = Quaternion.identity;
		instance.transform.localScale = Vector3.one;
		return instance;
	}
}
