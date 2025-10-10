//--------------------------------------------------------------
// プレイヤー親クラス [ PlayerBase.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MessagePack;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Pixeye.Unity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Shared.Interfaces.StreamingHubs.EnumManager;

abstract public class PlayerBase : CharacterBase
{
    //--------------------
    // フィールド

    #region アニメーションID
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Idle = 1,
        Run,
        Hit,
        Fall,
        Dead,
        Blink,
        DBJump,
        WallSlide,
        Zipline,
    }
    #endregion

    #region 共通ステータス
    [Foldout("共通ステータス")]
    protected int nowLv = 1;        // 現在レベル
    [Foldout("共通ステータス")]
    protected int nowExp = 0;       // 現在の獲得経験値
    [Foldout("共通ステータス")]
    protected int nextLvExp = 10;   // 次のレベルまでに必要な経験値

    [Foldout("共通ステータス")]
    protected int startHp = 0;      // 初期体力

    [Foldout("共通ステータス")]
    [SerializeField] protected float m_JumpForce = 400f;    // ジャンプ力
    [Foldout("共通ステータス")]
    [SerializeField] protected float wallJumpPower = 2f;    // 壁ジャンプ力
    [Foldout("共通ステータス")]
    [SerializeField] protected bool m_AirControl = false;   // 空中制御フラグ
    [Foldout("共通ステータス")]
    public bool canDoubleJump = true;                       // ダブルジャンプ使用フラグ

    [Foldout("共通ステータス")]
    [SerializeField] protected float m_BlinkForce = 38f;  // ブリンク力
    [Foldout("共通ステータス")]
    [SerializeField] protected float blinkTime = 0.35f;   // ブリンク時間
    [Foldout("共通ステータス")]
    [SerializeField] protected float blinkCoolDown = 1f;  // ブリンククールダウン

    [Foldout("共通ステータス")]
    [SerializeField] protected float m_ZipJumpForceX = 60f;  // ジップから降りるときの力(X軸)
    [Foldout("共通ステータス")]
    [SerializeField] protected float m_ZipJumpForceY = 40f;  // ジップから降りるときの力(Y軸)

    [Foldout("共通ステータス")]
    [SerializeField] protected float zipSpeed = 150f;   // 梯子移動速度

    [Foldout("共通ステータス")]
    [Range(0, .3f)][SerializeField] protected float m_MovementSmoothing = .05f;

    [Foldout("共通ステータス")]
    public bool invincible = false; // プレイヤーの死亡制御フラグ

    protected Player_Type playerType;                   // プレイヤータイプ
    protected float gravity;                            // 重力

    private float regeneTimer;              //  オートリジェネタイマー
    private float healGenerateTimer = 0f;   //  回復生成タイマー

    #endregion

    #region レリック関連ステータス

    [Foldout("レリック関連ステータス")]
    [SerializeField] protected GameObject healObjectPrefab; // 回復オブジェクトのPrefab

    protected Dictionary<DEBUFF_TYPE, float> giveDebuffRates = new Dictionary<DEBUFF_TYPE, float>()
        {
            { DEBUFF_TYPE.Burn, 0f },
            { DEBUFF_TYPE.Freeze, 0f },
            { DEBUFF_TYPE.Shock, 0f },
        };  // 状態異常付与率◎
    public float regainCodeRate = 0f;        // ◎与ダメ回復率
    public int scatterBugCnt = 0;            // （保留）ボム所持数
    public float holographicArmorRate = 0;   // ◎回避率
    public float mouseRate = 0;              // ◎クールダウン短縮率
    public int digitalMeatCnt = 0;           // ◎回復肉所持数
    public float firewallRate = 0;           // ◎被ダメージ軽減率
    public float lifeScavengerRate = 0;      // ◎キル時HP回復率
    public float rugrouterRate = 0;          // ◎ダブルアタック率
    public int buckupHDMICnt = 0;            // ◎リバイブ回数
    public float identificationAIRate = 0;   // ◎状態異常ダメージ倍率
    public float danborDollRate = 0;         // ◎防御貫通率◎
    public int chargedCoreCnt = 0;           // （保留）感電オーブ所持数
    public float illegalScriptRate = 0;      // ◎クリティカルオーバーキル発生率

    #endregion

    #region ステータス外部参照用プロパティ

    /// <summary>
    /// プレイヤーの動作制御フラグ
    /// </summary>
    public bool CanMove { get { return canMove; } set { canMove = value; } }

    /// <summary>
    /// 現レベル
    /// </summary>
    public int NowLv { get { return nowLv; } set { nowLv = value; } }

    /// <summary>
    /// 現獲得経験値
    /// </summary>
    public int NowExp { get { return nowExp; } set { nowExp = value; } }

    /// <summary>
    /// 次レベルまでの必要経験値
    /// </summary>
    public int NextLvExp { get { return nextLvExp; } set { nextLvExp = value; } }

    /// <summary>
    /// 操作キャラのタイプ
    /// </summary>
    public Player_Type PlayerType { get { return playerType; } }

    /// <summary>
    /// ボスエリア侵入フラグ
    /// </summary>
    public bool IsBossArea { get { return isBossArea; } }
    #endregion

    #region レリック外部参照用プロパティ
    /// <summary>
    /// 与ダメ回復率
    /// </summary>
    public float DmgHealRate { get { return regainCodeRate; } }

    /// <summary>
    /// 回復肉所持数
    /// </summary>
    public int DigitalMeatCnt { get { return digitalMeatCnt; } }

    /// <summary>
    /// 状態異常ダメージ倍率
    /// </summary>
    public float DebuffDmgRate { get { return identificationAIRate; } }

    /// <summary>
    /// 防御貫通率
    /// </summary>
    public float PierceRate { get { return danborDollRate; } }
    #endregion

    #region レイヤー・位置関連
    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected int enemyLayer = 6;

    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected LayerMask m_WhatIsGround;// どのレイヤーを地面と認識させるか

    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected LayerMask ziplineLayer;  // レイヤー

    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected Transform m_GroundCheck;	// プレイヤーが接地しているかどうかを確認する用

    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected Transform m_WallCheck;   // プレイヤーが壁に触れているかどうかを確認する用

    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected Transform attackCheck;   // 攻撃時の当たり判定

    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected Transform playerPos;		// プレイヤー位置情報

    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected CapsuleCollider2D playerCollider;

    protected float ladderPosX = 0; // 梯子のX座標
    #endregion

    #region プレイヤー情報取得変数
    protected Rigidbody2D m_Rigidbody2D;
    protected Vector3 velocity = Vector3.zero;
    protected bool m_FacingRight = true;  // プレイヤーの向きの判定フラグ（trueで右向き）
    protected bool m_FallFlag = false;
    protected float limitFallSpeed = 25f; // 落下速度の制限
    protected PlayerBase m_Player;
    #endregion

    #region エフェクト・UI
    [Foldout("エフェクト・UI")]
    [SerializeField] protected ParticleSystem particleJumpUp;

    [Foldout("エフェクト・UI")]
    [SerializeField] protected ParticleSystem particleJumpDown;

    [Foldout("エフェクト・UI")]
    [SerializeField] protected GameObject ziplineSpark;

    [Foldout("エフェクト・UI")]
    [SerializeField] protected GameObject interactObj;

    [Foldout("エフェクト・UI")]
    [SerializeField] protected Sprite[] interactSprits;         // [0] Pad [1] Key

    [Foldout("エフェクト・UI")]
    [SerializeField] protected ParticleSystem groundSmoke;
    #endregion

    #region カメラ
    protected GameObject cam;
    #endregion

    #region 動作フラグ関連
    protected bool canMove = true;      // プレイヤーの動作制御フラグ
    protected bool canBlink = true;     // ダッシュ制御フラグ
    protected bool canAttack = true;    // 攻撃可能フラグ
    protected bool m_Grounded;          // プレイヤーの接地フラグ
    protected bool m_IsWall = false;    // プレイヤーの前に壁があるか
    protected bool m_IsZipline = false; // 動作フラグ
    protected bool m_IsScaffold = false;// 足場動作フラグ
    protected bool isJump = false;      // ジャンプ入力フラグ
    protected bool isBlink = false;     // ダッシュ入力フラグ
    protected bool isBlinking = false;        // プレイヤーがダッシュ中かどうか
    protected bool isWallSliding = false;     // If player is sliding in a wall
    protected bool isWallJump = false;        // 壁ジャンプ中かどうか
    protected bool isAbnormalMove = false;    // 状態異常フラグ
    protected bool oldWallSlidding = false;   // If player is sliding in a wall in the previous frame
    protected float prevVelocityX = 0f;
    protected bool canCheck = false;          // For check if player is wallsliding
    protected float horizontalMove = 0f;      // 速度用変数
    protected float verticalMove = 0f;
    protected float jumpWallStartX = 0;
    protected float jumpWallDistX = 0;        // プレイヤーと壁の距離
    protected bool limitVelOnWallJump = false;// 低fpsで壁のジャンプ距離を制限する
    protected bool isSkill = false;   // スキル使用中フラグ
    protected bool canSkill = true;   // スキル使用可能フラグ
    protected bool isRegene = true;
    protected bool isBossArea = false;  
    protected bool isDead = false;
    #endregion

    #region 動作フラグ関連外部参照用
    public bool IsDead { get { return isDead; } private set { isDead = value; } }
    #endregion

    #region プレイヤーに関する定数
    protected const float REGENE_TIME = 1.5f;           // 自動回復間隔
    protected const float REGENE_STOP_TIME = 3.0f;      // 自動回復停止時間
    protected const float REGENE_MAGNIFICATION = 0.05f; // 自動回復倍率
    protected const float HEAL_GENERATE_TIME = 18f;     // 回復肉生成間隔
    protected const float MEATHEAL_RATE = 0.03f;        // 回復肉回復量

    protected const float GROUNDED_RADIUS = .2f;// 接地確認用の円の半径
    protected const float ATTACK_RADIUS = 1.2f; // 攻撃判定の円の半径

    protected const float KNOCKBACK_DIR = 40f;  // ノックバック
    protected const float KB_SMALL = 5f;        // ノックバック力（小）
    protected const float KB_MEDIUM = 10f;      // ノックバック力（中）
    protected const float KB_BIG = 20f;         // ノックバック力（大）

    protected const float STUN_TIME = 0.2f;        // スタン時間
    protected const float INVINCIBLE_TIME = 0.5f;  // 無敵時間

    protected const float SMOKE_SCALE = 0.22f; // 土煙のスケール

    protected const float STICK_DEAD_ZONE = 0.3f; // スティックのデッドゾーン
    #endregion

    [SerializeField] AudioClip useZipline_SE;   // ジップライン使用音
    [SerializeField] AudioClip usingZipline_SE; // ジップライン使用中音

    AudioSource audioSource;

    //--------------------
    // メソッド

    #region プレイヤー共通処理
    //---------------------------------------------------
    // プレイヤーに共通する関数をここに記載する

    /// <summary>
    /// Update前処理
    /// </summary>

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        // 各種値の取得
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_Player = GetComponent<PlayerBase>();
        gravity = m_Rigidbody2D.gravityScale;
        animator = GetComponent<Animator>();
        cam = Camera.main.gameObject;
        startHp = maxHp;
        audioSource = GetComponent<AudioSource>();

        // カメラのターゲットを自身に設定
        if(RoomModel.Instance == null)
        {
            //cam.GetComponent<CameraFollow>().Target = gameObject.transform;
        }
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    virtual protected void Update()
    {
        regeneTimer += Time.deltaTime;
        healGenerateTimer += Time.deltaTime;

        // 毎秒最大HPの1% を基礎値とし、1秒毎に基礎値分回復する
        if (regeneTimer >= REGENE_TIME)
        {
            if (HP < MaxHP)
            {
                if(isRegene)
                    HP += (int)(MaxHP * maxHealRate);

                if (HP >= MaxHP)
                {
                    HP = MaxHP;
                }

                UIManager.Instance.UpdatePlayerStatus();
            }

            regeneTimer = 0;
        }

        // 回復オブジェを一定間隔でhealMeatCnt分生成
        if (healGenerateTimer >= HEAL_GENERATE_TIME)
        {
            GenerateHealObject();
            healGenerateTimer = 0f;
        }

        // キャラの移動
        if (!canMove) return;

        horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;
        verticalMove = Input.GetAxisRaw("Vertical") * moveSpeed;

        // 走っている時に土煙を起こす
        if (animator.GetInteger("animation_id") == (int)ANIM_ID.Run)
            groundSmoke.Play();
        else
            groundSmoke.Stop();

            Ladder();

        if(m_IsZipline)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetAxisRaw("Horizontal") >= STICK_DEAD_ZONE)
            {
                animator.SetInteger("animation_id", (int)ANIM_ID.Fall);
                m_IsZipline = false;
                ziplineSpark.SetActive(false);
                m_Rigidbody2D.AddForce(new Vector2(-m_ZipJumpForceX,m_ZipJumpForceY));

                audioSource.Stop();     // ジップライン使用音停止
                audioSource.PlayOneShot(useZipline_SE);
            }
            else if(Input.GetKeyDown(KeyCode.D) || Input.GetAxisRaw("Horizontal") <= -STICK_DEAD_ZONE)
            {
                animator.SetInteger("animation_id", (int)ANIM_ID.Fall);
                m_IsZipline = false;
                ziplineSpark.SetActive(false);
                m_Rigidbody2D.AddForce(new Vector2(m_ZipJumpForceX, m_ZipJumpForceY));

                audioSource.Stop();     // ジップライン使用音停止
                audioSource.PlayOneShot(useZipline_SE);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump"))
            {   // ジャンプ押下時
                if (animator.GetInteger("animation_id") != (int)ANIM_ID.Blink)
                    isJump = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("Blink"))
            {   // ブリンク押下時
                    isBlink = true;
            }

            if (m_IsScaffold && Input.GetKeyDown(KeyCode.S) || m_IsScaffold && Input.GetAxisRaw("Vertical") <= -STICK_DEAD_ZONE)
            {
                gameObject.layer = 21;
                StartCoroutine(ScaffoldDown());
            }
        }
    }

    /// <summary>
    /// 定期更新処理
    /// </summary>
    virtual protected void FixedUpdate()
    {
        //---------------------------------
        // 地面判定

        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        if (isAbnormalMove)
        {
            animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
            m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
            return;
        }

        // グラウンドチェックが地面として指定されたものに当たった場合、プレーヤーを接地扱いにする
        // これはレイヤーを使って行うこともできますが、Sample Assetsはプロジェクトの設定を上書きしません。
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, GROUNDED_RADIUS, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            // Object→プレイヤーオブジェクトのこと？
            if (colliders[i].gameObject != gameObject)
                m_Grounded = true;

            isWallJump = false;

            if (m_Grounded && m_FallFlag)
            {   // 高所から着地時にスタン
                m_FallFlag = false;
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                //StartCoroutine(Stun(1f)); // スタン処理
            }

            if (!wasGrounded && canAttack)
            {   // 前フレームで地面に触れていない時
                animator.SetInteger("animation_id", (int)ANIM_ID.Idle);

                if (!m_IsWall && !isBlinking)
                    particleJumpDown.Play();
                canDoubleJump = true;
                if (m_Rigidbody2D.linearVelocity.y < 0f)
                    limitVelOnWallJump = false;
            }
        }

        // 接地時にIDLE or RUNモーションに戻るように設定
        if (m_Grounded && animator.GetInteger("animation_id") == (int)ANIM_ID.Idle && Mathf.Abs(horizontalMove) >= 0.1f
            || m_Grounded && animator.GetInteger("animation_id") == (int)ANIM_ID.Fall && Mathf.Abs(horizontalMove) >= 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Run);

        if (m_Grounded && animator.GetInteger("animation_id") == (int)ANIM_ID.Run && Mathf.Abs(horizontalMove) < 0.1f
            || m_Grounded && animator.GetInteger("animation_id") == (int)ANIM_ID.Fall && Mathf.Abs(horizontalMove) < 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Idle);

        //---------------------------------
        // 壁判定

        m_IsWall = false;

        if (!m_Grounded)
        {   // 空中に居るとき

            if (animator.GetInteger("animation_id") == (int)ANIM_ID.Idle || animator.GetInteger("animation_id") == (int)ANIM_ID.Run)
                animator.SetInteger("animation_id", (int)ANIM_ID.Fall);

            Collider2D[] collidersWall = Physics2D.OverlapCircleAll(m_WallCheck.position, GROUNDED_RADIUS, m_WhatIsGround);
            for (int i = 0; i < collidersWall.Length; i++)
            {
                if (collidersWall[i].gameObject != null)
                {
                    isBlinking = false;
                    if (gameObject.layer != 21) m_IsWall = true;
                }
            }
            prevVelocityX = m_Rigidbody2D.linearVelocity.x;
        }

        // 距離制限
        if (limitVelOnWallJump)
        {
            if (m_Rigidbody2D.linearVelocity.y < -0.5f)
                limitVelOnWallJump = false;
            jumpWallDistX = (jumpWallStartX - transform.position.x) * transform.localScale.x;
            if (jumpWallDistX < -0.5f && jumpWallDistX > -1f)
            {
                canMove = true;
            }
            else if (jumpWallDistX < -1f && jumpWallDistX >= -2f)
            {
                canMove = true;
                m_Rigidbody2D.linearVelocity = new Vector2(10f * transform.localScale.x, m_Rigidbody2D.linearVelocity.y);
            }
            else if (jumpWallDistX < -2f)
            {
                limitVelOnWallJump = false;
                m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
            }
            else if (jumpWallDistX > 0)
            {
                limitVelOnWallJump = false;
                m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
            }
        }

        //------------------------------
        // 梯子処理

        if (Ladder())
        {
            if (Input.GetKey(KeyCode.W) && canBlink && canSkill && canAttack || Input.GetAxisRaw("Vertical") >= STICK_DEAD_ZONE && canBlink && canSkill && canAttack)
            {
                if (!m_IsZipline)
                {
                    audioSource.PlayOneShot(useZipline_SE);
                    audioSource.PlayOneShot(usingZipline_SE);

                    ziplineSpark.SetActive(true);
                    animator.SetInteger("animation_id", (int)ANIM_ID.Zipline);
                }

                m_IsZipline = true;
            }
        }

        if (m_IsZipline)
        {
            m_Rigidbody2D.position = new Vector2(ladderPosX, m_Rigidbody2D.position.y);
            m_Rigidbody2D.linearVelocity = new Vector2(0, zipSpeed);
            m_Rigidbody2D.gravityScale = 0f;
        }
        else if(canMove)
        {
            m_Rigidbody2D.gravityScale = gravity;
        }

        Move(horizontalMove * Time.fixedDeltaTime, isJump, isBlink);
        isJump = false;
        isBlink = false;
    }

    /// <summary>
    /// レリックの最大値の変更＆それに応じた現在値の変更
    /// </summary>
    /// <param name="changeData">強化後のステータス</param>
    public void ChangeRelicStatusData(PlayerRelicStatusData relicStatus)
    {
        giveDebuffRates = relicStatus.GiveDebuffRates;
        regainCodeRate = relicStatus.RegainCodeRate;
        scatterBugCnt = relicStatus.ScatterBugCnt;
        holographicArmorRate = relicStatus.HolographicArmorRate;
        mouseRate = relicStatus.MouseRate;
        digitalMeatCnt = relicStatus.DigitalMeatCnt;
        firewallRate = relicStatus.FirewallRate;
        lifeScavengerRate = relicStatus.LifeScavengerRate;
        rugrouterRate = relicStatus.RugrouterRate;
        buckupHDMICnt = relicStatus.BuckupHDMICnt;
        identificationAIRate = relicStatus.IdentificationAIRate;
        danborDollRate = relicStatus.DanborDollRate;
        chargedCoreCnt = relicStatus.ChargedCoreCnt;
        illegalScriptRate = relicStatus.IllegalScriptRate;
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    /// <param name="move">移動量</param>
    /// <param name="jump">ジャンプ入力</param>
    /// <param name="blink">ダッシュ入力</param>
    virtual protected void Move(float move, bool jump, bool blink)
    {
        if (m_IsZipline) return;

        if (canMove)
        {
            //--------------------
            // 移動 & ダッシュ

            // ダッシュ入力 & ダッシュ可能 & 壁に触れてない
            if (blink && canBlink && !isWallSliding)
            {
                StartCoroutine(BlinkCooldown()); // ブリンククールダウン
            }
            
            if (isWallJump)
            {   // 壁ジャンプ中
                m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * 12, 12);
            }
            // 接地しているか空中制御ONの時
            else if (m_Grounded || m_AirControl)
            {
                // 落下速度制限処理
                if (m_Rigidbody2D.linearVelocity.y < -limitFallSpeed)
                {
                    m_FallFlag = true;
                    m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, -limitFallSpeed);
                }

                // キャラの目標移動速度を決定
                Vector3 targetVelocity = new Vector2();
                if (!canAttack)
                {
                    targetVelocity = new Vector2(move * 2f, m_Rigidbody2D.linearVelocity.y);
                }
                else
                {
                    targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.linearVelocity.y);
                }

                // SmoothDampにより、滑らかな移動を実現
                m_Rigidbody2D.linearVelocity = Vector3.SmoothDamp(m_Rigidbody2D.linearVelocity, targetVelocity, ref velocity, m_MovementSmoothing);

                // 攻撃時は反転しないように
                if (canAttack && animator.GetInteger("animation_id") != (int)ANIM_ID.Blink && !isSkill)
                {
                    // キャラが入力と反対方向を向いていた際に反転させる
                    if (move > 0 && !m_FacingRight && !isWallSliding)
                    {   // 右入力
                        Flip();
                    }
                    else if (move < 0 && m_FacingRight && !isWallSliding)
                    {   // 左入力
                        Flip();
                    }
                }
            }

            //--------------------
            // ジャンプ

            if (m_Grounded && jump && canAttack)
            {   // 接地状態 & ジャンプ入力
                    animator.SetInteger("animation_id", (int)ANIM_ID.Fall);
                    m_Grounded = false;
                    m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                    canDoubleJump = true;
                    particleJumpDown.Play();
                    particleJumpUp.Play();
            }
            else if (!m_Grounded && jump && canDoubleJump && !isWallSliding && canAttack)
            {   // ジャンプ中にジャンプ入力（ダブルジャンプ）
                canDoubleJump = false;
                m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, 0);
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce / 1.2f));

                animator.SetInteger("animation_id", (int)ANIM_ID.DBJump);
            }
            else if (m_IsWall && !m_Grounded && canAttack && !isSkill)
            {   // 壁に触れた && 空中
                if (!oldWallSlidding && m_Rigidbody2D.linearVelocity.y < 0 || isBlinking)
                {   // 前フレームで壁に触れていない && 下に落ちてる or ダッシュ可能
                    isWallSliding = true;
                    m_WallCheck.localPosition = new Vector3(-m_WallCheck.localPosition.x, m_WallCheck.localPosition.y, 0);
                    Flip();
                    StartCoroutine(WaitToCheck(0.1f));
                    canDoubleJump = true;

                    isWallJump = false;

                    if(canAttack) animator.SetInteger("animation_id", (int)ANIM_ID.WallSlide);
                }
                isBlinking = false;

                if (isWallSliding && canAttack)
                {   // 壁スライド中
                    if (move * transform.localScale.x > 0.1f)
                    {   // 壁の反対方向に入力された時
                        StartCoroutine(WaitToEndSliding());
                    }
                    else
                    {   // スライド処理
                        oldWallSlidding = true;

                        animator.SetInteger("animation_id", (int)ANIM_ID.WallSlide);

                        m_Rigidbody2D.linearVelocity = new Vector2(-transform.localScale.x * 2, -5);
                    }
                }

                if (jump && isWallSliding && canAttack)
                {   // スライディング中にジャンプ
                    animator.SetInteger("animation_id", (int)ANIM_ID.Fall);

                    m_Rigidbody2D.linearVelocity = new Vector2(0f, 0f);
                    m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_JumpForce * wallJumpPower, m_JumpForce));
                    jumpWallStartX = transform.position.x;
                    limitVelOnWallJump = true;
                    canDoubleJump = true;
                    isWallSliding = false;
                    oldWallSlidding = false;

                    // 壁ジャンコルーチン
                    StartCoroutine(WallJump());

                    m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                }
                else if (blink && canBlink)
                {   // ダッシュ押下時
                    isWallSliding = false;
                    oldWallSlidding = false;
                    m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                    canDoubleJump = true;
                    StartCoroutine(BlinkCooldown());
                }
            }
            else if (isWallSliding && !m_IsWall && canCheck && canAttack)
            {   // 壁スライドAnim再生中 && 前に壁が無い
                isWallSliding = false;
                oldWallSlidding = false;
                isWallJump = false;

                if (m_Grounded)
                {
                    animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
                }
                else
                {
                    animator.SetInteger("animation_id", (int)ANIM_ID.Fall);
                }

                m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                canDoubleJump = true;
            }
        }
    }

    /// <summary>
    /// 動作に関わるフラグをリセット
    /// </summary>
    protected void ResetMoveFlag()
    {
        canMove = true;
        canAttack = true;
    }

    /// <summary>
    /// ジップライン判定処理
    /// </summary>
    /// <returns></returns>
    protected bool Ladder()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, ziplineLayer);
        if (hit.collider != null)
        {
            ladderPosX = hit.collider.transform.position.x;
            return true;
        }
        else
        {
            ladderPosX = 0;
            m_IsZipline = false;
            return false;
        }
    }

    /// <summary>
    /// キャラ反転
    /// </summary>
    protected void Flip()
    {
        m_FacingRight = !m_FacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// レベルアップ処理
    /// </summary>
    protected void LevelUp()
    {
        // レベルアップ処理
        while (nowExp >= nextLvExp)
        {
            nowLv++;                        // 現在のレベルを上げる
            nowExp = nowExp - nextLvExp;    // 超過した分の経験値を現在の経験値量として保管

            // 次のレベルまで必要な経験値量を計算 （必要な経験値量 = 次のレベルの3乗 - 今のレベルの3乗）
            nextLvExp = (int)Math.Pow(nowLv + 1, 3) - (int)Math.Pow(nowLv, 3);

            // レベルアップによるステータス変化
            LevelUpStatusChange();
        }
    }

    /// <summary>
    /// コライダー接触判定
    /// </summary>
    /// <param name="collision"></param>
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Gimmick/Scaffold") m_IsScaffold = true;

        if (collision.gameObject.tag == "Heal")
        {
            int healVol = (int)((float)MaxHP * MEATHEAL_RATE);

            if(HP < MaxHP)
            {
                HP += healVol;

                if (HP > MaxHP)
                    HP = MaxHP;

                UIManager.Instance.UpdatePlayerStatus();

                UIManager.Instance.PopHealUI(healVol, transform.position);
                Destroy(collision.gameObject);
            }
            else
            {
                UIManager.Instance.PopHealUI(0, transform.position);
                Destroy(collision.gameObject);
            }
        }
    }

    /// <summary>
    /// トリガー接触判定
    /// </summary>
    /// <param name="collision"></param>
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        // 落下処理
        if (collision.gameObject.tag == "Gimmick/Abyss")
        {   // 最も近い復帰点に移動
            MoveCheckPoint();
        }

        if (collision.gameObject.tag == "BossArea") isBossArea = true;

        // インタラクトオブジェ接触判定
        if (collision.gameObject.tag == "Interact")
        {   // インタラクトUI表示
            SpriteRenderer spriteRenderer = interactObj.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = (UIManager.Instance.IsInputGamePad) ? interactSprits[0] : interactSprits[1];

            // UIアニメーション
            spriteRenderer.DOFade(1f, 0.4f);
            interactObj.GetComponent<Transform>().DOLocalMoveY(0.8f, 0.4f);
        }
    }

    /// <summary>
    /// トリガー接触判定
    /// </summary>
    /// <param name="collision"></param>
    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BossArea") isBossArea = false;

        // インタラクトオブジェ接触判定
        if (collision.gameObject.tag == "Interact")
        {   // インタラクトUI非表示
            SpriteRenderer spriteRenderer = interactObj.gameObject.GetComponent<SpriteRenderer>();

            // UIアニメーション
            spriteRenderer.DOFade(0f, 0.4f);
            interactObj.GetComponent<Transform>().DOLocalMoveY(0.5f, 0.4f);
        }
    }

    /// <summary>
    /// １番近いオブジェクトを取得する
    /// </summary>
    /// <param name="tagName">取得したいtagName</param>
    /// <returns>最小距離の指定Obj</returns>
    public Transform FetchNearObjectWithTag(string tagName)
    {
        // 該当タグが1つしか無い場合はそれを返す
        var targets = GameObject.FindGameObjectsWithTag(tagName);
        if (targets.Length == 1) return targets[0].transform;

        GameObject result = null;               // 返り値
        var minTargetDistance = float.MaxValue; // 最小距離
        foreach (var target in targets)
        {
            // 前回計測したオブジェクトよりも近くにあれば記録
            var targetDistance = Vector3.Distance(transform.position, target.transform.position);
            if (!(targetDistance < minTargetDistance)) continue;
            minTargetDistance = targetDistance;
            result = target.transform.gameObject;
        }

        // 最後に記録されたオブジェクトを返す
        return result?.transform;
    }

    /// <summary>
    /// 回復オブジェクト生成処理
    /// </summary>
    private void GenerateHealObject()
    {
        if (healObjectPrefab == null || digitalMeatCnt <= 0) return;

        float radius = 1.0f; // プレイヤーからの初期距離
        float minAngle = -90f;
        float maxAngle = 90f;
        float minForce = 150f; // AddForceの最小値
        float maxForce = 250f; // AddForceの最大値

        for (int i = 0; i < digitalMeatCnt; i++)
        {
            // -90°～90°の範囲でランダムな角度を決定
            float angle = UnityEngine.Random.Range(minAngle, maxAngle);
            float rad = angle * Mathf.Deg2Rad;

            // プレイヤー中心からradius分だけオフセット
            Vector3 offset = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0) * radius;
            Vector3 spawnPos = transform.position + offset;

            // オブジェクト生成
            GameObject obj = Instantiate(healObjectPrefab, spawnPos, Quaternion.identity);

            // ランダムな力を加える
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float force = UnityEngine.Random.Range(minForce, maxForce);
                Vector2 forceDir = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)).normalized;
                rb.AddForce(forceDir * force);
            }
        }
    }

    #endregion

    #region プレイヤー共通非同期処理
    /// <summary>
    /// 壁ジャンプ制限処理
    /// </summary>
    /// <returns></returns>
    protected IEnumerator WallJump()
    {
        isWallJump = true;
        yield return new WaitForSeconds(0.2f);  // ジャンプ時間
        isWallJump = false;
    }
    /// <summary>
    /// ダメージ後硬直処理
    /// </summary>
    public IEnumerator Stun(float time)
    {
        Debug.Log("スタン：" + time);
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
    /// <summary>
    /// 状態異常時硬直処理
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public IEnumerator AbnormalityStun(float time)
    {
        isAbnormalMove = true;
        yield return new WaitForSeconds(time);
        isAbnormalMove = false;
    }
    /// <summary>
    /// 無敵時間設定処理
    /// </summary>
    protected IEnumerator MakeInvincible(float time)
    {
        invincible = true;
        yield return new WaitForSeconds(time);
        invincible = false;
        ResetMoveFlag();
    }
    /// <summary>
    /// 動作不能処理
    /// </summary>
    protected IEnumerator WaitToMove(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
    /// <summary>
    /// 壁スライド中か確認する処理
    /// </summary>
    protected IEnumerator WaitToCheck(float time)
    {
        canCheck = false;
        yield return new WaitForSeconds(time);
        canCheck = true;
    }
    /// <summary>
    /// 壁スライド終了処理
    /// </summary>
    protected IEnumerator WaitToEndSliding()
    {
        yield return new WaitForSeconds(0.05f);
        canDoubleJump = true;
        isWallSliding = false;
        animator.SetInteger("animation_id", (int)ANIM_ID.Fall);
        oldWallSlidding = false;
        m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
    }
    /// <summary>
    /// 死亡処理
    /// </summary>
    protected IEnumerator WaitToDead()
    {
        if (isDead) yield break ;
        isDead = true;
        if (CharacterManager.Instance.PlayerObjSelf == gameObject)
        {
            // プレイヤー死亡したことを同期
            if (RoomModel.Instance)
            {
                PlayerDeathResult result = new PlayerDeathResult();
                yield return RoomModel.Instance.PlayerDeadAsync().ToCoroutine(r => result = r);
                buckupHDMICnt = result.BuckupHDMICnt;
                if (!result.IsDead)
                {
                    hp = maxHp;
                    UIManager.Instance.UpdatePlayerStatus();
                    StartCoroutine(MakeInvincible(1.5f)); // 無敵時間
                    yield break;
                }
            }
            // オフライン時の処理
            if (!RoomModel.Instance && buckupHDMICnt > 0)
            {   // 体力回復 & 残機減少
                hp = maxHp;
                UIManager.Instance.UpdatePlayerStatus();
                buckupHDMICnt--;
                StartCoroutine(MakeInvincible(1.5f)); // 無敵時間
                yield break;
            }
        }
        if (!RoomModel.Instance) UIManager.Instance.OnDeadPlayer();
        OnDead();
        yield return new WaitForSeconds(0.4f);
        m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
        yield return new WaitForSeconds(1.1f);

        if (CharacterManager.Instance.PlayerObjSelf) Camera.main.gameObject.GetComponent<SpectatorModeManager>().FocusCameraOnAlivePlayer(); 
        
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// 死亡時に呼び出し
    /// </summary>
    public void OnDead()
    {
        animator.SetInteger("animation_id", (int)ANIM_ID.Dead);
        canMove = false;
        invincible = true;
    }

    /// <summary>
    /// ダッシュ(ブリンク)制限処理
    /// </summary>
    /// <returns></returns>
    protected IEnumerator BlinkCooldown()
    {
        // ブリンク開始
        animator.SetInteger("animation_id", (int)ANIM_ID.Blink);
        isBlinking = true;
        canBlink = false;
        invincible = true;
        yield return new WaitForSeconds(blinkTime);  // ブリンク時間

        // ブリンク終了
        canAttack = true;
        isBlinking = false;
        invincible = false;

        // クールダウン時間
        UIManager.Instance.DisplayCoolDown(false, blinkCoolDown);
        yield return new WaitForSeconds(blinkCoolDown);
        canBlink = true;
    }

    /// <summary>
    /// 足場すり抜け処理
    /// </summary>
    /// <returns></returns>
    protected IEnumerator ScaffoldDown()
    {
        yield return new WaitForSeconds(0.3f);
        m_IsScaffold = false;
        gameObject.layer = 20;
    }
    #endregion

    #region 外部呼び出し関数

    /// <summary>
    /// ダメージを与える処理
    /// </summary>
    abstract public void DoDashDamage();

    /// <summary>
    /// 動作フラグをリセット
    /// </summary>
    abstract public void ResetFlag();

    /// <summary>
    /// 被ダメ処理
    /// (ノックバックはposに応じて有無が変わる)
    /// </summary>
    abstract public void ApplyDamage(int power, Vector3? position = null, KB_POW? kbPow = null, DEBUFF_TYPE? type = null);

    /// <summary>
    /// ブリンク終了処理
    /// </summary>
    public void BlinkEnd()
    {
        animator.SetInteger("animation_id", (int)ANIM_ID.Run);
    }

    /// <summary>
    /// 近くのチェックポイントに移動
    /// </summary>
    public void MoveCheckPoint()
    {
        // HPの5%ダメージ
        var damage = (int)((float)hp * 0.05);

        if (hp - damage <= 0) 
            hp = 1;
        else
            hp -= damage;

        UIManager.Instance.UpdatePlayerStatus();

        // 移動
        playerPos.position = FetchNearObjectWithTag("Gimmick/ChecKPoint").position;
    }

    /// <summary>
    /// 経験値獲得
    /// </summary>
    /// <param name="exp">経験値量</param>
    public void GetExp(int exp)
    {
        nowExp += exp;

        if (nextLvExp <= nowExp)
        {   // レベルアップ処理
            LevelUp();
        }

        // 経験値UIの更新
        UIManager.Instance.UpdateExperienceAndLevel();
    }

    /// <summary>
    /// 接地判定取得
    /// </summary>
    /// <returns></returns>
    public bool GetGrounded() { return m_Grounded; }

    /// <summary>
    /// 自動回復一定停止処理
    /// </summary>
    /// <returns></returns>
    protected IEnumerator RegeneStop()
    {
        isRegene = false;

        yield return new WaitForSeconds(REGENE_STOP_TIME);

        isRegene = true;
    }

    /// <summary>
    /// 状態異常付与抽選処理
    /// </summary>
    /// <returns></returns>
    public DEBUFF_TYPE[] LotteryDebuff()
    {
        List<DEBUFF_TYPE> debuffList = new List<DEBUFF_TYPE>();
        // 各状態異常の抽選
        float burnRand = UnityEngine.Random.Range(0f, 100f);
        if (burnRand <= giveDebuffRates[DEBUFF_TYPE.Burn]) debuffList.Add(DEBUFF_TYPE.Burn);

        float freezeRand = UnityEngine.Random.Range(0f, 100f);
        if (freezeRand <= giveDebuffRates[DEBUFF_TYPE.Freeze]) debuffList.Add(DEBUFF_TYPE.Freeze);

        float shockRand = UnityEngine.Random.Range(0f, 100f);
        if (shockRand <= giveDebuffRates[DEBUFF_TYPE.Shock]) debuffList.Add(DEBUFF_TYPE.Shock);

        return debuffList.ToArray();
    }

    /// <summary>
    /// レリック効果抽選処理
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool LotteryRelic(RELIC_TYPE type)
    {
        float rand = UnityEngine.Random.Range(0f, 100f);
        switch(type)
        {
            case RELIC_TYPE.HolographicArmor:
                if (rand <= holographicArmorRate) return true;
                break;
            case RELIC_TYPE.Mouse:
                if (rand <= mouseRate) return true;
                break;
            case RELIC_TYPE.Rugrouter:
                if (rand <= rugrouterRate) return true;
                break;
            case RELIC_TYPE.IllegalScript:
                if (rand <= illegalScriptRate) return true;
                break;
        }
        return false;
    }

    public bool LotterySkillCoolDown()
    {
        float rand = UnityEngine.Random.Range(0f, 100f);
        if (rand <= mouseRate)
        {
            // スキルクールダウン短縮成功
            Debug.Log("スキルクールダウン短縮成功");
            return true;
        }
        else
        {
            // スキルクールダウン短縮失敗
            return false;
        }
    }

    /// <summary>
    /// 敵撃破時HP回復処理
    /// </summary>
    public void KilledHPRegain()
    {
        HP += (int)(MaxHP * lifeScavengerRate);

        if (HP >= MaxHP)
            HP = MaxHP;
        UIManager.Instance.UpdatePlayerStatus();
    }

    /// <summary>
    /// 現在のレリックステータスデータを取得する
    /// </summary>
    /// <returns></returns>
    public PlayerRelicStatusData GetPlayerRelicStatusData()
    {
        // AddExpRateなし
        //+++++++++++++++++++++++++++++
        return new PlayerRelicStatusData()
        {
            GiveDebuffRates = giveDebuffRates,
            RegainCodeRate = regainCodeRate,
            ScatterBugCnt = scatterBugCnt,
            HolographicArmorRate = holographicArmorRate,
            MouseRate = mouseRate,
            DigitalMeatCnt = DigitalMeatCnt,
            FirewallRate = firewallRate,
            LifeScavengerRate = lifeScavengerRate,
            RugrouterRate = rugrouterRate,
            BuckupHDMICnt = buckupHDMICnt,
            IdentificationAIRate = identificationAIRate,
            DanborDollRate = danborDollRate,
            ChargedCoreCnt = chargedCoreCnt,
            IllegalScriptRate = illegalScriptRate,
        };
    }

    #endregion
}
