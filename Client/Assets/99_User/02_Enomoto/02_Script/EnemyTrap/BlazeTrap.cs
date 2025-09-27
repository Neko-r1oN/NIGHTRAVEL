//**************************************************
//  �G���[�g��(�u���C�Y)�ȓG�ɂ�鉊�̃g���b�v�̃X�N���v�g
//  Author:r-enomoto
//**************************************************
using static Shared.Interfaces.StreamingHubs.EnumManager;
using System.Collections.Generic;
using UnityEngine;

public class BlazeTrap : MonoBehaviour
{
    [SerializeField] List<ParticleSystem> particles = new List<ParticleSystem>();
    CapsuleCollider2D selfCollider;
    readonly float baseShapeRadius = 0.5f;
    readonly float baseEmissionRateOverTime = 12f;

    [SerializeField]
    float aliveTime = 1.0f;
    float timer;

    GameObject owner;

    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="owner"></param>
    public void InitializeBlazeTrap(GameObject owner)
    {
        this.owner = owner;
        var ownerCollider = owner.GetComponent<CapsuleCollider2D>();
        var radius = ownerCollider.bounds.size.x / 2;
        transform.localScale = owner.transform.localScale;
        selfCollider = GetComponent<CapsuleCollider2D>();
        selfCollider.size = new Vector2(radius * 2, selfCollider.size.y); // �R���C�_�[�̉��T�C�Y���p�[�e�B�N�������͈͂ɍ��킹��

        // owner�̃R���C�_�[�T�C�Y���Q�Ƃ��ăp�[�e�B�N���̃p�����[�^��ݒ�
        foreach (ParticleSystem particle in particles)
        {
            // �p�[�e�B�N���̊J�n�T�C�Y�𒲐�
            var main = particle.main;
            float minSize = ownerCollider.bounds.size.y / 4;
            float maxSize = ownerCollider.bounds.size.y / 2;
            main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);

            // �p�[�e�B�N���̐����͈́E�����̔䗦�𒲐�
            var shape = particle.shape;
            var emission = particle.emission;
            shape.radius = radius;
            emission.rateOverTime = baseEmissionRateOverTime * (radius / baseShapeRadius);    // �����͈͂ɔ�Ⴕ�āA�������𒲐�
        }

        // �����҂̑����ɍ��W�����炷
        Vector2 leftDown = selfCollider.bounds.min;
        var footPosition = new Vector2(owner.transform.position.x, leftDown.y);
        transform.position = footPosition;
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        // �j������Ƃ��ɁA�R���C�_�[���A�N�e�B�u�E�p�[�e�B�N���̍Đ����~
        if (timer > aliveTime && selfCollider.enabled)
        {
            foreach (ParticleSystem particle in particles)
            {
                var main = particle.main;
                main.loop = false;
                particle.Stop();
            }
            selfCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (owner == null || owner == collision.gameObject) return;

        string tag = collision.gameObject.tag;
        if (tag == "Player")
        {
            // ������ʂ�t�^����
            var controller = collision.gameObject.GetComponent<DebuffController>();
            if (controller) controller.ApplyStatusEffect(DEBUFF_TYPE.Burn);
        }
    }
}
