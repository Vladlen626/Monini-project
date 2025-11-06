using _Main.Scripts.Core;
using PlatformCore.Core;

public class Bootstrap : BaseBootstrap
{
	protected override BaseGameRoot CreateGameRoot()
	{
		return new GameRoot();
	}
}
