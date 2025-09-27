using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class PlayerStatusData
{
    public int NowLevel { get; set; }
    public int NowExp { get; set; }
    public int NextLevelExp { get; set; }
    public CharacterStatusData CharacterMaxStatusData { get; set;}
    public PlayerRelicStatusData PlayerRelicStatusData { get; set; }
}
