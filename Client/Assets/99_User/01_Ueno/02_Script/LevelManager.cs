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
    static public Dictionary<Guid, List<StatusUpgrateOptionData>> Options { get; set; } = new Dictionary<Guid, List<StatusUpgrateOptionData>>();

    /// <summary>
    /// 現在のゲーム難易度
    /// </summary>
    static public int GameLevel { get; set; } = 6;

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
        if (!RoomModel.Instance && Options.Count == 0)
        {
            SetTestData();  // テスト用、不要になり次第削除
        }
        if(RoomModel.Instance)
        {
            RoomModel.Instance.OnAscendDifficultySyn += this.OnAscendDifficulty;
        }

        if (GameLevel > 0)
        {
            UIManager.Instance.UpGameLevelText();
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

        statusesGroup1.Add(new StatusUpgrateOptionData() { TypeId = STAT_UPGRADE_OPTION.Common_Attack, Name = "攻撃", Explanation = "攻撃が上がる",StatusType1 = STATUS_TYPE.Power,Rarity = RARITY_TYPE.Common });
        statusesGroup1.Add(new StatusUpgrateOptionData() { TypeId = STAT_UPGRADE_OPTION.Common_MovementSpeed, Name = "スピード", Explanation = "スピード上がる" , StatusType1 = STATUS_TYPE.MoveSpeed, Rarity = RARITY_TYPE. Uncommon});
        statusesGroup1.Add(new StatusUpgrateOptionData() { TypeId = STAT_UPGRADE_OPTION.Common_Deffence, Name = "防御", Explanation = "防御上がる", StatusType1 = STATUS_TYPE.Defense, Rarity = RARITY_TYPE.Rare });

        statusesGroup2.Add(new StatusUpgrateOptionData() { TypeId = STAT_UPGRADE_OPTION.Common_HP, Name = "適当", Explanation = "適当に上がる" , StatusType1 = STATUS_TYPE.AttackSpeedFactor, Rarity = RARITY_TYPE.Unique });
        statusesGroup2.Add(new StatusUpgrateOptionData() { TypeId = STAT_UPGRADE_OPTION.Common_JumpingPower, Name = "a", Explanation = "適当に上がる", StatusType1 = STATUS_TYPE.JumpPower, Rarity = RARITY_TYPE.Legend });
        statusesGroup2.Add(new StatusUpgrateOptionData() { TypeId = STAT_UPGRADE_OPTION.Legend_AutomaticRecovery, Name = "b", Explanation = "適当に上がる" , StatusType1 = STATUS_TYPE.HealRate, Rarity = RARITY_TYPE.Common });

        Options.Add(Guid.NewGuid(), statusesGroup1);
        Options.Add(Guid.NewGuid(), statusesGroup2);
    }

    public Dictionary<DIFFICULTY_TYPE, string> LevelName = new Dictionary<DIFFICULTY_TYPE, string>
    {
        {DIFFICULTY_TYPE.Baby,"ベビー"},
        {DIFFICULTY_TYPE.Easy,"イージー" },
        {DIFFICULTY_TYPE.Normal,"ノーマル" },
        {DIFFICULTY_TYPE.Hard,"ハード" },
        {DIFFICULTY_TYPE.VeryHard,"ベリーハード" },
        {DIFFICULTY_TYPE.Hell,"ヘル" }
    };

    [ContextMenu("UpGameLevel")]
    public void UpGameLevel(int? setLevel = null)
    {
        if (setLevel == null) GameLevel++;
        else GameLevel = (int)setLevel;

        CharacterManager.Instance.ApplyDifficultyToAllEnemies();
        UIManager.Instance.UpGameLevelText();

        Debug.Log(GameLevel.ToString());
    }

    /// <summary>
    /// 難易度上昇通知
    /// </summary>
    /// <param name="dif"></param>
    void OnAscendDifficulty(int dif)
    {
        UpGameLevel(dif);
    }
}
