//**************************************************
//  �G�l�~�[�̒��ۃN���X
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared.Interfaces.StreamingHubs;
using static Shared.Interfaces.StreamingHubs.EnumManager;

abstract public class EnemyBase : CharacterBase
{
    #region �f�[�^�֘A

    [Foldout("�f�[�^�֘A")]
    [SerializeField]
    [Tooltip("�G�l�~�[���ID")]
    protected ENEMY_TYPE enemyTypeId;    // ���g�̃G�l�~�[���ID�@(DB�̃��R�[�h�ƕR�Â���)
    
    [Foldout("�f�[�^�֘A")]
    [SerializeField]
    [Tooltip("�������ꂽ�Ƃ��̎��ʗpID")]
    protected string uniqueId = "";    // �������ꂽ�Ƃ��̎��ʗpID  �����̓G�Əd�����Ȃ��悤����

    #endregion

    #region �v���C���[�E�^�[�Q�b�g
    [Header("�v���C���[�E�^�[�Q�b�g")]
    protected GameObject target;
    public GameObject Target { get { return target; } set { target = value; } }

    protected CharacterManager characterManager;
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
    float destroyWaitSec = 1f;

    [Foldout("�e�N�X�`���E�A�j���[�V����")]
    [SerializeField]
    List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    #endregion

    #region �`�F�b�N����
    protected LayerMask terrainLayerMask; // �ǂƒn�ʂ̃��C���[
    public LayerMask TerrainLayerMask { get { return terrainLayerMask; } }
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
    protected float hitTime = 0.5f;

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    [Tooltip("�^�[�Q�b�g���������܂łɕK�v�ȕb��")]
    protected float obstructionMaxTime = 3f;
    #endregion

    #region �I�v�V����
    [Foldout("�I�v�V����")]
    [SerializeField]
    protected bool isBoss = false;

    [Foldout("�I�v�V����")]
    [SerializeField]
    protected bool isElite = false;

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
    protected bool canCancelAttackOnHit = false;

    [Foldout("�I�v�V����")]
    [Tooltip("DeadZone�Ƃ̐ڐG������������Ƃ��\")]
    [SerializeField]
    protected bool canIgnoreDeadZoneCollision = false;
    #endregion

    #region ��ԊǗ�
    protected Dictionary<string,Coroutine> managedCoroutines = new Dictionary<string,Coroutine>();  // �Ǘ����Ă���R���[�`��
    protected bool isStun;
    protected bool isInvincible = true;
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

    #region �X�|�[���֘A
    [Foldout("�X�|�[���֘A")]
    [Tooltip("���������Ƃ��̒n�ʂ���̋���")]
    [SerializeField]
    protected float spawnGroundOffset;

    [Foldout("�X�|�[���֘A")]
    [SerializeField]
    protected int spawnWeight = 1;  // �X�|�[���̒��I����ۂ̏d��
    #endregion

    #region �V�X�e��

    [Foldout("�V�X�e��")]
    [Tooltip("�����`�悷�邩�ǂ���")]
    [SerializeField]
    protected bool canDrawRay = false;
    public bool CanDrawRay { get { return canDrawRay; } }

    [Foldout("�V�X�e��")]
    [SerializeField]
    protected int baseExp = 100;    // �����̊l���\�o���l��

    [Foldout("�V�X�e��")]
    [SerializeField]
    protected int exp = 100;    // ���݂̊l���\�o���l��
    #endregion

    #region �O���Q�Ɨp�v���p�e�B

    public ENEMY_TYPE EnemyTypeId { get { return enemyTypeId; } set { enemyTypeId = value; } }

    public string UniqueId { get { return uniqueId; } set { uniqueId = value; } }

    public int SpawnWeight { get { return spawnWeight; } }

    public int BaseExp { get { return baseExp; } }

    public int Exp { get { return exp; } set { exp = value; } }

    public float AttackDist { get { return attackDist; } }

    public float SpawnGroundOffset { get { return spawnGroundOffset; } }

    public bool IsBoss { get { return isBoss; } set { isBoss = value; } }

    public bool IsElite { get { return isElite; } }

    public List<SpriteRenderer> SpriteRenderers { get { return spriteRenderers; } }
    #endregion

    #region �萔
    private const int MAX_DAMAGE = 99999; // �ő�_���[�W��
    #endregion

    protected virtual void OnEnable()
    {
        if (!isStartComp || hp <= 0 || isDead) return;
        ResetAllStates();
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        characterManager = CharacterManager.Instance;
        terrainLayerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Gimmick");
        m_rb2d = GetComponent<Rigidbody2D>();
        sightChecker = GetComponent<EnemySightChecker>();
        chaseAI = GetComponent<EnemyChaseAI>();
        enemyElite = GetComponent<EnemyElite>();
        isStartComp = true;
        base.Start();
    }

    protected virtual void FixedUpdate()
    {
        if (target)
        {
            // �^�[�Q�b�g�Ƃ̋������擾����
            disToTarget = Vector3.Distance(target.transform.position, this.transform.position);
            disToTargetX = MathF.Abs(target.transform.position.x - transform.position.x);
        }
        else
        {
            disToTarget = float.MaxValue;
            disToTargetX = float.MaxValue;
        }

        if (isSpawn || isStun || isAttacking || isInvincible || hp <= 0 || !sightChecker) return;

        // �^�[�Q�b�g�����݂��Ȃ� || ���݂̃^�[�Q�b�g�����S���Ă���ꍇ
        if (characterManager.PlayerObjs.Count > 0 && !target || target && target.GetComponent<CharacterBase>().HP <= 0)
        {
            // �V�����^�[�Q�b�g��T��
            target = sightChecker.GetTargetInSight();
        }

        if (target)
        {
            // ���s���łȂ���΁A�^�[�Q�b�g���Ď�����R���[�`�����J�n
            string key = "CheckTargetObstructionCoroutine";
            if (!ContaintsManagedCoroutine(key))
            {
                Coroutine coroutine = StartCoroutine(CheckTargetObstructionCoroutine(() => {
                    RemoveAndStopCoroutineByKey(key);
                }));
                managedCoroutines.Add(key, coroutine);
            }
        }
        else
        {
            if (canPatrol && !isPatrolPaused) Patorol();
            else Idle();
            return;
        }

        // �^�[�Q�b�g�̂�������Ƀe�N�X�`���𔽓]
        if (canChaseTarget)
        {
            if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();
        }

        DecideBehavior();
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
    /// ���̏�ɑҋ@���鏈��
    /// </summary>
    /// <param name="waitingTime"></param>
    /// <returns></returns>
    protected IEnumerator Waiting(float waitingTime, Action onFinished)
    {
        doOnceDecision = false;
        Idle();
        yield return new WaitForSeconds(waitingTime);
        doOnceDecision = true;
        onFinished?.Invoke();
    }

    #region �v���C���[�E�^�[�Q�b�g�֘A

    /// <summary>
    /// �^�[�Q�b�g�Ƃ̊ԂɎՕ��������邩���Ď���������R���[�`��
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckTargetObstructionCoroutine(Action onFinished)
    {
        float currentTime = 0;
        float waitSec = 0.01f;

        // obstructionMaxTime�ȏ�o�߂Ń^�[�Q�b�g�������������Ƃɂ���
        while (currentTime < obstructionMaxTime)
        {
            yield return new WaitForSeconds(waitSec);

            if (sightChecker.IsObstructed() || !sightChecker.IsTargetVisible())
            {
                currentTime += waitSec;
            }
            else
            {
                currentTime = 0;
            }        
        }

        if (target && currentTime >= obstructionMaxTime)
        {
            target = null;
            if (chaseAI) chaseAI.Stop();
        }

        // ���s���łȂ���΁A���̏�ɑҋ@����R���[�`�����J�n
        string key = "Waiting";
        float waitTime = 2f;
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine waitCoroutine = StartCoroutine(Waiting(waitTime, () => { RemoveAndStopCoroutineByKey(key); }));
            managedCoroutines.Add(key, waitCoroutine);
        }

        onFinished?.Invoke();
    }

    /// <summary>
    /// �������Ă���v���C���[�̎擾����
    /// </summary>
    /// <returns></returns>
    protected List<GameObject> GetAlivePlayers()
    {
        List<GameObject> alivePlayers = new List<GameObject>();
        foreach (GameObject player in characterManager.PlayerObjs.Values)
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
    public GameObject GetNearPlayer(Vector3? offset = null)
    {
        Vector3 point = transform.position;
        if (offset != null) point += (Vector3)offset;
        GameObject nearPlayer = null;
        float dist = float.MaxValue;
        foreach (GameObject player in GetAlivePlayers())
        {
            if (player != null)
            {
                float distToPlayer = Vector2.Distance(point, player.transform.position);
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
    public void SetNearTarget(Vector3? offset = null)
    {
        GameObject target = GetNearPlayer(offset);
        if (target != null)
        {
            this.target = target;
        }
    }

    #endregion

    #region �G���[�g�́E��Ԉُ�֘A
    /// <summary>
    /// �G���[�g�̂ɂ��鏈��
    /// </summary>
    public void PromoteToElite(EnumManager.ENEMY_ELITE_TYPE type)
    {
        if (!isElite && type != ENEMY_ELITE_TYPE.None)
        {
            isElite = true;
            if (!enemyElite) enemyElite = GetComponent<EnemyElite>();
            enemyElite.Init(type);
        }
    }

    /// <summary>
    /// �K�p�������Ԉُ�̎�ނ��擾����
    /// </summary>
    public DEBUFF_TYPE? GetStatusEffectToApply()
    {
        bool isElite = this.isElite && enemyElite != null;
        DEBUFF_TYPE? applyEffect = null;
        if (isElite)
        {
            applyEffect = enemyElite.GetAddStatusEffectEnum();
        }
        return applyEffect;
    }
    #endregion

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
    protected void DoKnokBack()
    {
        float durationX = 130f;
        float durationY = 90f;
        m_rb2d.linearVelocity = Vector2.zero;
        m_rb2d.AddForce(new Vector2(-transform.localScale.x * durationX, durationY));
    }

    /// <summary>
    /// �_���[�W���󂯂��Ƃ��̏���
    /// </summary>
    protected virtual void OnHit()
    {
        isAttacking = false;
        doOnceDecision = true;

        // �q�b�g���ɍU�������Ȃǂ��~����
        if (canCancelAttackOnHit)
        {
            StopAllManagedCoroutines();
        }
    }

    /// <summary>
    /// ��_���[�W��\������
    /// </summary>
    protected void DrawHitDamageUI(int damage, Vector3? attackerPos = null)
    {
        // ��_���[�W�ʂ�UI��\������
        var attackerPoint = attackerPos != null ? (Vector3)attackerPos : transform.position;
        var hitPoint = TransformUtils.GetHitPointToTarget(transform, attackerPoint);
        if (hitPoint == null) hitPoint = transform.position;
        UIManager.Instance.PopDamageUI(damage, (Vector2)hitPoint, false);   // �_���[�W�\�L
    }

    /// <summary>
    /// �_���[�W�K�p���N�G�X�g
    /// </summary>
    /// <param name="damage"></param>
    async public virtual void ApplyDamageRequest(int power, GameObject attacker = null, bool isKnokBack = true, bool drawDmgText = true, params DEBUFF_TYPE[] debuffList)
    {
        GameObject playerSelf = CharacterManager.Instance.PlayerObjSelf;
        if (isInvincible || isDead || attacker && attacker != playerSelf) return;

        #region ���A���^�C�������p
        if (RoomModel.Instance)
        {
            if (attacker && attacker == playerSelf)
            {
                // �v���C���[�ɂ��_���[�W�K�p
                await RoomModel.Instance.EnemyHealthAsync(uniqueId, power, new List<DEBUFF_TYPE>(debuffList));
            }
            else if (RoomModel.Instance.IsMaster)
            {
                // �M�~�b�N���Ԉُ�ɂ��_���[�W�K�p(�}�X�^�N���C�A���g�̂݃��N�G�X�g�\)
                // await RoomModel.Instance.ApplyDamageToEnemyAsync();
            }
            return;
        }
        #endregion

        // �ȍ~�̓��[�J�� || �M�~�b�N�p

        PlayerBase plBase = null;
        int damage = 0;

        if (attacker != null && attacker.tag == "Player")
        {
            plBase = attacker.GetComponent<PlayerBase>();

            damage = CalculationLibrary.CalcDamage(power, Defense - (int)(Defense * plBase.PierceRate));   // �ђʗ��K�p

            //----------------
            // �����b�N�K�p

            // ���g���f�o�t���󂯂Ă��� + �����b�N�u����AI�v���L���A�_���[�W������
            var debuffController = GetComponent<DebuffController>();
            if (debuffController.GetAppliedStatusEffects().Count != 0) damage = (int)(damage * plBase.DebuffDmgRate);

            // �����b�N�u���Q�C���R�[�h�v���L���A�^�_���[�W�̈ꕔ��HP��
            if (plBase.DmgHealRate >= 0) plBase.HP += (int)(damage * plBase.DmgHealRate);

            // �����b�N�u�C���[�K���X�N���v�g�v�K�p���A�_���[�W��99999�ɂ���
            damage = (plBase.LotteryRelic(RELIC_TYPE.IllegalScript)) ? MAX_DAMAGE : damage;
        }
        else
        {
            // �M�~�b�N�Ȃǂɂ��_���[�W
            damage = power;
        }

        // �_���[�W�K�p
        int remainingHp = this.hp - Mathf.Abs(damage);
        ApplyDamage(damage, remainingHp, attacker, isKnokBack, drawDmgText, debuffList);
    }

    /// <summary>
    /// �_���[�W�K�p����
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="remainingHP"></param>
    /// <param name="attacker"></param>
    /// <param name="drawDmgText"></param>
    /// <param name="debuffList"></param>
    public void ApplyDamage(int damage, int remainingHP, GameObject attacker = null, bool isKnokBack = true, bool drawDmgText = true, params DEBUFF_TYPE[] debuffList)
    {
        if (isInvincible || isDead) return;
        var charaManager = CharacterManager.Instance;

        // �_���[�W�K�p�A�_���[�W�\�L
        Vector3? attackerPos = null;
        if (attacker != null) attackerPos = attacker.transform.position;
        if (drawDmgText && !isInvincible) DrawHitDamageUI(damage, attackerPos);
        hp = remainingHP;

        // ��Ԉُ��t�^����
        if (debuffList.Length > 0) effectController.ApplyStatusEffect(debuffList);

        // �A�^�b�J�[����������Ƀe�N�X�`���𔽓]�����A�m�b�N�o�b�N��������
        if (isKnokBack && attackerPos != null)
        {
            Vector3 pos = (Vector3)attackerPos;
            if (pos.x < transform.position.x && transform.localScale.x > 0
            || pos.x > transform.position.x && transform.localScale.x < 0) Flip();

            if (hp > 0 && canCancelAttackOnHit) ApplyStun(hitTime);

            if(canCancelAttackOnHit) DoKnokBack();
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
    /// ���S����
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator DestroyEnemy(PlayerBase player)
    {
        if (!isDead)
        {
            isDead = true;
            OnDead();
            if (player)
            {
                player.KilledHPRegain();
                if (!RoomModel.Instance)
                {
                    player.GetExp(exp);
                }
            }

            if (CharacterManager.Instance.Enemies[uniqueId].SpawnType == SPAWN_ENEMY_TYPE.ByTerminal)
            {   // �����^�C�v���^�[�~�i���Ȃ�
                var termId = CharacterManager.Instance.Enemies[uniqueId].TerminalID;    // �[��ID��ۊ�
                CharacterManager.Instance.RemoveEnemyFromList(uniqueId);                // ���X�g����폜
                
                if(CharacterManager.Instance.GetEnemysByTerminalID(termId).Count == 0)
                {   // �����[���̓G���S���|���ꂽ���V�o��
                    TerminalManager.Instance.DropRelic(termId);
                }
            }
            else
            {
                // Instance������Ȃ�G���j�֐����Ă�
                if (GameManager.Instance) GameManager.Instance.CrushEnemy(this);

                CharacterManager.Instance.RemoveEnemyFromList(uniqueId);
            }

            m_rb2d.excludeLayers = LayerMask.GetMask("BlinkPlayer") | LayerMask.GetMask("Player"); ;  // �v���C���[�Ƃ̔��������
            yield return new WaitForSeconds(destroyWaitSec);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �g���K�[�ڐG����
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �͈͊O�ɂł���j������
        if (!canIgnoreDeadZoneCollision && collision.gameObject.tag == "Gimmick/Abyss")
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region �X�^�������֘A

    /// <summary>
    /// �X�^������
    /// </summary>
    /// <param name="time"></param>
    public void ApplyStun(float time, bool isHit = true)
    {
        isStun = true;
        if(isHit) OnHit();
        Coroutine coroutine = StartCoroutine(StunTime(time));
        managedCoroutines.Add("StunTime", coroutine);
    }

    /// <summary>
    /// ��莞�ԃX�^�������鏈��
    /// </summary>
    /// <returns></returns>
    IEnumerator StunTime(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        isStun = false;
    }

    #endregion

    #region �e�N�X�`���E�A�j���[�V�����֘A

    #region �U���p�^�[���P

    /// <summary>
    /// �U���A�j���[�V�����̃C�x���g�ʒm����
    /// </summary>
    public virtual void OnAttackAnimEvent() { }

    /// <summary>
    /// �U���̃A�j���[�V�����I�����̃C�x���g�ʒm����
    /// </summary>
    public virtual void OnEndAttackAnimEvent() { }

    #endregion

    #region �U���p�^�[���Q

    /// <summary>
    /// �U���A�j���[�V�����̃C�x���g�ʒm����
    /// </summary>
    public virtual void OnAttackAnim2Event() { }

    /// <summary>
    /// �U���̃A�j���[�V�����I�����̃C�x���g�ʒm����
    /// </summary>
    public virtual void OnEndAttackAnim2Event() { }

    #endregion

    #region �U���p�^�[���R

    /// <summary>
    /// �U���A�j���[�V�����̃C�x���g�ʒm����
    /// </summary>
    public virtual void OnAttackAnim3Event() { }

    /// <summary>
    /// �U���̃A�j���[�V�����I�����̃C�x���g�ʒm����
    /// </summary>
    public virtual void OnEndAttackAnim3Event() { }

    #endregion

    #region �U���p�^�[���S

    /// <summary>
    /// �U���A�j���[�V�����̃C�x���g�ʒm����
    /// </summary>
    public virtual void OnAttackAnim4Event() { }

    /// <summary>
    /// �U���̃A�j���[�V�����I�����̃C�x���g�ʒm����
    /// </summary>
    public virtual void OnEndAttackAnim4Event() { }

    #endregion

    /// <summary>
    /// �ړ�����A�j���[�V�����C�x���g�ʒm
    /// </summary>
    public virtual void OnMoveAnimEvent() { }

    /// <summary>
    /// �ړ��I���A�j���[�V�����C�x���g�ʒm
    /// </summary>
    public virtual void OnEndMoveAnimEvent() { }

    /// <summary>
    /// �X�|�[���A�j�����[�V�����J�n��
    /// </summary>
    public virtual void OnSpawnAnimEvent()
    {
        if (!isStartComp) Start();
        isSpawn = false;
        isInvincible = true;    // ���G��Ԃɂ��� & �{���̍s���s��
    }

    /// <summary>
    /// �X�|�[���A�j���[�V�������I�������Ƃ�
    /// </summary>
    public virtual void OnEndSpawnAnimEvent()
    {
        isInvincible = false;
        isSpawn = false;
    }
    #endregion

    #region �R���[�`���Ǘ��֘A

    /// <summary>
    /// �Ǘ����Ă�����̂̒��ŁA�N�����̃R���[�`������������
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected bool ContaintsManagedCoroutine(string key)
    {
        bool isHit = false;
        if (managedCoroutines.Count > 0 && managedCoroutines.ContainsKey(key))
        {
            if (managedCoroutines[key] == null)
            {
                managedCoroutines.Remove(key);
            }
            else
            {
                isHit = true;
            }            
        }
        return isHit;
    }

    /// <summary>
    /// �w�肵��key�̃R���[�`���̗v�f���폜
    /// </summary>
    /// <param name="key"></param>
    protected void RemoveAndStopCoroutineByKey(string key)
    {
        if (ContaintsManagedCoroutine(key))
        {
            if (managedCoroutines[key] != null) StopCoroutine(managedCoroutines[key]);
            managedCoroutines.Remove(key);
        }
    }

    /// <summary>
    /// �Ǘ����Ă���S�ẴR���[�`�����~����
    /// </summary>
    protected void StopAllManagedCoroutines()
    {
        if (managedCoroutines.Count > 0)
        {
            foreach (var coroutine in managedCoroutines.Values)
            {
                if(coroutine != null) StopCoroutine(coroutine);
            }
        }

        managedCoroutines.Clear();
    }
    #endregion

    #region ���A���^�C�������֘A

    /// <summary>
    /// �}�X�^�N���C�A���g�؂�ւ����ɏ�Ԃ����Z�b�g����
    /// </summary>
    public virtual void ResetAllStates()
    {
        StopAllManagedCoroutines();
        isAttacking = false;
        isStun = false;
        isInvincible = false;
        doOnceDecision = true;
        isPatrolPaused = false;
        m_rb2d.bodyType = defaultType2D;
    }

    /// <summary>
    /// EnemyData���쐬����
    /// </summary>
    /// <param name="enemyData">�^���w��</param>
    /// <returns></returns>
    protected EnemyData SetEnemyData(EnemyData enemyData)
    {
        var debuffController = GetComponent<DebuffController>();

        enemyData.IsActiveSelf = this.gameObject.activeInHierarchy;
        enemyData.Status = new CharacterStatusData(
            hp: this.MaxHP,
            defence: this.MaxDefence,
            power: this.MaxPower,
            moveSpeed: this.MaxMoveSpeed,
            attackSpeedFactor: this.MaxAttackSpeedFactor,
            jumpPower: this.MaxJumpPower
        );
        enemyData.State = new CharacterStatusData(
            hp: this.HP,
            defence: this.defense,
            power: this.power,
            moveSpeed: this.moveSpeed,
            attackSpeedFactor: this.attackSpeedFactor,
            jumpPower: this.jumpPower
        );
        enemyData.Position = this.transform.position;
        enemyData.Scale = this.transform.localScale;
        enemyData.Rotation = this.transform.rotation;
        enemyData.AnimationId = this.GetAnimId();
        enemyData.DebuffList = debuffController.GetAppliedStatusEffects();

        // �ȉ��͐�p�v���p�e�B
        enemyData.UniqueId = this.UniqueId;
        enemyData.EnemyName = this.gameObject.name;
        enemyData.isBoss = this.IsBoss;
        enemyData.IsInvincible = this.isInvincible;
        Exp = this.Exp;

        return enemyData;
    }

    /// <summary>
    /// EnemyData�擾����
    /// </summary>
    /// <returns></returns>
    public virtual EnemyData GetEnemyData()
    {
        return SetEnemyData(new EnemyData());
    }

    /// <summary>
    /// �G�̓��������X�V����
    /// </summary>
    /// <param name="enemyData"></param>
    public virtual void UpdateEnemy(EnemyData enemyData)
    {
        uniqueId = enemyData.UniqueId;
        gameObject.name = enemyData.EnemyName;
        isBoss = enemyData.isBoss;
        isInvincible = enemyData.IsInvincible;
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

        // �����`��
        if (sightChecker != null)
        {
            sightChecker.DrawSightLine(canChaseTarget, target);
        }

        DrawDetectionGizmos();
    }

    #endregion
}