using System.Threading;
using _Main.Scripts.Core;
using _Main.Scripts.Player;
using FishNet.Object;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;
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

	public override async void OnStartClient()
	{
		base.OnStartClient();

		if (!IsOwner)
		{
			return;
		}
		
		var locator = Locator._current;
		var objectFactory = locator.Get<IObjectFactory>();
		var playerFactory = new PlayerFactory(locator);
		var lifecycle = locator.Get<LifecycleManager>();

		var ctx = await PlayerContext.Client.CreateAsync(playerView, objectFactory, playerFactory,
			CancellationToken.None);

		ctx.Camera.AttachTo(ctx.View.CameraRoot);

		foreach (var c in ctx.Controllers)
		{
			await lifecycle.RegisterAsync(c);
		}
	}
}