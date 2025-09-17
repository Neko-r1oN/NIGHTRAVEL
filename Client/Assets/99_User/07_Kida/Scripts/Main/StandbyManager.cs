using Shared.Interfaces.StreamingHubs;
using System;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class StandbyManager : MonoBehaviour
{
    [SerializeField] Text setText;
    [SerializeField] int characterId;//�V�}�b�`���O�p�̃L�����N�^�[ID
    [SerializeField] GameObject[] characters;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RoomModel.Instance.OnJoinedUser += OnJoinedUser;
        //���[�U�[���ގ���������OnLeavedUser���\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnLeavedUser += this.OnLeavedUser;
        //���[�U�[������������������OnReady���\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnReadySyn += this.OnReadySyn;
        //�Q�[���J�n���o�����Ԃ̎��Ƀ��\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnStartedGame += this.OnStartedGame;
        //�Q�[���J�n���o�����Ԃ̎��Ƀ��\�b�h�����s����悤�A���f���ɓo�^
        RoomModel.Instance.OnChangedMasterClient += this.OnChangedMasterClient;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        RoomModel.Instance.OnLeavedUser -= this.OnLeavedUser;
        RoomModel.Instance.OnReadySyn -= this.OnReadySyn;
        RoomModel.Instance.OnStartedGame -= this.OnStartedGame;

    }

    public void ChangeCharacter()
    {
        
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
        Debug.Log("�Q�[�����J�n���܂�");
        SceneManager.LoadScene("Stage Kida");
    }

    public void OnChangedMasterClient()
    {
        Debug.Log("���Ȃ����}�X�^�[�N���C�A���g�ɂȂ�܂����B");
        RoomModel.Instance.IsMaster = true;
        //SpawnManager.Instance.SpawnCnt = CharacterManager.Instance.Enemies.Count;
    }


}
