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
    /// 初期化処理
    /// </summary>
    /// <param name="owner"></param>
    public void InitializeBlazeTrap(Transform owner)
    {
        var sqSr = owner.GetComponent<SpriteRenderer>();
        var radius = sqSr.bounds.size.x / 2;
        transform.localScale = owner.localScale;
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        capsuleCollider.size = new Vector2(radius * 2, capsuleCollider.size.y); // コライダーの横サイズをパーティクル生成範囲に合わせる

        if (sqSr != null)
        {
            // 生成者の横の長さ(スプライト)にサイズを合わせる
            foreach (ParticleSystem particle in particles)
            {
                var shape = particle.shape;
                var emission = particle.emission;
                shape.radius = radius;   // 0.1は調整
                emission.rateOverTime = baseEmissionRateOverTime * (radius / baseShapeRadius);    // 生成範囲に比例して、生成数を調整
            }

            // 生成者の足元に座標をずらす
            Vector2 leftDown = sqSr.bounds.min;
            var footPosition = new Vector2(owner.position.x, leftDown.y);
            transform.position = footPosition;
        }
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        // 破棄するときに、コライダーを非アクティブ・パーティクルの再生を停止
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
            // 炎上効果を付与する
            var controller = collision.gameObject.GetComponent<StatusEffectController>();
            if (controller) controller.ApplyStatusEffect(StatusEffectController.EFFECT_TYPE.Burn);
        }
    }
}
