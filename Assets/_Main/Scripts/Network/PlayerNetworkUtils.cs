using _Main.Scripts.Core;
using FishNet;
using UnityEngine;

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
	
	public static void MoveToPersistent(GameObject gameObjectToMove)
	{
		var persistent = UnityEngine.SceneManagement.SceneManager.GetSceneByName(SceneNames.PersistentScene);
		if (persistent.IsValid())
		{
			UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObjectToMove, persistent);
		}
	}
}