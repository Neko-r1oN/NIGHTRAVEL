using Microsoft.Unity.VisualStudio.Editor;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEditor.PlayerSettings;

public class RelicManager : MonoBehaviour
{
    [SerializeField] List<Sprite> relicSprites;     // �����b�N�̃��X�g
    [SerializeField] List<Relic> haveRelicList;     // ���������b�N���X�g
    [SerializeField] List<GameObject> relicPrefab;  // �����b�N�v���t�@�u

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
            // �C���X�^���X���������݂��Ȃ��悤�ɁA
            // ���ɑ��݂��Ă����玩�g����������
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �����b�N���������ɒǉ����鏈��
    /// </summary>
    public void AddRelic(Relic relic)
    {
        haveRelicList.Add(relic);

        UIManager.Instance.DisplayRelic(relicSprites[relic.ID]);
    }

    /// <summary>
    /// �����b�N�𐶐����鏈��
    /// </summary>
    public void GenerateRelic(Vector3 bossPos)
    {
        int rand = Random.Range(0, relicSprites.Count);

        relic = Instantiate(relicPrefab[rand],bossPos,Quaternion.identity);

        Rigidbody rb = relic.GetComponent<Rigidbody>();  // rigidbody���擾
        Vector3 force = new Vector3(1.0f, 8.0f, 0f);  // �͂�ݒ�
        rb.AddForce(force, ForceMode.Impulse);          // �͂�������
    }

    [ContextMenu("GenerateRelicTest")]
    public void GenerateRelicTest()
    {
        int rand = Random.Range(0, relicSprites.Count);

        relic = Instantiate(relicPrefab[4], new Vector3(0,0,0), Quaternion.identity);

        Rigidbody2D rb = relic.GetComponent<Rigidbody2D>();  // rigidbody���擾
        float boundRnd = Random.Range(2f,6f);
        boundRnd = (int)Random.Range(0f,2f) == 0 ? boundRnd : boundRnd * -1;
        Vector3 force = new Vector3(boundRnd, 12.0f, 0f);  // �͂�ݒ�
        rb.AddForce(force, ForceMode2D.Impulse);          // �͂�������
    }

    /// <summary>
    /// �����������ւ��鏈��
    /// </summary>
    public void ShuffleRelic()
    {

    }
}