//===================
// �\�[�u���[�h�X�N���v�g
// Author:Nishiura
// Date:2025/07/02
//===================
using DG.Tweening;
using UnityEngine;

public class SawBlade : MonoBehaviour
{
    // �d������
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
            // �v���C���[�̍ő�HP10%�����̃_���[�W�ɐݒ�
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.10f);
            playerBase.ApplyDamage(damage);
        }

        if (isPowerd == true && collision.transform.tag == "Enemy")
        {
            audioSource.PlayOneShot(hitSE);
            var enemyBase = collision.gameObject.GetComponent<EnemyBase>();
            // �G�̍ő�HP10%�����̃_���[�W�ɐݒ�
            int damage = Mathf.FloorToInt(enemyBase.MaxHP * 0.10f);
            enemyBase.ApplyDamage(damage,enemyBase.HP,null,true,true);
        }

    }

    /// <summary>
    /// �d���I���֐�
    /// </summary>
    public void StateRotet()
    {
        isPowerd = true;
        // ��]������
        transform.DOLocalRotate(new Vector3(0, 0, 360f), 0.25f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
                        .SetLoops(-1, LoopType.Restart);
    }
}
