//--------------------------------------------------------------
// �{�^���X�N���v�g [ ButtonScipt.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    //----------------------------------------------------------
    // �t�B�[���h

    /// <summary>
    /// �{�^���̎��
    /// </summary>
    private enum ButtonType
    {
        Start,
        Option,
        Exit
    }

    [SerializeField] private UnityEngine.UI.Button startButton;

    /// <summary>
    /// ��ޕۊǗp
    /// </summary>
    [SerializeField] private ButtonType buttonType;

    //----------------------------------------------------------
    // ���\�b�h

    void Start()
    {
        startButton.onClick.AddListener(OnStartButtonPressed);
    }

    void OnStartButtonPressed()
    {
        // �{�^���������ꂽ�Ƃ��̏���
        Debug.Log("�X�^�[�g�{�^����������܂���");
    }
}