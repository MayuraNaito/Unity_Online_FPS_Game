using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;


public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{

    // プレイヤー情報を格納するリスト
    public List<PlayerInfo> playerList = new List<PlayerInfo>();
    // イベント作成(Photonの持ってるイベントもbyte型なので合わせる)
    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat
    }

    // ゲーム状態作成
    public enum Gamestate
    {
        // プレイ中
        Playing,
        // 終了中
        Ending
    }

    // ゲーム状態格納
    public Gamestate state;

    private void Start()
    {
        // ネットワークに繋がっていない時タイトルに戻る
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        } else
        {
            // 新規ユーザーがマスターに情報を送る関数の呼び出し
            NewPlayerGet(PhotonNetwork.NickName);

            // ゲームの状態を決める
            state = Gamestate.Playing;
        }
    }

    // コールバック関数
    public void OnEvent(EventData photonEvent)
    {
        // カスタムイベントなのか判定(Photonが独自のイベントを使う時は200以上の数値のためこの中に入るのは自分の作ったイベント確定)
        if (photonEvent.Code < 200)
        {
            // イベントコード格納
            EventCodes eventCode = (EventCodes)photonEvent.Code;
            // 送られてきたイベントデータを格納
            object[] data = (object[])photonEvent.CustomData;

            switch (eventCode)
            {
                case EventCodes.NewPlayer:
                    NewPlayerSet(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayersSet(data);
                    break;
                case EventCodes.UpdateStat:
                    ScoreSet(data);
                    break;
            }
        }
    }

    // コンポーネントがオンになると呼ばれる
    public override void OnEnable()
    {
        // コールバック登録 ゲームマネージャーをコールバック対象として登録
        PhotonNetwork.AddCallbackTarget(this);
    }

    // コンポーネントがオフになると呼ばれる
    public override void OnDisable()
    {
        // コールバック登録の解除 ゲームマネージャーをコールバック対象として解除
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // 新規ユーザーがネットワーク経由でマスターに自分の情報を送る関数
    public void NewPlayerGet(string name)
    {
        // 配列に自分の情報を格納
        object[] info = new object[4];
        info[0] = name; // 名前
        info[1] = PhotonNetwork.LocalPlayer.ActorNumber; // 管理番号
        info[2] = 0; // キル数
        info[3] = 0; // デス数

        // 新規ユーザー発生イベント(どんなイベントか、渡したいもの、誰に情報を渡すか、信用できる通信かどうか)
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            info,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true });
    }

    // 送られてきた新プレイヤーの情報をリストに格納する関数(マスターだけが呼ばれる)
    public void NewPlayerSet(object[] data)
    {
        // プレイヤー情報を変数に格納
        PlayerInfo player = new PlayerInfo((string)data[0], (int)data[1], (int)data[2], (int)data[3]);
        // プレイヤー情報をリストに格納
        playerList.Add(player);

        // 取得したプレイヤー情報をルーム内の全プレイヤーに送信する
        ListPlayersGet();
    }

    // 取得したプレイヤー情報をルーム内の全プレイヤーに送信する関数
    public void ListPlayersGet()
    {
        // 送信するユーザー情報を格納
        object[] info = new object[playerList.Count + 1];
        info[0] = state; // ゲームの状態

        // すべてのユーザー情報をinfoに格納
        for (int i = 0; i < playerList.Count; i++)
        {
            object[] temp = new object[4];
            // ユーザー情報を格納
            temp[0] = playerList[i].name; // 名前
            temp[1] = playerList[i].actor; // 管理番号
            temp[2] = playerList[i].kills; // キル数
            temp[3] = playerList[i].deaths; // デス数

            info[i + 1] = temp;
        }

        // 情報共有イベント発生(どんなイベントか、渡したいもの、誰に情報を渡すか、信用できる通信かどうか)
        PhotonNetwork.RaiseEvent(
                (byte)EventCodes.ListPlayers,
                info,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true });
    }

    // 新しいプレイヤー情報をリストに格納する関数
    public void ListPlayersSet(object[] data)
    {
        // プレイヤー管理リストの初期化
        playerList.Clear();
        // ゲームの状態
        state = (Gamestate)data[0];

        // すべてのユーザー情報をリストに格納(1はステートを格納しているので1からスタート)
        for (int i = 1; i < data.Length; i++)
        {
            object[] info = (object[])data[i];

            // 名前、管理番号、キル数、デス数
            PlayerInfo player = new PlayerInfo((string)info[0],
                (int)info[1],
                (int)info[2],
                (int)info[3]);

            playerList.Add(player);
        }
    }

    // キル数やデス数を取得してイベント発生させる関数(どのユーザーかの番号、キルかデスかの判定数値、加算する用の数値)
    public void ScoreGet(int actor, int state, int amount)
    {
        // 引数の値を配列に格納
        object[] package = new object[] { actor, state, amount };

        // キルデスイベント発生(どんなイベントか、渡したいもの、誰に情報を渡すか、信用できる通信かどうか)
        PhotonNetwork.RaiseEvent(
                (byte)EventCodes.UpdateStat,
                package,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true });
    }

    // 受け取ったデータをリストに反映する関数(ネットワーク経由で受け取る)
    public void ScoreSet(object[] data)
    {
        // キルデス情報の更新
        int actor = (int)data[0]; // 管理番号
        int state = (int)data[1]; // キル数(0はキル 1はデス)
        int amount = (int)data[2]; // 加算数値

        // actorに当てはまるプレイヤーが誰かを判定
        for (int i = 0; i < playerList.Count; i++)
        {
            // キルかデスが起こればtrue(加算)
            if (playerList[i].actor == actor)
            {
                switch(state)
                {
                    case 0:
                        playerList[i].kills += amount;
                        break;
                    case 1:
                        playerList[i].deaths += amount;
                        break;
                }
                break;
            }
        }
    }

}

// プレイヤーの情報を扱うクラス
[System.Serializable] // インスペクターに表示する
public class PlayerInfo
{
    // 名前
    public string name;
    // 管理番号、キル数、デス数
    public int actor, kills, deaths;

    // 変数に引数の値を格納(コンストラクタ)
    public PlayerInfo(string _name, int _actor, int _kills, int _deaths)
    {
        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _deaths;
    }
}