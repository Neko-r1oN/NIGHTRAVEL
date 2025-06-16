using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BlazeTrap : MonoBehaviour
{
    [SerializeField] List<ParticleSystem> particles = new List<ParticleSystem>();
    CapsuleCollider2D capsuleCollider;
    readonly float baseShapeRadius = 0.5f;
    readonly float baseEmissionRateOverTime = 12f;

    [SerializeField]
    float aliveTime = 1.0f;
    float timer;

    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="owner"></param>
    public void InitializeBlazeTrap(Transform owner)
    {
        var sqSr = owner.GetComponent<SpriteRenderer>();
        var radius = sqSr.bounds.size.x / 2;
        transform.localScale = owner.localScale;
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        capsuleCollider.size = new Vector2(radius * 2, capsuleCollider.size.y); // �R���C�_�[�̉��T�C�Y���p�[�e�B�N�������͈͂ɍ��킹��

        if (sqSr != null)
        {
            // �����҂̉��̒���(�X�v���C�g)�ɃT�C�Y�����킹��
            foreach (ParticleSystem particle in particles)
            {
                var shape = particle.shape;
                var emission = particle.emission;
                shape.radius = radius;   // 0.1�͒���
                emission.rateOverTime = baseEmissionRateOverTime * (radius / baseShapeRadius);    // �����͈͂ɔ�Ⴕ�āA�������𒲐�
            }

            // �����҂̑����ɍ��W�����炷
            Vector2 leftDown = sqSr.bounds.min;
            var footPosition = new Vector2(owner.position.x, leftDown.y);
            transform.position = footPosition;
        }
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        // �j������Ƃ��ɁA�R���C�_�[���A�N�e�B�u�E�p�[�e�B�N���̍Đ����~
        if (timer > aliveTime && capsuleCollider.enabled)
        {
            foreach (ParticleSystem particle in particles)
            {
                var main = particle.main;
                main.loop = false;
                particle.Stop();
            }
            capsuleCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.gameObject.tag;
        if (tag == "Player")
        {
            // ������ʂ�t�^����
            var controller = collision.gameObject.GetComponent<StatusEffectController>();
            if (controller) controller.ApplyStatusEffect(StatusEffectController.EFFECT_TYPE.Burn);
        }
    }
}
