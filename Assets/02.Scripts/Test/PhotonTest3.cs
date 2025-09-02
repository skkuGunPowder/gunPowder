// using Photon.Pun;
// using Photon.Realtime;
// using UnityEngine;
// using UnityEngine.SceneManagement;
//
// public class PhotonTest3 : MonoBehaviour
// {
//     public void Onclick()
//     {
//         PhotonNetwork.LeaveRoom();
//     }
//     public void MakeRoom()
//     {
//         
//         RoomOptions roomOptions = new RoomOptions();
//         
//         roomOptions.MaxPlayers = 2;
//         roomOptions.IsVisible = true;
//         roomOptions.IsOpen = true;
//         roomOptions.CustomRoomPropertiesForLobby = new string[]
//         {
//             $"{EProperties.MapSelected}",
//             $"{EProperties.IsLocked}",
//             $"{EProperties.PlayTime}",
//             $"{EProperties.Life}",
//             $"{EProperties.Gunpowder}",
//             $"{EProperties.DeclinePowder}",
//             $"{EProperties.Password}"
//         };
//
//         PhotonNetwork.CreateRoom("Fsdfadsfa", roomOptions, TypedLobby.Default);
//         
//         PhotonNetwork.LoadLevel("Test1");
//     }
// }
