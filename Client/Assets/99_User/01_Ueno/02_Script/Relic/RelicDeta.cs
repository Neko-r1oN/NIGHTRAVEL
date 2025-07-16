using UnityEngine;

public class RelicDeta
{
    public int ID { get; private set; }
    public int Rarity { get; private set; }

    public RelicDeta(int iD, int rarity)
    {
        ID = iD;
        Rarity = rarity;
    }
}
