using UnityEngine;

public class Re_RoomManager : MonoBehaviour
{
    [SerializeField] GameObject roomJoinButton;
    [SerializeField] GameObject roomCreateButton;
    [SerializeField] GameObject roomJoinPanel;
    [SerializeField] GameObject roomCreatepanel;

    //部屋生成者を判断
    private static bool isCreater;
    public static bool IsCreater
    { get { return isCreater; } }


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
        isCreater = true;
    }

    /// <summary>
    /// 参加ボタンを押した場合、作成パネルを非表示にし参加パネルを表示する
    /// </summary>
    public void OnJoinClickButton()
    {
        roomCreatepanel.SetActive(false);
        roomJoinPanel.SetActive(true);
        isCreater = false;
    }
}
