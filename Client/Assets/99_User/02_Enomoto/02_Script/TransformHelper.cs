//**************************************************
//  Transform（回転・スケールなど）関連処理
//  Author:r-enomoto
//**************************************************
using UnityEngine;

public static class TransformHelper
{
    /// <summary>
    /// 現在向いている方向を取得
    /// </summary>
    /// <param name="scaleX"></param>
    /// <returns></returns>
    public static float GetFacingDirection(Transform transform)
    {
        return (float)Mathf.Clamp(transform.localScale.x, -1, 1);
    }
}
