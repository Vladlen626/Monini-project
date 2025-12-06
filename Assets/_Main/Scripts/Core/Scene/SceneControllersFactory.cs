using _Main.Scripts.Core;
using _Main.Scripts.Interactables.Crumb.Controllers;
using PlatformCore.Core;

namespace _Main.Scripts.Player.Network
{
	public static class SceneControllersFactory
	{
		public static IBaseController[] GetExtractionSceneControllers(
			GameModelContext gameModelContext,
			INetworkService networkService)
		{
			return new IBaseController[]
			{
				new CollectCrumbsController(gameModelContext, networkService)
			};
		}
	}
}