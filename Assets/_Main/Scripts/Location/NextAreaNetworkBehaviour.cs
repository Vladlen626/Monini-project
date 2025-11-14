using System;
using FishNet.Object;
using UnityEngine;

namespace _Main.Scripts.Location
{
	public class NextAreaNetworkBehaviour : NetworkBehaviour
	{
		public event Action<int> OnPlayerEntered;
		public event Action<int> OnPlayerExited;
		private void OnTriggerEnter(Collider other)
		{
			if (!NetworkManager.ServerManager.Started)
			{
				return;
			}
			
			var player = other.GetComponent<NetworkObject>();
			if (player == null)
			{
				return;
			}
			
			OnPlayerEntered?.Invoke(player.OwnerId);
		}

		private void OnTriggerExit(Collider other)
		{
			if (!NetworkManager.ServerManager.Started)
			{
				return;
			}
			
			var player = other.GetComponent<NetworkObject>();
			if (player == null)
			{
				return;
			}
			
			OnPlayerExited?.Invoke(player.OwnerId);
		}
	}
}