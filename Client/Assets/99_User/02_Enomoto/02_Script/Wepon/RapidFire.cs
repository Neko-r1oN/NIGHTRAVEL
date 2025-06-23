using UnityEngine;

public class RapidFire : MonoBehaviour
{
    private void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Player‚É“–‚½‚Á‚½");
        }
    }
}
