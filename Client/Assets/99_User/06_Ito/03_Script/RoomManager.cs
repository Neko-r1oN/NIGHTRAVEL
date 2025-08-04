using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] GameObject roomJoinButton;
    [SerializeField] GameObject roomCreateButton;
    [SerializeField] GameObject roomJoinPanel;
    [SerializeField] GameObject roomCreatepanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
    }

    /// <summary>
    /// �Q���{�^�����������ꍇ�A�쐬�p�l�����\���ɂ��Q���p�l����\������
    /// </summary>
    public void OnJoinClickButton()
    {
        roomCreatepanel.SetActive(false);
        roomJoinPanel.SetActive(true);
    }
}
