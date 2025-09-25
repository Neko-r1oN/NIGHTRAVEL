////////////////////////////////////////////////////////////////
///
/// Unityとサーバーの接続を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseModel : MonoBehaviour
{
#if DEBUG
    //localhostかAzureのURLを入れる
   public const string ServerURL = "http://localhost:5244";
    //public const string ServerURL = "http://car-boom-crash.japaneast.cloudapp.azure.com:5244";
#else
    public const string ServerURL = "http://car-boom-crash.japaneast.cloudapp.azure.com:5244";
#endif
}
