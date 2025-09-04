using NUnit.Framework;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;

public class GimmickManager : MonoBehaviour
{
    [SerializeField] List<GimmickBase> gimmicks = new List<GimmickBase>();
    Dictionary<int, GimmickBase> managedGimmicks = new Dictionary<int, GimmickBase>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ���ʗpID��ݒ�
        for(int i = 0; i < gimmicks.Count; i++)
        {
            gimmicks[i].UniqueId = i;
            managedGimmicks.Add(i, gimmicks[i]);
        }
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
                    IsActivated = gimmick.Value.IsBoot,
                    Position = gimmick.Value.transform.position,
                    Rotation = gimmick.Value.transform.rotation,
                };

                gimmickDatas.Add(gimmickData);
            }
        }
        return gimmickDatas;
    }

    /// <summary>
    /// �M�~�b�N�N���ʒm
    /// </summary>
    /// <param name="uniqueId"></param>
    void OnBootGimmick(int uniqueId)
    {
        if (managedGimmicks.ContainsKey(uniqueId))
        {
            managedGimmicks[uniqueId].TurnOnPower(0);
        }
    }
}
