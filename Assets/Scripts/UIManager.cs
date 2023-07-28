using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 弾薬テキスト
    public Text ammoText;


    // テキスト更新用関数
    public void SettingBulletsText(int ammoClip, int ammunition)
    {
        // マガジン内の弾薬表示
        ammoText.text = ammoClip + " / " + ammunition;
    }


}
