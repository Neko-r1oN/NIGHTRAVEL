using UnityEngine;
using DG.Tweening;

public class BoxJoint : MonoBehaviour
{
    [SerializeField]
    float endValue = 0.3f;

    [SerializeField]
    float endDuration = 1.0f;

    void Start()
    {
        transform.DOLocalMoveY(endValue, endDuration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo).Play();
    }
}
