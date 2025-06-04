using UnityEngine;

public class CompleteEffect : MonoBehaviour
{
    public void OnCompleteExplosionEffect()
    {
        Destroy(this.gameObject);
    }

    //public void OnCompleteElectronicEffect()
    //{
    //    Destroy(this.gameObject);
    //}
}
