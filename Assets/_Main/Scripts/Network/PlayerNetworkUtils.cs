using FishNet;

public static class PlayerNetworkUtils
{
	public static PlayerNetworkBridge FindLocalPlayerBridge()
	{
		var objects = InstanceFinder.ClientManager.Objects;
		
		foreach (var nob in objects.Spawned)
		{
			if (nob.Value.Owner.IsLocalClient)
			{
				var bridge = nob.Value.GetComponent<PlayerNetworkBridge>();
				if (bridge != null)
				{
					return bridge;
				}
			}
		}

		return null;
	}
}