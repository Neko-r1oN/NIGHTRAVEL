//--------------------------------------------------------------
// �v���C���[���ېe�N���X [ PlayerBase.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using Pixeye.Unity;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

abstract public class PlayerBase : CharacterBase
{
    //--------------------
    // �t�B�[���h

    #region �A�j���[�V����ID
    /// <summary>
    /// �A�j���[�V����ID
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
    }
    #endregion

    #region ���ʃX�e�[�^�X
    [Foldout("���ʃX�e�[�^�X")]
    protected int nowLv = 1;          // ���݃��x��
    [Foldout("���ʃX�e�[�^�X")]
    protected int nowExp = 0;         // ���݂̊l���o���l
    [Foldout("���ʃX�e�[�^�X")]
    protected int nextLvExp = 0;      // ���̃��x���܂łɕK�v�Ȍo���l

    [Foldout("���ʃX�e�[�^�X")]
    protected int startHp = 0;                    // �����̗�

    [Foldout("���ʃX�e�[�^�X")]
    [SerializeField] protected float m_JumpForce = 400f;    // �W�����v��
    [Foldout("���ʃX�e�[�^�X")]
    [SerializeField] protected float wallJumpPower = 2f;    // �ǃW�����v��
    [Foldout("���ʃX�e�[�^�X")]
    [SerializeField] protected bool m_AirControl = false;   // �󒆐���t���O
    [Foldout("���ʃX�e�[�^�X")]
    public bool canDoubleJump = true;                       // �_�u���W�����v�g�p�t���O

    [Foldout("���ʃX�e�[�^�X")]
    [SerializeField] protected float m_BlinkForce = 38f;  // �u�����N��
    [Foldout("���ʃX�e�[�^�X")]
    [SerializeField] protected float blinkTime = 0.35f;   // �u�����N����
    [Foldout("���ʃX�e�[�^�X")]
    [SerializeField] protected float blinkCoolDown = 1f;  // �u�����N�N�[���_�E��

    [Foldout("���ʃX�e�[�^�X")]
    [SerializeField] protected float ladderSpeed = 1f;   // ��q�ړ����x

    [Foldout("���ʃX�e�[�^�X")]
    [Range(0, .3f)][SerializeField] protected float m_MovementSmoothing = .05f;

    [Foldout("���ʃX�e�[�^�X")]
    public bool invincible = false; // �v���C���[�̎��S����t���O

    [Foldout("���ʃX�e�[�^�X")]
    [SerializeField] protected int testExp = 10;      // �f�o�b�O�p�l���o���l

    protected float horizontalMove = 0f;      // ���x�p�ϐ�
    protected float gravity;  // �d��

    #endregion

    #region �X�e�[�^�X�O���Q�Ɨp�v���p�e�B
    /// <summary>
    /// �����x��
    /// </summary>
    public int NowLv { get { return nowLv; } }

    /// <summary>
    /// ���l���o���l
    /// </summary>
    public int NowExp { get { return nowExp; } }

    /// <summary>
    /// �����x���܂ł̕K�v�o���l
    /// </summary>
    public int NextLvExp { get { return nextLvExp; } }

    #endregion

    #region ���C���[�E�ʒu�֘A
    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] protected LayerMask m_WhatIsGround;// �ǂ̃��C���[��n�ʂƔF�������邩

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] protected LayerMask ladderLayer;   // ��q���C���[

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] protected Transform m_GroundCheck;	// �v���C���[���ڒn���Ă��邩�ǂ������m�F����p

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] protected Transform m_WallCheck;   // �v���C���[���ǂɐG��Ă��邩�ǂ������m�F����p

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] protected Transform attackCheck;     // �U�����̓����蔻��

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] protected Transform playerPos;		// �v���C���[�ʒu���

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] protected CapsuleCollider2D playerCollider;
    #endregion

    #region �v���C���[���擾�ϐ�
    protected Rigidbody2D m_Rigidbody2D;
    protected Vector3 velocity = Vector3.zero;
    protected bool m_FacingRight = true;  // �v���C���[�̌����̔���t���O�itrue�ŉE�����j
    protected bool m_FallFlag = false;
    protected float limitFallSpeed = 25f; // �������x�̐���
    #endregion

    #region �p�[�e�B�N��
    [Foldout("����t���O�֘A")]
    [SerializeField] protected ParticleSystem particleJumpUp;

    [Foldout("����t���O�֘A")]
    [SerializeField] protected ParticleSystem particleJumpDown;
    #endregion

    #region �J����
    protected GameObject cam;
    #endregion

    #region ����t���O�֘A
    protected bool canMove = true;      // �v���C���[�̓��쐧��t���O
    protected bool canBlink = true;     // �_�b�V������t���O
    protected bool nowAttack = true;    // �U���\�t���O
    protected bool m_Grounded;          // �v���C���[�̐ڒn�t���O
    protected bool m_IsWall = false;    // �v���C���[�̑O�ɕǂ����邩
    protected bool m_IsLadder = false;  // ��q����t���O
    protected bool isJump = false;      // �W�����v���̓t���O
    protected bool isBlink = false;     // �_�b�V�����̓t���O
    protected bool isBlinking = false;        // �v���C���[���_�b�V�������ǂ���
    protected bool isWallSliding = false;     // If player is sliding in a wall
    protected bool isWallJump = false;        // �ǃW�����v�����ǂ���
    protected bool isAbnormalMove = false;    // ��Ԉُ�t���O
    protected bool oldWallSlidding = false;   // If player is sliding in a wall in the previous frame
    protected float prevVelocityX = 0f;
    protected bool canCheck = false;          // For check if player is wallsliding
    protected float verticalMove = 0f;
    protected float jumpWallStartX = 0;
    protected float jumpWallDistX = 0;        // �v���C���[�ƕǂ̋���
    protected bool limitVelOnWallJump = false;// ��fps�ŕǂ̃W�����v�����𐧌�����
    #endregion

    #region ����W��
    protected const float k_GroundedRadius = .2f; // �ڒn�m�F�p�̉~�̔��a
    protected const float k_AttackRadius = .9f;   // �U������̉~�̔��a
    #endregion

    //--------------------
    // ���\�b�h

    #region �v���C���[���ʏ���
    //---------------------------------------------------
    // �v���C���[�ɋ��ʂ���֐��������ɋL�ڂ���

    /// <summary>
    /// Update�O����
    /// </summary>

    protected override void Awake()
    {
        base.Awake();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        gravity = m_Rigidbody2D.gravityScale;
        animator = GetComponent<Animator>();
        cam = Camera.main.gameObject;
        startHp = maxHp;
    }

    /// <summary>
    /// ����X�V����
    /// </summary>
    virtual protected void FixedUpdate()
    {
        //---------------------------------
        // �n�ʔ���

        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        if (isAbnormalMove)
        {
            animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
            m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
            return;
        }

        // �O���E���h�`�F�b�N���n�ʂƂ��Ďw�肳�ꂽ���̂ɓ��������ꍇ�A�v���[���[��ڒn�����ɂ���
        // ����̓��C���[���g���čs�����Ƃ��ł��܂����ASample Assets�̓v���W�F�N�g�̐ݒ���㏑�����܂���B
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            // gameObject���v���C���[�I�u�W�F�N�g�̂��ƁH
            if (colliders[i].gameObject != gameObject)
                m_Grounded = true;

            isWallJump = false;

            if (m_Grounded && m_FallFlag)
            {   // �������璅�n���ɃX�^��
                m_FallFlag = false;
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                //StartCoroutine(Stun(1f)); // �X�^������
            }

            if (!wasGrounded && nowAttack)
            {   // �O�t���[���Œn�ʂɐG��Ă��Ȃ���
                animator.SetInteger("animation_id", (int)ANIM_ID.Idle);

                if (!m_IsWall && !isBlinking)
                    particleJumpDown.Play();
                canDoubleJump = true;
                if (m_Rigidbody2D.linearVelocity.y < 0f)
                    limitVelOnWallJump = false;
            }
        }

        // �ڒn����IDLE or RUN���[�V�����ɖ߂�悤�ɐݒ�
        if (m_Grounded && animator.GetInteger("animation_id") == (int)ANIM_ID.Idle && Mathf.Abs(horizontalMove) >= 0.1f
            || m_Grounded && animator.GetInteger("animation_id") == (int)ANIM_ID.Fall && Mathf.Abs(horizontalMove) >= 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Run);

        if (m_Grounded && animator.GetInteger("animation_id") == (int)ANIM_ID.Run && Mathf.Abs(horizontalMove) < 0.1f
            || m_Grounded && animator.GetInteger("animation_id") == (int)ANIM_ID.Fall && Mathf.Abs(horizontalMove) < 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Idle);

        //---------------------------------
        // �ǔ���

        m_IsWall = false;

        if (!m_Grounded)
        {   // �󒆂ɋ���Ƃ�

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

        // ��������
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
        // ��q����

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
    /// �ړ�����
    /// </summary>
    /// <param name="move">�ړ���</param>
    /// <param name="jump">�W�����v����</param>
    /// <param name="blink">�_�b�V������</param>
    private void Move(float move, bool jump, bool blink)
    {
        if (canMove)
        {
            //--------------------
            // �ړ� & �_�b�V��

            // �_�b�V������ & �_�b�V���\ & �ǂɐG��ĂȂ�
            if (blink && canBlink && !isWallSliding)
            {
                //m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_DashForce, 0f));
                StartCoroutine(BlinkCooldown()); // �u�����N�N�[���_�E��
            }
            // If crouching, check to see if the character can stand up
            // �_�b�V�����̏ꍇ
            if (isBlinking)
            {   // �N�[���_�E���ɓ���܂ŉ���
                m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_BlinkForce, 0);
            }
            else if (isWallJump)
            {   // �ǃW�����v��
                m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * 12, 12);
            }
            // �ڒn���Ă��邩�󒆐���ON�̎�
            else if (m_Grounded || m_AirControl)
            {
                // �������x��������
                if (m_Rigidbody2D.linearVelocity.y < -limitFallSpeed)
                {
                    m_FallFlag = true;
                    m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, -limitFallSpeed);
                }

                // �L�����̖ڕW�ړ����x������
                Vector3 targetVelocity = new Vector2();
                if (!nowAttack)
                {
                    targetVelocity = new Vector2(move * 2f, m_Rigidbody2D.linearVelocity.y);
                }
                else
                {
                    targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.linearVelocity.y);
                }

                // SmoothDamp�ɂ��A���炩�Ȉړ�������
                m_Rigidbody2D.linearVelocity = Vector3.SmoothDamp(m_Rigidbody2D.linearVelocity, targetVelocity, ref velocity, m_MovementSmoothing);

                // �U�����͔��]���Ȃ��悤��
                if (nowAttack && animator.GetInteger("animation_id") != (int)ANIM_ID.Blink)
                {
                    // �L���������͂Ɣ��Ε����������Ă����ۂɔ��]������
                    if (move > 0 && !m_FacingRight && !isWallSliding)
                    {   // �E����
                        Flip();
                    }
                    else if (move < 0 && m_FacingRight && !isWallSliding)
                    {   // ������
                        Flip();
                    }
                }
            }

            //--------------------
            // �W�����v

            if (m_Grounded && jump && nowAttack)
            {   // �ڒn��� & �W�����v����
                    animator.SetInteger("animation_id", (int)ANIM_ID.Fall);
                    m_Grounded = false;
                    m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                    canDoubleJump = true;
                    particleJumpDown.Play();
                    particleJumpUp.Play();
            }
            else if (!m_Grounded && jump && canDoubleJump && !isWallSliding && nowAttack)
            {   // �W�����v���ɃW�����v���́i�_�u���W�����v�j
                canDoubleJump = false;
                m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, 0);
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce / 1.2f));

                animator.SetInteger("animation_id", (int)ANIM_ID.DBJump);
            }
            else if (m_IsWall && !m_Grounded && nowAttack)
            {   // �ǂɐG�ꂽ && ��
                if (!oldWallSlidding && m_Rigidbody2D.linearVelocity.y < 0 || isBlinking)
                {   // �O�t���[���ŕǂɐG��Ă��Ȃ� && ���ɗ����Ă� or �_�b�V���\
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

                if (isWallSliding && nowAttack)
                {   // �ǃX���C�h��
                    if (move * transform.localScale.x > 0.1f)
                    {   // �ǂ̔��Ε����ɓ��͂��ꂽ��
                        StartCoroutine(WaitToEndSliding());
                    }
                    else
                    {   // �X���C�h����
                        oldWallSlidding = true;

                        animator.SetInteger("animation_id", (int)ANIM_ID.WallSlide);

                        m_Rigidbody2D.linearVelocity = new Vector2(-transform.localScale.x * 2, -5);
                    }
                }

                if (jump && isWallSliding && nowAttack)
                {   // �X���C�f�B���O���ɃW�����v
                    animator.SetInteger("animation_id", (int)ANIM_ID.Fall);

                    m_Rigidbody2D.linearVelocity = new Vector2(0f, 0f);
                    m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_JumpForce * wallJumpPower, m_JumpForce));
                    jumpWallStartX = transform.position.x;
                    limitVelOnWallJump = true;
                    canDoubleJump = true;
                    isWallSliding = false;
                    oldWallSlidding = false;

                    // �ǃW�����R���[�`��
                    StartCoroutine(WallJump());

                    m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                }
                else if (blink && canBlink)
                {   // �_�b�V��������
                    isWallSliding = false;
                    oldWallSlidding = false;
                    m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                    canDoubleJump = true;
                    StartCoroutine(BlinkCooldown());
                }
            }
            else if (isWallSliding && !m_IsWall && canCheck && nowAttack)
            {   // �ǃX���C�hAnim�Đ��� && �O�ɕǂ�����
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
    /// ��q���菈��
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
    /// �L�������]
    /// </summary>
    protected void Flip()
    {
        m_FacingRight = !m_FacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// ���x���A�b�v����
    /// </summary>
    protected void LevelUp()
    {
        // ���x���A�b�v����
        nowLv++;
        nowExp = nowExp - nextLvExp;
        int nextLv = nowLv + 1;
        nextLvExp = (int)Math.Pow(nextLv, 3) - (int)Math.Pow(nowLv, 3);

        // HP���f����
        CalcHP();
    }

    /// <summary>
    /// HP�v�Z����
    /// </summary> ** ���[�J�� **
    private void CalcHP()
    {
        float hpRatio = (float)hp / (float)maxHp;
        maxHp = (int)(startHp + (int)Math.Pow(nowLv, 2));
        hp = (int)(maxHp * hpRatio);

        Debug.Log("�ő�̗́F" + maxHp + " ���̗́F" + hp);
    }
    #endregion

    #region �v���C���[���ʔ񓯊�����
    /// <summary>
    /// �ǃW�����v��������
    /// </summary>
    /// <returns></returns>
    protected IEnumerator WallJump()
    {
        isWallJump = true;
        yield return new WaitForSeconds(0.2f);  // �W�����v����
        isWallJump = false;
    }
    /// <summary>
    /// �_���[�W��d������
    /// </summary>
    public IEnumerator Stun(float time)
    {
        Debug.Log("�X�^���F" + time);
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
    /// <summary>
    /// ��Ԉُ펞�d������
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
    /// ���G���Ԑݒ菈��
    /// </summary>
    protected IEnumerator MakeInvincible(float time)
    {
        invincible = true;
        yield return new WaitForSeconds(time);
        invincible = false;
    }
    /// <summary>
    /// ����s�\����
    /// </summary>
    protected IEnumerator WaitToMove(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
    /// <summary>
    /// �ǃX���C�h�����m�F���鏈��
    /// </summary>
    protected IEnumerator WaitToCheck(float time)
    {
        canCheck = false;
        yield return new WaitForSeconds(time);
        canCheck = true;
    }
    /// <summary>
    /// �ǃX���C�h�I������
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
    /// ���S����
    /// </summary>
    protected IEnumerator WaitToDead()
    {
        animator.SetInteger("animation_id", (int)ANIM_ID.Dead);
        canMove = false;
        invincible = true;
        yield return new WaitForSeconds(0.4f);
        m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
        yield return new WaitForSeconds(1.1f);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
    /// <summary>
    /// �_�b�V��(�u�����N)��������
    /// </summary>
    /// <returns></returns>
    protected IEnumerator BlinkCooldown()
    {
        animator.SetInteger("animation_id", (int)ANIM_ID.Blink);
        isBlinking = true;
        canBlink = false;
        yield return new WaitForSeconds(blinkTime);  // �u�����N����
        gameObject.layer = 20;
        isBlinking = false;
        yield return new WaitForSeconds(blinkCoolDown);  // �N�[���_�E������
        canBlink = true;
    }
    #endregion

    #region �O���Ăяo���֐�

    /// <summary>
    /// �_���[�W��^���鏈��
    /// </summary>
    abstract public void DoDashDamage();

    /// <summary>
    /// �u�����N�I������
    /// </summary>
    public void BlinkEnd()
    {
        animator.SetInteger("animation_id", (int)ANIM_ID.Run);
    }

    /// <summary>
    /// ��_������(�m�b�N�o�b�N�L)
    /// </summary>
    public void ApplyDamage(int damage, Vector3? position = null)
    {
        if (!invincible)
        {
            if (position != null) animator.SetInteger("animation_id", (int)ANIM_ID.Hit);
            hp -= damage;

            // �m�b�N�o�b�N����
            if(position != null)
            {
                Vector2 damageDir = Vector3.Normalize(transform.position - (Vector3)position) * 40f;
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                m_Rigidbody2D.AddForce(damageDir * 10);
            }

            if (hp <= 0)
            {   // ���S����
                StartCoroutine(WaitToDead());
            }
            else
            {   // ��_���d��
                if(position != null) StartCoroutine(Stun(0.25f));

                StartCoroutine(MakeInvincible(1f));
            }
        }
    }

    /// <summary>
    /// �o���l�l��
    /// </summary>
    /// <param name="exp">�o���l��</param>
    public void GetExp(int exp)
    {
        nowExp += exp;

        if (nextLvExp <= nowExp)
        {   // ���x���A�b�v����
            LevelUp();
        }
    }

    /// <summary>
    /// �X�e�[�^�X�ϓ�����
    /// </summary>
    /// <param name="statusID">����������X�e�[�^�XID</param>
    /// <param name="value">�����l</param>
    public void ChangeStatus(int statusID, int value)
    {   //�����̓X�e�[�^�X�S�����܂񂾒ʐM�p�p�b�P�[�W������ēK�p

    }
    #endregion
}
