using Cysharp.Net.Http;
using Grpc.Net.Client;
using MagicOnion.Unity;
using MessagePack.Resolvers;
using MessagePack;
using UnityEngine;

public class MagicOnionInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void OnRuntimeInitialize()
    {
        // Initialize gRPC channel provider when the application is loaded.
        GrpcChannelProviderHost.Initialize(new DefaultGrpcChannelProvider(() => new GrpcChannelOptions()
        {
            HttpHandler = new YetAnotherHttpHandler()
            {
                Http2Only = true,
                SkipCertificateVerification = true
            },
            DisposeHttpClient = true,
        }));

        // Set extensions to default resolver.
        var resolver = MessagePack.Resolvers.CompositeResolver.Create(
            // enable extension packages first
            MessagePack.Unity.UnityResolver.Instance,

            // finally use standard (default) resolver
            StandardResolver.Instance
        );
        var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);

        // Pass options every time or set as default
        MessagePackSerializer.DefaultOptions = options;
    }
}


