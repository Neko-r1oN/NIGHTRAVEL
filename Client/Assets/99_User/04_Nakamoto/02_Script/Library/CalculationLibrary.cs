using UnityEngine;

public static class CalculationLibrary
{
    /// <summary>
    /// ダメージ計算
    /// </summary>
    /// <param name="power">攻撃力</param>
    /// <param name="defense">相手の防御力</param>
    /// <param name="magnification">ダメージ倍率</param>
    /// <returns></returns>
    public static int CalcDamage(int power,int defense)
    {
        int result = (int)((power / 2) - (defense / 4));
        return result <= 0 ? 0 : result;
    }
}
