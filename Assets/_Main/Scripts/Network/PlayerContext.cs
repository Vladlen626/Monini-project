using System;
using System.Collections.Generic;
using System.Threading;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Player;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.Factory;

public abstract class PlayerContext : IDisposable
{
	public PlayerView View { get; protected set; }
	public PlayerNetworkBridge Bridge { get; private set; }
	public List<IBaseController> Controllers { get; protected set; } = new();

	protected readonly List<IPlayerLocalService> _locals = new();
	
	public virtual void Dispose()
	{
		foreach (var s in _locals)
		{
			s.Dispose();
		}

		_locals.Clear();
	}

	public sealed class Client : PlayerContext
	{
		public IInputService Input { get; private set; }
		public ICameraService Camera { get; private set; }

		private Client()
		{
		}

		public static async UniTask<Client> CreateAsync(PlayerNetworkBridge bridge,
			PlayerView view, IObjectFactory factory, CancellationToken ct)
		{
			var context = new Client
			{
				View = view,
				Bridge = bridge
			};

			var input = new InputLocalService();
			var camera = new CameraLocalService(factory);

			await input.InitAsync(ct);
			await camera.InitAsync(ct);

			context._locals.Add(input);
			context._locals.Add(camera);

			context.Input = input;
			context.Camera = camera;

			context.Controllers.AddRange(
				PlayerControllersFactory.GetPlayerBaseControllers(context.Bridge, view, input, camera)
			);

			return context;
		}

		public override void Dispose()
		{
			base.Dispose();
			Input = null;
			Camera = null;
		}
	}
	
	// ========= SERVER =========
	public sealed class Server : PlayerContext
	{
		public PlayerModel Model { get; private set; }
		public int OwnerId { get; private set; }

		private Server() { }
		
		public static Server Create(
			int ownerId,
			PlayerView view,
			PlayerNetworkBridge bridge)
		{
			var context = new Server
			{
				OwnerId = ownerId,
				View = view,
				Bridge = bridge,
				Model = new PlayerModel()
			};

			context.Controllers.AddRange(PlayerControllersFactory.GetPlayerServerControllers(context));

			return context;
		}
	}
}