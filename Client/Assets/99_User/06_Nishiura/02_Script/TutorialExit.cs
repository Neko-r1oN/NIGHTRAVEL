using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialExit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            Initiate.DoneFading();
            Initiate.Fade("1_TitleScene", Color.black,0.5f);
        }
    }
}
