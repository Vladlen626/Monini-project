using UnityEngine;

namespace _Main.Scripts.Player.StateMachine.States
{
	public class PlayerNormalState : PlayerStateBase
	{
		public PlayerNormalState(PlayerView view, CharacterController cc)
			: base(view, cc) {}

		public override void Enter()
		{
		}

		public override void Exit() {}
	}
}