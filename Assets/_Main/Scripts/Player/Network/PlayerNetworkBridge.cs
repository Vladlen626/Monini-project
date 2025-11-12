using System;
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
	[SerializeField] private GameObject _slamFx;
	[SerializeField] private SlamTrigger _slamTrigger;

	private PlayerView _view;
	private INetworkService _network;

	private void Awake()
	{
		_view = GetComponent<PlayerView>();
		_slamTrigger.OnSlamImpactReceived += Server_NotifyObjectGetSlamImpact;
	}

	private void OnDestroy()
	{
		_slamTrigger.OnSlamImpactReceived -= Server_NotifyObjectGetSlamImpact;
	}

	public override async void OnStartClient()
	{
		base.OnStartClient();

		if (!IsOwner)
		{
			return;
		}

		var objectFactory = Locator.Resolve<IObjectFactory>();
		var lifecycle = Locator.Resolve<LifecycleService>();

		var playerFactory = new PlayerFactory();

		var ctx = await PlayerContext.Client.CreateAsync(_view, objectFactory, playerFactory,
			CancellationToken.None);

		ctx.Camera.AttachTo(ctx.View.CameraRoot);

		foreach (var c in ctx.Controllers)
		{
			await lifecycle.RegisterAsync(c);
		}
	}

	[ServerRpc]
	private void Server_NotifyObjectGetSlamImpact(NetworkObject target)
	{
		if (!IsOwner)
		{
			return;
		}

		var slamReceiver = target.GetComponent<ISlamImpactReceiver>();
		if (slamReceiver != null)
		{
			slamReceiver.OnSlamImpact();
		}
	}


	[ObserversRpc]
	public void Rpc_PlaySlamFX(Vector3 pos)
	{
		Instantiate(_slamFx, pos, Quaternion.Euler(-90, 0, 0));
	}
}