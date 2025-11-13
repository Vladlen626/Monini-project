using System;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;

namespace _Main.Scripts.Player
{
	public class SlamTrigger : MonoBehaviour
	{
		public event Action<int>  OnSlamImpactReceived;
		[SerializeField] private NetworkObject _netObject;

		private PlayerModel _playerModel;
		private bool _active = false;
		public void SetPlayerModel(PlayerModel playerModel)
		{
			_playerModel = playerModel;
			_active = true;
		}
		
		private void OnTriggerEnter(Collider other)
		{
			if (!_active)
			{
				return;
			}
			
			
			if (_playerModel.State != PlayerState.Slam)
			{
				return;
			}
			
			var netObject = other.GetComponent<NetworkObject>();
			if (netObject == null)
			{
				return;
			}

			if (netObject.ObjectId == _netObject.ObjectId)
			{
				return;
			}
			
			var impactReceiver = other.GetComponent<ISlamImpactReceiver>();
			if (impactReceiver == null)
			{
				return;
			}
			
			OnSlamImpactReceived?.Invoke(netObject.ObjectId);
		}
	}
	
	
}