using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

public class CurrencyRepository
{
    private string _userID;

    public CurrencyRepository()
    {
        InitUserID();
    }

    private void InitUserID()
    {
        FirebaseUser user = FirebaseManager.Instance.Auth.CurrentUser;
        _userID = user.UserId;
    }

    public async void SaveCurrencyData(Diamond playerDiamond, Gold playerGold, Exp playerExp)
    {
        DocumentReference docRef = FirebaseManager.Instance.DB.Collection("Currency").Document(_userID);
        try
        {
            await docRef.SetAsync(new CurrencySaveData(playerDiamond.GetAmount(), playerGold.GetAmount(), playerExp.GetValue()));
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"Currency 데이터 저장 실패. 에러코드 {e.ErrorCode} : {e.Message}");
        }
    }

    public async Task<CurrencySaveData> LoadCurrencyData()
    {
        DocumentReference docRef = FirebaseManager.Instance.DB.Collection("Currency").Document(_userID);
        try
        {
            DocumentSnapshot document = await docRef.GetSnapshotAsync();
            if (document.Exists)
            {
                return document.ConvertTo<CurrencySaveData>();
            }
            else
            {
                Debug.LogWarning("Currency  데이터가 존재하지 않습니다.");
                return null;
            }
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"Currency 데이터 저장 실패. 에러코드 {e.ErrorCode} : {e.Message}");
            return null;
        }
    }
}

[FirestoreData]
public class CurrencySaveData
{
    [FirestoreProperty] public int PlayerDiamondAmount { get; private set; }
    [FirestoreProperty] public int PlayerGoldAmount { get; private set; }
    [FirestoreProperty] public int PlayerExpAmount { get; private set; }

    public CurrencySaveData() { }

    public CurrencySaveData(int PlayerDiamond, int playerGold, int playerExp)
    {
        PlayerDiamondAmount = PlayerDiamond;
        PlayerGoldAmount = playerGold;
        PlayerExpAmount = playerExp;
    }
}
