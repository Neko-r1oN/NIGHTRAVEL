//----------------------------------------------------
// �����b�N�Ǘ��N���X
// Author : Souma Ueno
//----------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RelicManager : MonoBehaviour
{
    #region ���X�g
    [Header("���X�g")]
    [SerializeField] List<Sprite> relicSprites = new List<Sprite>();     // �����b�N�̃��X�g
    [SerializeField] List<RelicDeta> haveRelicList = new List<RelicDeta>();     // ���������b�N���X�g
    [SerializeField] List<GameObject> relicPrefab = new List<GameObject>();  // �����b�N�v���t�@�u
    #endregion

    float elapsedTime;

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
        Debug.Log(haveRelicList[haveRelicList.Count - 1]);

        /*foreach(Relic rlc in haveRelicList)
        {
            if (haveRelicList.Count > 1)
            {
                if (relic.ID == rlc.ID)
                {
                    CountRelic(relic.ID);
                }
                else
                {
                    UIManager.Instance.DisplayRelic(relicSprites[relic.ID]);
                }
            }
        }*/
    }

    /// <summary>
    /// �����b�N�𐶐����鏈��
    /// </summary>
    public void GenerateRelic(Vector3 bossPos)
    {
        int rand = Random.Range(0, relicSprites.Count);

        GameObject relic = Instantiate(relicPrefab[rand],bossPos,Quaternion.identity);

        Rigidbody2D rb = relic.GetComponent<Rigidbody2D>();  // rigidbody���擾
        float boundRnd = Random.Range(2f, 6f);
        boundRnd = (int)Random.Range(0f, 2f) == 0 ? boundRnd : boundRnd * -1;
        Vector3 force = new Vector3(boundRnd, 12.0f, 0f);  // �͂�ݒ�
        rb.AddForce(force, ForceMode2D.Impulse);          // �͂�������
    }

    [ContextMenu("GenerateRelicTest")]
    public void GenerateRelicTest()
    {
        int rand = Random.Range(0, relicSprites.Count);

        GameObject relic = Instantiate(relicPrefab[rand], new Vector3(0,0,0), Quaternion.identity);

        Rigidbody2D rb = relic.GetComponent<Rigidbody2D>();  // rigidbody���擾
        float boundRnd = Random.Range(2f,6f);
        boundRnd = (int)Random.Range(0f,2f) == 0 ? boundRnd : boundRnd * -1;
        Vector3 force = new Vector3(boundRnd, 12.0f, 0f);  // �͂�ݒ�
        rb.AddForce(force, ForceMode2D.Impulse);          // �͂�������
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
            relicPrefab[relicnum].GetComponent<Relic>().AddRelic();
        }
    }

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
}