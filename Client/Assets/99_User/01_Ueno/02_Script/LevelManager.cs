using UnityEngine;

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
        else
        {
            // インスタンスが複数存在しないように、既に存在していたら自身を消去する
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void InitLevel(GAME_LEVEL level)
    {
        gameLevel = level;
    }

    public void UpGameLevel()
    {
        gameLevel++;
    }
}
