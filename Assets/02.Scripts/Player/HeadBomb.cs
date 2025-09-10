using System.Collections;
using Photon.Pun;
using UnityEngine;

public class HeadBomb : MonoBehaviour
{
	[SerializeField] private Transform _headSlot;
	[SerializeField] private Transform _faceSlot;
	[SerializeField] private GameObject _originalHeadSkin;
	[SerializeField] private GameObject _originalFaceSkin;

	private GameObject _headInstance;
	private GameObject _faceInstance;
	private Player _ownerPlayer;

	private void Awake()
	{
		StartCoroutine(ApplySkinsWhenReady());
	}

	private IEnumerator ApplySkinsWhenReady()
	{
		PhotonView pv = GetComponent<PhotonView>();
		// 오너/플레이어 로딩이 끝나기를 잠깐 대기
		for (int i = 0; i < 60; i++)
		{
			_ownerPlayer = FindOwnerPlayer(pv);
			if (_ownerPlayer != null && _ownerPlayer.EquipedItemDict != null)
			{
				break;
			}
			yield return null;
		}

		ApplySkinsFromOwner();
	}

	private Player FindOwnerPlayer(PhotonView bombPhotonView)
	{
		if (bombPhotonView == null || bombPhotonView.Owner == null) { return null; }
		int ownerActorNumber = bombPhotonView.OwnerActorNr;
		Player[] players = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
		for (int i = 0; i < players.Length; i++)
		{
			var p = players[i];
			if (p != null && p.PhotonView != null && p.PhotonView.Owner != null && p.PhotonView.OwnerActorNr == ownerActorNumber)
			{
				return p;
			}
		}
		return null;
	}

	private void ApplySkinsFromOwner()
	{
		if (_ownerPlayer == null || _ownerPlayer.EquipedItemDict == null) { return; }

		_ownerPlayer.EquipedItemDict.TryGetValue(EItemType.Head, out ItemDTO headItem);
		_ownerPlayer.EquipedItemDict.TryGetValue(EItemType.Face, out ItemDTO faceItem);

		_headInstance = ReplacePrefabInSlot(_headInstance, _originalHeadSkin, headItem != null ? headItem.Prefab : null, _headSlot);
		_faceInstance = ReplacePrefabInSlot(_faceInstance, _originalFaceSkin, faceItem != null ? faceItem.Prefab : null, _faceSlot);
	}

	private GameObject ReplacePrefabInSlot(GameObject currentInstance, GameObject originalObject, GameObject newPrefab, Transform fallbackParent)
	{
		// 기존 교체본 제거
		if (currentInstance != null)
		{
			Destroy(currentInstance);
			currentInstance = null;
		}

		// 새 프리팹이 없으면 원본 복원
		if (newPrefab == null)
		{
			return ClearPrefabInSlot(originalObject);
		}

		Transform parent;
		Vector3 localPos;
		Quaternion localRot;
		Vector3 localScale;

		if (originalObject != null)
		{
			if (originalObject.activeSelf)
			{
				originalObject.SetActive(false);
			}
			parent = originalObject.transform.parent != null ? originalObject.transform.parent : transform;
			localPos = originalObject.transform.localPosition;
			localRot = originalObject.transform.localRotation;
			localScale = originalObject.transform.localScale;
		}
		else
		{
			parent = fallbackParent != null ? fallbackParent : transform;
			localPos = Vector3.zero;
			localRot = Quaternion.identity;
			localScale = Vector3.one;
		}

		GameObject instance = Instantiate(newPrefab, parent);
		instance.transform.localPosition = localPos;
		instance.transform.localRotation = localRot;
		instance.transform.localScale = localScale;

		return instance;
	}

	private GameObject ClearPrefabInSlot(GameObject originalObject)
	{
		if (originalObject != null && !originalObject.activeSelf)
		{
			originalObject.SetActive(true);
		}
		return null;
	}


	private void OnDestroy()
	{
		if (_headInstance != null) { Destroy(_headInstance); _headInstance = null; }
		if (_faceInstance != null) { Destroy(_faceInstance); _faceInstance = null; }
	}
}
