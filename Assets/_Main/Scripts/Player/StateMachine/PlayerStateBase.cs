using _Main.Scripts.Player;
using UnityEngine;

public abstract class PlayerStateBase
{
	protected readonly PlayerModel Model;
	protected readonly PlayerView View;
	protected readonly CharacterController CC;

	protected PlayerStateBase(
		PlayerModel model,
		PlayerView view,
		CharacterController cc)
	{
		Model = model;
		View = view;
		CC = cc;
	}

	public abstract void Enter();
	public abstract void Exit();
}