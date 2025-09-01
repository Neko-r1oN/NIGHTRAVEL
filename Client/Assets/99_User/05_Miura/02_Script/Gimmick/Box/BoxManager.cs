//====================
// ����Ƃ��锠���Ǘ�����X�N���v�g
// Aouther:y-miura
// Date:2025/07/20
//====================

using UnityEngine;

public class BoxManager : MonoBehaviour
{
    [SerializeField] GameObject BoxPrefab;  //���v���n�u�擾

    public float spawnTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //SpawnBox�֐����J��Ԃ��Ăяo���āA�����J��Ԃ���������
        InvokeRepeating("SpawnBox", 0.1f, spawnTime);
    }

    /// <summary>
    /// ���𐶐����鏈��
    /// </summary>
    public void SpawnBox()
    {
        float spawnX = Random.Range(1, 3);
        if (spawnX == 1)
        {
            //���𐶐�����
            Instantiate(BoxPrefab, new Vector2(27.09f, 27), Quaternion.identity);
        }
        if (spawnX == 2)
        {
            //���𐶐�����
            Instantiate(BoxPrefab, new Vector2(28.9f, 27), Quaternion.identity);
        }
    }
}
