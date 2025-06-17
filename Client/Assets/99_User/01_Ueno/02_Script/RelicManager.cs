using Microsoft.Unity.VisualStudio.Editor;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RelicManager : MonoBehaviour
{
    [SerializeField] List<Sprite> relicSprites;      // レリックのリスト
    [SerializeField] List<Relic> haveRelicList; // 所持レリックリスト

    GameObject relic;

    public enum RELIC_RARITY
    {
        CURSE,
        NORMAL,
        RARE,
        UNIQUE,
        SPECIAL
    }

    private static RelicManager instance;

    public static RelicManager Instance
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
    }

    /// <summary>
    /// レリックを持ち物に追加する処理
    /// </summary>
    public void AddRelic(Relic relic)
    {
        haveRelicList.Add(relic);

        UIManager.Instance.DisplayRelic(relicSprites[relic.ID]);
    }

    /// <summary>
    /// レリックを生成する処理
    /// </summary>
    public void GenerateRelic(Vector3 bossPos)
    {
        //int rand = Random.Range(0, relicSprites.Count);

        //relic = Instantiate(relicImages[rand], bossPos, Quaternion.identity);
    }

    /// <summary>
    /// 持ち物を入れ替える処理
    /// </summary>
    public void ShuffleRelic()
    {

    }
}