using System;
using MagicOnion;
using MagicOnion.Client;
using NIGHTRAVEL.Shared;
using NIGHTRAVEL.Shared.Unity;
using UnityEngine;

public class SampleScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        try
        {
            var channel = GrpcChannelx.ForAddress("http://localhost:5244");
            var client = MagicOnionClient.Create<IMyFirstService>(channel);

            // Vector3 à¯êîÇíËã`
            Vector3 vec1 = new Vector3(1, 2, 3);
            Vector3 vec2 = new Vector3(4, 5, 6);

            var result = await client.SumAsync(vec1,vec2);
            Debug.Log(result);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}