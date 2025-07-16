using UnityEngine;

public class UI_MapSetup : MonoBehaviour
{
    public ESceneList SelectedMap;
    
    public void SetupMap()
    {
        RoomManager.Instance.SelectedMap = SelectedMap;
        Debug.Log("Selected Map: " + RoomManager.Instance.SelectedMap);
    }
}
