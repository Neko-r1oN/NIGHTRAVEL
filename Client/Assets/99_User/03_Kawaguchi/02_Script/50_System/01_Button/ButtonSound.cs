using UnityEngine;
using KanKikuchi.AudioManager;

public class ButtonSound : MonoBehaviour
{
    /// <summary>
    /// �N���b�N���Đ��֐�
    /// </summary>
    public void ForcusButton()
    {
        SEManager.Instance.Play(
            audioPath: SEPath.BUTTON_FOCUS, //�Đ��������I�[�f�B�I�̃p�X
            volumeRate: 1,                //���ʂ̔{��
            delay: 0,                //�Đ������܂ł̒x������
            pitch: 1,                //�s�b�`
            isLoop: false,             //���[�v�Đ����邩
            callback: null              //�Đ��I����̏���
        );
    }

    /// <summary>
    /// �N���b�N���Đ��֐�
    /// </summary>
    public void PushButton()
    {
        SEManager.Instance.Play(
            audioPath: SEPath.BUTTON_PUSH, //�Đ��������I�[�f�B�I�̃p�X
            volumeRate: 1,                //���ʂ̔{��
            delay: 0,                //�Đ������܂ł̒x������
            pitch: 1,                //�s�b�`
            isLoop: false,             //���[�v�Đ����邩
            callback: null              //�Đ��I����̏���
        );
    }
}
