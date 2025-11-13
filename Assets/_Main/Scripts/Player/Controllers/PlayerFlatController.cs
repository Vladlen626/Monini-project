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
		private readonly PlayerView _playerView;

		public PlayerFlatController(PlayerModel playerModel, PlayerView playerView)
		{
			_playerModel = playerModel;
			_playerView = playerView;
		}

		public void Activate()
		{
			_playerView.OnSlamReceived += OnSlamReceivedHandler;
		}

		public void Deactivate()
		{
			_playerView.OnSlamReceived -= OnSlamReceivedHandler;
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