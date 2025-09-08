using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class EventSystem : MonoBehaviour
{
    [SerializeField] private Button firstButton;

    void Start()
    {
        // Å‰‚É‘I‘ğ‚³‚ê‚éƒ{ƒ^ƒ“‚ğİ’è
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
    }
}
