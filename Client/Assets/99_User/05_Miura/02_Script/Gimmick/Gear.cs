//==============================
// ベルトコンベアーの歯車のスクリプト
// Aouther:y-miura
// Date:2025/07/15
//==============================

using DG.Tweening;
using UnityEngine;

public class Gear : MonoBehaviour
{
    public float rotZ; //歯車の回転方向(Z軸)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.DOLocalRotate(new Vector3(0, 0, rotZ), 0.8f, RotateMode.FastBeyond360)
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Restart);

    }
}
