using UnityEngine;

public class EnemyElite : MonoBehaviour
{
    /// <summary>
    /// �G���[�g�̎��
    /// </summary>
    public enum ELITE_TYPE
    {
        None,
        Blaze,      // �u���C�Y�G���[�g
        Frost,      // �t���X�g�G���[�g
        Thunder     // �T���_�[�G���[�g
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
