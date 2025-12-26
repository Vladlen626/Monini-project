using Cysharp.Threading.Tasks;

public interface IMultiplayerService
{
	UniTask InitializeAsync();
	UniTask<string> CreateRoomAsync(string roomName, int maxPlayers);
	UniTask JoinRoomAsync(string joinCode);
	bool IsInitialized { get; }
}
