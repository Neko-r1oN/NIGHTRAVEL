//--------------------------------------------------------------
// �_���[�W�e�L�X�g�A�j���[�V���� [ DammageTextAnimation.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class DamageTextMotion : MonoBehaviour
{
    /// <summary>
    /// ��������
    /// </summary>
    void Start()
    {
        var pos = transform.position + new Vector3(0,20.0f,0);
        var sequence = DOTween.Sequence(); //Sequence����
        sequence.Append(transform.DOMove(pos, 0.5f))
                .Join(GetComponent<Text>().DOFade(0, 0.5f))
                .Join(GetComponent<Outline>().DOFade(0, 0.5f))
                .AppendCallback(() =>
                {
                    Destroy(gameObject);
                });
    }
}
