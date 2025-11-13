using System.Collections.Generic;
using _Main.Scripts.Player;
using _Main.Scripts.Player.StateMachine.States;
using UnityEngine;

public class PlayerStateMachine
{
	private readonly PlayerStateBase _normal;
	private readonly PlayerStateBase _slam;
	private readonly PlayerStateBase _flat;

	private PlayerStateBase _current;

	public PlayerStateMachine(PlayerModel model, PlayerView view, CharacterController cc)
	{
		_normal = new PlayerNormalState(model, view, cc);
		_slam = new PlayerSlamState(model, view, cc);
		_flat = new PlayerFlatState(model, view, cc);
	}

	public void HandleStateChange(PlayerState state)
	{
		var newState = ResolveState(state);
		if (newState == _current)
		{
			return;
		}

		_current?.Exit();

		_current = newState;

		_current?.Enter();
	}

	private PlayerStateBase ResolveState(PlayerState state)
	{
		switch (state)
		{
			case PlayerState.Normal:
			{
				return _normal;
			}
			case PlayerState.Slam:
			{
				return _slam;
			}
			case PlayerState.Flat:
			{
				return _flat;
			}
			default:
			{
				return _normal;
			}
		}
	}
}