using UnityEngine;

namespace _Main.Scripts.Player.StateMachine.States
{
	public class PlayerNormalState : PlayerStateBase
	{
		public PlayerNormalState(PlayerModel model, PlayerView view, CharacterController cc)
			: base(model, view, cc) {}

		public override void Enter()
		{
		}

		public override void Exit() {}
	}
}