//**************************************************
//  Transform�i��]�E���W�E�X�P�[���Ȃǁj���[�e�B���e�B�N���X
//  Author:r-enomoto
//**************************************************
using UnityEngine;

public static class TransformUtils
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

    /// <summary>
    /// �w�肵�����W���͈͓����ǂ����`�F�b�N
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
    /// �w�肵�����W���͈͓����ǂ����`�F�b�N
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