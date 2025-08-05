using UnityEngine;

public class BackGround : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Camera.main.gameObject.GetComponent<CameraFollow>().Target)
        {
            GameObject player = Camera.main.gameObject.GetComponent<CameraFollow>().Target.gameObject;
            //BackCity‚Ì“®‚«
            this.transform.GetChild(0).position = new Vector2(-(player.transform.position.x / 7), 3.25f);

            //FlontCity‚Ì“®‚«
            this.transform.GetChild(1).position = new Vector2(-(player.transform.position.x / 4), -1.6f);
        }
    }
}
