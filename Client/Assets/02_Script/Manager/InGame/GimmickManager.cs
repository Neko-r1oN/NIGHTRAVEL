using NUnit.Framework;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;

public class GimmickManager : MonoBehaviour
{
    [SerializeField] List<GimmickBase> gimmicks = new List<GimmickBase>();
    Dictionary<string, GimmickBase> managedGimmicks = new Dictionary<string, GimmickBase>();
    public Dictionary<string, GimmickBase> ManagedGimmicks { get { return managedGimmicks; } private set { managedGimmicks = value; } }

    static GimmickManager instance;
    public static GimmickManager Instance
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

            // ���ʗpID��ݒ�
            for (int i = 0; i < gimmicks.Count; i++)
            {
                gimmicks[i].UniqueId = $"{gimmicks[i].name}_{i}";
                managedGimmicks.Add(gimmicks[i].UniqueId, gimmicks[i]);
            }
        }
        else
        {
            // �C���X�^���X���������݂��Ȃ��悤�ɁA���ɑ��݂��Ă����玩�g����������
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnBootedGimmick += this.OnBootGimmick;
        RoomModel.Instance.OnChangedMasterClient += this.OnChangedMasterClient;
    }

    private void OnDisable()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnBootedGimmick -= this.OnBootGimmick;
        RoomModel.Instance.OnChangedMasterClient -= this.OnChangedMasterClient;
    }

    /// <summary>
    /// GimmickData�ɉ��H���ĕԂ�
    /// </summary>
    /// <returns></returns>
    public List<GimmickData> GetGimmickDatas()
    {
        var gimmickDatas = new List<GimmickData>();
        foreach(var gimmick in managedGimmicks)
        {
            if (gimmick.Value != null)
            {
                var gimmickData = new GimmickData()
                {
                    UniqueID = gimmick.Key,
                    GimmickName = gimmick.Value.name,
                    Position = gimmick.Value.transform.position,
                };

                gimmickDatas.Add(gimmickData);
            }
        }
        return gimmickDatas;
    }

    /// <summary>
    /// �ꊇ�ŃM�~�b�N���X�V����
    /// </summary>
    /// <param name="gimmickDatas"></param>
    public void UpdateGimmicks(List<GimmickData> gimmickDatas)
    {
        foreach(var data in gimmickDatas)
        {
            if (managedGimmicks.ContainsKey(data.UniqueID))
                managedGimmicks[data.UniqueID].UpdateGimmick(data);
        }
    }

    /// <summary>
    /// �M�~�b�N�N���ʒm
    /// </summary>
    /// <param name="uniqueId"></param>
    void OnBootGimmick(string uniqueId, bool triggerOnce)
    {
        if (managedGimmicks.ContainsKey(uniqueId))
        {
            managedGimmicks[uniqueId].TurnOnPower();
            if(triggerOnce) managedGimmicks.Remove(uniqueId);
        }
    }

    /// <summary>
    /// ���g���}�X�^�N���C�A���g�ɂȂ����Ƃ�
    /// </summary>
    void OnChangedMasterClient()
    {
        foreach(var gimmick in managedGimmicks.Values)
        {
            gimmick.Reactivate();
        }
    }

    /// <summary>
    /// �V���ȃM�~�b�N�����X�g�ɒǉ�
    /// </summary>
    /// <param name="uniqueId"></param>
    /// <param name="gimmick"></param>
    public void AddGimmickFromList(string uniqueId, GimmickBase gimmick)
    {
        managedGimmicks.Add(uniqueId, gimmick);
    }
}
