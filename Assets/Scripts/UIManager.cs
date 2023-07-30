using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 弾薬テキスト
    public Text ammoText;
    // HPスライダー格納
    public Slider hpSlider;


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

}
