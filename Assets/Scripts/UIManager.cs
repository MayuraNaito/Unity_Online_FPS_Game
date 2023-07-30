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

}
