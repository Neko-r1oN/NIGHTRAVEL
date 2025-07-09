//**************************************************
//  �G�l�~�[�̒��ۃN���X
//  Author:r-enomoto
//**************************************************
using Grpc.Core;
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

abstract public class EnemyBase : CharacterBase
{
    //  �}�l�[�W���[�N���X����Player���擾�ł���̂����z(�ϐ��폜�\��A�܂���SerializeField�폜�\��)
    #region �v���C���[�E�^�[�Q�b�g
    [Header("�v���C���[�E�^�[�Q�b�g")]
    protected GameObject target;   // SerializeField��Debug�p
    public GameObject Target { get { return target; } set { target = value; } }

    [SerializeField] List<GameObject> players = new List<GameObject>();
    public List<GameObject> Players { get { return players; } set { players = value; } }
    #endregion

    #region �R���|�[�l���g
    protected EnemyElite enemyElite;
    protected EnemySightChecker sightChecker;
    protected EnemyChaseAI chaseAI;
    protected Rigidbody2D m_rb2d;
    #endregion

    #region �e�N�X�`���E�A�j���[�V�����֘A
    [Foldout("�e�N�X�`���E�A�j���[�V����")]
    [SerializeField]
    List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    #endregion

    #region �`�F�b�N����
    protected LayerMask terrainLayerMask; // �ǂƒn�ʂ̃��C���[
    #endregion

    #region �X�e�[�^�X
    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected int bulletNum = 3;

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    [Tooltip("�e�̔��ˊԊu")]
    protected float shotsPerSecond = 0.5f;

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    [Tooltip("�U���̃N�[���^�C��")]
    protected float attackCoolTime = 0.5f;

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    [Tooltip("�U�����J�n���鋗��")]
    protected float attackDist = 1.5f;

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    [Tooltip("�ǐՉ\�͈�")]
    protected float trackingRange = 20f;
    
    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected float hitTime = 0.5f;

    [Foldout("�I�v�V����")]
    [SerializeField]
    protected bool isBoss = false;

    [Foldout("�I�v�V����")]
    [SerializeField]
    protected bool isElite = false;
    #endregion

    #region �I�v�V����
    [Foldout("�I�v�V����")]
    [Tooltip("�ڐG�Ń_���[�W��^���邱�Ƃ��\")]
    [SerializeField] 
    protected bool canDamageOnContact;

    [Foldout("�I�v�V����")]
    [Tooltip("��ɓ�����邱�Ƃ��\")]
    [SerializeField] 
    protected bool canPatrol;

    [Foldout("�I�v�V����")]
    [Tooltip("�^�[�Q�b�g��ǐՉ\")]
    [SerializeField] 
    protected bool canChaseTarget;
    
    [Foldout("�I�v�V����")]
    [Tooltip("�U���\")]
    [SerializeField]
    protected bool canAttack;
    
    [Foldout("�I�v�V����")]
    [Tooltip("�W�����v�\")]
    [SerializeField] 
    protected bool canJump;

    [Foldout("�I�v�V����")]
    [Tooltip("�U�����Ƀq�b�g�����ꍇ�A�U�����L�����Z���\")]
    [SerializeField]
    protected bool canCancelAttackOnHit = true;
    #endregion

    #region ��ԊǗ�
    protected List<Coroutine> cancellCoroutines = new List<Coroutine>();  // �q�b�g���ɃL�����Z������R���[�`��
    protected bool isStun;
    protected bool isInvincible;
    protected bool doOnceDecision;
    protected bool isAttacking;
    protected bool isDead;
    protected bool isPatrolPaused;
    protected bool isSpawn = true; // �X�|�[�������ǂ���
    protected bool isStartComp;
    #endregion

    #region �^�[�Q�b�g�Ƃ̋���
    protected float disToTarget;
    protected float disToTargetX;
    #endregion

    #region ���̑�
    [Foldout("���̑�")]
    [Tooltip("���������Ƃ��̒n�ʂ���̋���")]
    [SerializeField]
    float spawnGroundOffset;

    [Foldout("���̑�")]
    [Tooltip("�����`�悷�邩�ǂ���")]
    [SerializeField]
    protected bool canDrawRay = false;

    [Foldout("���̑�")]
    [SerializeField]
    protected int exp = 100;
    #endregion

    #region �O���Q�Ɨp�v���p�e�B

    public float AttackDist { get { return attackDist; } }

    public bool IsBoss { get { return isBoss; } set { isBoss = value; } }

    public bool IsElite { get { return isElite; } }

    public float SpawnGroundOffset { get { return spawnGroundOffset; } }

    public List<SpriteRenderer> SpriteRenderers { get { return spriteRenderers; } }
    #endregion

    protected virtual void Start()
    {
        terrainLayerMask = LayerMask.GetMask("Default");
        m_rb2d = GetComponent<Rigidbody2D>();
        sightChecker = GetComponent<EnemySightChecker>();
        chaseAI = GetComponent<EnemyChaseAI>();
        enemyElite = GetComponent<EnemyElite>();
        isStartComp = true;
    }

    protected virtual void FixedUpdate()
    {
        if (isSpawn || isStun || isAttacking || isInvincible || hp <= 0 || !doOnceDecision || !sightChecker) return;

        if (Players.Count > 0 && !target || Players.Count > 0 && target.GetComponent<CharacterBase>().HP <= 0)
        {
            // �V�����^�[�Q�b�g��T��
            target = sightChecker.GetTargetInSight();
        }
        else if (canChaseTarget && target && disToTarget > trackingRange || !canChaseTarget && target && !sightChecker.IsTargetVisible())
        {// �ǐՔ͈͊Oor�ǐՂ��Ȃ��ꍇ�͎������Ղ�ƃ^�[�Q�b�g��������
            target = null;
            if (chaseAI) chaseAI.Stop();
        }

        if (!target)
        {
            if (canPatrol && !isPatrolPaused) Patorol();
            else Idle();
            return;
        }

        // �^�[�Q�b�g�Ƃ̋������擾����
        disToTarget = Vector3.Distance(target.transform.position, this.transform.position);
        disToTargetX = target.transform.position.x - transform.position.x;

        // �^�[�Q�b�g�̂�������Ƀe�N�X�`���𔽓]
        if (canChaseTarget)
        {
            if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();
        }

        DecideBehavior();
    }

    /// <summary>
    /// �G��Ă����v���C���[�Ƀ_���[�W��K��������
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (canDamageOnContact && collision.gameObject.tag == "Player" && hp > 0 && !isInvincible)
        {
            if (!target)
            {
                // �^�[�Q�b�g��ݒ肵�A�^�[�Q�b�g�̕���������
                target = collision.gameObject;
                if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                    || target.transform.position.x > transform.position.x && transform.localScale.x < 0)
                {
                    Flip();
                }
                StartCoroutine(HitTime());
            }
            collision.gameObject.GetComponent<PlayerBase>().ApplyDamage(2, transform.position);
        }
    }

    /// <summary>
    /// �s���p�^�[�����s����
    /// </summary>
    abstract protected void DecideBehavior();

    /// <summary>
    /// �A�C�h������
    /// </summary>
    protected virtual void Idle() { }

    /// <summary>
    /// �����]��
    /// </summary>
    public void Flip()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// �G���[�g�̂ɂ��鏈��
    /// </summary>
    public void PromoteToElite(EnemyElite.ELITE_TYPE type)
    {
        if (!isElite)
        {
            isElite = true;
            if (!enemyElite) enemyElite = GetComponent<EnemyElite>();
            enemyElite.Init(type);
        }
    }

    /// <summary>
    /// ���̏�ɑҋ@���鏈��
    /// </summary>
    /// <param name="waitingTime"></param>
    /// <returns></returns>
    protected IEnumerator Waiting(float waitingTime)
    {
        doOnceDecision = false;
        Idle();
        yield return new WaitForSeconds(waitingTime);
        doOnceDecision = true;
    }

    /// <summary>
    /// �������Ă���v���C���[�̎擾����
    /// </summary>
    /// <returns></returns>
    protected List<GameObject> GetAlivePlayers()
    {
        List<GameObject> alivePlayers = new List<GameObject>();
        foreach (GameObject player in Players)
        {
            if (player && player.GetComponent<CharacterBase>().HP > 0)
            {
                alivePlayers.Add(player);
            }
        }
        return alivePlayers;
    }

    /// <summary>
    /// ��ԋ߂��v���C���[���擾����
    /// </summary>
    /// <returns></returns>
    public GameObject GetNearPlayer()
    {
        GameObject nearPlayer = null;
        float dist = float.MaxValue;
        foreach (GameObject player in GetAlivePlayers())
        {
            if (player != null)
            {
                float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
                if (Mathf.Abs(distToPlayer) < dist)
                {
                    nearPlayer = player;
                    dist = distToPlayer;
                }
            }
        }
        return nearPlayer;
    }

    /// <summary>
    /// ��ԋ߂��v���C���[���^�[�Q�b�g�ɐݒ肷��
    /// </summary>
    public void SetNearTarget()
    {
        GameObject target = GetNearPlayer();
        if (target != null)
        {
            this.target = target;
        }
    }

    #region �ړ������֘A

    /// <summary>
    /// �ǐՂ��鏈��
    /// </summary>
    protected virtual void Tracking() { }

    /// <summary>
    /// ���񂷂鏈��
    /// </summary>
    protected virtual void Patorol() { }

    #endregion

    #region �q�b�g�����֘A

    /// <summary>
    /// ���S���ɌĂ΂�鏈�� (��p�A�j���[�V�����Ȃ�)
    /// </summary>
    abstract protected void OnDead();

    /// <summary>
    /// �m�b�N�o�b�N����
    /// </summary>
    /// <param name="damage"></param>
    protected void DoKnokBack(int damage)
    {
        int direction = damage / Mathf.Abs(damage);
        transform.gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        transform.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction * 200f, 100f));
    }

    /// <summary>
    /// �_���[�W���󂯂��Ƃ��̏���
    /// </summary>
    protected virtual void OnHit()
    {
        isAttacking = false;
        doOnceDecision = true;
    }

    /// <summary>
    /// �_���[�W�K�p����
    /// </summary>
    /// <param name="damage"></param>
    public void ApplyDamage(int power, Transform attacker = null, params StatusEffectController.EFFECT_TYPE[] effectTypes)
    {
        if (isInvincible || isDead) return;

        var damage = CalculationLibrary.CalcDamage(power, Defense);
        var hitPoint = TransformUtils.GetHitPointToTarget(transform, attacker.position);
        if (hitPoint == null) hitPoint = transform.position;
        UIManager.Instance.PopDamageUI(damage, (Vector2)hitPoint, false);   // �_���[�W�\�L
        hp -= Mathf.Abs(damage);

        // �A�^�b�J�[����������Ƀe�N�X�`���𔽓]�����A�m�b�N�o�b�N��������
        if (attacker)
        {
            // �q�b�g���ɍU�������Ȃǂ��~����
            if (canCancelAttackOnHit)
            {
                foreach(Coroutine coroutine in cancellCoroutines)
                {
                    StopCoroutine(coroutine);
                }
            }

            // ��Ԉُ��t�^����
            if (effectTypes.Length > 0)
            {
                effectController.ApplyStatusEffect(effectTypes);
            }

            if (attacker.position.x < transform.position.x && transform.localScale.x > 0
            || attacker.position.x > transform.position.x && transform.localScale.x < 0) Flip();
            
            DoKnokBack(damage);

            if (hp > 0) StartCoroutine(HitTime());
        }

        if (hp <= 0)
        {
            // �S�ẴR���[�`��(�U��������X�^�������Ȃ�)���~����
            StopAllCoroutines();

            PlayerBase player = attacker ? attacker.gameObject.GetComponent<PlayerBase>() : null;
            StartCoroutine(DestroyEnemy(player));
        }
    }

    /// <summary>
    /// �_���[�W�K�����̖��G����
    /// </summary>
    /// <returns></returns>
    protected IEnumerator HitTime()
    {
        if (canCancelAttackOnHit)
        {
            isInvincible = true;
            OnHit();
            yield return new WaitForSeconds(hitTime);
            isInvincible = false;
        }
    }

    /// <summary>
    /// ���S����
    /// </summary>
    /// <returns></returns>
    protected IEnumerator DestroyEnemy(PlayerBase player)
    {
        if (!isDead)
        {
            isDead = true;
            if (player) player.GetExp(exp);
            OnDead();
            if (GameManager.Instance) GameManager.Instance.CrushEnemy(this);
            yield return new WaitForSeconds(0.25f);
            m_rb2d.excludeLayers = LayerMask.GetMask("BlinkPlayer") | LayerMask.GetMask("Player"); ;  // �v���C���[�Ƃ̔��������
            m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
    }

    #endregion

    #region �X�^�������֘A

    /// <summary>
    /// �X�^������
    /// </summary>
    /// <param name="time"></param>
    public void ApplyStun(float time)
    {
        if (!isStun)
        {
            StartCoroutine(StunTime(time));
        }
    }

    /// <summary>
    /// ��莞�ԃX�^�������鏈��
    /// </summary>
    /// <returns></returns>
    IEnumerator StunTime(float stunTime)
    {
        isStun = true;
        OnHit();
        yield return new WaitForSeconds(stunTime);
        isStun = false;
    }

    #endregion

    #region Debug�`�揈���֘A

    /// <summary>
    /// ���̌��m�͈͂̕`�揈��
    /// </summary>
    abstract protected void DrawDetectionGizmos();

    /// <summary>
    /// [ �f�o�b�N�p ] Gizmos���g�p���Č��o�͈͂�`��
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!canDrawRay) return;

        // �ǐՔ͈�
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, trackingRange);

        // �����`��
        if (sightChecker != null)
        {
            sightChecker.DrawSightLine(canChaseTarget);
        }

        DrawDetectionGizmos();
    }

    #endregion

    #region �e�N�X�`���E�A�j���[�V�����֘A

    /// <summary>
    /// �U���A�j���[�V�����̃C�x���g�ʒm�ōU�����������s����
    /// </summary>
    public virtual void OnAttackAnimEvent() { }

    /// <summary>
    /// �X�v���C�g�𓧖��ɂ���Ƃ��ɌĂ΂�鏈��
    /// </summary>
    protected virtual void OnTransparentSprites() { }

    /// <summary>
    /// �t�F�[�h�C�������������Ƃ��ɌĂ΂�鏈��
    /// </summary>
    protected virtual void OnFadeInComp() { }

    /// <summary>
    /// �X�v���C�g�𓧖��ɂ���
    /// </summary>
    public void TransparentSprites()
    {
        if (!isStartComp) Start();   // Start�����s����Ă��Ȃ��ꍇ
        isSpawn = false;
        isInvincible = true;    // ���G��Ԃɂ��� & �{���̍s���s��

        foreach (var spriteRenderer in spriteRenderers)
        {
            Color color = spriteRenderer.color;
            color.a = 0;
            spriteRenderer.color = color;
        }

        InvokeRepeating("FadeIn", 0, 0.1f);
        OnTransparentSprites();
    }

    /// <summary>
    /// �t�F�[�h�C������
    /// </summary>
    protected void FadeIn()
    {
        foreach (var spriteRenderer in spriteRenderers)
        {
            Color color = spriteRenderer.color;
            color.a += 0.2f;
            spriteRenderer.color = color;
        }

        // �t�F�[�h�C��������������A���G��ԉ���
        if (isInvincible && spriteRenderers[0].color.a >= 1)
        {
            isInvincible = false;
            OnFadeInComp();
            CancelInvoke("FadeIn");
        }
    }


    /// <summary>
    /// �X�|�[���A�j���[�V�������I�������Ƃ�
    /// </summary>
    public void OnEndSpawnAnim()
    {
        isSpawn = false;   // �s���ł���悤�ɂ���
    }

    /// <summary>
    /// �A�j���[�V�����ݒ菈��
    /// </summary>
    /// <param name="id"></param>
    public void SetAnimId(int id)
    {
        if (animator != null)
        {
            animator.SetInteger("animation_id", id);
        }
    }

    /// <summary>
    /// �A�j���[�V����ID�擾����
    /// </summary>
    /// <returns></returns>
    public int GetAnimId()
    {
        return animator != null ? animator.GetInteger("animation_id") : 0;
    }
    #endregion
}