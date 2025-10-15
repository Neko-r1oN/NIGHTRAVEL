using UnityEngine;

public class PlayerUICollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (UIManager.Instance) UIManager.Instance.SetPlayerUIVisibility(false);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (UIManager.Instance) UIManager.Instance.SetPlayerUIVisibility(true);
    }
}
