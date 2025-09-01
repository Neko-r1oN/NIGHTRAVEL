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
using UnityEditor.Experimental.GraphView;

abstract public class EnemyBase : CharacterBase
{
    #region �v���C���[�E�^�[�Q�b�g
    [Header("�v���C���[�E�^�[�Q�b�g")]
    protected GameObject target;
    public GameObject Target { get { return target; } set { target = value; } }

    public List<GameObject> players = new List<GameObject>();
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
    private EnumManager.SPAWN_ENEMY_TYPE spawnEnemyType;

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

    #region �V�X�e��
    [Foldout("�V�X�e��")]
    [Tooltip("���������Ƃ��̒n�ʂ���̋���")]
    [SerializeField]
    float spawnGroundOffset;

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

    [Foldout("�V�X�e��")]
    [SerializeField]
    protected int spawnWeight = 1;  // �X�|�[���̒��I����ۂ̏d��

    [Foldout("�V�X�e��")]
    private Terminal terminalManager;
    #endregion

    #region �O���Q�Ɨp�v���p�e�B

    public int SpawnWeight { get { return spawnWeight; } }

    public int BaseExp { get { return baseExp; } }

    public int Exp { get { return exp; } set { exp = value; } }

    public float AttackDist { get { return attackDist; } }

    public float SpawnGroundOffset { get { return spawnGroundOffset; } }

    public bool IsBoss { get { return isBoss; } set { isBoss = value; } }

    public bool IsElite { get { return isElite; } }

    public List<SpriteRenderer> SpriteRenderers { get { return spriteRenderers; } }

    public EnumManager.SPAWN_ENEMY_TYPE SpawnEnemyType { get { return spawnEnemyType; } set { spawnEnemyType = value; } }

    public Terminal TerminalManager { get { return terminalManager; } set { terminalManager = value; } }
    #endregion

    #region ID
    int selfID;

    /// <summary>
    /// ���g�̃��j�[�N��ID
    /// </summary>
    public int SelfID { get { return selfID; } set { selfID = value; } }
    #endregion

    protected override void Start()
    {
        base.Start();
        players = new List<GameObject>(CharacterManager.Instance.PlayerObjs.Values);
        terrainLayerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Gimmick");
        m_rb2d = GetComponent<Rigidbody2D>();
        sightChecker = GetComponent<EnemySightChecker>();
        chaseAI = GetComponent<EnemyChaseAI>();
        enemyElite = GetComponent<EnemyElite>();
        isStartComp = true;
    }

    protected virtual void FixedUpdate()
    {
        if (isSpawn || isStun || isAttacking || isInvincible || hp <= 0 || !sightChecker) return;

        // �^�[�Q�b�g�����݂��Ȃ� || ���݂̃^�[�Q�b�g�����S���Ă���ꍇ
        if (Players.Count > 0 && !target || target && target.GetComponent<CharacterBase>().HP <= 0)
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
                    RemoveCoroutineByKey(key);
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
        float obstructionMaxTime = 3f;
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
            Coroutine waitCoroutine = StartCoroutine(Waiting(waitTime, () => { RemoveCoroutineByKey(key); }));
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
        transform.gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        transform.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(-transform.localScale.x * durationX, durationY));
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
    public virtual void ApplyDamageRequest(int power, GameObject attacker = null, bool drawDmgText = true, params DEBUFF_TYPE[] debuffList)
    {
        GameObject playerSelf = CharacterManager.Instance.PlayerObjSelf;
        if (isInvincible || isDead || attacker && attacker != playerSelf) return;

        // ���A���^�C�����A�����ɂ��U���̏ꍇ�̓_���[�W���������N�G�X�g
        if (RoomModel.Instance && attacker && attacker == playerSelf)
        {
            return;
        }
        // �M�~�b�N���Ԉُ�ɂ��_���[�W�̓}�X�^�N���C�A���g�̂ݏ���������
        else if (RoomModel.Instance && !RoomModel.Instance.IsMaster)
        {
            return;
        }


        // �ȍ~�̓��[�J�� || �M�~�b�N�p
        int damage = CalculationLibrary.CalcDamage(power, Defense);
        int remainingHp = this.hp - Mathf.Abs(damage);
        ApplyDamage(damage, remainingHp, attacker, drawDmgText, debuffList);
    }

    /// <summary>
    /// �_���[�W�K�p����
    /// </summary>
    /// <param name="damegeData"></param>
    public void ApplyDamage(int damage, int remainingHP, GameObject attacker = null, bool drawDmgText = true, params DEBUFF_TYPE[] debuffList)
    {
        if (isInvincible || isDead) return;
        var charaManager = CharacterManager.Instance;

        // �_���[�W�K�p�A�_���[�W�\�L
        Vector3? attackerPos = null;
        if (attacker != null) attackerPos = attacker.transform.position;
        if (drawDmgText && !isInvincible) DrawHitDamageUI(damage, attackerPos);
        hp -= Mathf.Abs(damage);

        // ��Ԉُ��t�^����
        if (debuffList.Length > 0) effectController.ApplyStatusEffect(debuffList);

        // �A�^�b�J�[����������Ƀe�N�X�`���𔽓]�����A�m�b�N�o�b�N��������
        if (attackerPos != null)
        {
            Vector3 pos = (Vector3)attackerPos;
            if (pos.x < transform.position.x && transform.localScale.x > 0
            || pos.x > transform.position.x && transform.localScale.x < 0) Flip();

            DoKnokBack();

            if (hp > 0 && canCancelAttackOnHit) ApplyStun(hitTime);
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
                // �|�����̂��������g�̏ꍇ�͌o���l��^����
                if (!RoomModel.Instance || RoomModel.Instance && CharacterManager.Instance.PlayerObjSelf == player)
                    player.GetExp(exp);
            }

            if (spawnEnemyType == SPAWN_ENEMY_TYPE.ByTerminal)
            {// �����^�C�v���^�[�~�i���Ȃ�
                // ���񂾓G�����X�g����폜
                terminalManager.TerminalSpawnList.Remove(this.gameObject);
                if(terminalManager.TerminalSpawnList.Count <= 0)
                {// ���X�g�̃J�E���g��0�Ȃ�
                    // �����b�N�̐���
                    RelicManager.Instance.GenerateRelicTest();
                }
            }
            else
            {
                // Instance������Ȃ�G���j�֐����Ă�
                if (GameManager.Instance) GameManager.Instance.CrushEnemy(this);
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

    /// <summary>
    /// �U���A�j���[�V�����̃C�x���g�ʒm����
    /// </summary>
    public virtual void OnAttackAnimEvent() { }

    /// <summary>
    /// �U���̃A�j���[�V�����I�����̃C�x���g�ʒm����
    /// </summary>
    public virtual void OnEndAttackAnimEvent() { }

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
    protected void RemoveCoroutineByKey(string key)
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
                if (coroutine != null) StopCoroutine(coroutine);
            }
        }

        managedCoroutines.Clear();
    }
    #endregion

    #region ���A���^�C�������֘A

    /// <summary>
    /// �}�X�^�N���C�A���g�؂�ւ����ɏ�Ԃ����Z�b�g����
    /// </summary>
    abstract public void ResetAllStates();

    /// <summary>
    /// EnemyData���쐬����
    /// </summary>
    /// <param name="enemyData">�^���w��</param>
    /// <returns></returns>
    protected EnemyData CreateEnemyData(EnemyData enemyData)
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
        enemyData.EnemyID = this.SelfID;
        enemyData.EnemyName = this.gameObject.name;
        enemyData.isBoss = this.IsBoss;
        Exp = this.Exp;

        return enemyData;
    }

    /// <summary>
    /// EnemyData�擾����
    /// </summary>
    /// <returns></returns>
    public virtual EnemyData GetEnemyData()
    {
        return CreateEnemyData(new EnemyData());
    }

    /// <summary>
    /// �G�̓��������X�V����
    /// </summary>
    /// <param name="enemyData"></param>
    public virtual void UpdateEnemy(EnemyData enemyData)
    {
        selfID = enemyData.EnemyID;
        gameObject.name = enemyData.EnemyName;
        isBoss = enemyData.isBoss;
        exp = enemyData.Exp;
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
            sightChecker.DrawSightLine(canChaseTarget, target, players);
        }

        DrawDetectionGizmos();
    }

    #endregion
}