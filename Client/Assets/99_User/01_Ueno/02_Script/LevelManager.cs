using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using Shared.Interfaces.StreamingHubs;

public class LevelManager : MonoBehaviour
{
    /// <summary>
    /// �Q�[����Փx
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

    public Dictionary<GAME_LEVEL, string> LevelName = new Dictionary<GAME_LEVEL, string>
    {
        {GAME_LEVEL.Baby,"�x�r�["},
        {GAME_LEVEL.Easy,"�C�[�W�[" },
        {GAME_LEVEL.Normal,"�m�[�}��" },
        {GAME_LEVEL.Hard,"�n�[�h" },
        {GAME_LEVEL.Berryhard,"�x���[�n�[�h" },
        {GAME_LEVEL.Hell,"�w��" }
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
