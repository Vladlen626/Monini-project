using PlatformCore.Core;
using PlatformCore.Services.UI;

namespace _Main.Scripts.Player
{
	public class PlayerStaticContextController<T> : BaseContextController<T> where T: UIPlayerStaticHud
	{
		private readonly PlayerNetworkBridge _playerBridge;
		
		public PlayerStaticContextController(IUIService uiService, PlayerNetworkBridge bridge) : base(uiService)
		{
			_playerBridge = bridge;
		}

		protected override void OnActivate()
		{
			base.OnActivate();
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
		}
	}
}