using UnityEngine;

public class CompleteEffect : MonoBehaviour
{
    public void OnCompleteExplosionEffect()
    {
        Destroy(this.gameObject);
    }

}
