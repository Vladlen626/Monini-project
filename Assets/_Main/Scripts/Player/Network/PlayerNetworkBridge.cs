using _Main.Scripts.Core;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerView))]
public class PlayerNetworkBridge : NetworkBehaviour
{
	public PlayerView playerView => _playerView;
	
	private INetworkService _network;
	private PlayerView _playerView;

	public void Initialize(INetworkService networkService, PlayerView inPlayerView)
	{
		_network = networkService;
		_playerView = inPlayerView;
	}

	public override void OnNetworkSpawn()
	{
		if (IsOwner)
		{
			_network.InvokeLocalPlayerSpawned(this);
		}
	}
}