using FishNet;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;

public static class DebugNet
{
	public static bool Enabled = true;

	public static void Server(string message)
	{
		if (!Enabled)
		{
			return;
		}

		if (InstanceFinder.NetworkManager.IsServerStarted)
		{
			Debug.Log($"<color=#ff4444>[SERVER]</color> {message}");
		}
	}

	public static void Client(string message)
	{
		if (!Enabled)
		{
			return;
		}

		if (InstanceFinder.NetworkManager.IsClientOnlyStarted)
		{
			Debug.Log($"<color=#44aaff>[CLIENT]</color> {message}");
		}
	}

	public static void Owner(NetworkBehaviour nb, string message)
	{
		if (!Enabled)
		{
			return;
		}

		if (nb.IsOwner)
		{
			Debug.Log($"<color=#44ff44>[OWNER]</color> {message}");
		}
	}

	public static void TryAll(string message)
	{
		if (!Enabled)
		{
			return;
		}
		
		if (InstanceFinder.NetworkManager.IsServerStarted)
		{
			Debug.Log($"<color=#ff4444>[SERVER]</color> {message}");
		}

		if (InstanceFinder.NetworkManager.IsClientOnlyStarted)
		{
			Debug.Log($"<color=#44aaff>[CLIENT]</color> {message}");
		}
	}
}