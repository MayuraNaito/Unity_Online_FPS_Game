using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class Room : MonoBehaviour
{
    // ルーム名テキスト
    public Text buttonText;
    // ルーム情報
    private RoomInfo info;


    // このボタンの変数にルーム情報格納
    public void RegisterRoomDetails(RoomInfo info)
    {
        // ルーム情報格納
        this.info = info;
        // UI
        buttonText.text = this.info.Name;
    }

    // このルームボタンが管理しているルームに参加する関数
    public void OpenRoom()
    {
        // ルーム参加関数を呼び出す
        PhotonManager.instance.JoinRoom(info);
    }

}
