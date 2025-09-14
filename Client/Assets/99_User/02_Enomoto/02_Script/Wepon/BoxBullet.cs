using UnityEngine;
using static CharacterBase;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class BoxBullet : ProjectileBase
{
    bool isDead = false;
    Rigidbody2D rb2d;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        InvokeRepeating("Flip", 0, 0.2f);
    }

    void Flip()
    {
        var x  = Mathf.Abs(transform.localScale.x);
        var y  = transform.localScale.y;

        if (rb2d.linearVelocity.x < 0) transform.localScale = new Vector2(x * -1, y);
        else transform.localScale = new Vector2(x, y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // ’nŒ`‚âƒMƒ~ƒbƒN‚ÉG‚ê‚é‚±‚Æ‚Å”jŠü
        if (collision.gameObject.layer == 0 || collision.gameObject.layer == 13)
        {
            isDead = true;
            InvokeRepeating("Destroy", 0, 0.1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(isDead) return;

        if (collision.gameObject.tag == "Player")
        {
            DEBUFF_TYPE? debuff = null;
            if(debuffs.Count > 0) debuff = debuffs[0];
            collision.gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, KB_POW.Small, debuff);
        }
    }

    protected override void Destroy()
    {
        var spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (spriteRenderer.color.a <= 0)
        {
            Destroy(this.gameObject);
        }
        spriteRenderer.color = new Color(1, 1, 1, spriteRenderer.color.a - 0.2f);
    }
}
