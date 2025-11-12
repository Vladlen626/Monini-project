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
	public PlayerModel Model { get; protected set; }
	public List<IBaseController> Controllers { get; protected set; } = new();

	protected readonly List<IPlayerLocalService> _locals = new();

	// ReSharper disable Unity.PerformanceAnalysis
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

		public static async UniTask<Client> CreateAsync(
			PlayerView view, IObjectFactory factory, PlayerFactory playerFactory, CancellationToken ct)
		{
			var ctx = new Client
			{
				View = view,
				Model = new PlayerModel()
			};

			var input = new InputLocalService();
			var camera = new CameraLocalService(factory);

			await input.InitAsync(ct);
			await camera.InitAsync(ct);

			ctx._locals.Add(input);
			ctx._locals.Add(camera);

			ctx.Input = input;
			ctx.Camera = camera;

			ctx.Controllers.AddRange(
				playerFactory.GetPlayerBaseControllers(ctx.Model, view, input, camera)
			);

			return ctx;
		}

		public override void Dispose()
		{
			base.Dispose();
			Input = null;
			Camera = null;
		}
	}

	public sealed class Server : PlayerContext
	{
		private Server()
		{
		}

		public static async UniTask<Server> CreateAsync(
			PlayerView view, PlayerFactory playerFactory, CancellationToken ct)
		{
			var ctx = new Server
			{
				View = view,
				Model = new PlayerModel()
			};


			//Позде будут добавлены контроллеры, сейчас пустой
			ctx.Controllers.AddRange(
				playerFactory.GetServerControllers(ctx.Model, view)
			);

			return ctx;
		}
	}
}