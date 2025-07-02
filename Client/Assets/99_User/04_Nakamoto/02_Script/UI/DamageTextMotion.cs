//--------------------------------------------------------------
// ダメージテキストアニメーション [ DammageTextAnimation.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class DamageTextMotion : MonoBehaviour
{
    /// <summary>
    /// 初期処理
    /// </summary>
    void Start()
    {
        var pos = transform.position + new Vector3(0,20.0f,0);
        var sequence = DOTween.Sequence(); //Sequence生成
        sequence.Append(transform.DOMove(pos, 0.5f))
                .Join(GetComponent<Text>().DOFade(0, 0.5f))
                .Join(GetComponent<Outline>().DOFade(0, 0.5f))
                .AppendCallback(() =>
                {
                    Destroy(gameObject);
                });
    }
}
