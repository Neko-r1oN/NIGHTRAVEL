using DG.Tweening;                   //DOTween���g���Ƃ��͂���using������
using KanKikuchi.AudioManager;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Net.Http;
using Cysharp.Threading.Tasks.Triggers;
using Grpc.Net.Client;
using MagicOnion.Client;
using NIGHTRAVEL.Shared.Interfaces.Services;
using Shared.Interfaces.StreamingHubs;
using UnityEngine.UI;
using System.Threading.Tasks;


public class TitleManagerk : MonoBehaviour
{
    [SerializeField] GameObject fade;
    [SerializeField] SceneConducter conducter;
    [SerializeField] GameObject roomModelPrefab;
    [SerializeField] GameObject license;
    [SerializeField] GameObject exitLicense;
    // [SerializeField] GameObject menu;

    //steam���[�U�[��
    private static string steamusername;
    public static string SteamUserName
    {
        get { return steamusername; }
    }

    //�Q�[���̃��[�h
    //�\�����[�h 0,�}���`�v���C 1,�`���[�g���A�� 2
    private static int gamemode;

    public static int GameMode
    {
        get { return gamemode; }
    }

    public static bool isMenuFlag;

    bool isSuccess;


    void Start()
    {

        //Steam�̃��[�U�[�����擾
        if (SteamManager.Initialized)
        {
            steamusername = SteamFriends.GetPersonaName();
            Debug.Log("Steam Username: " + steamusername);
            // �擾�������[�U�[�����Q�[�����ŕ\�����鏈�����L�q
        }
        else
        {
            Debug.LogError("Steamworks is not initialized.");
        }

        fade.SetActive(true);               //�t�F�[�h��L����
      //  menu.SetActive(false);            //���j���[���\��
        isMenuFlag = false;                 //���j���[�t���O�𖳌���
        license.SetActive(false);           //���C�Z���X�̉�ʂ��\��
        exitLicense.SetActive(false);       //���C�Z���X��ʂ����UI���\��

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

        //�S�Ă�BGM���t�F�[�h�C��
        BGMManager.Instance.FadeIn(1.0f);
        //�S�Ă�SE���t�F�[�h�C��
        SEManager.Instance.FadeIn(13.0f);

        //���[�����f��������Ȃ�폜
        Destroy(GameObject.Find("RoomModel"));

        // �ێ����Ă����e�f�[�^�����Z�b�g����
        CharacterManager.SelfPlayerStatusData = null;
        RelicManager.HaveRelicList = new List<RelicData>();
        LevelManager.GameLevel = 0;
        LevelManager.Options = new Dictionary<Guid, List<StatusUpgrateOptionData>>();

        Invoke("NewRoomModel", 0.1f);

    }

    void NewRoomModel()
    {
        if (GameObject.Find("RoomModel") != null) return;
        conducter.Loading();
        //���[�����f����������x�쐬
        Instantiate(roomModelPrefab);
        Invoke("Loaded", 1.0f);
    }

    private void OnDisable()
    {
        RoomModel.Instance.OnCreatedRoom -= OnCreatedRoom;
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

    void CloseMenu()
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

    /// <summary>
    /// �\�����[�h
    /// </summary>
    public async void SinglePlayStart()
    {
        await RoomModel.Instance.ConnectAsync();
        RoomModel.Instance.OnCreatedRoom += OnCreatedRoom;
        gamemode = 0;
        await RoomModel.Instance.JoinedAsync("�I�I�\�����[�h��p�̃��[���ł��I�I", MatchingManager.UserID, SteamUserName, "404",gamemode);
    }

    /// <summary>
    /// �\�����[�h�p���[���쐬�ʒm
    /// </summary>
    public void OnCreatedRoom()
    {
        Initiate.DoneFading();
        Initiate.Fade("3_StandbyRoom", Color.black, 1.0f);   // �t�F�[�h����1�b
    }

    /// <summary>
    /// �}���`�v���C
    /// </summary>
    public void MultiPlayStart()
    {
        gamemode = 1;
        Initiate.DoneFading();
        Initiate.Fade("2_MultiRoomScene", Color.black, 1.0f);   // �t�F�[�h����1�b
    }

    /// <summary>
    /// �`���[�g���A��
    /// </summary>
    public void TutorialPlayStart()
    {
        gamemode = 2;
        Initiate.DoneFading();
        Initiate.Fade("Tutorial", Color.black, 1.0f);   // �t�F�[�h����1�b
    }

    /// <summary>
    /// ���C�Z���X
    /// </summary>
    public void License()
    {
        license.SetActive(true);            //���C�Z���X��ʂ�\��
        exitLicense.SetActive(true);        //���C�Z���X��ʂ����UI��\��
    }

    /// <summary>
    /// ���C�Z���X�����
    /// </summary>
    public void ExitLicense()
    {
        license.SetActive(false);          //���C�Z���X��ʂ��\��
        exitLicense.SetActive(false);      //���C�Z���X��ʂ����UI���\�� 
    }

    /// <summary>
    /// �Q�[���I��
    /// </summary>
    public void ExitGame()
    {
#if UNITY_EDITOR
        // Unity�G�f�B�^�[�ł̓���
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ���ۂ̃Q�[���I������
        Application.Quit();
#endif
    }

    private void Loaded()
    {
        conducter.Loaded();
    }
}

