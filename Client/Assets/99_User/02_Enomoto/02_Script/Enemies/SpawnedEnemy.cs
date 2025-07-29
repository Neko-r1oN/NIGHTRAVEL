//**************************************************
//  スポーンした敵のデータクラス
//  Author:r-enomoto
//**************************************************
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class SpawnedEnemy
{
    public int UniqueId { get; private set; }    // 個体識別用ID
    public GameObject Object { get; private set; }
    public EnemyBase Enemy { get; private set; }
    public EnumManager.SPAWN_ENEMY_TYPE SpawnType { get; private set; }

    public SpawnedEnemy(int uniqueId, GameObject gameObject, EnemyBase enemy, EnumManager.SPAWN_ENEMY_TYPE spawnType)
    {
        this.UniqueId = uniqueId;
        this.Object = gameObject;
        this.Enemy = enemy;
        this.SpawnType = spawnType;
    }
}
