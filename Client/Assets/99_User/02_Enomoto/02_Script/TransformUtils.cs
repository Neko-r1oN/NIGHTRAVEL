//**************************************************
//  Transform（回転・座標・スケールなど）ユーティリティクラス
//  Author:r-enomoto
//**************************************************
using UnityEngine;

public static class TransformUtils
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

    /// <summary>
    /// 指定した座標が範囲内かどうかチェック
    /// </summary>
    /// <param name="position"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool IsWithinBounds(Vector2 position, Vector2 min, Vector2 max)
    {
        return position.x >= min.x && position.x <= max.x &&
               position.y >= min.y && position.y <= max.y;
    }

    /// <summary>
    /// 指定した座標が範囲内かどうかチェック
    /// </summary>
    /// <param name="position"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool IsWithinBounds(Transform position, Transform min, Transform max)
    {
        return position.position.x >= min.position.x && position.position.x <= max.position.x &&
               position.position.y >= min.position.y && position.position.y <= max.position.y;
    }
}