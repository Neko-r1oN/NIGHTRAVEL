//--------------------------------------------------------------
// �T���v���L���� [ SampleChara.cs ]
// Author�FKenta Nakamoto
// ���p�Fhttps://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;
using Pixeye.Unity;
using System;

public class Sword : Player
{
    #region �A�j���[�V����ID
    /// <summary>
    /// �A�j���[�V����ID
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

    /// <summary>
    /// �X�e�[�^�XID
    /// </summary>
    public enum STATUS_ID
    {
        HP = 1,         // �̗�
        Power,          // �U����
        Defense,        // �h���
        MoveSpeed,      // �ړ����x
        AttackSpeed,    // �U�����x
        DmgMitigation,  // �_���[�W�y��

        HPMag = 20,     // �̗͔{��
        PowMag,         // �U���͔{��
        DefMag,         // �h��͔{��
        MSMag,          // �ړ����x�{��
        ASMag,          // �U�����x�{��
    }
    #endregion

    #region �X�e�[�^�X�֘A
    [Foldout("�X�e�[�^�X")]
    private int nowLv = 1;          // ���݃��x��
    [Foldout("�X�e�[�^�X")]
    private int nowExp = 0;         // ���݂̊l���o���l
    [Foldout("�X�e�[�^�X")]
    private int nextLvExp = 0;      // ���̃��x���܂łɕK�v�Ȍo���l

    [Foldout("�X�e�[�^�X")]
    [SerializeField] private int maxHp = 200;   // �ő�̗�
    [Foldout("�X�e�[�^�X")]
    [SerializeField] private int hp = 200;      // ���̗�
    [Foldout("�X�e�[�^�X")]
    private int startHp = 0;                    // �����̗�

    [Foldout("�X�e�[�^�X")]
    public int dmgValue = 20;       // �U����

    [Foldout("�X�e�[�^�X")]
    public float runSpeed = 40f;    // ���鑬�x

    [Foldout("�X�e�[�^�X")]
    [SerializeField] private float m_JumpForce = 400f;  // �W�����v��
    [Foldout("�X�e�[�^�X")]
    [SerializeField] private float wallJumpPower = 2f;  // �ǃW�����v��
    [Foldout("�X�e�[�^�X")]
    [SerializeField] private bool m_AirControl = false; // �󒆐���t���O
    [Foldout("�X�e�[�^�X")]
    public bool canDoubleJump = true;                   // �_�u���W�����v�g�p�t���O

    [Foldout("�X�e�[�^�X")]
    [SerializeField] private float m_BlinkForce = 45f;  // �u�����N��
    [Foldout("�X�e�[�^�X")]
    [SerializeField] private float blinkTime = 0.2f;    // �u�����N����
    [Foldout("�X�e�[�^�X")]
    [SerializeField] private float blinkCoolDown = 1f;  // �u�����N�N�[���_�E��

    [Foldout("�X�e�[�^�X")]
    [SerializeField] float ladderSpeed = 1f;   // ��q�ړ����x

    [Foldout("�X�e�[�^�X")]
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;

    [Foldout("�X�e�[�^�X")]
    public bool invincible = false; // �v���C���[�̎��S����t���O

    [Foldout("�X�e�[�^�X")]
    [SerializeField] GameObject throwableObject;    // ��������

    [Foldout("�X�e�[�^�X")]
    [SerializeField] private int testExp = 10;      // �f�o�b�O�p�l���o���l

    private float horizontalMove = 0f;      // ���x�p�ϐ�
    private float gravity;  // �d��

    #endregion

    #region �X�e�[�^�X�O���Q�Ɨp�v���p�e�B

    /// <summary>
    /// �ő�̗�
    /// </summary>
    public int MaxHP { get { return maxHp; } }

    /// <summary>
    /// �̗�
    /// </summary>
    public int HP { get { return hp; } }

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
    [SerializeField] private LayerMask m_WhatIsGround;  // �ǂ̃��C���[��n�ʂƔF�������邩

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] private LayerMask ladderLayer;     // ��q���C���[

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] private Transform m_GroundCheck;	// �v���C���[���ڒn���Ă��邩�ǂ������m�F����p

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] private Transform m_WallCheck;     // �v���C���[���ǂɐG��Ă��邩�ǂ������m�F����p

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] private Transform attackCheck;		// �U�����̓����蔻��

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] private Transform playerPos;		// �v���C���[�ʒu���

    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] private CapsuleCollider2D playerCollider;
    #endregion

    #region �v���C���[���擾�ϐ�
    private Rigidbody2D m_Rigidbody2D;
    private Animator animator;
    private Vector3 velocity = Vector3.zero;
    private bool m_FacingRight = true;  // �v���C���[�̌����̔���t���O�itrue�ŉE�����j
    private bool m_FallFlag = false;
    private float limitFallSpeed = 25f; // �������x�̐���
    #endregion

    #region �p�[�e�B�N��
    [Foldout("����t���O�֘A")]
    public ParticleSystem particleJumpUp;

    [Foldout("����t���O�֘A")]
    public ParticleSystem particleJumpDown;
    #endregion

    #region �J����
    private GameObject cam;
    #endregion

    #region ����t���O�֘A
    private bool canMove = true;    // �v���C���[�̓��쐧��t���O
    private bool canBlink = true;   // �_�b�V������t���O
    private bool canAttack = true;  // �U������t���O
    private bool m_Grounded;	    // �v���C���[�̐ڒn�t���O
    private bool m_IsWall = false;  // �v���C���[�̑O�ɕǂ����邩
    private bool m_IsLadder = false;// ��q����t���O
    private bool isJump = false;	// �W�����v���̓t���O
    private bool isBlink = false;	// �_�b�V�����̓t���O
    private bool isBlinking = false;        // �v���C���[���_�b�V�������ǂ���
    private bool isWallSliding = false;     // If player is sliding in a wall
    private bool isWallJump = false;        // �ǃW�����v�����ǂ���
    private bool oldWallSlidding = false;   // If player is sliding in a wall in the previous frame
    private float prevVelocityX = 0f;
    private bool canCheck = false;          // For check if player is wallsliding
    private float verticalMove = 0f;
    private float jumpWallStartX = 0;
    private float jumpWallDistX = 0;        // �v���C���[�ƕǂ̋���
    private bool limitVelOnWallJump = false;// ��fps�ŕǂ̃W�����v�����𐧌�����
    #endregion

    #region ����W��
    private const float k_GroundedRadius = .2f; // �ڒn�m�F�p�̉~�̔��a
    private const float k_AttackRadius = .9f;   // �U������̉~�̔��a
    #endregion

    /// <summary>
    /// Update�O����
    /// </summary>

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        gravity = m_Rigidbody2D.gravityScale;
        animator = GetComponent<Animator>();
        cam = Camera.main.gameObject;
        startHp = maxHp;
    }

    /// <summary>
    /// �X�V����
    /// </summary>
    private void Update()
    {
        // �L�����̈ړ�
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        verticalMove = Input.GetAxisRaw("Vertical") * runSpeed;
        Ladder();

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Jump"))
        {   // �W�����v������
            isJump = true;
        }

        if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Blink"))
        {   // �u�����N������
            isBlink = true;
            gameObject.layer = 21;
        }

        if (Input.GetKeyDown(KeyCode.X) && canAttack || Input.GetButtonDown("Attack1") && canAttack)
        {   // �U��1
            canAttack = false;
            animator.SetInteger("animation_id", (int)ANIM_ID.Attack);
            StartCoroutine(AttackCooldown());
        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
        {   // �U��2
            GameObject throwableWeapon = Instantiate(throwableObject, transform.position + new Vector3(transform.localScale.x * 0.5f, -0.2f), Quaternion.identity) as GameObject;
            Vector2 direction = new Vector2(transform.localScale.x, 0);
            throwableWeapon.GetComponent<ThrowableWeapon>().direction = direction;
            throwableWeapon.name = "ThrowableWeapon";
        }

        //-----------------------------
        // �f�o�b�O�p

        if(Input.GetKeyDown(KeyCode.L))
        {
            GetExp(testExp);
            Debug.Log("�l���o���l�F" + testExp + "�����x���F" + nowLv + " ���o���l�F" + nowExp + "�K�v�o���l" + nextLvExp);
        }
    }


    /// <summary>
    /// ����X�V����
    /// </summary>
    private void FixedUpdate()
    {
        //---------------------------------
        // �n�ʔ���

        bool wasGrounded = m_Grounded;
        m_Grounded = false;

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

            if (!wasGrounded)
            {   // �O�t���[���Œn�ʂɐG��Ă��Ȃ���
                animator.SetInteger("animation_id", (int)ANIM_ID.Idle);

                if (!m_IsWall && !isBlinking)
                    particleJumpDown.Play();
                canDoubleJump = true;
                if (m_Rigidbody2D.linearVelocity.y < 0f)
                    limitVelOnWallJump = false;
            }
        }

        if (m_Grounded && animator.GetInteger("animation_id") == (int)ANIM_ID.Idle && Mathf.Abs(horizontalMove) >= 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Run);

        if(m_Grounded && animator.GetInteger("animation_id") == (int)ANIM_ID.Run && Mathf.Abs(horizontalMove) < 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Idle);

        //---------------------------------
        // �ǔ���

        m_IsWall = false;

        if (!m_Grounded)
        {   // �󒆂ɋ���Ƃ�

            if(animator.GetInteger("animation_id") == (int)ANIM_ID.Idle || animator.GetInteger("animation_id") == (int)ANIM_ID.Run)
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
            if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                m_IsLadder = true;
            }
        }

        if(m_IsLadder)
        {
            m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, verticalMove * ladderSpeed);
            m_Rigidbody2D.gravityScale = 0f;
        }
        else
        {
            m_Rigidbody2D.gravityScale = gravity;
        }

        // 
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
        //Debug.Log(isWallJump);

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
                if (animator.GetInteger("animation_id") == (int)ANIM_ID.Attack)
                {
                    targetVelocity = new Vector2(move * 2f, m_Rigidbody2D.linearVelocity.y);
                }
                else
                {
                    targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.linearVelocity.y);
                }

                // SmoothDamp�ɂ��A���炩�Ȉړ�������
                m_Rigidbody2D.linearVelocity = Vector3.SmoothDamp(m_Rigidbody2D.linearVelocity, targetVelocity, ref velocity, m_MovementSmoothing);

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

            //--------------------
            // �W�����v

            if (m_Grounded && jump)
            {   // �ڒn��� & �W�����v����
                animator.SetInteger("animation_id", (int)ANIM_ID.Fall);
                
                m_Grounded = false;
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                canDoubleJump = true;
                particleJumpDown.Play();
                particleJumpUp.Play();
            }
            else if (!m_Grounded && jump && canDoubleJump && !isWallSliding)
            {   // �W�����v���ɃW�����v���́i�_�u���W�����v�j
                canDoubleJump = false;
                m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, 0);
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce / 1.2f));

                animator.SetInteger("animation_id", (int)ANIM_ID.DBJump);
            }
            else if (m_IsWall && !m_Grounded)
            {   // �ǂɐG�ꂽ && ��
                if (!oldWallSlidding && m_Rigidbody2D.linearVelocity.y < 0 || isBlinking)
                {   // �O�t���[���ŕǂɐG��Ă��Ȃ� && ���ɗ����Ă� or �_�b�V���\
                    isWallSliding = true;
                    m_WallCheck.localPosition = new Vector3(-m_WallCheck.localPosition.x, m_WallCheck.localPosition.y, 0);
                    Flip();
                    StartCoroutine(WaitToCheck(0.1f));
                    canDoubleJump = true;

                    isWallJump = false;

                    animator.SetInteger("animation_id", (int)ANIM_ID.WallSlide);
                }
                isBlinking = false;

                if (isWallSliding)
                {   // �ǃX���C�h��
                    if (move * transform.localScale.x > 0.1f)
                    {   // �ǂ̔��Ε����ɓ��͂��ꂽ��
                        StartCoroutine(WaitToEndSliding());
                    }
                    else
                    {   // �X���C�h����
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
                    //canMove = false;  �ǋ߂Ńo�O�����̂ň�U�����B����s�ǋN������čl
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
            else if (isWallSliding && !m_IsWall && canCheck)
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
    /// �L�������]
    /// </summary>
    private void Flip()
    {
        m_FacingRight = !m_FacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// ��q���菈��
    /// </summary>
    /// <returns></returns>
    private bool Ladder()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, ladderLayer);
        if (hit.collider != null)
            return true;
        else
            m_IsLadder = false;
            return false;
    }

    /// <summary>
    /// ���x���A�b�v����
    /// </summary>
    private void LevelUp()
    {
        // ���x���A�b�v����
        nowLv++;
        nowExp = nowExp - nextLvExp;
        int nextLv = nowLv + 1;
        nextLvExp = (int)Math.Pow(nextLv, 3) - (int)Math.Pow(nowLv, 3);

        // HP��������
        float hpRatio = (float)hp / (float)maxHp;
        maxHp = startHp + (int)Math.Pow(nowLv, 2);
        hp = (int)(maxHp * hpRatio);

        Debug.Log("�ő�̗́F" + maxHp + " ���̗́F" + hp);
    }

    //-------------------------------------------
    // ���ۊ֐��p������

    /// <summary>
    /// �_���[�W��^���鏈��
    /// </summary>
    override public void DoDashDamage()
    {
        dmgValue = Mathf.Abs(dmgValue);
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, k_AttackRadius);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Enemy")
            {
                if (collidersEnemies[i].transform.position.x - transform.position.x < 0)
                {
                    dmgValue = -dmgValue;
                }
                //++ GetComponent��Enemy�X�N���v�g���擾���AApplyDamage���Ăяo���悤�ɕύX
                //++ �j��ł���I�u�W�F�����ۂɂ̓I�u�W�F�̋��ʔ�_���֐����ĂԂ悤�ɂ���
                collidersEnemies[i].gameObject.GetComponent<EnemyController>().ApplyDamage(dmgValue,playerPos);
                cam.GetComponent<CameraFollow>().ShakeCamera();
            }
        }
    }

    /// <summary>
    /// �_���[�W�󂯂鏈��
    /// </summary>
    /// <param name="damage">�_���[�W��</param>
    /// <param name="position">�U�������I�u�W�F�̈ʒu</param>
    override public void ApplyDamage(int damage, Vector3 position)
    {
        if (!invincible)
        {
            animator.SetInteger("animation_id", (int)ANIM_ID.Hit);
            hp -= damage;

            // �m�b�N�o�b�N����
            Vector2 damageDir = Vector3.Normalize(transform.position - position) * 40f;
            m_Rigidbody2D.linearVelocity = Vector2.zero;
            m_Rigidbody2D.AddForce(damageDir * 10);

            if (hp <= 0)
            {   // ���S����
                StartCoroutine(WaitToDead());
            }
            else
            {   // ��_���d��
                StartCoroutine(Stun(0.25f));
                StartCoroutine(MakeInvincible(1f));
            }
        }
    }

    /// <summary>
    /// �o���l�l��
    /// </summary>
    /// <param name="exp">�o���l��</param>
    public override void GetExp(int exp)
    {
        nowExp += exp;

        if(nextLvExp <= nowExp)
        {   // ���x���A�b�v����
            LevelUp();
        }
    }

    /// <summary>
    /// �X�e�[�^�X�ϓ�����
    /// </summary>
    /// <param name="statusID">����������X�e�[�^�XID</param>
    /// <param name="value">�����l</param>
    public override void ChangeStatus(int statusID, int value)
    {

    }

    //----------------------------------
    // �񓯊�����

    /// <summary>
    /// �_�b�V��(�u�����N)��������
    /// </summary>
    /// <returns></returns>
    IEnumerator BlinkCooldown()
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

    /// <summary>
    /// �ǃW�����v��������
    /// </summary>
    /// <returns></returns>
    IEnumerator WallJump()
    {
        isWallJump = true;
        yield return new WaitForSeconds(0.2f);  // �W�����v����
        isWallJump = false;
    }

    /// <summary>
    /// �_���[�W��d������
    /// </summary>
    IEnumerator Stun(float time)
    {
        Debug.Log("�X�^���I�F" + time);
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
    /// <summary>
    /// ���G���Ԑݒ菈��
    /// </summary>
    IEnumerator MakeInvincible(float time)
    {
        invincible = true;
        yield return new WaitForSeconds(time);
        invincible = false;
    }
    /// <summary>
    /// ����s�\����
    /// </summary>
    IEnumerator WaitToMove(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
    /// <summary>
    /// �ǃX���C�h�����m�F���鏈��
    /// </summary>
    IEnumerator WaitToCheck(float time)
    {
        canCheck = false;
        yield return new WaitForSeconds(time);
        canCheck = true;
    }
    /// <summary>
    /// �ǃX���C�h�I������
    /// </summary>
    IEnumerator WaitToEndSliding()
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
    /// �U����������
    /// </summary>
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(0.25f);
        canAttack = true;
    }
}
