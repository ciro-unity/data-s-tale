﻿using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.InputSystem;

[UpdateBefore(typeof(MovementSystem))]
public class PlayerInputSystem : ComponentSystem, Player1InputActions.IPlayerActions
{
	private Player1InputActions player1Input;
	private Vector2 movementInput;
	private bool attackInput;

	//Input private variables
	private Vector2 cursorInitialPosition;
	private bool cursorPressed;

	protected override void OnCreate()
	{
		player1Input = new Player1InputActions();

		//Enabling the right set of bindings according to the platform
	#if UNITY_STANDALONE || UNITY_EDITOR
		player1Input.bindingMask = InputBinding.MaskByGroup(player1Input.KeyboardMouseScheme.bindingGroup);
	#elif UNITY_IOS || UNITY_ANDROID
		player1Input.bindingMask = InputBinding.MaskByGroup(player1Input.TouchScheme.bindingGroup);
	#endif

		player1Input.Player.SetCallbacks(this);
		player1Input.Player.Enable();
	}

	protected override void OnDestroy()
	{
		player1Input.Player.Disable();
	}

    protected override void OnUpdate()
    {
		Entities.ForEach((ref Movement movement, ref PlayerTag playerTag) =>
		{		
			//Pass the values to the ECS component
			Vector3 movement3 = new Vector3(movementInput.x, 0f, movementInput.y);
			movement = new Movement
			{
				MoveAmount = movement3,
			};
		});

		if(attackInput)
		{
			Entities.ForEach((ref Attack attack, ref PlayerTag playerTag) =>
			{		
				//Pass the values to the ECS component
				attack = new Attack
				{
					IsAttacking = true,
				};
			});

			attackInput = false;
		}
    }


	//----------------------------------  INPUT SYSTEM LISTENERS ----------------------------------------

	public void OnMove(InputAction.CallbackContext context)
	{
		movementInput = context.action.ReadValue<Vector2>();
	}

	public void OnPointerMove(InputAction.CallbackContext context)
	{
		switch (context.phase)
		{
			case InputActionPhase.Started:
				cursorInitialPosition = context.action.ReadValue<Vector2>();
				break;

			case InputActionPhase.Performed:
				movementInput = math.normalizesafe((context.action.ReadValue<Vector2>() - cursorInitialPosition) * .1f);
				break;

			case InputActionPhase.Canceled:
				movementInput = Vector2.zero;
				break;
		}
	}

	public void OnFire(InputAction.CallbackContext context)
	{
		attackInput = context.performed;
	}
}