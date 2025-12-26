using System.Threading;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting.UTP;
using PlatformCore.Services;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace _Main.Scripts.Core.Services
{
	public class MultiplayerRelayService : IMultiplayerService, IService
	{
		private readonly ILoggerService _logger;
		private readonly NetworkManager _networkManager;
		private UnityTransport _transport;

		public bool IsInitialized { get; private set; }

		public MultiplayerRelayService(ILoggerService logger)
		{
			_logger = logger;
			_networkManager = InstanceFinder.NetworkManager;
			_transport = _networkManager.TransportManager.GetTransport<UnityTransport>();

			if (_transport == null)
			{
				Debug.LogError("[RELAY] UnityTransport not found!");
			}
		}

		public async UniTask InitializeAsync()
		{
			if (IsInitialized) return;

			if (UnityServices.State != ServicesInitializationState.Initialized)
			{
				await UnityServices.InitializeAsync();
			}

			if (!AuthenticationService.Instance.IsSignedIn)
			{
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
			}

			IsInitialized = true;
			_logger.Log("Relay initialized");
		}

		public async UniTask<string> CreateRoomAsync(string roomName, int maxPlayers)
		{
			await InitializeAsync();

			try
			{
				Debug.Log($"[RELAY] Creating allocation for {maxPlayers} players...");

				Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
				Debug.Log($"[RELAY] Allocation ID: {allocation.AllocationId}");

				string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
				Debug.Log($"[RELAY] Join code: {joinCode}");

				// ✅ ИСПОЛЬЗУЙ СПЕЦИАЛЬНЫЙ МЕТОД FishyUnityTransport
				_transport.SetHostRelayData(
					allocation.RelayServer.IpV4,
					(ushort)allocation.RelayServer.Port,
					allocation.AllocationIdBytes,
					allocation.Key,
					allocation.ConnectionData,
					isSecure: false
				);

				Debug.Log("[RELAY] Host configured via SetHostRelayData");

				if (!_networkManager.IsServerStarted)
				{
					Debug.Log("[RELAY] Starting server immediately...");
					_networkManager.ServerManager.StartConnection();

					// Ждём подтверждения старта
					await UniTask.WaitUntil(() => _networkManager.IsServerStarted,
						cancellationToken: CancellationToken.None);
					Debug.Log("[RELAY] Server started successfully");
				}

				_logger.Log($"Room created: {joinCode}");
				return joinCode;
			}
			catch (System.Exception e)
			{
				Debug.LogError($"[RELAY] Create failed: {e}");
				throw;
			}
		}

		public async UniTask JoinRoomAsync(string joinCode)
		{
			await InitializeAsync();

			try
			{
				Debug.Log($"[RELAY] Joining with code: {joinCode}");

				JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
				Debug.Log($"[RELAY] Joined allocation: {allocation.AllocationId}");

				// ✅ ИСПОЛЬЗУЙ СПЕЦИАЛЬНЫЙ МЕТОД FishyUnityTransport
				_transport.SetClientRelayData(
					allocation.RelayServer.IpV4,
					(ushort)allocation.RelayServer.Port,
					allocation.AllocationIdBytes,
					allocation.Key,
					allocation.ConnectionData,
					allocation.HostConnectionData,
					isSecure: false
				);

				Debug.Log("[RELAY] Client configured via SetClientRelayData");

				_logger.Log($"Joined: {joinCode}");
			}
			catch (System.Exception e)
			{
				Debug.LogError($"[RELAY] Join failed: {e}");
				throw;
			}
		}

		public void Dispose()
		{
		}
	}
}