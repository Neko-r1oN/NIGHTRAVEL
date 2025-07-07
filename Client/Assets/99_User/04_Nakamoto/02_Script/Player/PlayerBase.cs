//--------------------------------------------------------------
// �v���C���[���ېe�N���X [ PlayerBase.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using Pixeye.Unity;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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
        Zipline,
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
    protected int startHp = 0;        // �����̗�

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
    [SerializeField] protected float m_ZipJumpForceX = 60f;  // �W�b�v����~���Ƃ��̗�(X��)
    [Foldout("���ʃX�e�[�^�X")]
    [SerializeField] protected float m_ZipJumpForceY = 40f;  // �W�b�v����~���Ƃ��̗�(Y��)

    [Foldout("���ʃX�e�[�^�X")]
    [SerializeField] protected float zipSpeed = 150f;   // ��q�ړ����x

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
    [SerializeField] protected LayerMask ziplineLayer;  // ���C���[

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] protected Transform m_GroundCheck;	// �v���C���[���ڒn���Ă��邩�ǂ������m�F����p

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] protected Transform m_WallCheck;   // �v���C���[���ǂɐG��Ă��邩�ǂ������m�F����p

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] protected Transform attackCheck;   // �U�����̓����蔻��

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] protected Transform playerPos;		// �v���C���[�ʒu���

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] protected CapsuleCollider2D playerCollider;

    protected float ladderPosX = 0; // ��q��X���W
    #endregion

    #region �v���C���[���擾�ϐ�
    protected Rigidbody2D m_Rigidbody2D;
    protected Vector3 velocity = Vector3.zero;
    protected bool m_FacingRight = true;  // �v���C���[�̌����̔���t���O�itrue�ŉE�����j
    protected bool m_FallFlag = false;
    protected float limitFallSpeed = 25f; // �������x�̐���
    #endregion

    #region �p�[�e�B�N���E�G�t�F�N�g
    [Foldout("�p�[�e�B�N���E�G�t�F�N�g")]
    [SerializeField] protected ParticleSystem particleJumpUp;

    [Foldout("�p�[�e�B�N���E�G�t�F�N�g")]
    [SerializeField] protected ParticleSystem particleJumpDown;

    [Foldout("�p�[�e�B�N���E�G�t�F�N�g")]
    [SerializeField] protected GameObject ziplineSpark;
    #endregion

    #region �J����
    protected GameObject cam;
    #endregion

    #region ����t���O�֘A
    protected bool canMove = true;      // �v���C���[�̓��쐧��t���O
    protected bool canBlink = true;     // �_�b�V������t���O
    protected bool canAttack = true;    // �U���\�t���O
    protected bool m_Grounded;          // �v���C���[�̐ڒn�t���O
    protected bool m_IsWall = false;    // �v���C���[�̑O�ɕǂ����邩
    protected bool m_IsZipline = false; // ����t���O
    protected bool m_IsScaffold = false;// ����ڒn�t���O
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
    protected bool isSkill = false;   // �X�L���g�p���t���O
    protected bool canSkill = true;   // �X�L���g�p�\�t���O
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

    virtual protected void Update()
    {
        // �L�����̈ړ�
        if(!canMove) return;

        horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;
        verticalMove = Input.GetAxisRaw("Vertical") * moveSpeed;
        Ladder();

        if(m_IsZipline)
        {
            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                animator.SetInteger("animation_id", (int)ANIM_ID.Fall);
                m_IsZipline = false;
                ziplineSpark.SetActive(false);
                m_Rigidbody2D.AddForce(new Vector2(-m_ZipJumpForceX,m_ZipJumpForceY));
            }
            else if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                animator.SetInteger("animation_id", (int)ANIM_ID.Fall);
                m_IsZipline = false;
                ziplineSpark.SetActive(false);
                m_Rigidbody2D.AddForce(new Vector2(m_ZipJumpForceX, m_ZipJumpForceY));
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Jump"))
            {   // �W�����v������
                if (animator.GetInteger("animation_id") != (int)ANIM_ID.Blink)
                    isJump = true;
            }

            if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Blink"))
            {   // �u�����N������
                if (canAttack)
                {
                    isBlink = true;
                    gameObject.layer = 21;
                }
            }

            if(m_IsScaffold && Input.GetKeyDown(KeyCode.DownArrow))
            {
                gameObject.layer = 21;
                StartCoroutine(ScaffoldDown());
            }
        }
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

            if (!wasGrounded && canAttack)
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
            if (Input.GetKey(KeyCode.UpArrow) && canBlink && canSkill && canAttack)
            {
                m_IsZipline = true;
                ziplineSpark.SetActive(true);
                animator.SetInteger("animation_id", (int)ANIM_ID.Zipline);
            }
        }

        if (m_IsZipline)
        {
            m_Rigidbody2D.position = new Vector2(ladderPosX, m_Rigidbody2D.position.y);
            m_Rigidbody2D.linearVelocity = new Vector2(0, zipSpeed);
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
        if (m_IsZipline) return;

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
                if (!canAttack)
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
                if (canAttack && animator.GetInteger("animation_id") != (int)ANIM_ID.Blink && !isSkill)
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

            if (m_Grounded && jump && canAttack)
            {   // �ڒn��� & �W�����v����
                    animator.SetInteger("animation_id", (int)ANIM_ID.Fall);
                    m_Grounded = false;
                    m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                    canDoubleJump = true;
                    particleJumpDown.Play();
                    particleJumpUp.Play();
            }
            else if (!m_Grounded && jump && canDoubleJump && !isWallSliding && canAttack)
            {   // �W�����v���ɃW�����v���́i�_�u���W�����v�j
                canDoubleJump = false;
                m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, 0);
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce / 1.2f));

                animator.SetInteger("animation_id", (int)ANIM_ID.DBJump);
            }
            else if (m_IsWall && !m_Grounded && canAttack && !isSkill)
            {   // �ǂɐG�ꂽ && ��
                if (!oldWallSlidding && m_Rigidbody2D.linearVelocity.y < 0 || isBlinking)
                {   // �O�t���[���ŕǂɐG��Ă��Ȃ� && ���ɗ����Ă� or �_�b�V���\
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

                if (jump && isWallSliding && canAttack)
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
            else if (isWallSliding && !m_IsWall && canCheck && canAttack)
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
    /// �W�b�v���C�����菈��
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

    /// <summary>
    /// �ڐG����
    /// </summary>
    /// <param name="collision"></param>
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        // ��������
        if (collision.gameObject.tag == "Abyss")
        {   // �ł��߂����A�_�Ɉړ�
            playerPos.position = FetchNearObjectWithTag("ChecKPoint").position;
        }
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        m_IsScaffold = true;
    }

    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Scaffold")
        {
            m_IsScaffold = false;
        }
    }

    /// <summary>
    /// �P�ԋ߂��I�u�W�F�N�g���擾����
    /// </summary>
    /// <param name="tagName">�擾������tagName</param>
    /// <returns>�ŏ������̎w��Obj</returns>
    private Transform FetchNearObjectWithTag(string tagName)
    {
        // �Y���^�O��1���������ꍇ�͂����Ԃ�
        var targets = GameObject.FindGameObjectsWithTag(tagName);
        if (targets.Length == 1) return targets[0].transform;

        GameObject result = null;               // �Ԃ�l
        var minTargetDistance = float.MaxValue; // �ŏ�����
        foreach (var target in targets)
        {
            // �O��v�������I�u�W�F�N�g�����߂��ɂ���΋L�^
            var targetDistance = Vector3.Distance(transform.position, target.transform.position);
            if (!(targetDistance < minTargetDistance)) continue;
            minTargetDistance = targetDistance;
            result = target.transform.gameObject;
        }

        // �Ō�ɋL�^���ꂽ�I�u�W�F�N�g��Ԃ�
        return result?.transform;
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
    /// ���ꂷ�蔲�����A����
    /// </summary>
    /// <returns></returns>
    protected IEnumerator ScaffoldDown()
    {
        yield return new WaitForSeconds(0.5f);
        m_IsScaffold = false;
        gameObject.layer = 20;
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

        Destroy(this.gameObject);
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
    /// ��_������
    /// (�m�b�N�o�b�N��pos�ɉ����ėL�����ς��)
    /// </summary>
    public void ApplyDamage(int damage, Vector3? position = null, StatusEffectController.EFFECT_TYPE? type = null)
    {
        if (!invincible)
        {
            UIManager.Instance.PopDamageUI(damage,transform.position,true);
            if (position != null && canAttack) animator.SetInteger("animation_id", (int)ANIM_ID.Hit);
            hp -= damage;
            Vector2 damageDir = Vector2.zero;

            // �m�b�N�o�b�N����
            if (position != null)
            {
                damageDir = Vector3.Normalize(transform.position - (Vector3)position) * 40f;
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                m_Rigidbody2D.AddForce(damageDir * 15);
            }

            if(type != null)
            {
                effectController.ApplyStatusEffect((StatusEffectController.EFFECT_TYPE)type);
            }

            if (hp <= 0)
            {   // ���S����
                m_Rigidbody2D.AddForce(damageDir * 10);
                StartCoroutine(WaitToDead());
            }
            else
            {   // ��_���d��
                if (position != null)
                {
                    StartCoroutine(Stun(0.35f));
                    StartCoroutine(MakeInvincible(0.4f));
                }
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
    /// �ڒn����擾
    /// </summary>
    /// <returns></returns>
    public bool GetGrounded() { return m_Grounded; }
    #endregion
}
