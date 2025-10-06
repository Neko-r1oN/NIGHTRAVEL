//====================
// 足場とする箱を管理するスクリプト
// Aouther:y-miura
// Date:2025/07/20
//====================

using System;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class BoxManager : MonoBehaviour
{
    [SerializeField] GameObject BoxPrefab;  //箱プレハブ取得
    const float leftPosX = 26.98f;
    const float rightPosX = 28.95f;
    const float posY = 27f;

    public float spawnTime;

    void Start()
    {
        //SpawnBox関数を繰り返し呼び出して、箱を繰り返し生成する
        InvokeRepeating("SpawnBox", 0.1f, spawnTime);

        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnSpawnedObjectSyn += OnSpawnObject;
    }

    private void OnDisable()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnSpawnedObjectSyn -= OnSpawnObject;
    }

    /// <summary>
    /// 箱を生成する処理
    /// </summary>
    void SpawnBox()
    {
        if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            float spawnPosX = UnityEngine.Random.Range(1, 3) == 1 ? leftPosX : rightPosX;
            float spawnPosY = posY;
            Vector2 spawnPos = new Vector2(spawnPosX, spawnPosY);
            SpawnObjectRequest(OBJECT_TYPE.Box, spawnPos);
        }
    }

    /// <summary>
    /// オブジェクト生成実行
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="spawnPos"></param>
    void SpawnObject(GameObject prefab, Vector2 spawnPos, string uniqueId)
    {
        var obj = Instantiate(prefab, spawnPos, Quaternion.identity);
        obj.GetComponent<GimmickBase>().UniqueId = uniqueId;
        GimmickManager.Instance.AddGimmickFromList(uniqueId, obj.GetComponent<GimmickBase>());
    }

    /// <summary>
    /// オブジェクト生成通知
    /// </summary>
    /// <param name="type"></param>
    /// <param name="spawnPos"></param>
    /// <param name="uniqueId"></param>
    void OnSpawnObject(OBJECT_TYPE type, Vector2 spawnPos, string uniqueId)
    {
        switch (type)
        {
            case OBJECT_TYPE.Box:
                SpawnObject(BoxPrefab, spawnPos, uniqueId);
                break;
        }
    }

    /// <summary>
    /// オブジェクト生成リクエスト
    /// </summary>
    /// <param name="type"></param>
    /// <param name="spawnPos"></param>
    public async void SpawnObjectRequest(OBJECT_TYPE type, Vector2 spawnPos)
    {
        if (RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            await RoomModel.Instance.SpawnObjectAsync(type, spawnPos);
        }
        else
        {
            // オフライン用
            SpawnObject(BoxPrefab, spawnPos, Guid.NewGuid().ToString());
        }
    }
}
