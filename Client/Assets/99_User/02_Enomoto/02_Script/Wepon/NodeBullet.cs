using UnityEngine;
using static CharacterBase;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class NodeBullet : ProjectileBase
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ���C���[���n�`��M�~�b�N�̏ꍇ
        if (collision.gameObject.layer == 0 || collision.gameObject.layer == 13)
        {
            this.Destroy();
        }
        else if (collision.gameObject.tag == "Player")
        {
            DEBUFF_TYPE? debuff = null;
            if (debuffs.Count > 0) debuff = debuffs[0];
            collision.gameObject.GetComponent<PlayerBase>().ApplyDamage(power, null, KB_POW.Medium, debuff);
        }
    }

    protected override void Destroy()
    {
        // ���e�p�[�e�B�N������


        Destroy(this.gameObject);
    }
}
