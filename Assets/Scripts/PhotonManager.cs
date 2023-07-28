using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    // static変数
    public static PhotonManager instance;
    // ロードパネル
    public GameObject loadingPanel;
    // ロードテキスト
    public Text loadingText;
    // ボタンの親オブジェクト
    public GameObject buttons;
    // ルーム作成パネル
    public GameObject createRoomPanel;
    // ルーム名の入力テキスト
    public Text enterRoomName;
    // ルームパネル
    public GameObject roomPanel;
    // ルームネーム
    public Text roomName;
    // エラーパネル
    public GameObject errorPanel;
    // エラーテキスト
    public Text errorText;
    // ルーム一覧
    public GameObject roomListPanel;
    // ルームボタン格納
    public Room originalRoomButton;
    // ルームボタンの親オブジェクト
    public GameObject roomButtonContent;
    // ルームの情報を扱う辞書(ルーム名：情報)
    Dictionary<string, RoomInfo> roomsList = new Dictionary<string, RoomInfo>();
    // ルームボタンを扱うリスト
    private List<Room> allRoomButtons = new List<Room>();
    // 名前テキスト
    public Text playerNameText;
    // 名前テキスト格納リスト
    private List<Text> allPlayerNames = new List<Text>();
    // 名前テキストの親オブジェクト
    public GameObject playerNameContent;
    // 名前入力パネル
    public GameObject nameInputPanel;
    // 名前入力表示テキスト
    public Text placeholderText;
    // 入力フィールド
    public InputField nameInput;
    // 名前を入力したか判定
    private bool setName;
    // ボタン格納
    public GameObject startButton;
    // 遷移シーン名
    public string levelToPlay;


    // Awake
    public void Awake()
    {
        // static変数に格納
        instance = this;
    }

    // Start
    private void Start()
    {
        // UIを全て閉じる関数の呼び出し
        CloseMenuUI();
        // パネルとテキスト更新
        loadingPanel.SetActive(true);
        loadingText.text = "ネットワークに接続中...";
        // ネットワークに接続
        if (!PhotonNetwork.IsConnected)
        {
            // ネットワークに繋がっていない時
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // メニューUIを全て閉じる関数
    public void CloseMenuUI()
    {
        loadingPanel.SetActive(false);
        buttons.SetActive(false);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(false);
        errorPanel.SetActive(false);
        roomListPanel.SetActive(false);
        nameInputPanel.SetActive(false);
    }

    // ロビーUIを表示する関数
    public void LobbyMenuDisplay()
    {
        CloseMenuUI();
        buttons.SetActive(true);
    }

    // マスターサーバーに接続された時に呼ばれる関数(継承：コールバック)
    public override void OnConnectedToMaster()
    {
        // ロビー接続
        PhotonNetwork.JoinLobby();
        // テキスト更新
        loadingText.text = "ロビーへ参加中...";

        // MasterClientと同じレベルをロード
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // ロビー接続時に呼ばれる関数(継承：コールバック)
    public override void OnJoinedLobby()
    {
        LobbyMenuDisplay();
        // 辞書の初期化
        roomsList.Clear();

        // 名前ランダム
        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        // 名前が入力済みか確認してUI更新
        ConfirmationName();
    }

    // ルーム作るボタン用
    public void OpenCreateRoomPanel()
    {
        CloseMenuUI();
        createRoomPanel.SetActive(true);
    }

    // ルーム作成ボタン用
    public void CreateRoomButton()
    {
        if (!string.IsNullOrEmpty(enterRoomName.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;

            // ルーム作成
            PhotonNetwork.CreateRoom(enterRoomName.text, options);

            CloseMenuUI();

            // ロードパネル表示
            loadingText.text = "ルーム作成中...";
            loadingPanel.SetActive(true);
        }
    }

    // ルーム参加時に呼ばれる関数(継承：コールバック)
    public override void OnJoinedRoom()
    {
        CloseMenuUI();
        roomPanel.SetActive(true);
        // ルームの名前を反映(参加しているルームの名前を反映する)
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        // ルームに居るプレイヤー情報を取得する
        GetAllPlayer();
        // マスターか判定してボタン表示
        CheckRoomMaster();
    }

    // ルーム退出用
    public void LeavRoom()
    {
        PhotonNetwork.LeaveRoom(); // ルームから退出

        CloseMenuUI();
        loadingText.text = "退出中...";
        loadingPanel.SetActive(true);
    }

    // ルーム退出時に呼ばれる関数(継承；コールバック)
    public override void OnLeftRoom()
    {
        // ロビーUI表示
        LobbyMenuDisplay();
    }

    // ルーム作成できなかった時に呼ばれる関数(継承：コールバック)
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CloseMenuUI();
        errorText.text = "ルームの作成に失敗しました。" + message;

        errorPanel.SetActive(true);
    }

    // ルーム一覧パネルを開く関数
    public void FindRoom()
    {
        CloseMenuUI();
        roomListPanel.SetActive(true);
    }

    // ルームリストに更新があった時に呼ばれる関数(継承：コールバック)
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // ルームボタンUI初期化
        RoomUiinitialization();
        // 辞書に登録する
        UpdateRoomList(roomList);
    }

    // ルーム情報を辞書に登録する関数
    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        // 辞書にルーム登録
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];

            // 満室かどうか判定(満室ならtrue, 入れる状態ならfalse)
            if (info.RemovedFromList)
            {
                roomsList.Remove(info.Name);
            } else
            {
                roomsList[info.Name] = info;
            }
        }

        // ルームボタン表示
        RoomListDisplay(roomsList);
    }

    // ルームボタンを作成して表示する関数
    public void RoomListDisplay(Dictionary<string, RoomInfo> cachedRoomList)
    {
        // 辞書に登録されているルーム数分ボタンを作成
        foreach (var roomInfo in cachedRoomList)
        {
            // ボタン作成
            Room newButton = Instantiate(originalRoomButton);
            // 生成したボタンにルーム情報を設定
            newButton.RegisterRoomDetails(roomInfo.Value);
            // 親の設定
            newButton.transform.SetParent(roomButtonContent.transform);
            // リストで管理
            allRoomButtons.Add(newButton);
        }
    }

    // 作られたルームボタンを削除してリストを初期化する関数
    public void RoomUiinitialization()
    {
        // ルームUIの数分ループ
        foreach (Room rm in allRoomButtons)
        {
            // ルームボタン削除
            Destroy(rm.gameObject);
        }

        // リストの初期化
        allRoomButtons.Clear();
    }

    // 引数のルームに入る関数
    public void JoinRoom(RoomInfo roomInfo)
    {
        // ルーム参加
        PhotonNetwork.JoinRoom(roomInfo.Name);

        CloseMenuUI();

        loadingText.text = "ルーム参加中...";
        loadingPanel.SetActive(true);
    }

    // ルームに居るプレイヤー情報を取得する関数
    public void GetAllPlayer()
    {
        // 名前テキストUI初期化
        InitializePlayerList();

        // プレイヤー表示
        PlayerDisplay();
    }

    // 名前テキストUI初期化関数
    public void InitializePlayerList()
    {
        foreach (var rm in allPlayerNames)
        {
            Destroy(rm.gameObject);
        }

        allPlayerNames.Clear();
    }

    // プレイヤー表示用関数
    public void PlayerDisplay()
    {
        // ルームに参加している人数分UI作成
        foreach (var players in PhotonNetwork.PlayerList)
        {
            // UI生成
            PlayerTextGeneration(players);
        }
    }

    // プレイヤーのUIを生成する関数
    public void PlayerTextGeneration(Player players)
    {
        // UI生成
        Text newPlayerText = Instantiate(playerNameText);
        // テキストに名前を反映
        newPlayerText.text = players.NickName;
        // 親オブジェクトの設定
        newPlayerText.transform.SetParent(playerNameContent.transform);
        // UIをリストに登録
        allPlayerNames.Add(newPlayerText);
    }

    // 名前が入力済みか確認してUI更新する関数
    public void ConfirmationName()
    {
        // 名前が設定されていない時
        if (!setName)
        {
            CloseMenuUI();
            // 名前入力パネルを表示
            nameInputPanel.SetActive(true);

            // 名前が保存されているか
            if (PlayerPrefs.HasKey("playerName"))
            {
                // テキストに保存されている名前を反映
                placeholderText.text = PlayerPrefs.GetString("playerName");
                nameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            // 名前が設定されていれば自動で反映
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }

    // 名前登録用の関数
    public void SetName()
    {
        // 入兎力フィールドに文字が入力されているか
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            // ユーザー名登録
            PhotonNetwork.NickName = nameInput.text;
            // ユーザー名保存
            PlayerPrefs.SetString("playerName", nameInput.text);
            // UI
            LobbyMenuDisplay();

            setName = true;
        }
    }

    // プレイヤーがルームに入った時に呼び出される関数(継承：コールバック)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // プレイヤーのテキストを生成する
        PlayerTextGeneration(newPlayer);
    }

    // プレイヤーがルームを離れるか、非アクティブになった時に呼び出される関数(継承：コールバック)
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GetAllPlayer();
    }

    // マスターか判定してボタン表示する関数
    public void CheckRoomMaster()
    {
        // 自分がマスターならtrue
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        } else
        {
            startButton.SetActive(false);
        }
    }
    // マスターが切り替わった時に呼ばれる関数(継承：コールバック)
    public override void OnMasterClientSwitched(Player newMasterClient)
    {

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
    }

    // 遷移用関数
    public void PlayGame()
    {
        // ルームにステージを読み込む
        PhotonNetwork.LoadLevel(levelToPlay);
    }

}
