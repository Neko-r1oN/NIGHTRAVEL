//////////////////////////////////////////////////////////////////
/////
///// ユーザーに対する動作を管理するスクリプト
///// 
///// Aughter:木田晃輔
/////
//////////////////////////////////////////////////////////////////


using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Newtonsoft.Json;
using NIGHTRAVEL.Shared.Interfaces.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Networking;

public class UserModel : BaseModel
{
    //const string ServerURL = "http://localhost:7000";
    //const string ServerURL = "http://realtime-game.japaneast.cloudapp.azure.com:7000";
    public int userId; //登録ユーザーID
    string userName; //登録ユーザーネーム
    string authToken; //トークン
    string password; //登録パスワード

    private static UserModel instance;
    public static UserModel Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObj = new GameObject("UserModel");
                instance = gameObj.AddComponent<UserModel>();
                DontDestroyOnLoad(gameObj);
            }
            return instance;
        }
    }

    //ユーザーIDをローカルファイルに保存する
    public void SaveUserData()
    {
        SaveData saveData = new SaveData();
        saveData.userId = userId;

        string json = JsonConvert.SerializeObject(saveData);
        var writer = new StreamWriter(Application.persistentDataPath + "/saveData.json");
        writer.Write(json);
        writer.Flush();
        writer.Close();
    }

    //ユーザーIDをローカルファイルから読み込む
    public bool LoadUserData()
    {
        if (!File.Exists(Application.persistentDataPath + "/saveData.json"))
        {
            return false;
        }

        var reader =
            new StreamReader(Application.persistentDataPath + "/saveData.json");
        string json = reader.ReadToEnd();
        reader.Close();
        SaveData saveData = JsonConvert.DeserializeObject<SaveData>(json);
        //authToken = saveData.authToken;
        userId = saveData.userId;
        //userName = saveData.userName;


        //if (authToken == null)
        //{

        //    StartCoroutine(Instance.CreateToken(result =>
        //    {
        //        SaveUserData();
        //    }));

        //}

        return true;
    }

    ////トークン生成処理
    //public IEnumerator CreateToken(Action<bool> responce)
    //{
    //    var requestData = new
    //    {
    //        user_id = userId
    //    };
    //    string json = JsonConvert.SerializeObject(requestData);
    //    UnityWebRequest request = UnityWebRequest.Post(ServerURL + "users/createToken", json, "application/json");
    //    yield return request.SendWebRequest();
    //    if (request.result == UnityWebRequest.Result.Success)
    //    {
    //        //通信が成功したとき、返ってきたJSONをオブジェクトに変換
    //        string resultJson = request.downloadHandler.text;
    //        RegistUserResponse response = JsonConvert.DeserializeObject<RegistUserResponse>(resultJson);

    //        //ファイルにユーザーIDを保存
    //        userId = response.UserID;
    //        authToken = response.Authtoken;
    //        SaveUserData();
    //    }
    //    responce?.Invoke(request.result == UnityWebRequest.Result.Success);
    //}

    /// <summary>
    /// ユーザーの登録
    /// </summary>
    /// <returns></returns>
    public async UniTask<bool> RegistUserAsync()
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler });
        var client = MagicOnionClient.Create<IUserService>(channel);
        try
        {//登録成功
            userId = await client.RegistUserAsync();
            SaveUserData();
            Debug.Log("登録成功");
            return true;
        }
        catch (RpcException e)
        {//登録しっぱい
            Debug.Log("登録失敗");
            return false;
        }
    }
}
