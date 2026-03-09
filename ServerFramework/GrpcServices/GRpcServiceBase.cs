using Grpc.Core;
using Grpc.Net.Client;
using ServerFramework.CommonUtils.Helper;

namespace ServerFramework.GrpcServices;

/// <summary>
/// GRpc 서버 베이스
/// </summary>
/// <param name="loggerService"></param>
/// <param name="serverId">자신의 서버이름</param>
/// <param name="serverAddress">구독할 서버 주소</param>
/// <typeparam name="TClientBase"></typeparam>
public abstract class GRpcServiceBase<TClientBase>(LoggerService loggerService, string serverId, string serverAddress)
    where TClientBase : ClientBase
{
    private GrpcChannel _channel;
    private CancellationTokenSource _cancellationTokenSource;
    private TClientBase _client;
    
    protected readonly LoggerService _loggerService = loggerService;
    protected abstract TClientBase _CreateClient(GrpcChannel channel);
    protected abstract Task SubscribeToEvents(CancellationToken token);

    protected TClientBase GrpcClient => _client;
    public readonly string ServerId = serverId;
    public void Initialize()
    {
        _channel = GrpcChannel.ForAddress(serverAddress);
        _client = _CreateClient(_channel);
        
        _cancellationTokenSource = new CancellationTokenSource();
        
        _loggerService.Information($"Initializing {ServerId} server at {serverAddress}");
    }

    public void Start()
    {
        Task.Run(() => SubscribeToEvents(_cancellationTokenSource.Token));
    }
}