using UnityEngine;

public static class CalculationLibrary
{
    public static int CalcDamage(int power,int defense)
    {
        return (int)((power / 2) - (defense / 4));
    }
}
