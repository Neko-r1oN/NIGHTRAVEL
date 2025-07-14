using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class LevelManager : MonoBehaviour
{
    /// <summary>
    /// ÉQÅ[ÉÄìÔà’ìx
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

    public void InitLevel(GAME_LEVEL level)
    {
        gameLevel = level;
    }

    public void UpGameLevel()
    {
        gameLevel = gameLevel + 1;
        Debug.Log(gameLevel.ToString());
    }
}
