using UnityEngine;

public class PlayerSkinManager : MonoBehaviour, IPlayerSkinManager
{
	[SerializeField] private Player _player;
	[SerializeField] private PlayerStat _playerStat;

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

	[Header("Die 슬롯 부모")]
	[SerializeField] private Transform _dieHeadSlotParent;
	[SerializeField] private Transform _dieFaceSlotParent;
	[SerializeField] private Transform _dieChestSlotParent;
	[SerializeField] private Transform _dieCapeSlotParent;

	[Header("Die 기본 스킨")]
	[SerializeField] private GameObject _dieOriginalHeadSkin;
	[SerializeField] private GameObject _dieOriginalFaceSkin;
	[SerializeField] private GameObject _dieOriginalChestSkin;
	[SerializeField] private GameObject _dieOriginalCapeSkin;

	private GameObject _currentDieHeadSkin;
	private GameObject _currentDieFaceSkin;
	private GameObject _currentDieChestSkin;
	private GameObject _currentDieCapeSkin;

	private void Awake()
	{
		_player = GetComponent<Player>();
		_playerStat = GetComponent<PlayerStat>();
	}

	public void ApplyHead(ItemDTO item)
	{
		if (item == null || item.Prefab == null) { return; }
		_currentHeadSkin = ReplacePrefabInSlot(_currentHeadSkin, _originalHeadSkin, item.Prefab, _headSlotParent);
		_currentDieHeadSkin = ReplaceDiePrefabInSlot(_currentDieHeadSkin, _dieOriginalHeadSkin, item.Prefab, _dieHeadSlotParent);
	}

	public void ClearHead()
	{
		_currentHeadSkin = ClearPrefabInSlot(_currentHeadSkin, _originalHeadSkin);
		_currentDieHeadSkin = ClearDiePrefabInSlot(_currentDieHeadSkin, _dieOriginalHeadSkin);
	}

	public void ApplyFace(ItemDTO item)
	{
		if (item == null || item.Prefab == null) { return; }
		_currentFaceSkin = ReplacePrefabInSlot(_currentFaceSkin, _originalFaceSkin, item.Prefab, _faceSlotParent);
		_currentDieFaceSkin = ReplaceDiePrefabInSlot(_currentDieFaceSkin, _dieOriginalFaceSkin, item.Prefab, _dieFaceSlotParent);
	}

	public void ClearFace()
	{
		_currentFaceSkin = ClearPrefabInSlot(_currentFaceSkin, _originalFaceSkin);
		_currentDieFaceSkin = ClearDiePrefabInSlot(_currentDieFaceSkin, _dieOriginalFaceSkin);
	}

	public void ApplyChest(ItemDTO item)
	{
		if (item == null || item.Prefab == null) { return; }
		_currentChestSkin = AdditiveEquip(_currentChestSkin, _chestSlotParent, item.Prefab, true);
		_currentDieChestSkin = AdditiveEquipDie(_currentDieChestSkin, _dieChestSlotParent, item.Prefab, _dieOriginalChestSkin);
	}

	public void ClearChest()
	{
		_currentChestSkin = RemoveAdditive(_currentChestSkin, true);
		_currentDieChestSkin = RemoveAdditiveDie(_currentDieChestSkin, _dieOriginalChestSkin);
	}

	public void ApplyCape(ItemDTO item)
	{
		if (item == null || item.Prefab == null) { return; }
		_currentCapeSkin = AdditiveEquip(_currentCapeSkin, _capeSlotParent, item.Prefab, true);
		_currentDieCapeSkin = AdditiveEquipDie(_currentDieCapeSkin, _dieCapeSlotParent, item.Prefab, _dieOriginalCapeSkin);
	}

	public void ClearCape()
	{
		_currentCapeSkin = RemoveAdditive(_currentCapeSkin, true);
		_currentDieCapeSkin = RemoveAdditiveDie(_currentDieCapeSkin, _dieOriginalCapeSkin);
	}

	private GameObject ReplacePrefabInSlot(GameObject currentInstance, GameObject originalObject, GameObject newPrefab, Transform overrideParent = null)
	{
		if (currentInstance != null)
		{
			RemoveInstanceComponentsFromLists(currentInstance);
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

		AddInstanceComponentsToLists(instance);
		return instance;
	}

	public Transform HeadSlotParent => _headSlotParent;
	public Transform FaceSlotParent => _faceSlotParent;

	private GameObject ClearPrefabInSlot(GameObject currentInstance, GameObject originalObject)
	{
		if (currentInstance != null)
		{
			RemoveInstanceComponentsFromLists(currentInstance);
			Destroy(currentInstance);
			currentInstance = null;
		}
		if (originalObject != null && !originalObject.activeSelf)
		{
			originalObject.SetActive(true);
			// 원본 복원 시 첫 프레임 타이밍 맞추기
			Animator origAnimator = originalObject.GetComponent<Animator>();
			if (origAnimator != null)
			{
				SyncAnimatorTiming(origAnimator);
			}
		}
		return null;
	}

	private GameObject AdditiveEquip(GameObject currentInstance, Transform slotParent, GameObject prefab, bool manageLists)
	{
		if (slotParent == null || prefab == null) { return currentInstance; }
		if (currentInstance != null)
		{
			if (manageLists) { RemoveInstanceComponentsFromLists(currentInstance); }
			Destroy(currentInstance);
			currentInstance = null;
		}
		GameObject instance = Instantiate(prefab, slotParent);
		instance.transform.localPosition = Vector3.zero;
		instance.transform.localRotation = Quaternion.identity;
		instance.transform.localScale = Vector3.one;
		if (manageLists) { AddInstanceComponentsToLists(instance); }
		return instance;
	}

	private GameObject RemoveAdditive(GameObject currentInstance, bool manageLists)
	{
		if (currentInstance != null)
		{
			if (manageLists) { RemoveInstanceComponentsFromLists(currentInstance); }
			Destroy(currentInstance);
		}
		return null;
	}

	private GameObject ReplaceDiePrefabInSlot(GameObject currentInstance, GameObject originalObject, GameObject newPrefab, Transform overrideParent = null)
	{
		if (currentInstance != null)
		{
			RemoveDieInstanceComponentsFromLists(currentInstance);
			Destroy(currentInstance);
			currentInstance = null;
		}

		if (originalObject == null || newPrefab == null) { return null; }

		Transform parent = overrideParent != null ? overrideParent : originalObject.transform;
		Vector3 localPos = originalObject.transform.localPosition;
		Quaternion localRot = originalObject.transform.localRotation;
		Vector3 localScale = originalObject.transform.localScale;

		GameObject instance = Instantiate(newPrefab, parent);
		instance.transform.localPosition = localPos;
		instance.transform.localRotation = localRot;
		instance.transform.localScale = localScale;

		SetDieOriginalVisibility(originalObject, false);
		AddDieInstanceComponentsToLists(instance);
		return instance;
	}

	private GameObject ClearDiePrefabInSlot(GameObject currentInstance, GameObject originalObject)
	{
		if (currentInstance != null)
		{
			RemoveDieInstanceComponentsFromLists(currentInstance);
			Destroy(currentInstance);
			currentInstance = null;
		}
		SetDieOriginalVisibility(originalObject, true);
		return null;
	}

	private GameObject AdditiveEquipDie(GameObject currentInstance, Transform slotParent, GameObject prefab, GameObject originalObject)
	{
		if (slotParent == null || prefab == null) { return currentInstance; }
		if (currentInstance != null)
		{
			RemoveDieInstanceComponentsFromLists(currentInstance);
			Destroy(currentInstance);
			currentInstance = null;
		}

		GameObject instance = Instantiate(prefab, slotParent);
		instance.transform.localPosition = Vector3.zero;
		instance.transform.localRotation = Quaternion.identity;
		instance.transform.localScale = Vector3.one;

		if (originalObject != null)
		{
			SetDieOriginalVisibility(originalObject, false);
		}

		AddDieInstanceComponentsToLists(instance);
		return instance;
	}

	private GameObject RemoveAdditiveDie(GameObject currentInstance, GameObject originalObject)
	{
		if (currentInstance != null)
		{
			RemoveDieInstanceComponentsFromLists(currentInstance);
			Destroy(currentInstance);
		}

		if (originalObject != null)
		{
			SetDieOriginalVisibility(originalObject, true);
		}

		return null;
	}

	private void AddInstanceComponentsToLists(GameObject instance)
	{
		if (instance == null) { return; }

		if (_player != null && _player.MyAnimatorList != null)
		{
			Animator[] animators = instance.GetComponentsInChildren<Animator>(true);
			for (int i = 0; i < animators.Length; i++)
			{
				Animator animator = animators[i];
				if (animator != null && !_player.MyAnimatorList.Contains(animator))
				{
					_player.MyAnimatorList.Add(animator);
					// 교체/추가 직후 애니메이션 타이밍 동기화
					SyncAnimatorTiming(animator);
				}
			}
		}

		if (_playerStat != null && _playerStat.MySpriteREndererList != null)
		{
			SpriteRenderer[] srs = instance.GetComponentsInChildren<SpriteRenderer>(true);
			for (int i = 0; i < srs.Length; i++)
			{
				SpriteRenderer sr = srs[i];
				if (sr != null)
				{
					if (!_playerStat.MySpriteREndererList.Contains(sr))
					{
						_playerStat.MySpriteREndererList.Add(sr);
					}
					// 색상 시스템 편입
					_player?.RegisterOriginalColor(sr);
					// 원본 sortingOrder 저장
					_player?.RegisterOriginalSortingOrder(sr);
				}
			}
		}
	}

	// 기준 애니메이터의 현재 상태 시간과만 동기화한다(파라미터 복제 없음)
	private void SyncAnimatorTiming(Animator target)
	{
		if (target == null || _player == null || _player.MyAnimatorList == null || _player.MyAnimatorList.Count == 0)
		{
			return;
		}

		Animator reference = _player.MyAnimatorList[0];
		if (reference == null) { return; }

		int layers = Mathf.Min(target.layerCount, reference.layerCount);
		for (int i = 0; i < layers; i++)
		{
			var refState = reference.GetCurrentAnimatorStateInfo(i);
			// 정규화된 시간만 맞추고 상태 해시 동일 경로로 재생
			target.Play(refState.fullPathHash, i, refState.normalizedTime % 1f);
		}
		// 즉시 샘플링하여 첫 프레임부터 일치하게 함
		target.Update(0f);
	}

	private void RemoveInstanceComponentsFromLists(GameObject instance)
	{
		if (instance == null) { return; }

		if (_player != null && _player.MyAnimatorList != null)
		{
			Animator[] animators = instance.GetComponentsInChildren<Animator>(true);
			for (int i = 0; i < animators.Length; i++)
			{
				Animator animator = animators[i];
				if (animator != null)
				{
					_player.MyAnimatorList.Remove(animator);
				}
			}
		}

		if (_playerStat != null && _playerStat.MySpriteREndererList != null)
		{
			SpriteRenderer[] srs = instance.GetComponentsInChildren<SpriteRenderer>(true);
			for (int i = 0; i < srs.Length; i++)
			{
				SpriteRenderer sr = srs[i];
				if (sr != null)
				{
					if (_playerStat.MySpriteREndererList.Contains(sr))
					{
						_playerStat.MySpriteREndererList.Remove(sr);
					}
					// 색상 시스템 해제
					_player?.UnregisterOriginalColor(sr);
					// sortingOrder 시스템 해제
					_player?.UnregisterOriginalSortingOrder(sr);
				}
			}
		}
	}

	private void AddDieInstanceComponentsToLists(GameObject instance)
	{
		if (instance == null || _player == null) { return; }
		
		EnableDiePartComponents(instance, true);

		SpriteRenderer[] srs = instance.GetComponentsInChildren<SpriteRenderer>(true);
		for (int i = 0; i < srs.Length; i++)
		{
			SpriteRenderer sr = srs[i];
			if (sr == null) { continue; }
			_player.RegisterDieSpriteRenderer(sr);
			sr.enabled = false;
		}
	}

	private void RemoveDieInstanceComponentsFromLists(GameObject instance)
	{
		if (instance == null || _player == null) { return; }
		
		EnableDiePartComponents(instance, false);

		SpriteRenderer[] srs = instance.GetComponentsInChildren<SpriteRenderer>(true);
		for (int i = 0; i < srs.Length; i++)
		{
			SpriteRenderer sr = srs[i];
			if (sr == null) { continue; }
			_player.UnregisterDieSpriteRenderer(sr);
		}
	}

	private void SetDieOriginalVisibility(GameObject target, bool isVisible)
	{
		if (target == null) { return; }

		SpriteRenderer[] srs = target.GetComponentsInChildren<SpriteRenderer>(true);
		for (int i = 0; i < srs.Length; i++)
		{
			if (srs[i] != null)
			{
				srs[i].enabled = isVisible;
			}
		}
	}

	private void EnableDiePartComponents(GameObject root, bool enable)
	{
		if (root == null)
		{
			return;
		}

		PlayerDiePart[] dieParts = root.GetComponentsInChildren<PlayerDiePart>(true);
		for (int i = 0; i < dieParts.Length; i++)
		{
			if (dieParts[i] != null)
			{
				dieParts[i].enabled = enable;
			}
		}

		BodyPartMarker[] markers = root.GetComponentsInChildren<BodyPartMarker>(true);
		for (int i = 0; i < markers.Length; i++)
		{
			if (markers[i] != null)
			{
				markers[i].enabled = enable;
			}
		}
	}
}


