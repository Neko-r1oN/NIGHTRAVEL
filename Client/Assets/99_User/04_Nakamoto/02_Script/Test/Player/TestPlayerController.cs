//--------------------------------------------------------------
// プレイヤーコントローラー [ PlayerController2D.cs ]
// Author：Kenta Nakamoto
// 引用：https://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;

public class TestPlayerController : MonoBehaviour
{
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
        Dash,
        DBJump,
        WallSlide,
    }

    [Header("ステータス")]
    [Space]
    public float life = 10f;
    public float runSpeed = 40f;    // 速度係数
    public float dmgValue = 4;		// 攻撃力
    float horizontalMove = 0f;		// 速度値
    [SerializeField] private float m_JumpForce = 400f;
    [SerializeField] private float m_DashForce = 25f;
    [SerializeField] private bool m_AirControl = false; // 空中制御フラグ
    [SerializeField] GameObject throwableObject;		// 投擲武器
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;
    public bool canDoubleJump = true;
    public bool invincible = false; // プレイヤーの死亡制御フラグ

    [Header("レイヤー関連")]
    [Space]
    [SerializeField] private LayerMask m_WhatIsGround;	// どのレイヤーを地面と認識させるか
    [SerializeField] private Transform m_GroundCheck;	// プレイヤーが接地しているかどうかを確認する用
    [SerializeField] private Transform m_WallCheck;     // プレイヤーが壁に触れているかどうかを確認する用
    [SerializeField] private Transform attackCheck;		// 攻撃時の当たり判定

    const float k_GroundedRadius = .2f; // 接地確認用の円の半径
    private bool m_Grounded;			// プレイヤーの接地フラグ
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  // プレイヤーの向きの判定フラグ（trueで右向き）
    private Vector3 velocity = Vector3.zero;
    private float limitFallSpeed = 25f; // 落下速度の制限

    private bool canMove = true;    // プレイヤーの動作制御フラグ
    private bool canDash = true;    // ダッシュ制御フラグ
    private bool canAttack = true;	// 攻撃制御フラグ
    private bool m_IsWall = false;  // プレイヤーの前に壁があるか
    private bool isJump = false;	// ジャンプ入力フラグ
    private bool isDash = false;	// ダッシュ入力フラグ
    private bool isDashing = false;	// プレイヤーがダッシュ中かどうか
    private bool isWallSliding = false;	 //If player is sliding in a wall
    private bool oldWallSlidding = false;//If player is sliding in a wall in the previous frame
    private float prevVelocityX = 0f;
    private bool canCheck = false;  //For check if player is wallsliding

    private Animator animator;

    [Header("パーティクル")]
    [Space]
    public ParticleSystem particleJumpUp;
    public ParticleSystem particleJumpDown;

    [Header("カメラ")]
    [Space]
    public GameObject cam;

    private float jumpWallStartX = 0;
    private float jumpWallDistX = 0;        // プレイヤーと壁の距離
    private bool limitVelOnWallJump = false;// 低fpsで壁のジャンプ距離を制限する

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    /// <summary>
    /// Update前処理
    /// </summary>

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    private void Update()
    {
        // キャラの移動
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Jump"))
        {   // ジャンプ押下時
            isJump = true;
        }

        if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Dash"))
        {   // ブリンク押下時
            isDash = true;
        }

        if (Input.GetKeyDown(KeyCode.X) && canAttack || Input.GetButtonDown("Attack1") && canAttack)
        {   // 攻撃1
            canAttack = false;
            animator.SetInteger("animation_id", (int)ANIM_ID.Attack);
            StartCoroutine(AttackCooldown());
        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
        {   // 攻撃2
            GameObject throwableWeapon = Instantiate(throwableObject, transform.position + new Vector3(transform.localScale.x * 0.5f, -0.2f), Quaternion.identity) as GameObject;
            Vector2 direction = new Vector2(transform.localScale.x, 0);
            throwableWeapon.GetComponent<ThrowableWeapon>().direction = direction;
            throwableWeapon.name = "ThrowableWeapon";
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
            if (!wasGrounded)
            {   // 前フレームで地面に触れていない時
                animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
                
                if (!m_IsWall && !isDashing)
                    particleJumpDown.Play();
                canDoubleJump = true;
                if (m_Rigidbody2D.linearVelocity.y < 0f)
                    limitVelOnWallJump = false;
            }
        }

        if(m_Grounded && animator.GetInteger("animation_id") == (int)ANIM_ID.Idle && Mathf.Abs(horizontalMove) >= 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Run);

        if(m_Grounded && animator.GetInteger("animation_id") == (int)ANIM_ID.Run && Mathf.Abs(horizontalMove) < 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Idle);

        //---------------------------------
        // 壁判定

        m_IsWall = false;

        if (!m_Grounded)
        {   // 空中に居るとき

            if(animator.GetInteger("animation_id") == (int)ANIM_ID.Idle)
                animator.SetInteger("animation_id", (int)ANIM_ID.Fall);
            
            Collider2D[] collidersWall = Physics2D.OverlapCircleAll(m_WallCheck.position, k_GroundedRadius, m_WhatIsGround);
            for (int i = 0; i < collidersWall.Length; i++)
            {
                if (collidersWall[i].gameObject != null)
                {
                    isDashing = false;
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

        Move(horizontalMove * Time.fixedDeltaTime, isJump, isDash);
        isJump = false;
        isDash = false;
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    /// <param name="move">移動量</param>
    /// <param name="jump">ジャンプ入力</param>
    /// <param name="dash">ダッシュ入力</param>
    public void Move(float move, bool jump, bool dash)
    {
        if (canMove)
        {
            //--------------------
            // 移動 & ダッシュ

            // ダッシュ入力 & ダッシュ可能 & 壁に触れてない
            if (dash && canDash && !isWallSliding)
            {
                //m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_DashForce, 0f));
                StartCoroutine(DashCooldown()); // Dash&クールダウン
            }
            // If crouching, check to see if the character can stand up
            // ダッシュ中の場合
            if (isDashing)
            {   // クールダウンに入るまで加速
                m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_DashForce, 0);
            }
            // 接地しているか空中制御ONの時
            else if (m_Grounded || m_AirControl)
            {
                // 落下速度制限処理
                if (m_Rigidbody2D.linearVelocity.y < -limitFallSpeed)
                    m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, -limitFallSpeed);

                // キャラの目標移動速度を決定
                Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.linearVelocity.y);

                // SmoothDampにより、滑らかな移動を実現
                m_Rigidbody2D.linearVelocity = Vector3.SmoothDamp(m_Rigidbody2D.linearVelocity, targetVelocity, ref velocity, m_MovementSmoothing);

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
                if (!oldWallSlidding && m_Rigidbody2D.linearVelocity.y < 0 || isDashing)
                {   // 前フレームで壁に触れていない && 下に落ちてる or ダッシュ可能
                    isWallSliding = true;
                    m_WallCheck.localPosition = new Vector3(-m_WallCheck.localPosition.x, m_WallCheck.localPosition.y, 0);
                    Flip();
                    StartCoroutine(WaitToCheck(0.1f));
                    canDoubleJump = true;
                    animator.SetInteger("animation_id", (int)ANIM_ID.WallSlide);
                }
                isDashing = false;

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

                        if (id != (int)ANIM_ID.Attack)
                        {
                            animator.SetInteger("animation_id", (int)ANIM_ID.WallSlide);
                        }
                        
                        m_Rigidbody2D.linearVelocity = new Vector2(-transform.localScale.x * 2, -5);
                    }
                }

                if (jump && isWallSliding)
                {   // スライディング中にジャンプ
                    animator.SetInteger("animation_id", (int)ANIM_ID.Fall);

                    m_Rigidbody2D.linearVelocity = new Vector2(0f, 0f);
                    m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_JumpForce * 1.2f, m_JumpForce));
                    jumpWallStartX = transform.position.x;
                    limitVelOnWallJump = true;
                    canDoubleJump = true;
                    isWallSliding = false;
                    oldWallSlidding = false;
                    m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                    canMove = false;
                }
                else if (dash && canDash)
                {
                    isWallSliding = false;
                    oldWallSlidding = false;
                    m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                    canDoubleJump = true;
                    StartCoroutine(DashCooldown());
                }
            }
            else if (isWallSliding && !m_IsWall && canCheck)
            {   // 壁スライドAnim再生中 && 前に壁が無い && 空中に居るとき
                isWallSliding = false;
                oldWallSlidding = false;
                m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                canDoubleJump = true;
            }
        }
    }

    /// <summary>
    /// キャラ反転
    /// </summary>
    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// ダメージを与える処理
    /// </summary>
    public void DoDashDamage()
    {
        dmgValue = Mathf.Abs(dmgValue);
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, 0.9f);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Enemy")
            {
                if (collidersEnemies[i].transform.position.x - transform.position.x < 0)
                {
                    dmgValue = -dmgValue;
                }
                //++ GetComponentでEnemyスクリプトを取得し、ApplyDamageを呼び出すように変更
                collidersEnemies[i].gameObject.SendMessage("ApplyDamage", dmgValue);
                cam.GetComponent<MainCameraFollow>().ShakeCamera();
            }
        }
    }

    /// <summary>
    /// ダメージ受ける処理
    /// </summary>
    /// <param name="damage">ダメージ量</param>
    /// <param name="position"></param>
    public void ApplyDamage(float damage, Vector3 position)
    {
        if (!invincible)
        {
            animator.SetInteger("animation_id", (int)ANIM_ID.Hit);
            life -= damage;
            Vector2 damageDir = Vector3.Normalize(transform.position - position) * 40f;
            m_Rigidbody2D.linearVelocity = Vector2.zero;
            m_Rigidbody2D.AddForce(damageDir * 10);
            if (life <= 0)
            {
                StartCoroutine(WaitToDead());
            }
            else
            {
                StartCoroutine(Stun(0.25f));
                StartCoroutine(MakeInvincible(1f));
            }
        }
    }

    /// <summary>
    /// ダッシュ(ブリンク)制限処理
    /// </summary>
    /// <returns></returns>
    IEnumerator DashCooldown()
    {
        animator.SetInteger("animation_id", (int)ANIM_ID.Dash);
        isDashing = true;
        canDash = false;
        yield return new WaitForSeconds(0.1f);
        isDashing = false;
        yield return new WaitForSeconds(0.5f);
        canDash = true;
    }

    /// <summary>
    /// ダメージ後硬直処理
    /// </summary>
    IEnumerator Stun(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
    /// <summary>
    /// 無敵時間設定処理
    /// </summary>
    IEnumerator MakeInvincible(float time)
    {
        invincible = true;
        yield return new WaitForSeconds(time);
        invincible = false;
    }
    /// <summary>
    /// 動作不能処理
    /// </summary>
    IEnumerator WaitToMove(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
    /// <summary>
    /// 壁スライド中か確認する処理
    /// </summary>
    IEnumerator WaitToCheck(float time)
    {
        canCheck = false;
        yield return new WaitForSeconds(time);
        canCheck = true;
    }
    /// <summary>
    /// 壁スライド終了処理
    /// </summary>
    IEnumerator WaitToEndSliding()
    {
        yield return new WaitForSeconds(0.1f);
        canDoubleJump = true;
        isWallSliding = false;
        animator.SetInteger("animation_id", (int)ANIM_ID.Fall);
        oldWallSlidding = false;
        m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
    }
    /// <summary>
    /// 死亡処理
    /// </summary>
    IEnumerator WaitToDead()
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
    /// 攻撃制限処理
    /// </summary>
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(0.25f);
        canAttack = true;
    }
}
