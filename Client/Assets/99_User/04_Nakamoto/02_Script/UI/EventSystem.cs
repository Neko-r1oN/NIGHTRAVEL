using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class EventSystem : MonoBehaviour
{
    [SerializeField] private Button firstButton;

    void Start()
    {
        // �ŏ��ɑI�������{�^����ݒ�
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
    }
}
