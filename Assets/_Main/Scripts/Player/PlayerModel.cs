using System;
using FishNet.Object.Synchronizing;

namespace _Main.Scripts.Player
{
	public class PlayerModel
	{
		public event Action<PlayerState> OnPlayerStateChanged;
		public event Action<int> OnCrumbValueChanged;
		public PlayerState State { get; private set; } = PlayerState.Normal;
		public int crumbsCount { get; private set; }

		public void SetState(PlayerState newState)
		{
			if (State == newState)
			{
				return;
			}

			State = newState;
			OnPlayerStateChanged?.Invoke(newState);
		}
		public void CollectCrumbs(int crumbsNum)
		{
			crumbsCount += crumbsNum;
			OnCrumbValueChanged?.Invoke(crumbsCount);
		}

		public void SpendCrumbs(int crumbsNum)
		{
			crumbsCount -= crumbsNum;
			OnCrumbValueChanged?.Invoke(crumbsCount);
		}
	}
}