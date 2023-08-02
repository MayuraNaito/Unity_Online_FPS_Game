using UnityEngine;

public class Gun : MonoBehaviour
{
    // 射撃間隔
    [Tooltip("射撃間隔")]
    public float shootInterval = 0.1f;
    // 威力
    [Tooltip("威力")]
    public int shootDamage;
    // 覗き込み時のズーム
    [Tooltip("覗き込み時のズーム")]
    public float adsZoom;
    // 覗き込み時の速度
    [Tooltip("覗き込み時の速度")]
    public float adsSpeed;
    // 弾痕オブジェクト
    [Tooltip("弾痕オブジェクト")]
    public GameObject bulletImpact;
    // 銃声の音源格納
    [Tooltip("銃声の音源格納")]
    public AudioSource shotSound;
    // 弾切れの音源格納
    [Tooltip("弾切れの音源格納")]
    public AudioSource emptySound;


    // Update
    public void Update()
    {
        if (!emptySound.isPlaying)
        {
            emptySound.enabled = false;
        }
    }


    // 単発銃の銃声を鳴らす関数
    public void SoundGunShot()
    {
        // 1回鳴らす
        shotSound.Play();
    }

    // アサルトライフルの銃声を鳴らす関数
    public void LoopON_SubmachineGun()
    {
        // 音がなっているか判定(押してる間は音がループするように)
        if (!shotSound.isPlaying)
        {
            shotSound.loop = true;
            shotSound.Play();
        }
    }

    // アサルトライフルの銃声を止める関数
    public void LoopOFF_SubmachineGun()
    {
        shotSound.loop = false;
        shotSound.Stop();
    }

    // 弾切れの音を鳴らす関数
    public void EmptySound()
    {
        if (Input.GetMouseButtonDown(0))
        {
            emptySound.enabled = true;
            emptySound.Play();
        }
    }
}
