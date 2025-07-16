using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class LevelManager : MonoBehaviour
{
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

    public Dictionary<GAME_LEVEL, string> LevelName = new Dictionary<GAME_LEVEL, string>
    {
        {GAME_LEVEL.Baby,"ベビー"},
        {GAME_LEVEL.Easy,"イージー" },
        {GAME_LEVEL.Normal,"ノーマル" },
        {GAME_LEVEL.Hard,"ハード" },
        {GAME_LEVEL.Berryhard,"ベリーハード" }
    };

    public void InitLevel(GAME_LEVEL level)
    {
        gameLevel = level;
    }

    [ContextMenu("UpGameLevel")]
    public void UpGameLevel()
    {
        gameLevel = gameLevel + 1;
        UIManager.Instance.UpGameLevelText();
        Debug.Log(gameLevel.ToString());
    }
}
