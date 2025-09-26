using System.Collections;
using UnityEngine;

public class ShortCircuit : MonoBehaviour
{
    [SerializeField] 
    AudioSource shortCircuitSE;

    [SerializeField] 
    Transform attackCheckPoint;

    [SerializeField]
    Vector2 attackRange = Vector2.zero;

    [SerializeField]
    bool canDrawRay = false;

    private void Start()
    {
        StartCoroutine(ApplyDamageCoroutine());
    }

    /// <summary>
    /// ダメージ適用コルーチン
    /// </summary>
    /// <returns></returns>
    IEnumerator ApplyDamageCoroutine()
    {
        const float waitSec = 0.5f;
        while (true)
        {
            bool isHit = false;

            // 範囲内に攻撃可能なキャラクターがいるかチェック
            Collider2D[] collidersEnemies = Physics2D.OverlapBoxAll(attackCheckPoint.transform.position, attackRange, 0);

            for (int i = 0; i < collidersEnemies.Length; i++)
            {
                var character = collidersEnemies[i].gameObject;

                if (character.tag == "Player" || character.tag == "Enemy")
                {
                    var damage = Mathf.FloorToInt(character.GetComponent<CharacterBase>().MaxHP * 0.05f);

                    if(character.tag == "Player") character.GetComponent<PlayerBase>().ApplyDamage(damage);
                    else if (character.tag == "Enemy") character.GetComponent<EnemyBase>().ApplyDamageRequest(damage);

                    isHit = true;
                }
            }

            if(isHit) shortCircuitSE.Play();
            yield return new WaitForSeconds(waitSec);
        }
    }

    private void OnDrawGizmos()
    {
        if (canDrawRay)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackCheckPoint.transform.position, attackRange);
        }
    }
}
