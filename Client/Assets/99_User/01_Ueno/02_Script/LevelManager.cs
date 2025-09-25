using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using Shared.Interfaces.StreamingHubs;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Rewired.Utils.Classes.Data;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;

public class LevelManager : MonoBehaviour
{
    Dictionary<Guid, List<StatusUpgrateOptionData>> options = new Dictionary<Guid, List<StatusUpgrateOptionData>>();
    public Dictionary<Guid, List<StatusUpgrateOptionData>> Options { get { return options; } set { options = value; } }

    int gameLevel;
    public int GameLevel { get { return gameLevel; } }

    readonly public int LevelHellId = 5;

    public static LevelManager instance;

    public static LevelManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!RoomModel.Instance)
        {
            SetTestData();  // �e�X�g�p�A�s�v�ɂȂ莟��폜
            return;
        }
        else
        {
            RoomModel.Instance.OnAscendDifficultySyn += this.OnAscendDifficulty;
        }
    }

    private void OnDisable()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnAscendDifficultySyn -= this.OnAscendDifficulty;
    }

    void SetTestData()
    {
        List<StatusUpgrateOptionData> statusesGroup1 = new List<StatusUpgrateOptionData>();
        List<StatusUpgrateOptionData> statusesGroup2 = new List<StatusUpgrateOptionData>();

        statusesGroup1.Add(new StatusUpgrateOptionData() { TypeId = STAT_UPGRADE_OPTION.Common_Attack, Name = "�U��", Explanation = "�U�����オ��",StatusType1 = STATUS_TYPE.Power });
        statusesGroup1.Add(new StatusUpgrateOptionData() { TypeId = STAT_UPGRADE_OPTION.Common_MovementSpeed, Name = "�X�s�[�h", Explanation = "�X�s�[�h�オ��" , StatusType1 = STATUS_TYPE.MoveSpeed});
        statusesGroup1.Add(new StatusUpgrateOptionData() { TypeId = STAT_UPGRADE_OPTION.Common_Deffence, Name = "�h��", Explanation = "�h��オ��", StatusType1 = STATUS_TYPE.Defense });

        statusesGroup2.Add(new StatusUpgrateOptionData() { TypeId = STAT_UPGRADE_OPTION.Common_HP, Name = "�K��", Explanation = "�K���ɏオ��" , StatusType1 = STATUS_TYPE.AttackSpeedFactor });
        statusesGroup2.Add(new StatusUpgrateOptionData() { TypeId = STAT_UPGRADE_OPTION.Common_JumpingPower, Name = "a", Explanation = "�K���ɏオ��", StatusType1 = STATUS_TYPE.JumpPower });
        statusesGroup2.Add(new StatusUpgrateOptionData() { TypeId = STAT_UPGRADE_OPTION.Legend_AutomaticRecovery, Name = "b", Explanation = "�K���ɏオ��" , StatusType1 = STATUS_TYPE.HealRate});

        options.Add(Guid.NewGuid(), statusesGroup1);
        options.Add(Guid.NewGuid(), statusesGroup2);
    }

    public Dictionary<DIFFICULTY_TYPE, string> LevelName = new Dictionary<DIFFICULTY_TYPE, string>
    {
        {DIFFICULTY_TYPE.Baby,"�x�r�["},
        {DIFFICULTY_TYPE.Easy,"�C�[�W�[" },
        {DIFFICULTY_TYPE.Normal,"�m�[�}��" },
        {DIFFICULTY_TYPE.Hard,"�n�[�h" },
        {DIFFICULTY_TYPE.VeryHard,"�x���[�n�[�h" },
        {DIFFICULTY_TYPE.Hell,"�w��" }
    };

    public void InitLevel(int level)
    {
        gameLevel = level;
    }

    [ContextMenu("UpGameLevel")]
    public void UpGameLevel(int? targetLevel = null)
    {
        if (targetLevel == null) gameLevel++;
        else gameLevel = (int)targetLevel;

        foreach (var enemy in CharacterManager.Instance.Enemies.Values)
        {
            enemy.Enemy.ApplyMaxStatusModifierByRate(0.1f, STATUS_TYPE.HP, STATUS_TYPE.Power, STATUS_TYPE.Defense);
        }

        UIManager.Instance.UpGameLevelText();

        Debug.Log(gameLevel.ToString());
    }

    /// <summary>
    /// ��Փx�㏸�ʒm
    /// </summary>
    /// <param name="dif"></param>
    void OnAscendDifficulty(int dif)
    {
        UpGameLevel(dif);
    }
}
