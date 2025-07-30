using UnityEngine;

public static class CalculationLibrary
{
    public static int CalcDamage(int power,int defense)
    {
        int result = (int)((power / 2) - (defense / 4));
        return result <= 0 ? 0 : result;
    }
}
