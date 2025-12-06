using System;
using UnityEngine;

namespace _Main.Scripts.Player.UI
{
	public class DisableOnAwake : MonoBehaviour
	{
		private void Awake()
		{
			gameObject.SetActive(false);
		}
	}
}