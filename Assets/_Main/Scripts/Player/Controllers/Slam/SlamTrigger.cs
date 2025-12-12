using System;
using Cysharp.Threading.Tasks;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;

namespace _Main.Scripts.Player
{
	public class SlamTrigger : MonoBehaviour
	{
		[SerializeField] 
		private NetworkObject _netObject;

		[SerializeField]
		private PlayerNetworkBridge _bridge;
		
		private Collider _collider;

		private void Start()
		{
			_collider = GetComponent<Collider>();

			if (_bridge)
			{
				_bridge.State.OnChange += OnPlayerStateChangedHandler;

				if (_bridge.State.Value == PlayerState.Slam)
				{
					EnableCollider();
				}
				else
				{
					DisableCollider();
				}
			}
		}

		private void OnDestroy()
		{
			if (_bridge)
			{
				_bridge.State.OnChange -= OnPlayerStateChangedHandler;
			}
		}

		private void OnPlayerStateChangedHandler(PlayerState playerState, PlayerState next, bool asServer)
		{
			if (next == PlayerState.Slam)
			{
				EnableCollider();
			}
			else
			{
				DisableCollider();
			}
		}

		private void EnableCollider()
		{
			_collider.enabled = true;
		}

		private async void DisableCollider()
		{
			await UniTask.WaitForFixedUpdate();
			_collider.enabled = false;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!_netObject.IsServerStarted)
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

			impactReceiver.OnSlamImpact();
		}
	}
}