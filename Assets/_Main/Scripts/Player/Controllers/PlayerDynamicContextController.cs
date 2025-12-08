using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.UI;

namespace _Main.Scripts.Player
{
	public class PlayerDynamicContextController<T> : BaseContextController<T> where T: UIPlayerDynamicHud
	{
		private readonly PlayerNetworkBridge _playerBridge;
		
		public PlayerDynamicContextController(IUIService uiService, PlayerNetworkBridge bridge) : base(uiService)
		{
			_playerBridge = bridge;
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			_playerBridge.CrumbsCount.OnChange += CrumbsCountOnOnChange;
			_context.SetCrumbsNumber(_playerBridge.CrumbsCount.Value);
		}

		protected override void OnDeactivate()
		{
			_playerBridge.CrumbsCount.OnChange -= CrumbsCountOnOnChange;
			base.OnDeactivate();
		}

		private void CrumbsCountOnOnChange(int prev, int next, bool asServer)
		{
			_context.SetCrumbsNumber(next);
		}
	}
}