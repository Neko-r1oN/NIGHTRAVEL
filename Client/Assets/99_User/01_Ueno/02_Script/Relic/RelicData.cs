using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class RelicData
{
    public RELIC_TYPE ID { get; private set; }
    public RARITY_TYPE Rarity { get; private set; }
    public string Name { get; set; }
    public string ExplanationText { get; set; }

    public RelicData(RELIC_TYPE id, RARITY_TYPE rarity,string name, string explanationText)
    {
        ID = id;
        Rarity = rarity;
        Name = name;
        ExplanationText = explanationText;
    }
}
