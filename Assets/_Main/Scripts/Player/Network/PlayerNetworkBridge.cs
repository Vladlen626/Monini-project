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
	
	private PlayerView _view;
	private INetworkService _network;

	private void Awake()
	{
		_view = GetComponent<PlayerView>();
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
	public void Server_NotifySlamImpact(Vector3 pos, float radius)
	{
		Collider[] hits = Physics.OverlapSphere(pos, radius, LayerMask.GetMask($"Destructible"));
		foreach (var c in hits)
		{
			if (c.TryGetComponent<ISlamImpactReceiver>(out var r))
				r.OnSlamImpact(new ImpactCtx { Position = pos, Radius = radius });
		}
		Rpc_PlaySlamFX(pos);
	}

	[ObserversRpc]
	private void Rpc_PlaySlamFX(Vector3 pos)
	{
		Instantiate(_slamFx, pos, Quaternion.Euler(-90, 0, 0));
	}
}