using System;
using System.Threading;
using _Main.Scripts.Core;
using _Main.Scripts.Player;
using FishNet.Connection;
using FishNet.Object;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;
using UnityEngine;

[RequireComponent(typeof(PlayerView))]
public class PlayerNetworkBridge : NetworkBehaviour, ISlamImpactReceiver
{
	[SerializeField] private GameObject _slamFx;
	[SerializeField] private SlamTrigger _slamTrigger;

	private PlayerView _view;

	private void Awake()
	{
		_view = GetComponent<PlayerView>();
		_slamTrigger.OnSlamImpactReceived += OnSlamImpactHandler;
	}

	private void OnDestroy()
	{
		_slamTrigger.OnSlamImpactReceived -= OnSlamImpactHandler;
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
		_slamTrigger.SetPlayerModel(ctx.Model);

		foreach (var c in ctx.Controllers)
		{
			await lifecycle.RegisterAsync(c);
		}
	}

	private void OnSlamImpactHandler(int targetId)
	{
		if (!IsOwner)
		{
			return;
		}

		Server_NotifyObjectGetSlamImpact( targetId);
	}
	
	[Server]
	public void OnSlamImpact()
	{
		Target_ApplyFlat(Owner);
	}

	[TargetRpc]
	private void Target_ApplyFlat(NetworkConnection target)
	{
		_view.OnSlamImpact();
	}

	[ServerRpc]
	private void Server_NotifyObjectGetSlamImpact(int targetId)
	{
		if (!NetworkManager.ServerManager.Objects.Spawned.TryGetValue(targetId, out var networkObject))
		{
			return;
		}
		
		var slamReceiver = networkObject.GetComponent<ISlamImpactReceiver>();
		if (slamReceiver != null)
		{
			slamReceiver.OnSlamImpact();
		}
	}

	[ServerRpc(RequireOwnership = true)]
	public void Server_PlaySlamFX(Vector3 pos)
	{
		Rpc_PlaySlamFX(pos);
	}

	[ObserversRpc]
	public void Rpc_PlaySlamFX(Vector3 pos)
	{
		Instantiate(_slamFx, pos, Quaternion.Euler(-90, 0, 0));
	}
}