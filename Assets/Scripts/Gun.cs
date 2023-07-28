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
    public GameObject bulletImpact;


}
