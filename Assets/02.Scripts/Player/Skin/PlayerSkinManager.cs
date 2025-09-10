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

	private void Awake()
	{
		_player = GetComponent<Player>();
		_playerStat = GetComponent<PlayerStat>();
	}

	public void ApplyHead(ItemDTO item)
	{
		if (item == null || item.Prefab == null) { return; }
		_currentHeadSkin = ReplacePrefabInSlot(_currentHeadSkin, _originalHeadSkin, item.Prefab, _headSlotParent);
	}

	public void ClearHead()
	{
		_currentHeadSkin = ClearPrefabInSlot(_currentHeadSkin, _originalHeadSkin);
	}

	public void ApplyFace(ItemDTO item)
	{
		if (item == null || item.Prefab == null) { return; }
		_currentFaceSkin = ReplacePrefabInSlot(_currentFaceSkin, _originalFaceSkin, item.Prefab, _faceSlotParent);
	}

	public void ClearFace()
	{
		_currentFaceSkin = ClearPrefabInSlot(_currentFaceSkin, _originalFaceSkin);
	}

	public void ApplyChest(ItemDTO item)
	{
		if (item == null || item.Prefab == null) { return; }
		_currentChestSkin = AdditiveEquip(_currentChestSkin, _chestSlotParent, item.Prefab, true);
	}

	public void ClearChest()
	{
		_currentChestSkin = RemoveAdditive(_currentChestSkin, true);
	}

	public void ApplyCape(ItemDTO item)
	{
		if (item == null || item.Prefab == null) { return; }
		_currentCapeSkin = AdditiveEquip(_currentCapeSkin, _capeSlotParent, item.Prefab, true);
	}

	public void ClearCape()
	{
		_currentCapeSkin = RemoveAdditive(_currentCapeSkin, true);
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

	private void AddInstanceComponentsToLists(GameObject instance)
	{
		if (instance == null) { return; }
		Debug.Log($"[SkinMgr] AddInstanceComponentsToLists start. instance={instance.name}");
		if (_player != null && _player.MyAnimatorList != null)
		{
			int addedAnimators = 0;
			Animator[] animators = instance.GetComponentsInChildren<Animator>(true);
			for (int i = 0; i < animators.Length; i++)
			{
				Animator animator = animators[i];
				if (animator != null && !_player.MyAnimatorList.Contains(animator))
				{
					_player.MyAnimatorList.Add(animator);
					// 교체/추가 직후 애니메이션 타이밍 동기화
					SyncAnimatorTiming(animator);
					addedAnimators++;
				}
			}
			Debug.Log($"[SkinMgr] Animators found={animators.Length}, added={addedAnimators} under {instance.name}");
		}

		if (_playerStat != null && _playerStat.MySpriteREndererList != null)
		{
			int addedRenderers = 0;
			SpriteRenderer[] srs = instance.GetComponentsInChildren<SpriteRenderer>(true);
			for (int i = 0; i < srs.Length; i++)
			{
				SpriteRenderer sr = srs[i];
				if (sr != null)
				{
					if (!_playerStat.MySpriteREndererList.Contains(sr))
					{
						_playerStat.MySpriteREndererList.Add(sr);
						addedRenderers++;
					}
					// 색상 시스템 편입
					_player?.RegisterOriginalColor(sr);
				}
			}
			Debug.Log($"[SkinMgr] SpriteRenderers found={srs.Length}, added={addedRenderers} under {instance.name}");
		}
		Debug.Log("[SkinMgr] AddInstanceComponentsToLists end");
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
			Animator animator = instance.GetComponent<Animator>();
			if (animator != null)
			{
				_player.MyAnimatorList.Remove(animator);
			}
		}

		if (_playerStat != null && _playerStat.MySpriteREndererList != null)
		{
			SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
			if (sr != null)
			{
				_playerStat.MySpriteREndererList.Remove(sr);
				// 색상 시스템 해제
				_player?.UnregisterOriginalColor(sr);
			}
		}
	}
}


