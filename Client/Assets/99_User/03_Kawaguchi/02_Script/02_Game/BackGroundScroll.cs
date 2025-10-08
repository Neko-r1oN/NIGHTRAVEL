using UnityEngine;

public class BackGroundScroll : MonoBehaviour
{
    [SerializeField, Header("éãç∑å¯â "), Range(0, 1)]
    public float parallaxEffect;

    [SerializeField] GameObject cam;
    private float length;
    private float startPosX;

    private void Start()
    {
        startPosX = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        cam = Camera.main.gameObject;
    }

    private void FixedUpdate()
    {
        Parallax();
    }

    private void Parallax()
    {
        float temp = cam.transform.position.x *(1 - parallaxEffect);
        float dist = cam.transform.position.x * parallaxEffect;

        if(temp > startPosX + length)
        {
            startPosX += length;
        }
        else if(temp < startPosX - length)  
        {
            startPosX -= length;
        }
    }

}
