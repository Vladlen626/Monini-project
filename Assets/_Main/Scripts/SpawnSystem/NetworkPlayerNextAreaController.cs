using System.Collections.Generic;
using _Main.Scripts.Core;
using _Main.Scripts.Location;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;

namespace _Main.Scripts.Player.Network
{
	public class NetworkPlayerNextAreaController : IBaseController, IActivatable, IUpdatable
	{
		private const float RequiredTime = 5f;

		private readonly NextAreaNetworkBehaviour _nextAreaNetworkBehaviour;
		private readonly INetworkService _networkService;
		private readonly ISceneFlowService _sceneFlowService;

		private HashSet<int> _playerIds = new HashSet<int>();
		private float _timer;

		private bool _isCounting;
		private bool _teleportExecuted;

		public NetworkPlayerNextAreaController(NextAreaNetworkBehaviour nextAreaNetworkBehaviour,
			INetworkService networkService, ISceneFlowService sceneFlowService)
		{
			_nextAreaNetworkBehaviour = nextAreaNetworkBehaviour;
			_networkService = networkService;
			_sceneFlowService = sceneFlowService;
		}

		public void Activate()
		{
			_nextAreaNetworkBehaviour.OnPlayerEntered += OnPlayerEnteredHandler;
			_nextAreaNetworkBehaviour.OnPlayerExited += OnPlayerExitedHandler;
		}

		public void Deactivate()
		{
			_nextAreaNetworkBehaviour.OnPlayerEntered -= OnPlayerEnteredHandler;
			_nextAreaNetworkBehaviour.OnPlayerExited -= OnPlayerExitedHandler;
		}

		public void OnUpdate(float deltaTime)
		{
			if (!_networkService.IsServer || _teleportExecuted)
			{
				return;
			}

			// Если игроков в зоне меньше чем нужно — сбрасываем
			if (_playerIds.Count != _networkService.PlayersCount)
			{
				_isCounting = false;
				_timer = 0f;
				return;
			}

			_isCounting = true;

			if (_isCounting)
			{
				_timer += deltaTime;

				if (_timer >= RequiredTime)
				{
					_teleportExecuted = true;
					_sceneFlowService.RequestSceneChange(SceneNames.ParisCenter);
				}
			}
		}

		private void OnPlayerEnteredHandler(int playerId)
		{
			if (!_networkService.IsServer)
			{
				return;
			}

			_playerIds.Add(playerId);

			if (_playerIds.Count == _networkService.PlayersCount)
			{
				_isCounting = true;
				_timer = 0f;
			}
		}

		private void OnPlayerExitedHandler(int playerId)
		{
			if (!_networkService.IsServer)
			{
				return;
			}

			_playerIds.Remove(playerId);

			_isCounting = false;
			_timer = 0f;
		}
	}
}