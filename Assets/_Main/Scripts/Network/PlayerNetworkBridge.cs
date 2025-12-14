using System;
using _Main.Scripts.Player;
using _Main.Scripts.Player.Controllers;
using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

[RequireComponent(typeof(PlayerView))]
public class PlayerNetworkBridge : NetworkBehaviour, ISlamImpactReceiver
{
	public event Action OnSlamReceived;

	[SerializeField] 
	private GameObject _slamFx;

	public readonly SyncVar<string> PlayerName = new();
	public readonly SyncVar<PlayerState> State = new();
	public readonly SyncVar<int> CrumbsCount = new();

	private PlayerView _view;

	private void Awake()
	{
		_view = GetComponent<PlayerView>();
	}
	public override void OnStartClient()
	{
		base.OnStartClient(); 
		PlayerNetworkUtils.MoveToPersistent(gameObject);
		
		if (!IsOwner)
		{
			InitializeRemote();
		}
	}
	
	private void InitializeRemote()
	{
		var config = new PlayerConfig();
		var movement = new PlayerMovementController(config, _view);
		var slam = new PlayerSlamBounceController(movement, _view, this, null, config);

		var stateController = new ClientPlayerStateController(this, _view);

		var brain = GetComponent<PlayerNetworkBrain>();
		brain.Construct(movement, slam, null, null);
		
		var lifecycle = Locator.Resolve<LifecycleService>();
		lifecycle.RegisterAsync(stateController).Forget();
		lifecycle.RegisterAsync(movement).Forget();
		lifecycle.RegisterAsync(slam).Forget();
	}

	public void OnSlamImpact()
	{
		OnSlamReceived?.Invoke();
	}

	[ObserversRpc]
	public void Rpc_PlaySlamFX(Vector3 pos)
	{
		Instantiate(_slamFx, pos, Quaternion.Euler(-90, 0, 0));
	}

	// ReSharper disable Unity.PerformanceAnalysis
	[ServerRpc]
	public void ServerBreakNearbyBoxes(Vector3 impactPosition, float radius)
	{
		Collider[] hits = Physics.OverlapSphere(impactPosition, radius);
		foreach (var hit in hits)
		{
			if (hit.transform == transform)
			{
				continue;
			}

			var receiver = hit.GetComponent<ISlamImpactReceiver>();
			receiver?.OnSlamImpact();
		}
	}
}