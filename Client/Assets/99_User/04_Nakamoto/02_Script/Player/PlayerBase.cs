//--------------------------------------------------------------
// プレイヤー抽象親クラス [ PlayerBase.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using Pixeye.Unity;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

abstract public class PlayerBase : MonoBehaviour
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
        Attack,
        Run,
        Hit,
        Fall,
        Dead,
        Blink,
        DBJump,
        WallSlide,
    }
    #endregion

    #region 共通ステータス
    [Foldout("共通ステータス")]
    protected int nowLv = 1;          // 現在レベル
    [Foldout("共通ステータス")]
    protected int nowExp = 0;         // 現在の獲得経験値
    [Foldout("共通ステータス")]
    protected int nextLvExp = 0;      // 次のレベルまでに必要な経験値

    [Foldout("共通ステータス")]
    [SerializeField] protected int maxHp = 200;   // 最大体力
    [Foldout("共通ステータス")]
    [SerializeField] protected int hp = 200;      // 現体力
    [Foldout("共通ステータス")]
    protected int startHp = 0;                    // 初期体力

    [Foldout("共通ステータス")]
    [SerializeField] protected int power = 20;        // 攻撃力

    [Foldout("共通ステータス")]
    public float moveSpeed = 40f;   // 走る速度

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
    [SerializeField] protected float ladderSpeed = 1f;   // 梯子移動速度

    [Foldout("共通ステータス")]
    [Range(0, .3f)][SerializeField] protected float m_MovementSmoothing = .05f;

    [Foldout("共通ステータス")]
    public bool invincible = false; // プレイヤーの死亡制御フラグ

    [Foldout("共通ステータス")]
    [SerializeField] protected int testExp = 10;      // デバッグ用獲得経験値

    protected float horizontalMove = 0f;      // 速度用変数
    protected float gravity;  // 重力

    #endregion

    #region ステータス外部参照用プロパティ
    /// <summary>
    /// 最大体力
    /// </summary>
    public int MaxHP { get { return maxHp; } }

    /// <summary>
    /// 体力
    /// </summary>
    public int HP { get { return hp; } }

    /// <summary>
    /// 現レベル
    /// </summary>
    public int NowLv { get { return nowLv; } }

    /// <summary>
    /// 現獲得経験値
    /// </summary>
    public int NowExp { get { return nowExp; } }

    /// <summary>
    /// 次レベルまでの必要経験値
    /// </summary>
    public int NextLvExp { get { return nextLvExp; } }

    #endregion

    #region レイヤー・位置関連
    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected LayerMask m_WhatIsGround;// どのレイヤーを地面と認識させるか

    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected LayerMask ladderLayer;   // 梯子レイヤー

    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected Transform m_GroundCheck;	// プレイヤーが接地しているかどうかを確認する用

    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected Transform m_WallCheck;   // プレイヤーが壁に触れているかどうかを確認する用

    [Foldout("レイヤー・位置関連")]
    [SerializeField] private Transform attackCheck;     // 攻撃時の当たり判定

    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected Transform playerPos;		// プレイヤー位置情報

    [Foldout("レイヤー・位置関連")]
    [SerializeField] protected CapsuleCollider2D playerCollider;
    #endregion

    #region プレイヤー情報取得変数
    protected Rigidbody2D m_Rigidbody2D;
    protected Animator animator;
    protected Vector3 velocity = Vector3.zero;
    protected bool m_FacingRight = true;  // プレイヤーの向きの判定フラグ（trueで右向き）
    protected bool m_FallFlag = false;
    protected float limitFallSpeed = 25f; // 落下速度の制限
    #endregion

    #region パーティクル
    [Foldout("動作フラグ関連")]
    protected ParticleSystem particleJumpUp;

    [Foldout("動作フラグ関連")]
    protected ParticleSystem particleJumpDown;
    #endregion

    #region カメラ
    protected GameObject cam;
    #endregion

    #region 動作フラグ関連
    protected bool canMove = true;      // プレイヤーの動作制御フラグ
    protected bool canBlink = true;     // ダッシュ制御フラグ
    protected bool canAttack = true;    // 攻撃制御フラグ
    protected bool m_Grounded;          // プレイヤーの接地フラグ
    protected bool m_IsWall = false;    // プレイヤーの前に壁があるか
    protected bool m_IsLadder = false;  // 梯子動作フラグ
    protected bool isJump = false;      // ジャンプ入力フラグ
    protected bool isBlink = false;     // ダッシュ入力フラグ
    protected bool isBlinking = false;        // プレイヤーがダッシュ中かどうか
    protected bool isWallSliding = false;     // If player is sliding in a wall
    protected bool isWallJump = false;        // 壁ジャンプ中かどうか
    protected bool oldWallSlidding = false;   // If player is sliding in a wall in the previous frame
    protected float prevVelocityX = 0f;
    protected bool canCheck = false;          // For check if player is wallsliding
    protected float verticalMove = 0f;
    protected float jumpWallStartX = 0;
    protected float jumpWallDistX = 0;        // プレイヤーと壁の距離
    protected bool limitVelOnWallJump = false;// 低fpsで壁のジャンプ距離を制限する
    #endregion

    #region 判定係数
    protected const float k_GroundedRadius = .2f; // 接地確認用の円の半径
    protected const float k_AttackRadius = .9f;   // 攻撃判定の円の半径
    #endregion

    //--------------------
    // メソッド

    #region プレイヤー共通処理
    //---------------------------------------------------
    // プレイヤーに共通する関数をここに記載する

    /// <summary>
    /// Update前処理
    /// </summary>

    protected void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        gravity = m_Rigidbody2D.gravityScale;
        animator = GetComponent<Animator>();
        cam = Camera.main.gameObject;
        startHp = maxHp;
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    private void Update()
    {
        // キャラの移動
        horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;
        verticalMove = Input.GetAxisRaw("Vertical") * moveSpeed;
        Ladder();

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Jump"))
        {   // ジャンプ押下時
            if (animator.GetInteger("animation_id") != (int)ANIM_ID.Blink)
                isJump = true;
        }

        if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Blink"))
        {   // ブリンク押下時
            isBlink = true;
            gameObject.layer = 21;
        }

        if (Input.GetKeyDown(KeyCode.X) && canAttack || Input.GetButtonDown("Attack1") && canAttack)
        {   // 攻撃1
            canAttack = false;
            animator.SetInteger("animation_id", (int)ANIM_ID.Attack);
            StartCoroutine(AttackCooldown());
        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
        {   // 攻撃2

        }

        //-----------------------------
        // デバッグ用

        if (Input.GetKeyDown(KeyCode.L))
        {
            GetExp(testExp);
            Debug.Log("獲得経験値：" + testExp + "現レベル：" + nowLv + " 現経験値：" + nowExp + "必要経験値" + nextLvExp);
        }
    }

    /// <summary>
    /// 定期更新処理
    /// </summary>
    private void FixedUpdate()
    {
        //---------------------------------
        // 地面判定

        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // グラウンドチェックが地面として指定されたものに当たった場合、プレーヤーを接地扱いにする
        // これはレイヤーを使って行うこともできますが、Sample Assetsはプロジェクトの設定を上書きしません。
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            // gameObject→プレイヤーオブジェクトのこと？
            if (colliders[i].gameObject != gameObject)
                m_Grounded = true;

            isWallJump = false;

            if (m_Grounded && m_FallFlag)
            {   // 高所から着地時にスタン
                m_FallFlag = false;
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                //StartCoroutine(Stun(1f)); // スタン処理
            }

            if (!wasGrounded)
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

            Collider2D[] collidersWall = Physics2D.OverlapCircleAll(m_WallCheck.position, k_GroundedRadius, m_WhatIsGround);
            for (int i = 0; i < collidersWall.Length; i++)
            {
                if (collidersWall[i].gameObject != null)
                {
                    isBlinking = false;
                    m_IsWall = true;
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
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                m_IsLadder = true;
            }
        }

        if (m_IsLadder)
        {
            m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, verticalMove * ladderSpeed);
            m_Rigidbody2D.gravityScale = 0f;
        }
        else
        {
            m_Rigidbody2D.gravityScale = gravity;
        }

        Move(horizontalMove * Time.fixedDeltaTime, isJump, isBlink);
        isJump = false;
        isBlink = false;
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    /// <param name="move">移動量</param>
    /// <param name="jump">ジャンプ入力</param>
    /// <param name="blink">ダッシュ入力</param>
    private void Move(float move, bool jump, bool blink)
    {
        if (canMove)
        {
            //--------------------
            // 移動 & ダッシュ

            // ダッシュ入力 & ダッシュ可能 & 壁に触れてない
            if (blink && canBlink && !isWallSliding)
            {
                //m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_DashForce, 0f));
                StartCoroutine(BlinkCooldown()); // ブリンククールダウン
            }
            // If crouching, check to see if the character can stand up
            // ダッシュ中の場合
            if (isBlinking)
            {   // クールダウンに入るまで加速
                m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_BlinkForce, 0);
            }
            else if (isWallJump)
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
                if (animator.GetInteger("animation_id") == (int)ANIM_ID.Attack)
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
                if (animator.GetInteger("animation_id") != (int)ANIM_ID.Attack && animator.GetInteger("animation_id") != (int)ANIM_ID.Blink)
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

            if (m_Grounded && jump)
            {   // 接地状態 & ジャンプ入力
                animator.SetInteger("animation_id", (int)ANIM_ID.Fall);

                m_Grounded = false;
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                canDoubleJump = true;
                particleJumpDown.Play();
                particleJumpUp.Play();
            }
            else if (!m_Grounded && jump && canDoubleJump && !isWallSliding)
            {   // ジャンプ中にジャンプ入力（ダブルジャンプ）
                canDoubleJump = false;
                m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, 0);
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce / 1.2f));

                animator.SetInteger("animation_id", (int)ANIM_ID.DBJump);
            }
            else if (m_IsWall && !m_Grounded)
            {   // 壁に触れた && 空中
                if (!oldWallSlidding && m_Rigidbody2D.linearVelocity.y < 0 || isBlinking)
                {   // 前フレームで壁に触れていない && 下に落ちてる or ダッシュ可能
                    isWallSliding = true;
                    m_WallCheck.localPosition = new Vector3(-m_WallCheck.localPosition.x, m_WallCheck.localPosition.y, 0);
                    Flip();
                    StartCoroutine(WaitToCheck(0.1f));
                    canDoubleJump = true;

                    isWallJump = false;

                    int id = animator.GetInteger("animation_id");

                    animator.SetInteger("animation_id", (int)ANIM_ID.WallSlide);
                }
                isBlinking = false;

                if (isWallSliding)
                {   // 壁スライド中
                    if (move * transform.localScale.x > 0.1f)
                    {   // 壁の反対方向に入力された時
                        StartCoroutine(WaitToEndSliding());
                    }
                    else
                    {   // スライド処理
                        oldWallSlidding = true;
                        int id = animator.GetInteger("animation_id");

                        animator.SetInteger("animation_id", (int)ANIM_ID.WallSlide);

                        m_Rigidbody2D.linearVelocity = new Vector2(-transform.localScale.x * 2, -5);
                    }
                }

                if (jump && isWallSliding)
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
            else if (isWallSliding && !m_IsWall && canCheck)
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
    /// 梯子判定処理
    /// </summary>
    /// <returns></returns>
    protected bool Ladder()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, ladderLayer);
        if (hit.collider != null)
            return true;
        else
            m_IsLadder = false;
        return false;
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
        nowLv++;
        nowExp = nowExp - nextLvExp;
        int nextLv = nowLv + 1;
        nextLvExp = (int)Math.Pow(nextLv, 3) - (int)Math.Pow(nowLv, 3);

        // HP反映処理
        CalcHP();
    }

    /// <summary>
    /// HP計算処理
    /// </summary> ** ローカル **
    private void CalcHP()
    {
        float hpRatio = (float)hp / (float)maxHp;
        maxHp = (int)(startHp + (int)Math.Pow(nowLv, 2));
        hp = (int)(maxHp * hpRatio);

        Debug.Log("最大体力：" + maxHp + " 現体力：" + hp);
    }
    #endregion

    #region プレイヤー共通非同期処理
    /// <summary>
    /// 攻撃制限処理
    /// </summary>
    protected IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(0.25f);
        canAttack = true;
    }
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
    protected IEnumerator Stun(float time)
    {
        Debug.Log("スタン！：" + time);
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
    /// <summary>
    /// 無敵時間設定処理
    /// </summary>
    protected IEnumerator MakeInvincible(float time)
    {
        invincible = true;
        yield return new WaitForSeconds(time);
        invincible = false;
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
        animator.SetInteger("animation_id", (int)ANIM_ID.Dead);
        canMove = false;
        invincible = true;
        canAttack = false;
        yield return new WaitForSeconds(0.4f);
        m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
        yield return new WaitForSeconds(1.1f);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
    /// <summary>
    /// ダッシュ(ブリンク)制限処理
    /// </summary>
    /// <returns></returns>
    IEnumerator BlinkCooldown()
    {
        animator.SetInteger("animation_id", (int)ANIM_ID.Blink);
        isBlinking = true;
        canBlink = false;
        yield return new WaitForSeconds(blinkTime);  // ブリンク時間
        gameObject.layer = 20;
        isBlinking = false;
        yield return new WaitForSeconds(blinkCoolDown);  // クールダウン時間
        canBlink = true;
    }
    #endregion

    #region 外部呼び出し関数

    /// <summary>
    /// ダメージを与える処理
    /// </summary>
    public void DoDashDamage()
    {
        power = Mathf.Abs(power);
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, k_AttackRadius);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Enemy")
            {
                if (collidersEnemies[i].transform.position.x - transform.position.x < 0)
                {
                    power = -power;
                }
                //++ GetComponentでEnemyスクリプトを取得し、ApplyDamageを呼び出すように変更
                //++ 破壊できるオブジェを作る際にはオブジェの共通被ダメ関数を呼ぶようにする
                collidersEnemies[i].gameObject.GetComponent<EnemyController>().ApplyDamage(power, playerPos);
                cam.GetComponent<CameraFollow>().ShakeCamera();
            }
        }
    }

    /// <summary>
    /// 被ダメ処理(ノックバック無)
    /// </summary>
    public void DealDamage(GameObject dealer, int damage)
    {
        switch (dealer.gameObject.tag)
        {
            case "Short circuit":
                hp -= damage;
                animator.SetInteger("animation_id", (int)ANIM_ID.Hit);
                if (hp <= 0)
                {   // 死亡処理
                    StartCoroutine(WaitToDead());
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 被ダメ処理(ノックバック有)
    /// </summary>
    public void ApplyDamage(int damage, Vector3 position)
    {
        if (!invincible)
        {
            animator.SetInteger("animation_id", (int)ANIM_ID.Hit);
            hp -= damage;

            // ノックバック処理
            Vector2 damageDir = Vector3.Normalize(transform.position - position) * 40f;
            m_Rigidbody2D.linearVelocity = Vector2.zero;
            m_Rigidbody2D.AddForce(damageDir * 10);

            if (hp <= 0)
            {   // 死亡処理
                StartCoroutine(WaitToDead());
            }
            else
            {   // 被ダメ硬直
                StartCoroutine(Stun(0.25f));
                StartCoroutine(MakeInvincible(1f));
            }
        }
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
    }

    /// <summary>
    /// ステータス変動処理
    /// </summary>
    /// <param name="statusID">増減させるステータスID</param>
    /// <param name="value">増減値</param>
    public void ChangeStatus(int statusID, int value)
    {   //引数はステータス全部を含んだ通信用パッケージを作って適用

    }
    #endregion
}
