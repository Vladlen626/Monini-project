using _Main.Scripts.Location;
using FishNet.Object;
using UnityEngine;

namespace _Main.Scripts.Interactables.Crumb
{
	public class CrumbPlayerTrigger : PlayerTriggerNetworkBehaviour
	{
		[SerializeField]
		private int _crumbsValue = 1;
		
		public int CrumbsValue => _crumbsValue;

		protected override void OnPlayerEnterInTriggerClient(NetworkObject playerNetworkObject)
		{
			base.OnPlayerEnterInTriggerClient(playerNetworkObject);
			gameObject.SetActive(false);
		}
	}
}