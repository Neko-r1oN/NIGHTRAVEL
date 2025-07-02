using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] Transform player;

    // Update is called once per frame
    void Update()
    {
        TransformUtils.GetHitPointToTarget(player, transform.position);
    }
}
