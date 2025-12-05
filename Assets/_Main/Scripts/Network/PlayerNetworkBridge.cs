using System;
using System.Threading;
using _Main.Scripts.Core;
using _Main.Scripts.Player;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;
using UnityEngine;

[RequireComponent(typeof(PlayerView), typeof(NetworkTransform))]
public class PlayerNetworkBridge : NetworkBehaviour, ISlamImpactReceiver
{
	[SerializeField] 
	private GameObject _slamFx;
	[SerializeField] 
	private SlamTrigger _slamTrigger;

	public event Action OnSlamReceived;
	public readonly SyncVar<PlayerState> State = new();

	private PlayerView _view;
	private NetworkTransform _networkTransform;

	private void Awake()
	{
		_view = GetComponent<PlayerView>();
		_networkTransform = GetComponent<NetworkTransform>();
	}

	public override void OnStartServer()
	{
		_slamTrigger.SetupBridge(this);
		_slamTrigger.OnSlamImpactReceived += OnSlamImpactHandler;
	}

	public override void OnStopServer()
	{
		_slamTrigger.OnSlamImpactReceived -= OnSlamImpactHandler;
	}

	public override async void OnStartClient()
	{
		base.OnStartClient(); 
		MoveToPersistent(gameObject);

		if (!IsOwner)
		{
			return;
		}

		var objectFactory = Locator.Resolve<IObjectFactory>();
		var lifecycle = Locator.Resolve<LifecycleService>();
		var ctx = await PlayerContext.Client.CreateAsync(this, _view, objectFactory,
			CancellationToken.None);

		ctx.Camera.AttachTo(ctx.View.CameraRoot);
		var cam = ctx.Camera.GetCameraTransform();
		MoveToPersistent(cam.gameObject);

		foreach (var c in ctx.Controllers)
		{
			await lifecycle.RegisterAsync(c);
		}
	}
	private void MoveToPersistent(GameObject gameObjectToMove)
	{
		var persistent = UnityEngine.SceneManagement.SceneManager.GetSceneByName(SceneNames.PersistentScene);
		if (persistent.IsValid())
		{
			UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObjectToMove, persistent);
		}
	}

	private void OnSlamImpactHandler(int targetId)
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

	public void OnSlamImpact()
	{
		OnSlamReceived?.Invoke();
	}
	
	[ServerRpc]
	public void Server_ChangeState(PlayerState requestedState)
	{
		State.Value = requestedState;
		State.DirtyAll();
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
	
	[Server]
	public void Server_TeleportOwner(Vector3 position, Quaternion rotation)
	{
		if (!Owner.IsValid)
		{
			return;
		}

		Target_TeleportOwner(Owner, position, rotation);
	}

	[TargetRpc]
	private void Target_TeleportOwner(NetworkConnection target, Vector3 position, Quaternion rotation)
	{
		_view.TeleportTo(position, rotation);
		_networkTransform.Teleport();
	}
}