//--------------------------------------------------------------
// プレイヤーエフェクト [ PlayerEffect.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using UnityEngine;

public class PlayerEffect : MonoBehaviour
{
    //------------------------------
    // フィールド

    /// <summary>
    /// ダッシュエフェクト
    /// </summary>
    [SerializeField] private ParticleSystem dashEffect = null;

    /// <summary>
    /// ビームエフェクト
    /// </summary>
    [SerializeField] private GameObject beamEffect = null;

    /// <summary>
    /// アニメーター
    /// </summary>
    [SerializeField]　private Animator animator;

    private const int ANIMATION_ID_DASH = 2;

    //------------------------------
    // メソッド

    /// <summary>
    /// 更新処理
    /// </summary>
    void Update()
    {
        // 走っている時に土煙を起こす
        if (animator.GetInteger("animation_id") == ANIMATION_ID_DASH)
        {
            if (!dashEffect.isPlaying) dashEffect.Play();
        }
        else
        {
            if (dashEffect.isPlaying) dashEffect.Stop();
        }
    }

    /// <summary>
    /// ビームエフェクト表示切替
    /// </summary>
    /// <param name="isActive"></param>
    public void BeamEffectActive(bool isActive)
    {
        if (beamEffect != null) beamEffect.SetActive(isActive);
    }
}
