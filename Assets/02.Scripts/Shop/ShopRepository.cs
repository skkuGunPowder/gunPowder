using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using BackEnd;
using LitJson;
using System;

public class ShopRepository
{
    private const int SHOP_DATA_FOLDER_ID = 2622;
    private string _userID;

    public event Action<Dictionary<EItemType, List<ShopItem>>> OnLoadComplete;

    public ShopRepository()
    {
        InitUserID();
        LoadShopItem();
    }

    private void InitUserID()
    {
        FirebaseUser user = FirebaseManager.Instance.Auth.CurrentUser;
        _userID = user.UserId;
    }

    public void LoadShopItem()
    {
        Dictionary<EItemType, List<ShopItem>> shopItemDict = new Dictionary<EItemType, List<ShopItem>>();
        for (int i = 0; i < (int)EItemType.Count; i++)
        {
            shopItemDict.Add((EItemType)i, new List<ShopItem>());
        }

        try
        {
            Backend.Chart.GetChartListByFolderV2(SHOP_DATA_FOLDER_ID, result =>
            {
                if (!result.IsSuccess())
                {
                    Debug.LogError($"상점 아이템 데이터 불러오기 실패: {result.GetMessage()}");
                    return;
                }

                var itemResult = Backend.Chart.GetChartContents(result.FlattenRows()[0]["selectedChartFileId"].ToString());
                if (!itemResult.IsSuccess())
                {
                    Debug.LogError($"상점 아이템 데이터 불러오기 실패: {itemResult.GetMessage()}");
                    return;
                }

                foreach (JsonData iteminfo in itemResult.FlattenRows())
                {
                    ShopItem item = new ShopItem(iteminfo);
                    shopItemDict[item.ItemInfo.ItemType].Add(item);
                }

                Debug.LogWarning($"상점 아이템 데이터 불러오기 성공: {itemResult.GetMessage()}");
                OnLoadComplete?.Invoke(shopItemDict);
            });
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public async Task<Result> BuyItem(ShopItem item)
    {
        DocumentReference docRef = FirebaseManager.Instance.DB.Collection("Shop").Document(_userID);
        try
        {
            int purchaseAmount = 0;

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                purchaseAmount = snapshot.GetValue<int>(item.ID);
            }

            if (purchaseAmount + 1 > item.MaxAmount)
            {
                return new Result(false, $"{item.ItemInfo.Name} 구매 가능한 개수 초과");
            }

            await docRef.SetAsync(new { purchaseAmount = purchaseAmount + 1 });
            return new Result(true, $"{item.ItemInfo.Name} 구매 성공");
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"ItemStorage 데이터 저장 실패. 에러코드 {e.ErrorCode} : {e.Message}");
            return new Result(false, e.Message);
        }
    }
}

