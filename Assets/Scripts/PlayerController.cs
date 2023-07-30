using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    // カメラの親オブジェクト
    public Transform viewPoint;
    // 視点移動の速度
    public float mouseSensitivity = 1f;
    // ユーザーのマウス入力格納
    private Vector2 mouseInput;
    // Y軸の回転格納
    private float verticalMouseInput;
    // カメラ
    private Camera cam;
    // 入力された値格納
    private Vector3 moveDir;
    // 進む方向格納
    private Vector3 movement;
    // 実際の移動速度
    private float activeMoveSpeed = 4f;
    // ジャンプ力
    public Vector3 jumpForce = new Vector3(0, 6, 0);
    // レイを飛ばすオブジェクトの位置
    public Transform groundCheckPoint;
    // 地面レイヤー
    public LayerMask groundLayers;
    // 剛体
    private Rigidbody rb;
    // 歩き速度
    public float walkSpeed = 4f;
    // 走り速度
    public float runSpeed = 8f;
    // カーソルの表示判定
    private bool cursorLock = true;
    // 武器の格納リスト
    public List<Gun> guns = new List<Gun>();
    // 選択中の武器管理用数値
    private int selectedGun = 0;
    // 射撃間隔
    private float shotTimer;
    // 所持弾薬
    [Tooltip("所持弾薬")]
    public int[] ammunition;
    // 最高所持弾薬数
    [Tooltip("最高所持弾薬数")]
    public int[] maxAmmunition;
    // マガジン内の弾数
    [Tooltip("マガジン内の弾数")]
    public int[] ammoClip;
    // マガジンに入る最大の数
    [Tooltip("マガジンに入る最大の数")]
    public int[] maxAmmoClip;
    // UIManager格納用
    private UIManager UIManager;
    // SpawnManager格納
    private SpawnManager spawnManager;
    // アニメーター
    public Animator animator;
    // プレイヤーモデルを格納
    public GameObject[] playerModel;
    // 銃ホルダー(自分用、他人用)
    public Gun[] gunsHolder, otherGunsHolder;
    // 最大HP
    public int maxHP = 100;
    // 現在HP
    private int currentHP;

    private void Awake()
    {
        // UIManager格納
        UIManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        // spawnManager格納
        spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();
    }

    // Start
    private void Start()
    {
        // 現在HPに最大HPを代入
        currentHP = maxHP;

        // カメラ格納
        cam = Camera.main;
        // 剛体
        rb = GetComponent<Rigidbody>();
        // カーソル関数の呼び出し
        UpdateCursorLock();
        // ランダムな位置でスポーンさせる関数の呼び出し
        //transform.position = spawnManager.GetSpawnPoint().position;
        // 銃を扱うリストの初期化
        guns.Clear();

        // モデルや銃の表示切り替え(自分だった場合true)
        if (photonView.IsMine)
        {
            foreach (var model in playerModel)
            {
                model.SetActive(false);
            }
            // 表示する方の銃を設定
            foreach (Gun gun in gunsHolder)
            {
                guns.Add(gun);
            }

            // HPをスライダーに反映
            UIManager.UpdateHP(maxHP, currentHP);
        } else
        {
            // 表示する方の銃を設定
            foreach (Gun gun in otherGunsHolder)
            {
                guns.Add(gun);
            }
        }

        // 銃を表示する関数の呼び出し
        //SwitchGun();
        // 全プレイヤーで共有できる銃の切り替え
        photonView.RPC("SetGun", RpcTarget.All, selectedGun);
    }


    // Update
    private void Update()
    {
        // 自分のプレイヤーオブジェクトだけ操作する
        if (!photonView.IsMine)
        {
            return;
        }

        // 視点移動関数の呼び出し
        PlayerRotate();
        // 移動関数の呼び出し
        PlayerMove();

        // 地面に着いている時
        if (IsGround())
        {
            // 走り関数の呼び出し
            Run();
            // ジャンプ関数の呼び出し
            Jump();
        }

        // 覗き込み関数の呼び出し
        Aim();
        // 射撃ボタン検知関数の帯だし
        Fire();
        // リロード関数の呼び出し
        Reload();
        // 武器の変更キー検知関数の呼び出し
        SwitchingGuns();
        // カーソル関数の呼び出し
        UpdateCursorLock();
        // アニメーション遷移関数の呼び出し
        AnimatorSet();
    }

    // FixedUpdate
    private void FixedUpdate()
    {
        // 自分のプレイヤーオブジェクトだけ操作する
        if (!photonView.IsMine)
        {
            return;
        }

        // テキスト更新関数の呼び出し
        UIManager.SettingBulletsText(ammoClip[selectedGun], ammunition[selectedGun]);
    }


    // 視点移動関数
    public void PlayerRotate()
    {
        // ユーザーのマウスの動きを検知して変数に格納
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X") * mouseSensitivity,
            Input.GetAxisRaw("Mouse Y") * mouseSensitivity);

        // マウスのX軸の動きを反映
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x,
            transform.eulerAngles.y + mouseInput.x,
            transform.eulerAngles.z);

        // Y軸の値に現在の値を足す
        verticalMouseInput += mouseInput.y;

        // 数値を丸める
        verticalMouseInput = Mathf.Clamp(verticalMouseInput, -60f, 60f);

        // viewPointに丸めた数値を反映
        viewPoint.rotation = Quaternion.Euler(-verticalMouseInput,
            viewPoint.transform.rotation.eulerAngles.y,
            viewPoint.transform.rotation.eulerAngles.z);
    }

    // LateUpdate
    private void LateUpdate()
    {
        // 自分のプレイヤーオブジェクトだけ操作する
        if (!photonView.IsMine)
        {
            return;
        }

        // カメラの位置調整
        cam.transform.position = viewPoint.position;
        // 回転
        cam.transform.rotation = viewPoint.rotation;
    }


    // プレイヤーの動き用
public void PlayerMove()
    {
        // 移動用キーの入力を検知して値を格納
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0,
            Input.GetAxisRaw("Vertical"));

        // 進む方向を出して変数に格納
        movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized;

        // 現在位置に反映
        transform.position += movement * activeMoveSpeed * Time.deltaTime;
    }

    // ジャンプ用
    public void Jump()
    {
        // プレイヤーが地面に接しており、スペースが押された時
        if (IsGround() && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(jumpForce, ForceMode.Impulse);
        }
    }

    // 地面判定(地面に着いていればtrue)
    public bool IsGround()
    {
        // 引数(レーザー飛ばすポジション、方向、距離、地面判定するレイヤー)
        return Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayers);
    }

    // 走り関数
    public void Run()
    {
        // シフトが押されている時にスピードを切り替える
        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
        } else
        {
            activeMoveSpeed = walkSpeed;
        }
    }

    // カーソル表示切り替え
    public void UpdateCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLock = false; // 表示
        } else if (Input.GetMouseButton(0))
        {
            cursorLock = true; // 非表示
        }

        if (cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked; // カーソルを中央にし非表示
        } else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // ホイールで武器切り替え
    public void SwitchingGuns()
    {
        // マウスホイールがプラスかマイナスかでループするように
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            selectedGun++;

            if (selectedGun >= guns.Count)
            {
                selectedGun = 0;
            }

            // 銃を切り替える関数の呼び出し
            //SwitchGun();
            // 全プレイヤーで共有できる銃の切り替え
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            selectedGun--;

            if (selectedGun < 0)
            {
                selectedGun = guns.Count - 1;
            }

            // 銃を切り替える関数の呼び出し
            //SwitchGun();
            // 全プレイヤーで共有できる銃の切り替え
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        }

        // 数値キー入力で銃の切り替え
        for (int i = 0; i < guns.Count; i++)
        {
            // 数値キーを押したかの判定
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                // 銃切り替え
                selectedGun = i;
                //SwitchGun();
                // 全プレイヤーで共有できる銃の切り替え
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            }
        }
    }

    // 銃を切り替える関数
    public void SwitchGun()
    {
        foreach (Gun gun in guns)
        {
            // 全ての銃を非表示
            gun.gameObject.SetActive(false);
        }

        // リストにある特定の要素の銃が表示される
        guns[selectedGun].gameObject.SetActive(true);
    }

    // 覗き込み関数
    public void Aim()
    {
        // 右クリックの検知
        if (Input.GetMouseButton(1))
        {
            // Lerp(開始地点、終了地点、保管数値の割合で近づく)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,
                guns[selectedGun].adsZoom,
                guns[selectedGun].adsSpeed * Time.deltaTime);
        } else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,
                60f,
                guns[selectedGun].adsSpeed * Time.deltaTime);
        }
    }

    // 射撃ボタン検知関数
    public void Fire()
    {
        // 撃ち出せるのか判定(マウスがクリックされているか、マガジンに弾があるか、)
        if (Input.GetMouseButton(0) && ammoClip[selectedGun] > 0 && Time.time > shotTimer)
        {
            // 弾を撃ち出す関数の呼び出し
            FiringBullet();
        }
    }

    // 弾を撃ち出す関数
    public void FiringBullet()
    {
        // 弾を減らす
        ammoClip[selectedGun]--;

        // カメラの中心から光線を作る
        Ray ray = cam.ViewportPointToRay(new Vector2(0.5f, 0.5f));

        // 何かにレーザーが当たったらtrue
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //Debug.Log("当たったオブジェクトは" + hit.collider.gameObject.name);

            // レーザーの当たったオブジェクトがプレイヤーならtrue
            if (hit.collider.gameObject.tag == "Player")
            {
                hit.collider.gameObject.GetPhotonView().RPC("Hit",
                    RpcTarget.All,
                    guns[selectedGun].shootDamage,
                    photonView.Owner.NickName,
                    PhotonNetwork.LocalPlayer.ActorNumber);
            } else
            {
                // プレイヤー以外に当たったら弾痕を当たった場所に生成
                GameObject bulletImpactObject = Instantiate(guns[selectedGun].bulletImpact,
                    hit.point + (hit.normal * 0.02f),
                    Quaternion.LookRotation(hit.normal, Vector3.up));

                Destroy(bulletImpactObject, 10f);
            }
        }

        // 射撃間隔の設定　ゲームの経過時間にインターバルを足しshotTimerに入れる(連続で撃てなくなる)
        shotTimer = Time.time + guns[selectedGun].shootInterval;
    }

    // リロード関数
    private void Reload()
    {
        // Rボタンが押されたか判定
        if (Input.GetKeyDown(KeyCode.R))
        {
            // リロードで補充する弾数の取得
            int amountNeed = maxAmmoClip[selectedGun] - ammoClip[selectedGun];

            // 補充したい弾薬、所持弾薬の数を比較し格納
            int ammoAvailable = amountNeed < ammunition[selectedGun] ? amountNeed : ammunition[selectedGun];

            // 弾薬満タンでない時＆弾薬所持している時
            if (amountNeed != 0 && ammunition[selectedGun] != 0)
            {
                // 所持弾薬からリロードする弾薬を引く
                ammunition[selectedGun] -= ammoAvailable;

                // 銃に弾薬セット
                ammoClip[selectedGun] += ammoAvailable;
            }
        }
    }

    // アニメーション遷移関数
    public void AnimatorSet()
    {
        // walk判定
        if (moveDir != Vector3.zero)
        {
            animator.SetBool("walk", true);
        } else
        {
            animator.SetBool("walk", false);
        }

        // run判定
        if (Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetBool("run", true);
        } else
        {
            animator.SetBool("run", false);
        }
    }

    // リモート呼び出し可能な、銃切り替え関数
    [PunRPC] // 他のユーザーからも呼び出し可能になる
    public void SetGun(int gunNo)
    {
        if (gunNo < guns.Count)
        {
            selectedGun = gunNo;

            SwitchGun();
        }
    }

    // 被弾関数(全プレイヤー共有)
    [PunRPC] // 他のユーザーからも呼び出し可能になる
    public void Hit(int damage, string name, int actor)
    {
        // HPを減らす関数の呼び出し
        ReceiveDamage(name, damage, actor);
    }

    // HPを減らす関数
    public void ReceiveDamage(string name, int damage, int actor)
    {
        // プレイヤーの管理者が自分かどうか(撃たれたのが自分かどうか)
        if (photonView.IsMine)
        {
            // HPを減らす
            currentHP -= damage;

            // 0以下になったか判定をする
            if (currentHP <= 0)
            {
                // 死亡関数の呼び出し
                Death(name, actor);
            }

            // HPをスライダーに反映
            UIManager.UpdateHP(maxHP, currentHP);
        }
    }

    // 死亡時の処理をする関数
    public void Death(string name, int actor)
    {
        // マイナスになってるかもしれないので0にする
        currentHP = 0;
        // Death関数の呼び出し
        UIManager.UpdateDeathUI(name);
        // リスポーン関数の呼び出し
        spawnManager.Die();
    }

}
