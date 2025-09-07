using UnityEngine;
using static CharacterBase;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class BoxBullet : ProjectileBase
{
    bool isDead = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        // レイヤーが地形やギミックの場合
        if (collision.gameObject.layer == 0 || collision.gameObject.layer == 13)
        {
            isDead = true;
            InvokeRepeating("Destroy", 0, 0.2f);
        }
        else if (collision.gameObject.tag == "Player")
        {
            DEBUFF_TYPE? debuff = null;
            if(debuffs.Count > 0) debuff = debuffs[0];
            collision.gameObject.GetComponent<PlayerBase>().ApplyDamage(power, null, KB_POW.Medium, debuff);
        }
    }

    private void Destroy()
    {
        var spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (spriteRenderer.color.a <= 0)
        {
            Destroy(this.gameObject);
        }
        spriteRenderer.color = new Color(1, 1, 1, spriteRenderer.color.a - 0.2f);
    }
}
