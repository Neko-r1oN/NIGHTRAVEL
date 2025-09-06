using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class EnemySpawnEntry
{
    public EnumManager.ENEMY_TYPE? EnemyType { get; private set; }
    public Vector3 Position { get; private set; }  
    public Vector3 Scale {  get; private set; }
    public string PresetId { get; private set; }    // –‘O‚Éİ’è‚³‚ê‚Ä‚ ‚é¯•Ê—pID

    public EnemySpawnEntry(EnumManager.ENEMY_TYPE? enemyType, Vector3 position, Vector3 scale, string presetId = "")
    {
        EnemyType = enemyType;
        Position = position;
        Scale = scale;
        PresetId = presetId;
    }
}
