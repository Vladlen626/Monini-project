using _Main.Scripts.Core;
using PlatformCore.Core;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerView))]
public class PlayerNetworkBridge : NetworkBehaviour
{
	public PlayerView playerView => _playerView;
	private PlayerView _playerView;
	private INetworkService _network;
	public void Initialize(PlayerView inPlayerView)
	{
		_playerView = inPlayerView;
	}

	public override void OnNetworkSpawn()
	{
		_network = Locator.Resolve<INetworkService>();
		_network.InvokeLocalPlayerSpawned(this, IsOwner);
	}
}