using _Main.Scripts.Player;
using UnityEngine;

public abstract class PlayerStateBase
{
	protected readonly PlayerView View;
	protected readonly CharacterController CC;

	protected PlayerStateBase(
		PlayerView view,
		CharacterController cc)
	{
		View = view;
		CC = cc;
	}

	public abstract void Enter();
	public abstract void Exit();
}