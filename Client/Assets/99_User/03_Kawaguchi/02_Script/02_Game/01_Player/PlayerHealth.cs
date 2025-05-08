using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI����ɕK�v

public class PlayerHealth : MonoBehaviour
{
    // �v���C���[�̍ő�HP�ƌ��݂�HP
    public int maxHp = 10;
    private int currentHp;

    // �X���C�_�[�̎Q��
    public Slider hpSlider;

    void Start()
    {
        // �����ݒ�
        currentHp = maxHp; // HP���ő�l�ɐݒ�
        hpSlider.maxValue = maxHp; // �X���C�_�[�̍ő�l��ݒ�
        hpSlider.value = currentHp; // ���݂�HP�𔽉f
    }

    public void TakeDamage(int damage)
    {
        // HP�����炷����
        currentHp -= damage;
        if (currentHp < 0) currentHp = 0;

        // �X���C�_�[�Ɍ��݂�HP�𔽉f
        hpSlider.value = currentHp;

        // HP��0�ɂȂ����Ƃ��̏���
        if (currentHp == 0)
        {
            Debug.Log("�Q�[���I�[�o�[�I");
            // �����ɃQ�[���I�[�o�[�̏�����ǉ�
        }
    }
}