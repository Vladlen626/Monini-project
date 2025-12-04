using System;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;

namespace _Main.Scripts.Player
{
	public class PlayerFlatController : IBaseController, IActivatable
	{
		private readonly PlayerModel _playerModel;
		private readonly PlayerNetworkBridge _bridge;

		public PlayerFlatController(PlayerModel playerModel, PlayerNetworkBridge bridge)
		{
			_playerModel = playerModel;
			_bridge = bridge;
		}

		public void Activate()
		{
			_bridge.OnSlamReceived += OnSlamReceivedHandler;
		}

		public void Deactivate()
		{
			_bridge.OnSlamReceived -= OnSlamReceivedHandler;
		}

		private void OnSlamReceivedHandler()
		{
			FlatStateProcess().Forget();
		}
		
		private async UniTask FlatStateProcess()
		{
			_playerModel.SetState(PlayerState.Flat);
			await UniTask.Delay(5000);
			_playerModel.SetState(PlayerState.Normal);
		}
		
	}
}