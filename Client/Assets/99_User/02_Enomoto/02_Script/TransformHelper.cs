//**************************************************
//  Transform�i��]�E�X�P�[���Ȃǁj�֘A����
//  Author:r-enomoto
//**************************************************
using UnityEngine;

public static class TransformHelper
{
    /// <summary>
    /// ���݌����Ă���������擾
    /// </summary>
    /// <param name="scaleX"></param>
    /// <returns></returns>
    public static float GetFacingDirection(Transform transform)
    {
        return (float)Mathf.Clamp(transform.localScale.x, -1, 1);
    }
}
