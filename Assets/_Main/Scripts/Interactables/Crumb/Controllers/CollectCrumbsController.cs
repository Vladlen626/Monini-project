using _Main.Scripts.Player;
using PlatformCore.Core;
using Unity.Services.Multiplayer;

namespace _Main.Scripts.Interactables.Crumb.Controllers
{
	public class CollectCrumbsController : IBaseController
	{
		private readonly GameModelContext _gameModelContext;
		public CollectCrumbsController(GameModelContext gameModelContext)
		{
			_gameModelContext = gameModelContext;
		}
		
	}
}