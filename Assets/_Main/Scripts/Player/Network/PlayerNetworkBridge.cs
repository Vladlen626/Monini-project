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
	
	private float _jumpBufferEndTime;
	private const float JumpBufferLifetime = 0.15f;
	
	private PlayerInputData _cachedInput;
	public ref readonly PlayerInputData CachedInput => ref _cachedInput;

	public void Initialize(PlayerView inPlayerView)
	{
		_playerView = inPlayerView;
	}

	public override void OnNetworkSpawn()
	{
		_network = Locator.Resolve<INetworkService>();
		_network.InvokeLocalPlayerSpawned(this, IsOwner);
	}
	
	public void SetCachedInput(PlayerInputData input)
	{
		_cachedInput = input;
	}
	
	[ServerRpc]
	public void SendInputServerRpc(PlayerInputData input)
	{
		_cachedInput.Move = input.Move;
		_cachedInput.IsSprinting = input.IsSprinting;

		if (input.IsJumping)
		{
			_jumpBufferEndTime = Time.time + JumpBufferLifetime;
		}
	}

	public PlayerInputData GetBufferedInput()
	{
		var input = _cachedInput;
		input.IsJumping = Time.time < _jumpBufferEndTime;
		return input;
	}
}