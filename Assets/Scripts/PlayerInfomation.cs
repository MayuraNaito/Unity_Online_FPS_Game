using UnityEngine;
using UnityEngine.UI;

public class PlayerInformation : MonoBehaviour
{
    // 名前テキスト
    public Text playerNameText;
    // キルテイスト
    public Text killsText;
    // デステキスト
    public Text deathsText;

    // 表に名前やキルデス数を表示する関数
    public void SetPlayerDetails(string name, int kill, int death)
    {
        playerNameText.text = name;
        killsText.text = kill.ToString();
        deathsText.text = death.ToString();
    }

}
