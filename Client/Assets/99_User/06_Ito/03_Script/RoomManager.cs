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
    /// 作成ボタンを押した場合、参加パネルを非表示にし作成パネルを表示する
    /// </summary>
    public void OnCreateClickButton()
    {
        roomJoinPanel.SetActive(false);
        roomCreatepanel.SetActive(true);
    }

    /// <summary>
    /// 参加ボタンを押した場合、作成パネルを非表示にし参加パネルを表示する
    /// </summary>
    public void OnJoinClickButton()
    {
        roomCreatepanel.SetActive(false);
        roomJoinPanel.SetActive(true);
    }
}
