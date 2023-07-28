using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviour
{
    // spawnPoints格納配列
    public Transform[] spawnPoints;
    // 生成するプレイヤーオブジェクト
    public GameObject playerPrefab;
    // 生成したプレイヤーオブジェクト
    private GameObject player;
    // スポーンまでのインターバル
    public float respawnInterval = 5f;


    // start
    private void Start()
    {
        // スポーンオブジェクトを全て非表示
        foreach (Transform position in spawnPoints)
        {
            position.gameObject.SetActive(false);
        }

        // 生成関数の呼び出し
        if (PhotonNetwork.IsConnected)
        {
            // ネットワークオブジェクトとしてプレイヤーを生成する
            SpawnPlayer();
        }
    }


    // ランダムにスポーンポイントの一つを選択する関数
    public Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    public void SpawnPlayer()
    {
        // ランダムなスポーンポジションを変数に格納
        Transform spawnPoint = GetSpawnPoint();
        // ネットワークオブジェクト生成
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }


}
