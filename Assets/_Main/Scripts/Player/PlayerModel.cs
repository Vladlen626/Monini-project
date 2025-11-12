using System;

namespace _Main.Scripts.Player
{
	public class PlayerModel
	{
		public PlayerConfig config => _config;

		public event Action<PlayerState> OnPlayerStateChanged;
		public PlayerState State { get; private set; } = PlayerState.Normal;

		private readonly PlayerConfig _config;

		public PlayerModel()
		{
			_config = new PlayerConfig();
		}

		public void SetState(PlayerState newState)
		{
			if (State == newState)
				return;

			State = newState;
			OnPlayerStateChanged?.Invoke(newState);
		}
	}
}