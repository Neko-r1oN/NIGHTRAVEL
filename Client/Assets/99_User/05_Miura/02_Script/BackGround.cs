using UnityEngine;

public class BackGround : MonoBehaviour
{
    PlayerBase player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player=CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();
    }

    // Update is called once per frame
    void Update()
    {
        //BackCity‚Ì“®‚«
        this.transform.GetChild(0).position=new Vector2(-(player.transform.position.x/7), 3.25f);

        //FlontCity‚Ì“®‚«
        this.transform.GetChild(1).position = new Vector2(-(player.transform.position.x / 4), -1.6f);
    }
}
