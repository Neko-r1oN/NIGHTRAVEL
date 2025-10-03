//--------------------------------------------------------------
// 回復オブジェ [ HealObject.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using DG.Tweening;
using UnityEngine;

public class HealObject : MonoBehaviour
{
    [SerializeField] public float healRate = 0.03f;
    float time = 0;
    float destroyTime = 5.0f;

    private void Update()
    {
        time += Time.deltaTime;

        if (time >= destroyTime)
        {
            Destroy(this.gameObject);
        }
    }
}
