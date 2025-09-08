using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class RelicData
{
    public string ID { get; private set; }
    public RARITY_TYPE Rarity { get; private set; }
    public string Name { get; private set; }

    public RelicData(string id, RARITY_TYPE rarity)
    {
        ID = id;
        Rarity = rarity;
    }
}
