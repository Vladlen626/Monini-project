using _Main.Scripts.Location;
using UnityEngine;

namespace _Main.Scripts.Interactables.Crumb
{
	public class CrumbPlayerTrigger : PlayerTriggerNetworkBehaviour
	{
		[SerializeField]
		private int _crumbsValue;
		
		public int CrumbsValue => _crumbsValue;
	}
}