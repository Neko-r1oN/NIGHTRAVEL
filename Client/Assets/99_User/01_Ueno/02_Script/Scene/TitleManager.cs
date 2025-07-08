using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void ButtonPush()
    {
        SceneManager.LoadScene("Stage");
    }
}
