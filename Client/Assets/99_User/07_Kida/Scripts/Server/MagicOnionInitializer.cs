using MagicOnion.Client;
using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Unity;

/// <summary>
/// MagicOnion�p�C���^�t�F�[�X�̃R�[�h����
/// </summary>
[MagicOnionClientGeneration(typeof(Shared.Interfaces.StreamingHubs.IRoomHubReceiver))]
partial class MagicOnionInitializer
{
    /// <summary>
    /// Resolver�̓o�^����
    /// </summary>
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterResolvers()
    {
        StaticCompositeResolver.Instance.Register(
            MagicOnionInitializer.Resolver,
            MessagePackGeneratedResolver.Instance,
            BuiltinResolver.Instance,
            UnityResolver.Instance,
            PrimitiveObjectResolver.Instance
        );

        MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions
            .WithResolver(StaticCompositeResolver.Instance);
    }
}