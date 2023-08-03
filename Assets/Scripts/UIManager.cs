using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 弾薬テキスト
    public Text ammoText;
    // HPスライダー格納
    public Slider hpSlider;
    // 死亡パネル
    public GameObject deathPanel;
    // デステキスト
    public Text deathText;
    // スコアボード格納
    public GameObject scoreboard;
    // PlayerInformation格納
    public PlayerInformation info;
    // 終了パネル
    public GameObject endPanel;
    // 設定パネル
    public GameObject controlPanel;
    // 設定画面の終了と戻るボタン
    [HideInInspector]
    public bool exitButtonFlg = false;
    [HideInInspector]
    public bool backButtonFlg = false;


    // テキスト更新用関数
    public void SettingBulletsText(int ammoClip, int ammunition)
    {
        // マガジン内の弾薬表示
        ammoText.text = ammoClip + " / " + ammunition;
    }

    // HP更新関数
    public void UpdateHP(int maxHP, int currentHP)
    {
        hpSlider.maxValue = maxHP;
        hpSlider.value = currentHP;
    }

    // デスパネルを更新して開く関数
    public void UpdateDeathUI(string name)
    {
        // パネルを開く
        deathPanel.SetActive(true);
        // テキストに名前を表示
        deathText.text = name + "にやられた！";
        // デスパネルを閉じる
        Invoke("CloseDeathUI", 5f);
    }

    // デスパネルを閉じる関数
    public void CloseDeathUI()
    {
        deathPanel.SetActive(false);
    }

    // スコアボードを開く関数
    public void ChangeScoreUI()
    {
        // 表示非表示を切り替える(ヒエラルキー上で表示されていればtrueになるので反転)
        scoreboard.SetActive(!scoreboard.activeInHierarchy);
    }

    // ゲーム終了画面を表示する関数
    public void OpenEndPanel()
    {
        endPanel.SetActive(true);
    }

    // 設定画面を表示する関数
    public void OpenControlPanel()
    {
        controlPanel.SetActive(true);
    }

    // 設定画面を非表示にする関数
    public void CloseControlPanel()
    {
        controlPanel.SetActive(false);
        // フラグのリセット
        exitButtonFlg = false;
        backButtonFlg = false;
    }

    // 設定画面のゲーム終了ボタン用イベント関数
    public void OnExitButton()
    {
        exitButtonFlg = true;
    }

    // 設定画面の戻るボタン用イベント関数
    public void OnBackButton()
    {
        backButtonFlg = true;
    }

}
