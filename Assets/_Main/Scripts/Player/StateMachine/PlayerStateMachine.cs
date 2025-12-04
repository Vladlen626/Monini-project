using System;
using System.Collections.Generic;
using _Main.Scripts.Player;
using _Main.Scripts.Player.StateMachine.States;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerStateMachine
{
	private readonly PlayerStateBase _normal;
	private readonly PlayerStateBase _slam;
	private readonly PlayerStateBase _flat;

	private PlayerStateBase _current;

	public PlayerStateMachine(PlayerView view, CharacterController cc)
	{
		_normal = new PlayerNormalState(view, cc);
		_slam = new PlayerSlamState(view, cc);
		_flat = new PlayerFlatState(view, cc);
	}

	public void ChangeState(PlayerState state)
	{
		var newState = ResolveState(state);

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