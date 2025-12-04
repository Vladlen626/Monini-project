using UnityEngine;

namespace _Main.Scripts.Player.StateMachine.States
{
	public class PlayerSlamState : PlayerStateBase
	{
		public PlayerSlamState(PlayerView view, CharacterController cc)
			: base(view, cc) {}

		public override void Enter()
		{
			CC.excludeLayers = LayerMask.GetMask("SlamReceiver", "Player");
		}

		public override void Exit()
		{
			CC.excludeLayers = 0;
		}
	}
}