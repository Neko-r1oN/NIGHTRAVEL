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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPowerd == true && collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            var playerBase = collision.gameObject.GetComponent<PlayerBase>();
            // �v���C���[�̍ő�HP30%�����̃_���[�W�ɐݒ�
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.30f);
            playerBase.ApplyDamage(damage);
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
