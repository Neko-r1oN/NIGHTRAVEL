//----------------------------------------------------
// �����b�N�Ǘ��N���X
// Author : Souma Ueno
//----------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor.U2D;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


public class RelicManager : MonoBehaviour
{
    #region ���X�g
    [Header("���X�g")]
    [SerializeField] List<Sprite> relicSprites = new List<Sprite>();     // �����b�N�̃��X�g
    [SerializeField] List<RelicDeta> haveRelicList = new List<RelicDeta>();     // ���������b�N���X�g
    [SerializeField] List<GameObject> relicPrefabs = new List<GameObject>();  // �����b�N�v���t�@�u
    #endregion

    float elapsedTime;
    GameObject relic;

    public Material defaultSpriteMaterial;

    [SerializeField] GameObject relicPrefab;

    RELIC_RARITY randomRarity;

    /// <summary>
    /// �����b�N�̃��A���e�B
    /// </summary>
    public enum RELIC_RARITY
    {
        CURSE,�@// ��
        NORMAL, // �m�[�}��
        RARE,   // ���A
        UNIQUE, // ���j�[�N
        SPECIAL // ����
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
            // �C���X�^���X���������݂��Ȃ��悤�ɁA
            // ���ɑ��݂��Ă����玩�g����������
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (RoomModel.Instance == null) return;
        //���f����OnSpawnedRelic���Ăяo������OnSpawnRelic���Ăяo�����
        //RoomModel.Instance.OnSpawnedRelic += this.OnSpawnRelic;
    }

    /// <summary>
    /// ���A���e�B�̊m���̐ݒ�
    /// </summary>
    private Dictionary<RELIC_RARITY, float> rarityWeight = new Dictionary<RELIC_RARITY, float>()
    {   
        {RELIC_RARITY.CURSE,3 },    // �m���F3%
        {RELIC_RARITY.NORMAL,70 },  // �m���F70%
        {RELIC_RARITY.RARE,20 },    // �m���F20%
        {RELIC_RARITY.SPECIAL,6 },  // �m���F6%
        {RELIC_RARITY.UNIQUE,1 }    // �m���F1%
    };

    /// <summary>
    /// �����b�N���������ɒǉ����鏈��
    /// </summary>
    public void AddRelic(RelicDeta relic)
    {
        if (haveRelicList.Find(X => X.ID == relic.ID) != null)
        {
            CountRelic(relic.ID);
        }
        else
        {
            UIManager.Instance.DisplayRelic(relicSprites[relic.ID]);
        }

        haveRelicList.Add(relic);
        //Debug.Log(haveRelicList[haveRelicList.Count - 1]);
        Debug.Log(randomRarity);
    }

    /// <summary>
    /// �����b�N�𐶐����鏈��
    /// </summary>
    //public void GenerateRelic(Vector3 bossPos)
    //{
    //    randomRarity = GetRandomRarity();

    //    List<GameObject> filteredRelics = relicPrefab.
    //        Where(prefab =>
    //        {
    //            Relic relic = prefab.GetComponent<Relic>();
    //            return relic != null && relic.Rarity == (int)randomRarity;
    //        }).ToList();

    //    if (filteredRelics.Count > 0)
    //    {
    //        int random = Random.Range(0, filteredRelics.Count);
    //        GameObject selectedRelic = filteredRelics[random];
    //        relic = Instantiate(selectedRelic, bossPos, Quaternion.identity);
    //    }

    //    if (relic != null)
    //    {
    //        Rigidbody2D rb = relic.GetComponent<Rigidbody2D>();  // rigidbody���擾
    //        float boundRnd = Random.Range(2f, 6f);
    //        boundRnd = (int)Random.Range(0f, 2f) == 0 ? boundRnd : boundRnd * -1;
    //        Vector3 force = new Vector3(boundRnd, 12.0f, 0f);    // �͂�ݒ�
    //        rb.AddForce(force, ForceMode2D.Impulse);             // �͂�������
    //    }
    //}


    [ContextMenu("GenerateRelicTest")]
    public async void GenerateRelicTest()
    {
        relic = Instantiate(relicPrefab, new Vector3(0, 0, -0.1f), UnityEngine.Quaternion.identity);

        SpriteRenderer spriteRenderer = relic.transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer sr = relic.transform.GetChild(0).GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = relicSprites[Random.Range(0, relicSprites.Count)];
        }

        if (sr != null)
        {
            // �����Ń}�e���A�������蓖��
            sr.material = defaultSpriteMaterial;
        }

            //relic.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = relicSprites[Random.Range(0, relicSprites.Count)];

            //        randomRarity = GetRandomRarity();

            //        List<GameObject> filteredRelics = relicPrefabs.
            //            Where(prefab =>
            //            {
            //                Relic relic = prefab.GetComponent<Relic>();
            //                return relic != null && relic.Rarity == (int)randomRarity;
            //            }).ToList();

            //        if (filteredRelics.Count > 0)
            //        {
            //            int random = Random.Range(0, filteredRelics.Count);
            //            GameObject selectedRelic = filteredRelics[random];
            //#if UNITY_EDITOR
            //            relic = Instantiate(selectedRelic, new Vector3(0, 0, 0), Quaternion.identity);
            //#else
            //            //�����b�N�̐ݒ�
            //            relic = selectedRelic;
            //            //�����b�N�̐������������s
            //            await RoomModel.Instance.SpawnRelicAsync(relic.transform.position);
            //#endif
            //}
        }

    /// <summary>
    /// �����b�N�̐����̒ʒm
    /// </summary>
    /// <param name="relicId"></param>
    /// <param name="pos"></param>
    void OnSpawnRelic(int relicId,Vector2 pos)
    {
       relic = Instantiate(relicPrefabs[relicId], pos,UnityEngine.Quaternion.identity);
    }

    [ContextMenu("ShuffleRelic")]
    /// <summary>
    /// �����������ւ��鏈��
    /// </summary>
    public void ShuffleRelic()
    {
        int count = haveRelicList.Count;

        haveRelicList.Clear();
        UIManager.Instance.ClearRelic();

        for (int i = 0; i < count; i++)
        {
            int relicnum = Random.Range(0, relicSprites.Count);

            relicPrefabs[relicnum].GetComponent<Relic>().AddRelic();
        }
    }

    /// <summary>
    /// ���������b�N�������Ă��鐔�𐔂���
    /// </summary>
    /// <param name="id"></param>
    public void CountRelic(int id)
    {
        int relicCnt = 0;

        foreach (RelicDeta rlc in haveRelicList)
        {
            if (id == rlc.ID)
            {
                relicCnt++;
            }
        }

        UIManager.Instance.totalRelics(relicSprites[id], relicCnt);
    }

    /// <summary>
    /// ���A���e�B���o
    /// </summary>
    /// <returns></returns>
    public RELIC_RARITY GetRandomRarity()
    {
        float totalWeight = 0;
        float currentWeight = 0;

        foreach(var pair in rarityWeight)
        {
            totalWeight += pair.Value;
        }

        float randomNum = Random.Range(0, totalWeight);

        foreach(var pair in rarityWeight)
        {
            currentWeight += pair.Value;
            if(randomNum < currentWeight)
            {
                return pair.Key;
            }
        }

        return RELIC_RARITY.NORMAL;
    }
}