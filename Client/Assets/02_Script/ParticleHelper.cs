using UnityEngine;

static public class ParticleHelper
{
    /// <summary>
    /// Shapeモジュールの半径を、Spriteの横のサイズに合わせる処理
    /// </summary>
    /// <param name="particle"></param>
    /// <param name="radius"></param>
    /// <param name="offset"></param>
    static public void MatchRadiusToSpriteWidth(CapsuleCollider2D capsule2D, ParticleSystem particle, float offset = 0)
    {
        var shape = particle.shape;
        shape.radius = capsule2D.bounds.size.x / 2 + offset;
        //Debug.Log("横のサイズ：" + capsule2D.bounds.size.x);
        //Debug.Log("縦のサイズ：" + capsule2D.bounds.size.y);
    }

    /// <summary>
    /// Emissionモジュールの時間ごとの率を、Sprite の横幅に比例して調整する処理
    /// </summary>
    /// <param name="spriteRenderer"></param>
    /// <param name="particle"></param>
    static public void AdjustRateOverTimeBySpriteWidth(CapsuleCollider2D capsule2D, ParticleSystem particle, float baseTime)
    {
        var emission = particle.emission;
        emission.rateOverTime = baseTime * capsule2D.bounds.size.x;
        //Debug.Log("横のサイズ：" + capsule2D.bounds.size.x);
        //Debug.Log("縦のサイズ：" + capsule2D.bounds.size.y);
    }
}
