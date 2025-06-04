using UnityEngine;

public class Abyss : MonoBehaviour
{
    SampleChara_Copy sampleChara_Copy = new SampleChara_Copy();
    //float characterHP;
    //int damage

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("ƒvƒŒƒCƒ„[‚ª“Ş—‚É—‚¿‚½");
            //sampleChara_Copy.ApplyDamage(damage,this.transform.position);
        }
        if(collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("“G‚ª“Ş—‚É—‚¿‚½");
        }
    }
}
