//--------------------------------------------------------------
// �v���C���[�R���g���[���[ [ PlayerController2D.cs ]
// Author�FKenta Nakamoto
// ���p�Fhttps://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;

public class TestPlayerController : MonoBehaviour
{
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
        Dash,
        DBJump,
        WallSlide,
    }

    [Header("�X�e�[�^�X")]
    [Space]
    public float life = 10f;
    public float runSpeed = 40f;    // ���x�W��
    public float dmgValue = 4;		// �U����
    float horizontalMove = 0f;		// ���x�l
    [SerializeField] private float m_JumpForce = 400f;
    [SerializeField] private float m_DashForce = 25f;
    [SerializeField] private bool m_AirControl = false; // �󒆐���t���O
    [SerializeField] GameObject throwableObject;		// ��������
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;
    public bool canDoubleJump = true;
    public bool invincible = false; // �v���C���[�̎��S����t���O

    [Header("���C���[�֘A")]
    [Space]
    [SerializeField] private LayerMask m_WhatIsGround;	// �ǂ̃��C���[��n�ʂƔF�������邩
    [SerializeField] private Transform m_GroundCheck;	// �v���C���[���ڒn���Ă��邩�ǂ������m�F����p
    [SerializeField] private Transform m_WallCheck;     // �v���C���[���ǂɐG��Ă��邩�ǂ������m�F����p
    [SerializeField] private Transform attackCheck;		// �U�����̓����蔻��

    const float k_GroundedRadius = .2f; // �ڒn�m�F�p�̉~�̔��a
    private bool m_Grounded;			// �v���C���[�̐ڒn�t���O
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  // �v���C���[�̌����̔���t���O�itrue�ŉE�����j
    private Vector3 velocity = Vector3.zero;
    private float limitFallSpeed = 25f; // �������x�̐���

    private bool canMove = true;    // �v���C���[�̓��쐧��t���O
    private bool canDash = true;    // �_�b�V������t���O
    private bool canAttack = true;	// �U������t���O
    private bool m_IsWall = false;  // �v���C���[�̑O�ɕǂ����邩
    private bool isJump = false;	// �W�����v���̓t���O
    private bool isDash = false;	// �_�b�V�����̓t���O
    private bool isDashing = false;	// �v���C���[���_�b�V�������ǂ���
    private bool isWallSliding = false;	 //If player is sliding in a wall
    private bool oldWallSlidding = false;//If player is sliding in a wall in the previous frame
    private float prevVelocityX = 0f;
    private bool canCheck = false;  //For check if player is wallsliding

    private Animator animator;

    [Header("�p�[�e�B�N��")]
    [Space]
    public ParticleSystem particleJumpUp;
    public ParticleSystem particleJumpDown;

    [Header("�J����")]
    [Space]
    public GameObject cam;

    private float jumpWallStartX = 0;
    private float jumpWallDistX = 0;        // �v���C���[�ƕǂ̋���
    private bool limitVelOnWallJump = false;// ��fps�ŕǂ̃W�����v�����𐧌�����

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    /// <summary>
    /// Update�O����
    /// </summary>

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// �X�V����
    /// </summary>
    private void Update()
    {
        // �L�����̈ړ�
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Jump"))
        {   // �W�����v������
            isJump = true;
        }

        if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Dash"))
        {   // �u�����N������
            isDash = true;
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
            if (!wasGrounded)
            {   // �O�t���[���Œn�ʂɐG��Ă��Ȃ���
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
        // �ǔ���

        m_IsWall = false;

        if (!m_Grounded)
        {   // �󒆂ɋ���Ƃ�

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

        Move(horizontalMove * Time.fixedDeltaTime, isJump, isDash);
        isJump = false;
        isDash = false;
    }

    /// <summary>
    /// �ړ�����
    /// </summary>
    /// <param name="move">�ړ���</param>
    /// <param name="jump">�W�����v����</param>
    /// <param name="dash">�_�b�V������</param>
    public void Move(float move, bool jump, bool dash)
    {
        if (canMove)
        {
            //--------------------
            // �ړ� & �_�b�V��

            // �_�b�V������ & �_�b�V���\ & �ǂɐG��ĂȂ�
            if (dash && canDash && !isWallSliding)
            {
                //m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_DashForce, 0f));
                StartCoroutine(DashCooldown()); // Dash&�N�[���_�E��
            }
            // If crouching, check to see if the character can stand up
            // �_�b�V�����̏ꍇ
            if (isDashing)
            {   // �N�[���_�E���ɓ���܂ŉ���
                m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_DashForce, 0);
            }
            // �ڒn���Ă��邩�󒆐���ON�̎�
            else if (m_Grounded || m_AirControl)
            {
                // �������x��������
                if (m_Rigidbody2D.linearVelocity.y < -limitFallSpeed)
                    m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, -limitFallSpeed);

                // �L�����̖ڕW�ړ����x������
                Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.linearVelocity.y);

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
                if (!oldWallSlidding && m_Rigidbody2D.linearVelocity.y < 0 || isDashing)
                {   // �O�t���[���ŕǂɐG��Ă��Ȃ� && ���ɗ����Ă� or �_�b�V���\
                    isWallSliding = true;
                    m_WallCheck.localPosition = new Vector3(-m_WallCheck.localPosition.x, m_WallCheck.localPosition.y, 0);
                    Flip();
                    StartCoroutine(WaitToCheck(0.1f));
                    canDoubleJump = true;
                    animator.SetInteger("animation_id", (int)ANIM_ID.WallSlide);
                }
                isDashing = false;

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
            {   // �ǃX���C�hAnim�Đ��� && �O�ɕǂ����� && �󒆂ɋ���Ƃ�
                isWallSliding = false;
                oldWallSlidding = false;
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
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// �_���[�W��^���鏈��
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
                //++ GetComponent��Enemy�X�N���v�g���擾���AApplyDamage���Ăяo���悤�ɕύX
                collidersEnemies[i].gameObject.SendMessage("ApplyDamage", dmgValue);
                cam.GetComponent<MainCameraFollow>().ShakeCamera();
            }
        }
    }

    /// <summary>
    /// �_���[�W�󂯂鏈��
    /// </summary>
    /// <param name="damage">�_���[�W��</param>
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
    /// �_�b�V��(�u�����N)��������
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
    /// �_���[�W��d������
    /// </summary>
    IEnumerator Stun(float time)
    {
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
        yield return new WaitForSeconds(0.1f);
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
