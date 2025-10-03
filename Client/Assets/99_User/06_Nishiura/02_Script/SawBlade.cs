//===================
// ソーブレードスクリプト
// Author:Nishiura
// Date:2025/07/02
//===================
using DG.Tweening;
using UnityEngine;

public class SawBlade : MonoBehaviour
{
    // 電源判定
    bool isPowerd;


    [SerializeField] AudioClip hitSE;
    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPowerd == true && collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            audioSource.PlayOneShot(hitSE);
            var playerBase = collision.gameObject.GetComponent<PlayerBase>();
            // プレイヤーの最大HP10%相当のダメージに設定
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.10f);
            playerBase.ApplyDamage(damage);
        }

        if (isPowerd == true && collision.transform.tag == "Enemy")
        {
            audioSource.PlayOneShot(hitSE);
            var enemyBase = collision.gameObject.GetComponent<EnemyBase>();
            // 敵の最大HP10%相当のダメージに設定
            int damage = Mathf.FloorToInt(enemyBase.MaxHP * 0.10f);
            enemyBase.ApplyDamage(damage,enemyBase.HP,null,true,true);
        }

    }

    /// <summary>
    /// 電源オン関数
    /// </summary>
    public void StateRotet()
    {
        isPowerd = true;
        // 回転させる
        transform.DOLocalRotate(new Vector3(0, 0, 360f), 0.25f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
                        .SetLoops(-1, LoopType.Restart);
    }
}
