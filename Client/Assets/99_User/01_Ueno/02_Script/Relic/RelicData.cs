using UnityEngine;

public class RelicData
{
    public int ID { get; private set; }
    public int Rarity { get; private set; }
    public string Name { get; private set; }

    public RelicData(int id, int rarity)
    {
        ID = id;
        Rarity = rarity;
    }
}
