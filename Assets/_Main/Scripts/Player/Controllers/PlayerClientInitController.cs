using System.Threading;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;

namespace _Main.Scripts.Player.Controllers
{
	public class PlayerClientInitController : IBaseController, IActivatable
	{
		private readonly INetworkConnectionEvents _connectionEvents;
		private readonly IObjectFactory _objectFactory;
		private readonly LifecycleService _lifecycle;

		private CancellationTokenSource _cts;

		public PlayerClientInitController(
			INetworkConnectionEvents connectionEvents,
			IObjectFactory objectFactory,
			LifecycleService lifecycle)
		{
			_connectionEvents = connectionEvents;
			_objectFactory = objectFactory;
			_lifecycle = lifecycle;
		}

		public void Activate()
		{
			_cts = new CancellationTokenSource();

			_connectionEvents.OnLocalClientLoadedStartScenes += OnLocalClientLoadedStartScenes;
		}

		public void Deactivate()
		{
			_connectionEvents.OnLocalClientLoadedStartScenes -= OnLocalClientLoadedStartScenes;

			_cts?.Cancel();
			_cts?.Dispose();
			_cts = null;
		}

		private void OnLocalClientLoadedStartScenes()
		{
			InitializeLocalPlayerAsync(_cts.Token).Forget();
		}

		private async UniTaskVoid InitializeLocalPlayerAsync(CancellationToken ct)
		{
			PlayerNetworkBridge bridge = null;
			
			await UniTask.WaitUntil(() =>
			{
				bridge = PlayerNetworkUtils.FindLocalPlayerBridge();
				return bridge != null;
			}, cancellationToken: ct);

			if (bridge == null)
			{
				return;
			}

			var view = bridge.GetComponent<PlayerView>();

			var ctx = await PlayerContext.Client.CreateAsync(bridge, view, _objectFactory, ct);

			ctx.Camera.AttachTo(view.CameraRoot);

			foreach (var controller in ctx.Controllers)
			{
				await _lifecycle.RegisterAsync(controller);
			}
		}
	}
}