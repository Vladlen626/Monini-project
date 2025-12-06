using System.Collections.Generic;
using _Main.Scripts.Core;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Interactables.Crumb.Controllers
{
	public class CollectCrumbsController : IBaseController, IActivatable
	{
		private readonly GameModelContext _gameModelContext;
		private readonly INetworkService _networkService;
		private readonly Dictionary<int, CrumbPlayerTrigger> _crumbPlayerTriggers = new ();
		private SceneContext SceneContext => _gameModelContext.SceneContext;
		private NetworkModel NetworkModel => _gameModelContext.NetworkModel;
		public CollectCrumbsController(GameModelContext gameModelContext, INetworkService networkService)
		{
			_gameModelContext = gameModelContext;
			_networkService = networkService;
			foreach (var crumbPlayerTrigger in SceneContext.CrumbsNetworkBehaviours)
			{
				_crumbPlayerTriggers.Add(crumbPlayerTrigger.ObjectId, crumbPlayerTrigger);
			}
		}

		public void Activate()
		{
			foreach (var crumbPlayerTrigger in SceneContext.CrumbsNetworkBehaviours)
			{
				crumbPlayerTrigger.OnPlayerEntered += PlayerEnteredCrumbHandler;
			}
		}

		public void Deactivate()
		{
			foreach (var crumbPlayerTrigger in SceneContext.CrumbsNetworkBehaviours)
			{
				crumbPlayerTrigger.OnPlayerEntered -= PlayerEnteredCrumbHandler;
			}
			_crumbPlayerTriggers.Clear();
		}

		private void PlayerEnteredCrumbHandler(int objectId, int playerId)
		{
			var crumb = _crumbPlayerTriggers[objectId];
			NetworkModel.ownerContexts[playerId].Model.CollectCrumbs(crumb.CrumbsValue);
			_networkService.Despawn(crumb);
		}
		
	}
}