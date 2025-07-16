//////////////////////////////////////////////////////////////////
/////
///// ���[�U�[�ɑ΂��铮����Ǘ�����X�N���v�g
///// 
///// Aughter:�ؓc�W��
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
    public int userId; //�o�^���[�U�[ID
    string userName; //�o�^���[�U�[�l�[��
    string authToken; //�g�[�N��
    string password; //�o�^�p�X���[�h

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

    //���[�U�[ID�����[�J���t�@�C���ɕۑ�����
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

    //���[�U�[ID�����[�J���t�@�C������ǂݍ���
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

    ////�g�[�N����������
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
    //        //�ʐM�����������Ƃ��A�Ԃ��Ă���JSON���I�u�W�F�N�g�ɕϊ�
    //        string resultJson = request.downloadHandler.text;
    //        RegistUserResponse response = JsonConvert.DeserializeObject<RegistUserResponse>(resultJson);

    //        //�t�@�C���Ƀ��[�U�[ID��ۑ�
    //        userId = response.UserID;
    //        authToken = response.Authtoken;
    //        SaveUserData();
    //    }
    //    responce?.Invoke(request.result == UnityWebRequest.Result.Success);
    //}

    /// <summary>
    /// ���[�U�[�̓o�^
    /// </summary>
    /// <returns></returns>
    public async UniTask<bool> RegistUserAsync()
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler });
        var client = MagicOnionClient.Create<IUserService>(channel);
        try
        {//�o�^����
            userId = await client.RegistUserAsync();
            SaveUserData();
            Debug.Log("�o�^����");
            return true;
        }
        catch (RpcException e)
        {//�o�^�����ς�
            Debug.Log("�o�^���s");
            return false;
        }
    }
}
