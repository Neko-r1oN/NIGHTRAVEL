//--------------------------------------------------------------
// �v���C���[�e�N���X [ PlayerBase.cs ]
// Author�FKenta Nakamoto
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
    protected int nowLv = 1;        // ���݃��x��
    [Foldout("���ʃX�e�[�^�X")]
    protected int nowExp = 0;       // ���݂̊l���o���l
    [Foldout("���ʃX�e�[�^�X")]
    protected int nextLvExp = 10;   // ���̃��x���܂łɕK�v�Ȍo���l

    [Foldout("���ʃX�e�[�^�X")]
    protected int startHp = 0;      // �����̗�

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

    protected Player_Type playerType;                   // �v���C���[�^�C�v
    protected float gravity;                            // �d��

    private float regeneTimer;              //  �I�[�g���W�F�l�^�C�}�[
    private float healGenerateTimer = 0f;   //  �񕜐����^�C�}�[

    #endregion

    #region �����b�N�֘A�X�e�[�^�X

    [Foldout("�����b�N�֘A�X�e�[�^�X")]
    [SerializeField] protected GameObject healObjectPrefab; // �񕜃I�u�W�F�N�g��Prefab

    protected Dictionary<DEBUFF_TYPE, float> giveDebuffRates = new Dictionary<DEBUFF_TYPE, float>()
        {
            { DEBUFF_TYPE.Burn, 0f },
            { DEBUFF_TYPE.Freeze, 0f },
            { DEBUFF_TYPE.Shock, 0f },
        };  // ��Ԉُ�t�^����
    public float regainCodeRate = 0f;        // ���^�_���񕜗�
    public int scatterBugCnt = 0;            // �i�ۗ��j�{��������
    public float holographicArmorRate = 0;   // �����
    public float mouseRate = 0;              // ���N�[���_�E���Z�k��
    public int digitalMeatCnt = 0;           // ���񕜓�������
    public float firewallRate = 0;           // ����_���[�W�y����
    public float lifeScavengerRate = 0;      // ���L����HP�񕜗�
    public float rugrouterRate = 0;          // ���_�u���A�^�b�N��
    public int buckupHDMICnt = 0;            // �����o�C�u��
    public float identificationAIRate = 0;   // ����Ԉُ�_���[�W�{��
    public float danborDollRate = 0;         // ���h��ђʗ���
    public int chargedCoreCnt = 0;           // �i�ۗ��j���d�I�[�u������
    public float illegalScriptRate = 0;      // ���N���e�B�J���I�[�o�[�L��������

    #endregion

    #region �X�e�[�^�X�O���Q�Ɨp�v���p�e�B

    /// <summary>
    /// �v���C���[�̓��쐧��t���O
    /// </summary>
    public bool CanMove { get { return canMove; } set { canMove = value; } }

    /// <summary>
    /// �����x��
    /// </summary>
    public int NowLv { get { return nowLv; } set { nowLv = value; } }

    /// <summary>
    /// ���l���o���l
    /// </summary>
    public int NowExp { get { return nowExp; } set { nowExp = value; } }

    /// <summary>
    /// �����x���܂ł̕K�v�o���l
    /// </summary>
    public int NextLvExp { get { return nextLvExp; } set { nextLvExp = value; } }

    /// <summary>
    /// ����L�����̃^�C�v
    /// </summary>
    public Player_Type PlayerType { get { return playerType; } }

    /// <summary>
    /// �{�X�G���A�N���t���O
    /// </summary>
    public bool IsBossArea { get { return isBossArea; } }
    #endregion

    #region �����b�N�O���Q�Ɨp�v���p�e�B
    /// <summary>
    /// �^�_���񕜗�
    /// </summary>
    public float DmgHealRate { get { return regainCodeRate; } }

    /// <summary>
    /// �񕜓�������
    /// </summary>
    public int DigitalMeatCnt { get { return digitalMeatCnt; } }

    /// <summary>
    /// ��Ԉُ�_���[�W�{��
    /// </summary>
    public float DebuffDmgRate { get { return identificationAIRate; } }

    /// <summary>
    /// �h��ђʗ�
    /// </summary>
    public float PierceRate { get { return danborDollRate; } }
    #endregion

    #region ���C���[�E�ʒu�֘A
    [Foldout("���C���[�E�ʒu�֘A")]
    [SerializeField] protected int enemyLayer = 6;

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
    protected PlayerBase m_Player;
    #endregion

    #region �G�t�F�N�g�EUI
    [Foldout("�G�t�F�N�g�EUI")]
    [SerializeField] protected ParticleSystem particleJumpUp;

    [Foldout("�G�t�F�N�g�EUI")]
    [SerializeField] protected ParticleSystem particleJumpDown;

    [Foldout("�G�t�F�N�g�EUI")]
    [SerializeField] protected GameObject ziplineSpark;

    [Foldout("�G�t�F�N�g�EUI")]
    [SerializeField] protected GameObject interactObj;

    [Foldout("�G�t�F�N�g�EUI")]
    [SerializeField] protected Sprite[] interactSprits;         // [0] Pad [1] Key

    [Foldout("�G�t�F�N�g�EUI")]
    [SerializeField] protected ParticleSystem groundSmoke;
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
    protected bool m_IsScaffold = false;// ���ꓮ��t���O
    protected bool isJump = false;      // �W�����v���̓t���O
    protected bool isBlink = false;     // �_�b�V�����̓t���O
    protected bool isBlinking = false;        // �v���C���[���_�b�V�������ǂ���
    protected bool isWallSliding = false;     // If player is sliding in a wall
    protected bool isWallJump = false;        // �ǃW�����v�����ǂ���
    protected bool isAbnormalMove = false;    // ��Ԉُ�t���O
    protected bool oldWallSlidding = false;   // If player is sliding in a wall in the previous frame
    protected float prevVelocityX = 0f;
    protected bool canCheck = false;          // For check if player is wallsliding
    protected float horizontalMove = 0f;      // ���x�p�ϐ�
    protected float verticalMove = 0f;
    protected float jumpWallStartX = 0;
    protected float jumpWallDistX = 0;        // �v���C���[�ƕǂ̋���
    protected bool limitVelOnWallJump = false;// ��fps�ŕǂ̃W�����v�����𐧌�����
    protected bool isSkill = false;   // �X�L���g�p���t���O
    protected bool canSkill = true;   // �X�L���g�p�\�t���O
    protected bool isRegene = true;
    protected bool isBossArea = false;  
    protected bool isDead = false;
    #endregion

    #region ����t���O�֘A�O���Q�Ɨp
    public bool IsDead { get { return isDead; } private set { isDead = value; } }
    #endregion

    #region �v���C���[�Ɋւ���萔
    protected const float REGENE_TIME = 1.5f;           // �����񕜊Ԋu
    protected const float REGENE_STOP_TIME = 3.0f;      // �����񕜒�~����
    protected const float REGENE_MAGNIFICATION = 0.05f; // �����񕜔{��
    protected const float HEAL_GENERATE_TIME = 18f;     // �񕜓������Ԋu
    protected const float MEATHEAL_RATE = 0.03f;        // �񕜓��񕜗�

    protected const float GROUNDED_RADIUS = .2f;// �ڒn�m�F�p�̉~�̔��a
    protected const float ATTACK_RADIUS = 1.2f; // �U������̉~�̔��a

    protected const float KNOCKBACK_DIR = 40f;  // �m�b�N�o�b�N
    protected const float KB_SMALL = 5f;        // �m�b�N�o�b�N�́i���j
    protected const float KB_MEDIUM = 10f;      // �m�b�N�o�b�N�́i���j
    protected const float KB_BIG = 20f;         // �m�b�N�o�b�N�́i��j

    protected const float STUN_TIME = 0.2f;        // �X�^������
    protected const float INVINCIBLE_TIME = 0.5f;  // ���G����

    protected const float SMOKE_SCALE = 0.22f; // �y���̃X�P�[��

    protected const float STICK_DEAD_ZONE = 0.3f; // �X�e�B�b�N�̃f�b�h�]�[��
    #endregion

    [SerializeField] AudioClip useZipline_SE;   // �W�b�v���C���g�p��
    [SerializeField] AudioClip usingZipline_SE; // �W�b�v���C���g�p����

    AudioSource audioSource;

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
    }

    protected override void Start()
    {
        base.Start();

        // �e��l�̎擾
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_Player = GetComponent<PlayerBase>();
        gravity = m_Rigidbody2D.gravityScale;
        animator = GetComponent<Animator>();
        cam = Camera.main.gameObject;
        startHp = maxHp;
        audioSource = GetComponent<AudioSource>();

        // �J�����̃^�[�Q�b�g�����g�ɐݒ�
        if(RoomModel.Instance == null)
        {
            //cam.GetComponent<CameraFollow>().Target = gameObject.transform;
        }
    }

    /// <summary>
    /// �X�V����
    /// </summary>
    virtual protected void Update()
    {
        regeneTimer += Time.deltaTime;
        healGenerateTimer += Time.deltaTime;

        // ���b�ő�HP��1% ����b�l�Ƃ��A1�b���Ɋ�b�l���񕜂���
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

        // �񕜃I�u�W�F�����Ԋu��healMeatCnt������
        if (healGenerateTimer >= HEAL_GENERATE_TIME)
        {
            GenerateHealObject();
            healGenerateTimer = 0f;
        }

        // �L�����̈ړ�
        if (!canMove) return;

        horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;
        verticalMove = Input.GetAxisRaw("Vertical") * moveSpeed;

        // �����Ă��鎞�ɓy�����N����
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

                audioSource.Stop();     // �W�b�v���C���g�p����~
                audioSource.PlayOneShot(useZipline_SE);
            }
            else if(Input.GetKeyDown(KeyCode.D) || Input.GetAxisRaw("Horizontal") <= -STICK_DEAD_ZONE)
            {
                animator.SetInteger("animation_id", (int)ANIM_ID.Fall);
                m_IsZipline = false;
                ziplineSpark.SetActive(false);
                m_Rigidbody2D.AddForce(new Vector2(m_ZipJumpForceX, m_ZipJumpForceY));

                audioSource.Stop();     // �W�b�v���C���g�p����~
                audioSource.PlayOneShot(useZipline_SE);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump"))
            {   // �W�����v������
                if (animator.GetInteger("animation_id") != (int)ANIM_ID.Blink)
                    isJump = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("Blink"))
            {   // �u�����N������
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, GROUNDED_RADIUS, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            // Object���v���C���[�I�u�W�F�N�g�̂��ƁH
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
    /// �����b�N�̍ő�l�̕ύX������ɉ��������ݒl�̕ύX
    /// </summary>
    /// <param name="changeData">������̃X�e�[�^�X</param>
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
    /// �ړ�����
    /// </summary>
    /// <param name="move">�ړ���</param>
    /// <param name="jump">�W�����v����</param>
    /// <param name="blink">�_�b�V������</param>
    virtual protected void Move(float move, bool jump, bool blink)
    {
        if (m_IsZipline) return;

        if (canMove)
        {
            //--------------------
            // �ړ� & �_�b�V��

            // �_�b�V������ & �_�b�V���\ & �ǂɐG��ĂȂ�
            if (blink && canBlink && !isWallSliding)
            {
                StartCoroutine(BlinkCooldown()); // �u�����N�N�[���_�E��
            }
            
            if (isWallJump)
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
    /// ����Ɋւ��t���O�����Z�b�g
    /// </summary>
    protected void ResetMoveFlag()
    {
        canMove = true;
        canAttack = true;
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
        while (nowExp >= nextLvExp)
        {
            nowLv++;                        // ���݂̃��x�����グ��
            nowExp = nowExp - nextLvExp;    // ���߂������̌o���l�����݂̌o���l�ʂƂ��ĕۊ�

            // ���̃��x���܂ŕK�v�Ȍo���l�ʂ��v�Z �i�K�v�Ȍo���l�� = ���̃��x����3�� - ���̃��x����3��j
            nextLvExp = (int)Math.Pow(nowLv + 1, 3) - (int)Math.Pow(nowLv, 3);

            // ���x���A�b�v�ɂ��X�e�[�^�X�ω�
            LevelUpStatusChange();
        }
    }

    /// <summary>
    /// �R���C�_�[�ڐG����
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
    /// �g���K�[�ڐG����
    /// </summary>
    /// <param name="collision"></param>
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        // ��������
        if (collision.gameObject.tag == "Gimmick/Abyss")
        {   // �ł��߂����A�_�Ɉړ�
            MoveCheckPoint();
        }

        if (collision.gameObject.tag == "BossArea") isBossArea = true;

        // �C���^���N�g�I�u�W�F�ڐG����
        if (collision.gameObject.tag == "Interact")
        {   // �C���^���N�gUI�\��
            SpriteRenderer spriteRenderer = interactObj.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = (UIManager.Instance.IsInputGamePad) ? interactSprits[0] : interactSprits[1];

            // UI�A�j���[�V����
            spriteRenderer.DOFade(1f, 0.4f);
            interactObj.GetComponent<Transform>().DOLocalMoveY(0.8f, 0.4f);
        }
    }

    /// <summary>
    /// �g���K�[�ڐG����
    /// </summary>
    /// <param name="collision"></param>
    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BossArea") isBossArea = false;

        // �C���^���N�g�I�u�W�F�ڐG����
        if (collision.gameObject.tag == "Interact")
        {   // �C���^���N�gUI��\��
            SpriteRenderer spriteRenderer = interactObj.gameObject.GetComponent<SpriteRenderer>();

            // UI�A�j���[�V����
            spriteRenderer.DOFade(0f, 0.4f);
            interactObj.GetComponent<Transform>().DOLocalMoveY(0.5f, 0.4f);
        }
    }

    /// <summary>
    /// �P�ԋ߂��I�u�W�F�N�g���擾����
    /// </summary>
    /// <param name="tagName">�擾������tagName</param>
    /// <returns>�ŏ������̎w��Obj</returns>
    public Transform FetchNearObjectWithTag(string tagName)
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

    /// <summary>
    /// �񕜃I�u�W�F�N�g��������
    /// </summary>
    private void GenerateHealObject()
    {
        if (healObjectPrefab == null || digitalMeatCnt <= 0) return;

        float radius = 1.0f; // �v���C���[����̏�������
        float minAngle = -90f;
        float maxAngle = 90f;
        float minForce = 150f; // AddForce�̍ŏ��l
        float maxForce = 250f; // AddForce�̍ő�l

        for (int i = 0; i < digitalMeatCnt; i++)
        {
            // -90���`90���͈̔͂Ń����_���Ȋp�x������
            float angle = UnityEngine.Random.Range(minAngle, maxAngle);
            float rad = angle * Mathf.Deg2Rad;

            // �v���C���[���S����radius�������I�t�Z�b�g
            Vector3 offset = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0) * radius;
            Vector3 spawnPos = transform.position + offset;

            // �I�u�W�F�N�g����
            GameObject obj = Instantiate(healObjectPrefab, spawnPos, Quaternion.identity);

            // �����_���ȗ͂�������
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
        ResetMoveFlag();
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
        if (isDead) yield break ;
        isDead = true;
        if (CharacterManager.Instance.PlayerObjSelf == gameObject)
        {
            // �v���C���[���S�������Ƃ𓯊�
            if (RoomModel.Instance)
            {
                PlayerDeathResult result = new PlayerDeathResult();
                yield return RoomModel.Instance.PlayerDeadAsync().ToCoroutine(r => result = r);
                buckupHDMICnt = result.BuckupHDMICnt;
                if (!result.IsDead)
                {
                    hp = maxHp;
                    UIManager.Instance.UpdatePlayerStatus();
                    StartCoroutine(MakeInvincible(1.5f)); // ���G����
                    yield break;
                }
            }
            // �I�t���C�����̏���
            if (!RoomModel.Instance && buckupHDMICnt > 0)
            {   // �̗͉� & �c�@����
                hp = maxHp;
                UIManager.Instance.UpdatePlayerStatus();
                buckupHDMICnt--;
                StartCoroutine(MakeInvincible(1.5f)); // ���G����
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
    /// ���S���ɌĂяo��
    /// </summary>
    public void OnDead()
    {
        animator.SetInteger("animation_id", (int)ANIM_ID.Dead);
        canMove = false;
        invincible = true;
    }

    /// <summary>
    /// �_�b�V��(�u�����N)��������
    /// </summary>
    /// <returns></returns>
    protected IEnumerator BlinkCooldown()
    {
        // �u�����N�J�n
        animator.SetInteger("animation_id", (int)ANIM_ID.Blink);
        isBlinking = true;
        canBlink = false;
        invincible = true;
        yield return new WaitForSeconds(blinkTime);  // �u�����N����

        // �u�����N�I��
        canAttack = true;
        isBlinking = false;
        invincible = false;

        // �N�[���_�E������
        UIManager.Instance.DisplayCoolDown(false, blinkCoolDown);
        yield return new WaitForSeconds(blinkCoolDown);
        canBlink = true;
    }

    /// <summary>
    /// ���ꂷ�蔲������
    /// </summary>
    /// <returns></returns>
    protected IEnumerator ScaffoldDown()
    {
        yield return new WaitForSeconds(0.3f);
        m_IsScaffold = false;
        gameObject.layer = 20;
    }
    #endregion

    #region �O���Ăяo���֐�

    /// <summary>
    /// �_���[�W��^���鏈��
    /// </summary>
    abstract public void DoDashDamage();

    /// <summary>
    /// ����t���O�����Z�b�g
    /// </summary>
    abstract public void ResetFlag();

    /// <summary>
    /// ��_������
    /// (�m�b�N�o�b�N��pos�ɉ����ėL�����ς��)
    /// </summary>
    abstract public void ApplyDamage(int power, Vector3? position = null, KB_POW? kbPow = null, DEBUFF_TYPE? type = null);

    /// <summary>
    /// �u�����N�I������
    /// </summary>
    public void BlinkEnd()
    {
        animator.SetInteger("animation_id", (int)ANIM_ID.Run);
    }

    /// <summary>
    /// �߂��̃`�F�b�N�|�C���g�Ɉړ�
    /// </summary>
    public void MoveCheckPoint()
    {
        // HP��5%�_���[�W
        var damage = (int)((float)hp * 0.05);

        if (hp - damage <= 0) 
            hp = 1;
        else
            hp -= damage;

        UIManager.Instance.UpdatePlayerStatus();

        // �ړ�
        playerPos.position = FetchNearObjectWithTag("Gimmick/ChecKPoint").position;
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

        // �o���lUI�̍X�V
        UIManager.Instance.UpdateExperienceAndLevel();
    }

    /// <summary>
    /// �ڒn����擾
    /// </summary>
    /// <returns></returns>
    public bool GetGrounded() { return m_Grounded; }

    /// <summary>
    /// �����񕜈���~����
    /// </summary>
    /// <returns></returns>
    protected IEnumerator RegeneStop()
    {
        isRegene = false;

        yield return new WaitForSeconds(REGENE_STOP_TIME);

        isRegene = true;
    }

    /// <summary>
    /// ��Ԉُ�t�^���I����
    /// </summary>
    /// <returns></returns>
    public DEBUFF_TYPE[] LotteryDebuff()
    {
        List<DEBUFF_TYPE> debuffList = new List<DEBUFF_TYPE>();
        // �e��Ԉُ�̒��I
        float burnRand = UnityEngine.Random.Range(0f, 100f);
        if (burnRand <= giveDebuffRates[DEBUFF_TYPE.Burn]) debuffList.Add(DEBUFF_TYPE.Burn);

        float freezeRand = UnityEngine.Random.Range(0f, 100f);
        if (freezeRand <= giveDebuffRates[DEBUFF_TYPE.Freeze]) debuffList.Add(DEBUFF_TYPE.Freeze);

        float shockRand = UnityEngine.Random.Range(0f, 100f);
        if (shockRand <= giveDebuffRates[DEBUFF_TYPE.Shock]) debuffList.Add(DEBUFF_TYPE.Shock);

        return debuffList.ToArray();
    }

    /// <summary>
    /// �����b�N���ʒ��I����
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
            // �X�L���N�[���_�E���Z�k����
            Debug.Log("�X�L���N�[���_�E���Z�k����");
            return true;
        }
        else
        {
            // �X�L���N�[���_�E���Z�k���s
            return false;
        }
    }

    /// <summary>
    /// �G���j��HP�񕜏���
    /// </summary>
    public void KilledHPRegain()
    {
        HP += (int)(MaxHP * lifeScavengerRate);

        if (HP >= MaxHP)
            HP = MaxHP;
        UIManager.Instance.UpdatePlayerStatus();
    }

    /// <summary>
    /// ���݂̃����b�N�X�e�[�^�X�f�[�^���擾����
    /// </summary>
    /// <returns></returns>
    public PlayerRelicStatusData GetPlayerRelicStatusData()
    {
        // AddExpRate�Ȃ�
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
