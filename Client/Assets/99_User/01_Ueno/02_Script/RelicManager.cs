using Microsoft.Unity.VisualStudio.Editor;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEditor.PlayerSettings;

public class RelicManager : MonoBehaviour
{
    [SerializeField] List<Sprite> relicSprites;     // レリックのリスト
    [SerializeField] List<Relic> haveRelicList;     // 所持レリックリスト
    [SerializeField] List<GameObject> relicPrefab;  // レリックプレファブ

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
            // インスタンスが複数存在しないように、
            // 既に存在していたら自身を消去する
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
        int rand = Random.Range(0, relicSprites.Count);

        relic = Instantiate(relicPrefab[rand],bossPos,Quaternion.identity);

        Rigidbody rb = relic.GetComponent<Rigidbody>();  // rigidbodyを取得
        Vector3 force = new Vector3(1.0f, 8.0f, 0f);  // 力を設定
        rb.AddForce(force, ForceMode.Impulse);          // 力を加える
    }

    [ContextMenu("GenerateRelicTest")]
    public void GenerateRelicTest()
    {
        int rand = Random.Range(0, relicSprites.Count);

        relic = Instantiate(relicPrefab[4], new Vector3(0,0,0), Quaternion.identity);

        Rigidbody2D rb = relic.GetComponent<Rigidbody2D>();  // rigidbodyを取得
        float boundRnd = Random.Range(2f,6f);
        boundRnd = (int)Random.Range(0f,2f) == 0 ? boundRnd : boundRnd * -1;
        Vector3 force = new Vector3(boundRnd, 12.0f, 0f);  // 力を設定
        rb.AddForce(force, ForceMode2D.Impulse);          // 力を加える
    }

    /// <summary>
    /// 持ち物を入れ替える処理
    /// </summary>
    public void ShuffleRelic()
    {

    }
}