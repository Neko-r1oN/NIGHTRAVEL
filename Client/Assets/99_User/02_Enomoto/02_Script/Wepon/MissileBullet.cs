using UnityEngine;
using static CharacterBase;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class MissileBullet : ProjectileBase
{
    [SerializeField]
    GameObject explosionParticle;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // レイヤーが地形やギミックの場合
        if (collision.gameObject.layer == 0 || collision.gameObject.layer == 13)
        {
            Destroy();
        }
        else if (collision.gameObject.tag == "Player")
        {
            DEBUFF_TYPE? debuff = null;
            if (debuffs.Count > 0) debuff = debuffs[0];
            collision.gameObject.GetComponent<PlayerBase>().ApplyDamage(power, null, KB_POW.Medium, debuff);
            Destroy();
        }
    }

    protected override void Destroy()
    {
        Instantiate(explosionParticle, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
