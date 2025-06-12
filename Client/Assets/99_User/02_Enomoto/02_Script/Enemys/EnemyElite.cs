using UnityEngine;

public class EnemyElite : MonoBehaviour
{
    /// <summary>
    /// エリートの種類
    /// </summary>
    public enum ELITE_TYPE
    {
        None,
        Blaze,      // ブレイズエリート
        Frost,      // フロストエリート
        Thunder     // サンダーエリート
    }
    [SerializeField]
    ELITE_TYPE type;
    public ELITE_TYPE Type { get { return type; }  set { type = value; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
