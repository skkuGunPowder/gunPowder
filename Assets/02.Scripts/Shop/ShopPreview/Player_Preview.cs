using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Player_Preview : MonoBehaviour
{
    [SerializeField] private List<Animator> _myAnimatorList;
    public Dictionary<EItemType, ItemDTO> EquipedItemDict;

    public Transform FireTransform;

    private Rigidbody2D _rigidbody2D;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        EquipedItemDict = new Dictionary<EItemType, ItemDTO>();
    }

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.X))
    //     {
    //         HandleSpecialBomb();
    //     }
    // }

    private void HandleSpecialBomb()
    {
        if (!EquipedItemDict.ContainsKey(EItemType.Bomb))
        {
            return;
        }

        SoundManager.Instance.PlayLocalRandomSound("PlayerShot", transform, 1, 2);


        GameObject bomb = Instantiate(EquipedItemDict[EItemType.Bomb].Prefab, FireTransform.position, Quaternion.identity);
        
        if (bomb == null)
        {
            return;
        }
        
        // 2. Bomb 컴포넌트 가져오기
        Bomb bombComponent = bomb.GetComponent<Bomb>();
        if (bombComponent == null)
        {
            return;
        }
        
        
        // 4. RPC 호출 (SetOwner 먼저, 그 다음 폭탄 동작)
        bombComponent.ThrowBomb(FireTransform.right, FireTransform.up, FireTransform.forward);


        foreach (Animator animator in _myAnimatorList)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void EquipItem(ShopItem item)
    {
        if (EquipedItemDict.ContainsKey(item.ItemInfo.ItemType))
        {
            EquipedItemDict[item.ItemInfo.ItemType] = item.ItemInfo;
        }
        else
        {
            EquipedItemDict.Add(item.ItemInfo.ItemType, item.ItemInfo);
        }
    }

    protected virtual void ApplyRecoil(Transform bombSpawnPoint, float recoilPower = 5f, float upPower = 1f)
    {
        if (_rigidbody2D == null) return;
        Vector2 dir = (transform.position - bombSpawnPoint.position).normalized;
        Vector2 recoil = new Vector2(dir.x, dir.y).normalized * recoilPower;
        recoil.y += upPower;
        _rigidbody2D.AddForce(recoil, ForceMode2D.Impulse);
    }
}
