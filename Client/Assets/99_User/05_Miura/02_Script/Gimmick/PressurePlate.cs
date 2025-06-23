//=================
//�����̃X�N���v�g
//Aouther:y-miura
//Date:20025/06/18
//=================

using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] GameObject gameObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //���ŃI�u�W�F�N�g��ݒu���āA�ŏ��͔�\����Ԃɂ���
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// �v���C���[�܂��͓G�������ł𓥂񂾏ꍇ�̏���
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player")||collision.gameObject.CompareTag("Enemy"))
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// �v���C���[�܂��͓G�������ł��痣�ꂽ�ꍇ�̏���
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            gameObject.SetActive(false);
        }
    }
}
