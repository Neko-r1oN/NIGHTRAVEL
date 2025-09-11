using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;                   //DOTween���g���Ƃ��͂���using������
using KanKikuchi.AudioManager;

public class TitleManagerk : MonoBehaviour
{

    [SerializeField] GameObject fade;
   // [SerializeField] GameObject menu;

    public static bool isMenuFlag;

    bool isSuccess;

    void Start()
    {
        //�S�Ă�BGM���t�F�[�h�A�E�g
        BGMManager.Instance.FadeIn(1.0f);
        //�S�Ă�BGM���t�F�[�h�A�E�g
        SEManager.Instance.FadeIn(1.0f);

        fade.SetActive(true);               //�t�F�[�h��L����
      //  menu.SetActive(false);              //���j���[���\��
        isMenuFlag = false;                 //���j���[�t���O�𖳌���

        //���[�J���̃��[�U�[�f�[�^���擾
       // isSuccess = UserModel.Instance.LoadUserData();

        //���[�J���̃��[�U�[�f�[�^���擾
       // isSuccess = OptionModel.Instance.LoadOptionData();

        //BGM�Đ�
        BGMManager.Instance.Play(
            audioPath: BGMPath.TITLE, //�Đ��������I�[�f�B�I�̃p�X
            volumeRate: 0.55f,                //���ʂ̔{��
            delay: 1.0f,                //�Đ������܂ł̒x������
            pitch: 1,                //�s�b�`
            isLoop: true,             //���[�v�Đ����邩
            allowsDuplicate: false             //����BGM�Əd�����čĐ������邩
        );

        SEManager.Instance.Play(
           audioPath: SEPath.TITLE_WIND, //�Đ��������I�[�f�B�I�̃p�X
           volumeRate: 0.1f,                //���ʂ̔{��
           delay: 0,                //�Đ������܂ł̒x������
           pitch: 1,                //�s�b�`
           isLoop: true,             //���[�v�Đ����邩
           callback: null              //�Đ��I����̏���
        );

        SEManager.Instance.Play(
           audioPath: SEPath.TITLE_NOISE, //�Đ��������I�[�f�B�I�̃p�X
           volumeRate: 0.1f,                //���ʂ̔{��
           delay: 0,                //�Đ������܂ł̒x������
           pitch: 1,                //�s�b�`
           isLoop: true,             //���[�v�Đ����邩
           callback: null              //�Đ��I����̏���
        );

        
    }

    public void OpenOptionButton()
    {
       // menu.SetActive(true);
        isMenuFlag = true;
        
    }

    public void CloseOptionButton()
    {
        SEManager.Instance.Play(
            audioPath: SEPath.SYSTEM20, //�Đ��������I�[�f�B�I�̃p�X
            volumeRate: 1,                //���ʂ̔{��
            delay: 0,                //�Đ������܂ł̒x������
            pitch: 1,                //�s�b�`
            isLoop: false,             //���[�v�Đ����邩
            callback: null              //�Đ��I����̏���
        );

        isMenuFlag = false;
        Invoke("CloseMenu", 0.5f);
        
    }

    void MuteSound()
    {
       // menu.SetActive(false);
    }

    public void OnClickStart()
    {

        //�S�Ă�BGM���t�F�[�h�A�E�g
        BGMManager.Instance.FadeOut(BGMPath.TITLE, 3, () => {
            Debug.Log("BGM�t�F�[�h�A�E�g�I��");
        });
        SEManager.Instance.FadeOut(SEPath.TITLE_WIND, 3, () => {
            Debug.Log("BGM�t�F�[�h�A�E�g�I��");
        });
        SEManager.Instance.FadeOut(SEPath.TITLE_NOISE, 3, () => {
            Debug.Log("BGM�t�F�[�h�A�E�g�I��");
        });
    }
}

