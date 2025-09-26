//**************************************************
//  エリート個体(ブレイズ)な敵による炎のトラップのスクリプト
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
    /// 初期化処理
    /// </summary>
    /// <param name="owner"></param>
    public void InitializeBlazeTrap(GameObject owner)
    {
        this.owner = owner;
        var ownerCollider = owner.GetComponent<CapsuleCollider2D>();
        var radius = ownerCollider.bounds.size.x / 2;
        transform.localScale = owner.transform.localScale;
        selfCollider = GetComponent<CapsuleCollider2D>();
        selfCollider.size = new Vector2(radius * 2, selfCollider.size.y); // コライダーの横サイズをパーティクル生成範囲に合わせる

        // ownerのコライダーサイズを参照してパーティクルのパラメータを設定
        foreach (ParticleSystem particle in particles)
        {
            // パーティクルの開始サイズを調整
            var main = particle.main;
            float minSize = ownerCollider.bounds.size.y / 4;
            float maxSize = ownerCollider.bounds.size.y / 2;
            main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);

            // パーティクルの生成範囲・生成の比率を調整
            var shape = particle.shape;
            var emission = particle.emission;
            shape.radius = radius;
            emission.rateOverTime = baseEmissionRateOverTime * (radius / baseShapeRadius);    // 生成範囲に比例して、生成数を調整
        }

        // 生成者の足元に座標をずらす
        Vector2 leftDown = selfCollider.bounds.min;
        var footPosition = new Vector2(owner.transform.position.x, leftDown.y);
        transform.position = footPosition;
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        // 破棄するときに、コライダーを非アクティブ・パーティクルの再生を停止
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
            // 炎上効果を付与する
            var controller = collision.gameObject.GetComponent<DebuffController>();
            if (controller) controller.ApplyStatusEffect(DEBUFF_TYPE.Burn);
        }
    }
}
