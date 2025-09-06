//**************************************************
//  �L�����N�^�[�̒��ۃN���X
//  Author:r-enomoto
//**************************************************
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using Shared.Interfaces.StreamingHubs;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using static UnityEditor.ShaderGraph.Internal.Texture2DShaderProperty;

abstract public class CharacterBase : MonoBehaviour
{
    //--------------------------------------------------------------
    // �t�B�[���h

    #region �f�[�^�֘A
    protected Guid uniqueId;    // �������ꂽ�Ƃ��̎��ʗpID
    public Guid UniqueId { get { return uniqueId; } set { uniqueId = value; } }
    #endregion

    #region �����X�e�[�^�X�֘A
    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected int baseHp = 10;   // HP

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected int baseDefense = 10;  // �h���

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected int basePower = 10;    // �U����

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected float baseJumpPower = 10;  // ������

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected float baseMoveSpeed = 1f;   // �ړ����x

    [Foldout("�X�e�[�^�X")]
    protected float baseAttackSpeedFactor = 1f;    // �U�����x(Animator�̌W��)

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected float baseHealRate = 0.001f;  // �����񕜔{�̔{��
    #endregion

    #region �����X�e�[�^�X�O���Q�Ɨp�v���p�e�B
    /// <summary>
    /// �̗�
    /// </summary>
    public int BaseHP { get { return baseHp; } }

    /// <summary>
    /// �h���
    /// </summary>
    public int BaseDefense { get { return baseDefense; } }

    /// <summary>
    /// �U����
    /// </summary>
    public int BasePower { get { return basePower; } }

    /// <summary>
    /// ������
    /// </summary>
    public float BaseJumpPower { get { return baseJumpPower; } }

    /// <summary>
    /// �ړ����x
    /// </summary>
    public float BaseMoveSpeed { get { return baseMoveSpeed; } }

    /// <summary>
    /// �U�����x(Animator�̌W��)
    /// </summary>
    public float BaseAttackSpeedFactor { get { return baseAttackSpeedFactor; } }

    /// <summary>
    /// �����񕜂̔{��
    /// </summary>
    public float BaseHealRate {  get { return baseHealRate; } }
    #endregion
        
    #region ���݂̃X�e�[�^�X�̏���l�֘A
    public int maxHp;
    public int maxDefense;
    public int maxPower;
    public float maxJumpPower;
    public float maxMoveSpeed;
    public float maxAttackSpeedFactor;
    public float maxHealRate;
    #endregion

    #region ���݂̃X�e�[�^�X�̏���l�O���Q�Ɨp�v���p�e�B
    /// <summary>
    /// �ő�̗�
    /// </summary>
    public int MaxHP { get { return maxHp; } }

    public int MaxDefence { get { return maxDefense; } }

    /// <summary>
    /// �ő�U����
    /// </summary>
    public int MaxPower { get { return maxPower; } }

    /// <summary>
    /// �ő咵����
    /// </summary>
    public float MaxJumpPower { get { return maxJumpPower; } }

    /// <summary>
    /// �ő�ړ����x
    /// </summary>
    public float MaxMoveSpeed { get { return maxMoveSpeed; } }

    /// <summary>
    /// �ő�U�����x�W��(Animator�p)
    /// </summary>
    public float MaxAttackSpeedFactor { get { return maxAttackSpeedFactor; } }

    /// <summary>
    /// �ő厩���񕜂̔{��
    /// </summary>
    public float MaxHealRate { get {return maxHealRate; } }
    #endregion

    #region ���݂̃X�e�[�^�X�֘A
    // �C���X�y�N�^�[��Ō��邽�߂�public
    // ���protected�ɂ���
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public int hp;
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public int defense;
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public int power;
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public float jumpPower;
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public float moveSpeed;
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public float attackSpeedFactor;
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public float healRate;

    protected DebuffController effectController;
    #endregion

    #region �X�e�[�^�X�O���Q�Ɨp�v���p�e�B
    /// <summary>
    /// �̗�
    /// </summary>
    public int HP { get { return hp; } set { hp = value; } }

    /// <summary>
    /// �h���
    /// </summary>
    public int Defense { get { return defense; } set { defense = value; } }

    /// <summary>
    /// �U����
    /// </summary>
    public int Power { get { return power; } set { power = value; } }

    /// <summary>
    /// ������
    /// </summary>
    public float JumpPower { get { return jumpPower; } set { jumpPower = value; } }

    /// <summary>
    /// �ړ����x
    /// </summary>
    public float MoveSpeed { get { return moveSpeed; } set { moveSpeed = value; } }

    /// <summary>
    /// �U�����x(Animator�̌W��)
    /// </summary>
    public float AttackSpeedFactor { get { return attackSpeedFactor; } set { attackSpeedFactor = value; } }

    /// <summary>
    /// �����񕜂̔{��
    /// </summary>
    public float HealRate { get { return healRate; } set { healRate = value; } }

    /// <summary>
    /// ��Ԉُ�Ǘ��̃R���|�[�l���g
    /// </summary>
    public DebuffController EffectController { get { return effectController; } }
    #endregion

    #region �e�N�X�`���E�A�j���[�V����
    [Foldout("�e�N�X�`���E�A�j���[�V����")]
    [SerializeField] 
    protected Animator animator;
    #endregion

    #region �m�b�N�o�b�N�p���[ID
    /// <summary>
    /// �m�b�N�o�b�N����ID
    /// </summary>
    public enum KB_POW
    {
        Small = 1,
        Medium,
        Big,
    }
    #endregion

    #region �萔

    private const float LEVEL_UP_RATE = 0.05f; // ���x���A�b�v���̃X�e�[�^�X�㏸��

    #endregion

    #region ���̑�
    protected RigidbodyType2D defaultType2D;
    #endregion

    //--------------------------------------------------------------
    // ���\�b�h

    #region ��������

    protected virtual void Awake()
    {
        CharacterStatusData baseStatus = new CharacterStatusData(
                hp: baseHp,
                defence: baseDefense,
                power: basePower,
                moveSpeed: baseMoveSpeed,
                attackSpeedFactor: baseAttackSpeedFactor,
                jumpPower: baseJumpPower,
                healRate: baseHealRate
            );
        OverridMaxStatus(baseStatus, STATUS_TYPE.All);
        OverridCurrentStatus(baseStatus, STATUS_TYPE.All);
        if (!animator) animator = GetComponent<Animator>();
        effectController = GetComponent<DebuffController>();
    }

    protected virtual void Start()
    {
        // ���A���^�C����&&�}�X�^�[�N���C�A���g�ł͂Ȃ��ꍇ
        if (RoomModel.Instance && RoomModel.Instance.ConnectionId != Guid.Empty && !RoomModel.Instance.IsMaster)
        {
            // ���g���G || ����L�����ł͂Ȃ��ꍇ�̓X�N���v�g���A�N�e�B�u�ɂ���
            if (gameObject.tag == "Enemy" || CharacterManager.Instance.PlayerObjSelf != this.gameObject)
            {
                var rb2d = GetComponent<Rigidbody2D>();
                defaultType2D = rb2d.bodyType;
                rb2d.bodyType = RigidbodyType2D.Static;
                this.enabled = false;
            }
        }
    }

    #endregion

    /// <summary>
    /// �S�ẴX�e�[�^�X�̎�ނ��擾
    /// </summary>
    /// <returns></returns>
    List<STATUS_TYPE> GetAllStatusType()
    {
        return new List<STATUS_TYPE> { 
            STATUS_TYPE.HP, STATUS_TYPE.Defense, STATUS_TYPE.Power, STATUS_TYPE.JumpPower,
                STATUS_TYPE.MoveSpeed, STATUS_TYPE.AttackSpeedFactor, STATUS_TYPE.HealRate};
    }

    /// <summary>
    /// ���݂̃X�e�[�^�X���㉺���ɐ�������
    /// </summary>
    void ClampStatus()
    {
        hp = Mathf.Clamp(hp, 0, maxHp);
        defense = Mathf.Clamp(defense, 0, defense);
        power = Mathf.Clamp(power, 0, maxPower);
        moveSpeed = Mathf.Clamp(moveSpeed, 0f, maxMoveSpeed);
        jumpPower = Mathf.Clamp(jumpPower, 0f, maxJumpPower);
        attackSpeedFactor = Mathf.Clamp(attackSpeedFactor, 0f, maxAttackSpeedFactor);
        healRate = Mathf.Clamp(healRate, 0f, maxHealRate);
    }

    #region �X�e�[�^�X���㏑������

    /// <summary>
    /// ���݂̃X�e�[�^�X���㏑������
    /// </summary>
    public void OverridCurrentStatus(CharacterStatusData statusData, params STATUS_TYPE[] types)
    {
        List<STATUS_TYPE> statusList = new List<STATUS_TYPE>(types);
        if (statusList.Contains(STATUS_TYPE.All)) statusList = GetAllStatusType();

        foreach (STATUS_TYPE type in statusList)
        {
            switch (type)
            {
                case STATUS_TYPE.HP:
                    hp = statusData.hp;
                    break;
                case STATUS_TYPE.Defense:
                    defense = statusData.defence;
                    break;
                case STATUS_TYPE.Power:
                    power = statusData.power;
                    break;
                case STATUS_TYPE.JumpPower:
                    jumpPower = statusData.jumpPower;
                    break;
                case STATUS_TYPE.MoveSpeed:
                    moveSpeed = statusData.moveSpeed;
                    break;
                case STATUS_TYPE.AttackSpeedFactor:
                    attackSpeedFactor = statusData.attackSpeedFactor;
                    break;
                case STATUS_TYPE.HealRate:
                    healRate = statusData.healRate;
                    break;
            }
        }
        ClampStatus();
        OverrideAnimaterParam();
    }

    /// <summary>
    /// �ő�l�̕ύX������ɉ��������ݒl�̕ύX
    /// </summary>
    /// <param name="changeData">������̃X�e�[�^�X</param>
    public void ChangeAccordingStatusToMaximumValue(CharacterStatusData changeData)
    {
        // �e�X�e�[�^�X�̍ő�l�ɑ΂��錻�ݒl�̊������v�Z
        float hpRate = (float)hp / (float)maxHp;
        float defenseRate = (float)defense / (float)maxDefense;
        float powerRate = (float)power / (float)maxPower;
        float jumpPowerRate = jumpPower / maxJumpPower;
        float moveSpeedRate = moveSpeed / maxMoveSpeed;
        float attackSpeedFactorRate = attackSpeedFactor / maxAttackSpeedFactor;

        // �ő�l�̍X�V
        maxHp = changeData.hp;
        maxDefense = changeData.defence;
        maxPower = changeData.power;
        maxJumpPower = changeData.jumpPower;
        maxMoveSpeed = changeData.moveSpeed;
        maxAttackSpeedFactor = changeData.attackSpeedFactor;
        maxHealRate = changeData.healRate;

        // �ύX��̍ő�l�ɉ��������ݒl�̕ύX
        hp = (int)((float)maxHp * hpRate);
        defense = (int)((float)maxDefense * defenseRate);
        power = (int)((float)maxPower * powerRate);
        jumpPower = maxJumpPower * jumpPowerRate;
        moveSpeed = maxMoveSpeed * moveSpeedRate;
        attackSpeedFactor = maxAttackSpeedFactor * attackSpeedFactorRate;

        OverrideAnimaterParam();
    }

    /// <summary>
    /// ���݂̍ő�X�e�[�^�X���㏑�����A����ɉ����Č��݂̃X�e�[�^�X���X�V����
    /// </summary>
    public void OverridMaxStatus(CharacterStatusData statusData, params STATUS_TYPE[] types)
    {
        List<STATUS_TYPE> statusList = new List<STATUS_TYPE>(types);
        if (statusList.Contains(STATUS_TYPE.All)) statusList = GetAllStatusType();

        foreach (STATUS_TYPE type in statusList)
        {
            switch (type)
            {
                case STATUS_TYPE.HP:
                    maxHp = statusData.hp;
                    break;
                case STATUS_TYPE.Defense:
                    maxDefense = statusData.defence;
                    break;
                case STATUS_TYPE.Power:
                    maxPower = statusData.power;
                    break;
                case STATUS_TYPE.JumpPower:
                    maxJumpPower = statusData.jumpPower;
                    break;
                case STATUS_TYPE.MoveSpeed:
                    maxMoveSpeed = statusData.moveSpeed;
                    break;
                case STATUS_TYPE.AttackSpeedFactor:
                    maxAttackSpeedFactor = statusData.attackSpeedFactor;
                    break;
                case STATUS_TYPE.HealRate:
                    maxHealRate = statusData.healRate;
                    break;
            }
        }

        CharacterStatusData changeData = new CharacterStatusData(
            hp: maxHp,
            defence: maxDefense,
            power: maxPower,
            moveSpeed: maxMoveSpeed,
            attackSpeedFactor: maxAttackSpeedFactor,
            jumpPower: maxJumpPower,
            healRate: maxHealRate
           );

        ChangeAccordingStatusToMaximumValue(changeData);
        OverrideAnimaterParam();
    }

    #endregion

    #region �������w�肵�ăX�e�[�^�X�ɉ��Z����

    /// <summary>
    /// �������w�肵�čő�X�e�[�^�X�ɉ��Z���A����ɉ����Č��݂̃X�e�[�^�X���X�V����
    /// </summary>
    /// <param name="rate"></param>
    /// <param name="canResetHpToMax"></param>
    /// <param name="types"></param>
    public void ApplyMaxStatusModifierByRate(float rate, params STATUS_TYPE[] types)
    {
        List<STATUS_TYPE> statusList = new List<STATUS_TYPE>(types);
        if (statusList.Contains(STATUS_TYPE.All)) statusList = GetAllStatusType();

        foreach (STATUS_TYPE type in statusList)
        {
            switch (type)
            {
                case STATUS_TYPE.HP:
                    maxHp += (int)(baseHp * rate);
                    break;
                case STATUS_TYPE.Defense:
                    maxDefense += (int)(baseDefense * rate);
                    break;
                case STATUS_TYPE.Power:
                    maxPower += (int)(basePower * rate);
                    break;
                case STATUS_TYPE.JumpPower:
                    maxJumpPower = baseJumpPower * rate;
                    break;
                case STATUS_TYPE.MoveSpeed:
                    maxMoveSpeed += baseMoveSpeed * rate;
                    break;
                case STATUS_TYPE.AttackSpeedFactor:
                    maxAttackSpeedFactor += baseAttackSpeedFactor * rate;
                    break;
                case STATUS_TYPE.HealRate:
                    maxHealRate += baseHealRate * rate;
                    break;
            }
        }
        CharacterStatusData changeData = new CharacterStatusData(
            hp: maxHp,
            defence: maxDefense,
            power: maxPower,
            moveSpeed: maxMoveSpeed,
            attackSpeedFactor: maxAttackSpeedFactor,
            jumpPower: maxJumpPower,
            healRate: maxHealRate
            );
        ChangeAccordingStatusToMaximumValue(changeData);
        OverrideAnimaterParam();
    }

    /// <summary>
    /// �����̒l���A���݂̃X�e�[�^�X�ɉ��Z����
    /// </summary>
    /// <param name="rate"></param>
    /// <param name="canResetHpToMax"></param>
    /// <param name="types"></param>
    public void ApplyCurrentStatusModifierByRate(float rate, params STATUS_TYPE[] types)
    {
        List<STATUS_TYPE> statusList = new List<STATUS_TYPE>(types);
        if (statusList.Contains(STATUS_TYPE.All)) statusList = GetAllStatusType();

        foreach (STATUS_TYPE type in statusList)
        {
            switch (type)
            {
                case STATUS_TYPE.HP:
                    hp += (int)(baseHp * rate);
                    break;
                case STATUS_TYPE.Defense:
                    defense += (int)(baseDefense * rate);
                    break;
                case STATUS_TYPE.Power:
                    power += (int)(basePower * rate);
                    break;
                case STATUS_TYPE.JumpPower:
                    jumpPower = baseJumpPower * rate;
                    break;
                case STATUS_TYPE.MoveSpeed:
                    moveSpeed += baseMoveSpeed * rate;
                    break;
                case STATUS_TYPE.AttackSpeedFactor:
                    attackSpeedFactor += baseAttackSpeedFactor * rate;
                    break;
                case STATUS_TYPE.HealRate:
                    //healRate += baseHealRate * rate;
                    break;
            }
        }
        ClampStatus();
        OverrideAnimaterParam();
    }

    /// <summary>d
    /// ���x���A�b�v���̃X�e�[�^�X�ω�����
    /// </summary>
    public void LevelUpStatusChange()
    {
        // �e�X�e�[�^�X�̍ő�l�ɑ΂��錻�ݒl�̊������v�Z
        float hpRate = (float)hp / (float)maxHp;
        float defenseRate = (float)defense / (float)maxDefense;
        float powerRate = (float)power / (float)maxPower;

        // �ő�l�̍X�V
        maxHp = maxHp + (int)(maxHp * LEVEL_UP_RATE);
        maxDefense = maxDefense + (int)(maxDefense * LEVEL_UP_RATE);
        maxPower = maxPower + (int)(maxPower * LEVEL_UP_RATE);

        // �ύX��̍ő�l�ɉ��������ݒl�̕ύX
        hp = (int)((float)maxHp * hpRate);
        defense = (int)((float)maxDefense * defenseRate);
        power = (int)((float)maxPower * powerRate);
    }

    #endregion

    #region �A�j���[�V�����֘A

    /// <summary>
    /// �A�j���[�^�[�̃p�����[�^�[���㏑������
    /// </summary>
    public void OverrideAnimaterParam()
    {
        if (animator)
        {
            animator.SetFloat("attack_speed", attackSpeedFactor);
            animator.SetFloat("move_speed", maxMoveSpeed / moveSpeed);
        }
    }

    /// <summary>
    /// �A�j���[�V�����ݒ菈��
    /// </summary>
    /// <param name="id"></param>
    public virtual void SetAnimId(int id)
    {
        if (animator == null) return;
        animator.SetInteger("animation_id", id);
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