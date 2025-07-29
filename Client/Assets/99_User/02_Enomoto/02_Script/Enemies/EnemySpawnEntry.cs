using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class EnemySpawnEntry
{
    public EnumManager.ENEMY_TYPE? EnemyType { get; private set; }
    public Vector3 Position { get; private set; }  
    public Vector3 Scale {  get; private set; }

    public EnemySpawnEntry(EnumManager.ENEMY_TYPE? enemyType, Vector3 position, Vector3 scale)
    {
        EnemyType = enemyType;
        Position = position;
        Scale = scale;
    }
}
