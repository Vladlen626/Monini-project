using System;
using FishNet.Object.Synchronizing;

namespace _Main.Scripts.Player
{
	public class PlayerModel
	{
		public event Action<PlayerState> OnPlayerStateChanged;
		public event Action<int> OnCrumbValueChanged;
		public event Action<string> OnPlayerNameChanged;
		
		public string playerName { get; private set; } = "Unknown";
		public PlayerState state { get; private set; } = PlayerState.Normal;
		public int crumbsCount { get; private set; }

		// ReSharper disable Unity.PerformanceAnalysis
		public void SetState(PlayerState newState)
		{
			if (state == newState)
			{
				return;
			}

			state = newState;
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
		
		public void SetPlayerName(string inPlayerName)
		{
			playerName = inPlayerName;
			OnPlayerNameChanged?.Invoke(playerName);
		}
	}
	
	// Ввод от игрока

	// Состояние для исправления ошибок (Reconcile)
}