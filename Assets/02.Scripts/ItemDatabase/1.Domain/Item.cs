using System;
using LitJson;
using UnityEngine;
using UnityEngine.AddressableAssets;


[Serializable]
public class Item
{
    public readonly string ID;
    public readonly EItemType ItemType;
    public readonly string Name;
    public readonly string Explanation;
    public readonly string ImageAddress;
    public readonly string PrefabAddress;

    public readonly Sprite Image;
    public readonly GameObject Prefab;


    public Item()
    {

    }

    public Item(JsonData json)
    {
        if (json == null)
        {
            throw new Exception("Json이 유효하지 않습니다");
        }

        ID = json["ItemID"].ToString();
        ItemType = (EItemType)Enum.Parse(typeof(EItemType), json["ItemType"].ToString());
        Name = json["Name"].ToString();
        Explanation = json["Explanation"].ToString();
        ImageAddress = json["ImageAddress"].ToString();
        PrefabAddress = json["PrefabAddress"].ToString();

        Image = Addressables.LoadAssetAsync<Sprite>(ImageAddress).WaitForCompletion();
        Prefab = Addressables.LoadAssetAsync<GameObject>(PrefabAddress).WaitForCompletion();
    }

    public Item(string id, EItemType itemType, string name, string explanation, string imageAddress, string prefabAddress, Sprite image, GameObject prefab)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new Exception("ID가 비어있습니다.");
        }

        if (itemType == EItemType.None)
        {
            throw new Exception("아이템 타입이 올바르지 않습니다.");
        }

        if (string.IsNullOrEmpty(name))
        {
            throw new Exception("아이템 이름이 비어있습니다.");
        }

        if (string.IsNullOrEmpty(explanation))
        {
            throw new Exception("아이템 설명이 비어있습니다.");
        }

        if (string.IsNullOrEmpty(imageAddress))
        {
            throw new Exception("아이콘 이미지 주소가 없습니다.");
        }

        if (string.IsNullOrEmpty(prefabAddress))
        {
            throw new Exception("프리펩 주소가 없습니다.");
        }

        if (image == null)
        {
            throw new Exception("이미지가 없습니다.");
        }

        if (prefab == null)
        {
            throw new Exception("프리펩이 없습니다.");
        }

        ID = id;
        ItemType = itemType;
        Name = name;
        Explanation = explanation;
        ImageAddress = imageAddress;
        PrefabAddress = prefabAddress;

        Image = image;
        Prefab = prefab;
    }
}
