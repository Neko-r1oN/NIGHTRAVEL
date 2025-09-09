using NUnit.Framework;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;

public class GimmickManager : MonoBehaviour
{
    [SerializeField] List<GimmickBase> gimmicks = new List<GimmickBase>();
    Dictionary<int, GimmickBase> managedGimmicks = new Dictionary<int, GimmickBase>();

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

            // 識別用IDを設定
            for (int i = 0; i < gimmicks.Count; i++)
            {
                gimmicks[i].UniqueId = i;
                managedGimmicks.Add(i, gimmicks[i]);
            }
        }
        else
        {
            // インスタンスが複数存在しないように、既に存在していたら自身を消去する
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnBootedGimmick += this.OnBootGimmick;
    }

    private void OnDisable()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnBootedGimmick -= this.OnBootGimmick;
    }

    /// <summary>
    /// GimmickDataに加工して返す
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
    /// 一括でギミックを更新する
    /// </summary>
    /// <param name="gimmickDatas"></param>
    public void UpdateGimmicks(List<GimmickData> gimmickDatas)
    {
        foreach(var data in gimmickDatas)
        {
            managedGimmicks[data.GimmickID].IsBoot = data.IsActivated;
            managedGimmicks[data.GimmickID].transform.position = data.Position;
            managedGimmicks[data.GimmickID].transform.rotation = data.Rotation;
        }
    }

    /// <summary>
    /// ギミック起動通知
    /// </summary>
    /// <param name="uniqueId"></param>
    void OnBootGimmick(int uniqueId)
    {
        if (managedGimmicks.ContainsKey(uniqueId))
        {
            managedGimmicks[uniqueId].TurnOnPower();
        }
    }
}
