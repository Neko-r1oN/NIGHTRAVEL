using UnityEngine;

public class Re_RoomManager : MonoBehaviour
{
    [SerializeField] GameObject roomJoinButton;
    [SerializeField] GameObject roomCreateButton;
    [SerializeField] GameObject roomJoinPanel;
    [SerializeField] GameObject roomCreatepanel;

    //���������҂𔻒f
    private static bool isCreater = false;
    public static bool IsCreater
    { get { return isCreater; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isCreater = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// �쐬�{�^�����������ꍇ�A�Q���p�l�����\���ɂ��쐬�p�l����\������
    /// </summary>
    public void OnCreateClickButton()
    {
        roomJoinPanel.SetActive(false);
        roomCreatepanel.SetActive(true);
        isCreater = true;
    }

    /// <summary>
    /// �Q���{�^�����������ꍇ�A�쐬�p�l�����\���ɂ��Q���p�l����\������
    /// </summary>
    public void OnJoinClickButton()
    {
        roomCreatepanel.SetActive(false);
        roomJoinPanel.SetActive(true);
        isCreater = false;
    }
}
