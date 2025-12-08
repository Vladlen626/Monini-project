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
			_playerBridge.PlayerName.OnChange += PlayerNameOnChangeHandler;
			_context.SetPlayerName(_playerBridge.PlayerName.Value);
		}

		protected override void OnDeactivate()
		{
			_playerBridge.PlayerName.OnChange += PlayerNameOnChangeHandler;
		}


		private void PlayerNameOnChangeHandler(string prev, string next, bool asServer)
		{
			_context.SetPlayerName(next);
		}

		
	}
}