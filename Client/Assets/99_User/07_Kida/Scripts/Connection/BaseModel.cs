////////////////////////////////////////////////////////////////
///
/// Unity�ƃT�[�o�[�̐ڑ����Ǘ�����X�N���v�g
/// 
/// Aughter:�ؓc�W��
///
////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseModel : MonoBehaviour
{
    //localhost��Azure��URL������
    public const string ServerURL = "https://localhost:7180";
    /*#if DEBUG

    #else
        //public const string ServerURL = "http://localhost:7000";
        public const string ServerURL = "http://realtime-game.japaneast.cloudapp.azure.com:7000";
    #endif*/
}
