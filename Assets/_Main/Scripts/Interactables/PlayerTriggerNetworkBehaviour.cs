using System;
using FishNet.Object;
using UnityEngine;

namespace _Main.Scripts.Location
{
	public class PlayerTriggerNetworkBehaviour : NetworkBehaviour
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

			var bridge = other.GetComponent<PlayerNetworkBridge>();
			if (bridge == null)
			{
				return;
			}

			OnPlayerEntered?.Invoke(bridge.OwnerId);
			
			OnPlayerEnterInTrigger(player);
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

			OnPlayerExitTrigger(player);
			OnPlayerExited?.Invoke(player.OwnerId);
		}

		protected virtual void OnPlayerEnterInTrigger(NetworkObject playerNetworkObject)
		{
		}
		
		protected virtual void OnPlayerExitTrigger(NetworkObject playerNetworkObject)
		{
		}
	}
}