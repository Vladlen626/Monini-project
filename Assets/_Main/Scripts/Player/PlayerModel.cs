using System;

namespace _Main.Scripts.Player
{
	public class PlayerModel
	{
		public int crumbsCount { get; private set; }
		public event Action<PlayerState> OnPlayerStateChanged;
		public PlayerState State { get; private set; } = PlayerState.Normal;

		public void SetState(PlayerState newState)
		{
			if (State == newState)
			{
				return;
			}

			State = newState;
			OnPlayerStateChanged?.Invoke(newState);
		}
		public void CollectCrumbs()
		{
			crumbsCount++;
		}
	}
}