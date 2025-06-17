using Microsoft.Unity.VisualStudio.Editor;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RelicManager : MonoBehaviour
{
    [SerializeField] List<Sprite> relicSprites;      // �����b�N�̃��X�g
    [SerializeField] List<Relic> haveRelicList; // ���������b�N���X�g

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
            // �C���X�^���X���������݂��Ȃ��悤�ɁA���ɑ��݂��Ă����玩�g����������
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
        //int rand = Random.Range(0, relicSprites.Count);

        //relic = Instantiate(relicImages[rand], bossPos, Quaternion.identity);
    }

    /// <summary>
    /// �����������ւ��鏈��
    /// </summary>
    public void ShuffleRelic()
    {

    }
}