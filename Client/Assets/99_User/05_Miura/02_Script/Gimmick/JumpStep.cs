//===================
//�W�����v��̃X�N���v�g
//Author:y-miura
//===================

using UnityEngine;

public class JumpStep : MonoBehaviour
{
    [SerializeField] AudioClip jumpStepSE;
    AudioSource audioSource;

    //������̗͂ʂ̕ϐ�
    public float addPow;

    private void Start()
    {
        audioSource=GetComponent<AudioSource>();
    }

    /// <summary>
    /// �G�ꂽ�I�u�W�F�N�g�ɗ͂������鏈��
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            //�Ԃ������I�u�W�F�N�g�ɁAaddPow���̗͂�������
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, addPow));

            //�W�����v����ʉ��Đ�
            audioSource.PlayOneShot(jumpStepSE);
        }
    }
}
