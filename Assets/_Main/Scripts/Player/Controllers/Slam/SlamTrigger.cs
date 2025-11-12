using System;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;

namespace _Main.Scripts.Player
{
	public class SlamTrigger : MonoBehaviour
	{
		public event Action<NetworkObject>  OnSlamImpactReceived;
		private void OnTriggerEnter(Collider other)
		{
			var impactReceiver = other.GetComponent<ISlamImpactReceiver>();
			if (impactReceiver != null)
			{
				var netObject = other.GetComponent<NetworkObject>();
				if (netObject != null)
				{
					OnSlamImpactReceived?.Invoke(netObject);
				}
			}
		}
	}
	
	
}