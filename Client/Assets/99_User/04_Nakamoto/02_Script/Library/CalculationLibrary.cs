using UnityEngine;

public static class CalculationLibrary
{
    /// <summary>
    /// �_���[�W�v�Z
    /// </summary>
    /// <param name="power">�U����</param>
    /// <param name="defense">����̖h���</param>
    /// <param name="magnification">�_���[�W�{��</param>
    /// <returns></returns>
    public static int CalcDamage(int power,int defense)
    {
        int result = (int)((float)((float)power / 2) - (float)((float)defense / 4));
        return result <= 0 ? 0 : result;
    }
}
