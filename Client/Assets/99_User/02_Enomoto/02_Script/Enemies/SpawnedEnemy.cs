//**************************************************
//  スポーンした敵のデータクラス
//  Author:r-enomoto
//**************************************************
using Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;

public class SpawnedEnemy
{
    public string UniqueId { get; private set; }    // 個体識別用ID
    public GameObject Object { get; private set; }
    public EnemyBase Enemy { get; private set; }
    public EnumManager.SPAWN_ENEMY_TYPE SpawnType { get; private set; }
    public int TerminalID { get; set; } = -1; // 生成された端末ID。端末生成タイプの敵にのみ設定される

    public SpawnedEnemy(string uniqueId, GameObject gameObject, EnemyBase enemy, EnumManager.SPAWN_ENEMY_TYPE spawnType, int? termID = null)
    {
        this.UniqueId = uniqueId;
        this.Object = gameObject;
        this.Enemy = enemy;
        this.SpawnType = spawnType;
        this.TerminalID = termID==null ? -1 : (int)termID;
    }
}
