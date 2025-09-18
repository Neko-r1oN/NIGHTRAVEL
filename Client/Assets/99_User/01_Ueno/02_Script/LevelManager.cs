using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using Shared.Interfaces.StreamingHubs;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Rewired.Utils.Classes.Data;

public class LevelManager : MonoBehaviour
{
    Dictionary<Guid, List<Status_Enhancement>> options = new Dictionary<Guid, List<Status_Enhancement>>();
    public Dictionary<Guid, List<Status_Enhancement>> Options { get { return options; } set { options = value; } }

    /// <summary>
    /// ゲーム難易度
    /// </summary>
    public enum GAME_LEVEL
    {
        Baby,
        Easy,
        Normal,
        Hard,
        Berryhard,
        Hell
    }
    GAME_LEVEL gameLevel;
    public GAME_LEVEL GameLevel { get { return gameLevel; } }

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
            SetTestData();  // テスト用、不要になり次第削除
            return;
        }
    }

    void SetTestData()
    {
        List<Status_Enhancement> statusesGroup1 = new List<Status_Enhancement>();
        List<Status_Enhancement> statusesGroup2 = new List<Status_Enhancement>();

        statusesGroup1.Add(new Status_Enhancement() { id = 0, name = "攻撃", explanation = "攻撃が上がる" });
        statusesGroup1.Add(new Status_Enhancement() { id = 1, name = "スピード", explanation = "スピード上がる" });
        statusesGroup1.Add(new Status_Enhancement() { id = 2, name = "防御", explanation = "防御上がる" });

        statusesGroup2.Add(new Status_Enhancement() { id = 3, name = "適当", explanation = "適当に上がる" });
        statusesGroup2.Add(new Status_Enhancement() { id = 4, name = "a", explanation = "適当に上がる" });
        statusesGroup2.Add(new Status_Enhancement() { id = 5, name = "b", explanation = "適当に上がる" });

        options.Add(Guid.NewGuid(), statusesGroup1);
        options.Add(Guid.NewGuid(), statusesGroup2);
    }

    public Dictionary<GAME_LEVEL, string> LevelName = new Dictionary<GAME_LEVEL, string>
    {
        {GAME_LEVEL.Baby,"ベビー"},
        {GAME_LEVEL.Easy,"イージー" },
        {GAME_LEVEL.Normal,"ノーマル" },
        {GAME_LEVEL.Hard,"ハード" },
        {GAME_LEVEL.Berryhard,"ベリーハード" },
        {GAME_LEVEL.Hell,"ヘル" }
    };

    public void InitLevel(GAME_LEVEL level)
    {
        gameLevel = level;
    }

    [ContextMenu("UpGameLevel")]
    public void UpGameLevel()
    {
        if(gameLevel + 1 > (GAME_LEVEL)Enum.GetValues(typeof(GAME_LEVEL)).Length)
        {
            return;
        }

        gameLevel++;

        foreach (var enemy in CharacterManager.Instance.Enemies.Values)
        {
            enemy.Enemy.ApplyMaxStatusModifierByRate(0.1f, STATUS_TYPE.HP, STATUS_TYPE.Power, STATUS_TYPE.Defense);
        }

        

        UIManager.Instance.UpGameLevelText();

        Debug.Log(gameLevel.ToString());
    }
}
