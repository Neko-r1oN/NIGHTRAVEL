//====================
// ����Ƃ��锠���Ǘ�����X�N���v�g
// Aouther:y-miura
// Date:2025/07/20
//====================

using System;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class BoxManager : MonoBehaviour
{
    [SerializeField] GameObject BoxPrefab;  //���v���n�u�擾
    const float leftPosX = 26.98f;
    const float rightPosX = 28.95f;
    const float posY = 27f;

    public float spawnTime;

    void Start()
    {
        //SpawnBox�֐����J��Ԃ��Ăяo���āA�����J��Ԃ���������
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
    /// ���𐶐����鏈��
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
    /// �I�u�W�F�N�g�������s
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
    /// �I�u�W�F�N�g�����ʒm
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
    /// �I�u�W�F�N�g�������N�G�X�g
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
            // �I�t���C���p
            SpawnObject(BoxPrefab, spawnPos, Guid.NewGuid().ToString());
        }
    }
}
