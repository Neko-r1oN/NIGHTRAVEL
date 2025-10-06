using NUnit.Framework;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class StandbyManager : MonoBehaviour
{
    [SerializeField] int characterId;//�V�}�b�`���O�p�̃L�����N�^�[ID
    [SerializeField] GameObject[] characters;
    [SerializeField] SceneConducter conducter;
    [SerializeField] GameObject fade;
    [SerializeField] GameObject[] characterImage;
    [SerializeField] GameObject playerReadyFieldPrehub;
    [SerializeField] GameObject readyButton;
    [SerializeField] Text characterNameText;
    [SerializeField] Image[] iconImages;
    [SerializeField] Sprite[] iconCharacterImage;
    [SerializeField] Text logTextPrefab;

    [SerializeField] GameObject logs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RoomModel.Instance.OnJoinedUser += OnJoinedUser;
        //���[�U�[���ގ���������OnLeavedUser���\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnLeavedUser += this.OnLeavedUser;
        //���[�U�[���L�����N�^�[�ύX�������ꍇ��
        RoomModel.Instance.OnChangedCharacter += this.OnChangeIcon;
        //���[�U�[������������������OnReady���\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnReadySyn += this.OnReadySyn;
        //�Q�[���J�n���o�����Ԃ̎��Ƀ��\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnStartedGame += this.OnStartedGame;

        for (int i = 0; i < RoomModel.Instance.joinedUserList.Count; i++) 
        {
            Instantiate(playerReadyFieldPrehub);
        }

    }

    private void OnDisable()
    {
        RoomModel.Instance.OnJoinedUser -= OnJoinedUser;
        RoomModel.Instance.OnLeavedUser -= this.OnLeavedUser;
        RoomModel.Instance.OnReadySyn -= this.OnReadySyn;
        RoomModel.Instance.OnStartedGame -= this.OnStartedGame;
    }

    public async void ReturnMaching()
    {
        await RoomModel.Instance.LeavedAsync();
        Initiate.DoneFading();
        Initiate.Fade("2_MultiRoomScene", Color.black, 1.0f);   // �t�F�[�h����1�b
    }

    public async void ChangeCharacter(int changeCharacterId)
    {
        readyButton.SetActive(true);                                //���������{�^����\��
        characterImage[characterId].SetActive(false);               //�O�̃L�������r���[��\��
        characterImage[changeCharacterId].SetActive(true);          //�V�����L�������r���[��\��
        this.characterId = changeCharacterId;                       //�L�����N�^�[�h�c���ŐV��
        characterNameText.text = characterImage[characterId].name;  //�L�����N�^�[�����ŐV��

        if (RoomModel.Instance)
            await RoomModel.Instance.ChangeCharacterAsync(this.characterId);
        else
            ChangeIcon(this.characterId);
    }

    /// <summary>
    /// �A�C�R���\��
    /// �������������ۂɉ����ꂽ�L�����̃{�^����ID����A�C�R���̉摜��ύX���銴��
    /// �g���ϐ� 
    /// Image[] iconImages (�A�C�R���̃C���[�W)
    /// Material[] iconCharacterImage (�\��t�������摜)
    /// ��FiconImages[������ID].material = iconCharacterImage[changeIconId]
    /// </summary>
    /// <param name="changeIconId"></param>
    public void ChangeIcon(int changeIconId)
    {
        iconImages[MatchingManager.UserID].sprite = iconCharacterImage[changeIconId];
    }

    /// <summary>
    /// �A�C�R���\���ʒm
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="changeIconId"></param>
    public void OnChangeIcon( Guid guid,int changeIconId)
    {
        iconImages[RoomModel.Instance.joinedUserList[guid].JoinOrder-1].sprite = iconCharacterImage[changeIconId];
    }


    private void Loaded()
    {
        conducter.Loaded();
    }


    /// <summary>
    /// �ގ�����
    /// Aughter:�ؓc�W��
    /// </summary>
    public async void LeaveRoom()
    {
        //RoomHub�̑ގ��������Ăяo��
        await RoomModel.Instance.LeavedAsync();
    }

    /// <summary>
    /// ������������
    /// Aughter:�ؓc�W��
    /// </summary>
    public async void Ready()
    {
        //�L�����N�^�[�𑗂�
        int character = characterId;
        await RoomModel.Instance.ReadyAsync(character);
    }


    /// <summary>
    /// �ҋ@��ʂ���}���`�I����ʂɈړ�
    /// </summary>
    //public void BackOnlineButton()
    //{
    //    //SceneManager.LoadScene("OnlineMultiScene");
    //    if (CharacterManager.Instance.PlayerObjs.Values.Count > 1)
    //    {
    //        Initiate.Fade("OnlineMultiScene", Color.black, 1.0f);   // �t�F�[�h����1�b
    //    }
    //    else
    //    {
    //        Initiate.Fade("Title Ueno", Color.black, 1.0f);   // �t�F�[�h����1�b
    //    }
    //}

    //public void SetReady()
    //{
    //    if (CharacterManager.Instance.PlayerObjs.Values.Count > 1)
    //    {
    //        setText.text = "��������";
    //    }
    //    else
    //    {
    //        setText.text = "GAME START";
    //    }
    //}

    //public void ChangeScene()
    //{
    //    Initiate.Fade("Stage Ueno", Color.black, 1.0f);   // �t�F�[�h����1�b
    //}

    /// <summary>
    /// ���������ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnJoinedUser(JoinedUser joinedUser)
    {
        foreach (var data in RoomModel.Instance.joinedUserList.Values)
        {
            //���������Ƃ��̏���������
            Debug.Log(data.UserData.Name + "���������܂����B");
            Text gameObject = Instantiate(logTextPrefab);
            gameObject.text = data.UserData.Name+ "���������܂����B";
            gameObject.transform.position = logs.transform.position;
        }
    }

    /// <summary>
    /// �ގ������ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnLeavedUser(JoinedUser joinedUser)
    {
        //�ގ������Ƃ��̏���������
        Debug.Log(joinedUser.UserData.Name + "���ގ����܂����B");
        Text gameObject = Instantiate(logTextPrefab);
        gameObject.text = joinedUser.UserData.Name + "���ގ����܂����B";
        gameObject.transform.position = logs.transform.position;
    }

    /// <summary>
    /// ���������ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnReadySyn(Guid guid)
    {
        //�������������Ƃ��̏���������
        Debug.Log(guid.ToString() + "���������I�I");
    }

    /// <summary>
    /// �Q�[���J�n�ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnStartedGame()
    { 
        //�Q�[���J�n�̎��̏���������
        conducter.Loading();
        Debug.Log("�Q�[�����J�n���܂�");
        //SceneManager.LoadScene("4_Stage_01");
        SceneManager.LoadScene("4_Stage_01");
        Invoke("Loaded", 1.0f);

    }
}
