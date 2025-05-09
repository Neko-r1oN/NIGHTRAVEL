using UnityEngine;
using UnityEngine.Audio;

public class TestCharacter : MonoBehaviour
{
    //�}�b�v�̎g�p�����m���߂邽�߂̃L�����N�^�[�̃X�N���v�g
    //Aouther:Yuki Miura(y-miura)

    [SerializeField] LayerMask blockLayer;

    Rigidbody2D rigidbody2d;
    float speed;
    float jumpPower = 480;

    public enum DIRECTION_TYPE
    {
        STOP, RIGHT, LEFT,
    }
    DIRECTION_TYPE directionType = DIRECTION_TYPE.STOP;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //�����L�[�擾
        float x = Input.GetAxis("Horizontal");

        if (x == 0)//���͂�����������~�܂�
        {
            directionType = DIRECTION_TYPE.STOP;
        }
        else if (x > 0)//�E�ɐi��
        {
            directionType = DIRECTION_TYPE.RIGHT;
        }
        else if (x < 0)//���ɐi��
        {
            directionType = DIRECTION_TYPE.LEFT;
        }

        void FixedUpdate()
        {
          

            switch (directionType)
            {
                case DIRECTION_TYPE.STOP:
                    speed = 0;
                    break;

                case DIRECTION_TYPE.RIGHT:
                    speed = 5;
                    transform.localScale = new Vector3(1, 1, 1);
                    break;

                case DIRECTION_TYPE.LEFT:
                    speed = -5;
                    transform.localScale = new Vector3(-1, 1, 1);

                    break;
            }

            //���x���㏑��������@
            rigidbody2d.linearVelocity = new Vector2(speed, rigidbody2d.linearVelocity.y);

        }

        void Jump()
        {
            rigidbody2d.AddForce(Vector2.up * jumpPower);
            transform.localPosition += new Vector3(0, 0.1f, 0);
        }
    }
}
