using System.Collections.Generic;
using _Main.Scripts.Core;
using _Main.Scripts.Location;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Player.Network
{
	public class NetworkPlayerNextAreaController : IBaseController, IActivatable, IUpdatable
	{
		private readonly NextAreaNetworkBehaviour _nextAreaNetworkBehaviour;
		private readonly INetworkService _networkService;
		
		private HashSet<int> _playerIds = new HashSet<int>();
		
		public NetworkPlayerNextAreaController(NextAreaNetworkBehaviour nextAreaNetworkBehaviour, INetworkService networkService)
		{
			_nextAreaNetworkBehaviour = nextAreaNetworkBehaviour;
			_networkService = networkService;
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
			if (_playerIds.Count == 0)
			{
				return;
			}

			if (_playerIds.Count == _networkService.PlayersCount)
			{
				
			}
		}

		private void OnPlayerEnteredHandler(int playerId)
		{
			
		}

		private void OnPlayerExitedHandler(int playerId)
		{
			
		}
		
	}
}