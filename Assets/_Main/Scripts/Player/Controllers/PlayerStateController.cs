using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

namespace _Main.Scripts.Player.Controllers
{
	//TODO: Позднее заменить на полноценную state machine с стеком состояний
	// У каждого стейта будет выход и вход, они сами будут подчищать за собой
	public class PlayerStateController : IBaseController, IActivatable
	{
		private readonly PlayerModel _playerModel;
		private readonly PlayerView _playerView;
		private readonly CharacterController _characterController;

		public PlayerStateController(PlayerModel playerModel, PlayerView playerView,
			CharacterController characterController)
		{
			_playerModel = playerModel;
			_playerView = playerView;
			_characterController = characterController;
		}

		public void Activate()
		{
			_playerModel.OnPlayerStateChanged += OnPlayerStateChangedHandler;
			OnPlayerStateChangedHandler(_playerModel.State);
		}

		public void Deactivate()
		{
			_playerModel.OnPlayerStateChanged -= OnPlayerStateChangedHandler;
		}

		private void OnPlayerStateChangedHandler(PlayerState state)
		{
			switch (state)
			{
				case PlayerState.Normal:
					SetupNormalState();
					break;
				case PlayerState.Slam:
					SetupSlamState();
					break;
			}
		}

		private void SetupNormalState()
		{
			_playerView.DisableSlamTrigger();
			_characterController.excludeLayers = LayerMask.GetMask();
		}

		private void SetupSlamState()
		{
			_playerView.EnableSlamTrigger();
			_characterController.excludeLayers = LayerMask.GetMask("Destructible");
		}
}
}