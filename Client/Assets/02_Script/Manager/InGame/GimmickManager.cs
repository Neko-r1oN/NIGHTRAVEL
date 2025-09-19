using NUnit.Framework;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;

public class GimmickManager : MonoBehaviour
{
    [SerializeField] List<GimmickBase> gimmicks = new List<GimmickBase>();
    Dictionary<int, GimmickBase> managedGimmicks = new Dictionary<int, GimmickBase>();
    public Dictionary<int, GimmickBase> ManagedGimmicks { get; private set; }

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
                gimmicks[i].UniqueId = i;
                managedGimmicks.Add(i, gimmicks[i]);
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
                    GimmickID = gimmick.Key,
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
            if (managedGimmicks.ContainsKey(data.GimmickID))
                managedGimmicks[data.GimmickID].transform.position = data.Position;
        }
    }

    /// <summary>
    /// �M�~�b�N�N���ʒm
    /// </summary>
    /// <param name="uniqueId"></param>
    void OnBootGimmick(int uniqueId)
    {
        if (managedGimmicks.ContainsKey(uniqueId))
        {
            managedGimmicks[uniqueId].TurnOnPower();
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
    public void AddGimmickFromList(int uniqueId, GimmickBase gimmick)
    {
        managedGimmicks.Add(uniqueId, gimmick);
    }
}
