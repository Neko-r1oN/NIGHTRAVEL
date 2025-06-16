using UnityEngine;

static public class ParticleHelper
{
    /// <summary>
    /// Shape���W���[���̔��a���ASprite�̉��̃T�C�Y�ɍ��킹�鏈��
    /// </summary>
    /// <param name="particle"></param>
    /// <param name="radius"></param>
    /// <param name="offset"></param>
    static public void MatchRadiusToSpriteWidth(CapsuleCollider2D capsule2D, ParticleSystem particle, float offset = 0)
    {
        var shape = particle.shape;
        shape.radius = capsule2D.bounds.size.x / 2 + offset;
        //Debug.Log("���̃T�C�Y�F" + capsule2D.bounds.size.x);
        //Debug.Log("�c�̃T�C�Y�F" + capsule2D.bounds.size.y);
    }

    /// <summary>
    /// Emission���W���[���̎��Ԃ��Ƃ̗����ASprite �̉����ɔ�Ⴕ�Ē������鏈��
    /// </summary>
    /// <param name="spriteRenderer"></param>
    /// <param name="particle"></param>
    static public void AdjustRateOverTimeBySpriteWidth(CapsuleCollider2D capsule2D, ParticleSystem particle, float baseTime)
    {
        var emission = particle.emission;
        emission.rateOverTime = baseTime * capsule2D.bounds.size.x;
        //Debug.Log("���̃T�C�Y�F" + capsule2D.bounds.size.x);
        //Debug.Log("�c�̃T�C�Y�F" + capsule2D.bounds.size.y);
    }
}
