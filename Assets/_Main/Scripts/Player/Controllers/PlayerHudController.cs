using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.UI;

namespace _Main.Scripts.Player
{
	public class PlayerHudController : IBaseController, IActivatable
	{
		private readonly IUIService _uiService;

		public PlayerHudController(IUIService uiService)
		{
			_uiService = uiService;
		}

		public void Activate()
		{
		}

		public void Deactivate()
		{
		}
	}
}	