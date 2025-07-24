//====================
// ����Ƃ��锠���Ǘ�����X�N���v�g
// Aouther:y-miura
// Date:2025/07/20
//====================

using UnityEngine;

public class BoxManager : MonoBehaviour
{
    [SerializeField] GameObject BoxPrefab;  //���v���n�u�擾

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //SpawnBox�֐����J��Ԃ��Ăяo���āA�����J��Ԃ���������
        InvokeRepeating("SpawnBox", 5.5f, 10);
    }

    /// <summary>
    /// ���𐶐����鏈��
    /// </summary>
    public void SpawnBox()
    {
        GameObject boxObj = BoxPrefab;
        float spawnX = Random.Range(1, 3);

        if (spawnX == 1)
        {
            //���𐶐�����
            Instantiate(boxObj, new Vector2(27.09f, 27), Quaternion.identity);
        }
        if (spawnX == 2)
        {
            //���𐶐�����
            Instantiate(boxObj, new Vector2(28.9f, 27), Quaternion.identity);
        }
    }
}
