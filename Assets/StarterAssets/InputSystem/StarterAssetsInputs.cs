using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		[Header("GameObjects")]
		public PauseMenu pauseMenu;
		public GameObject flashlight;

		public GameObject mainCam;

		public GameObject flyingCam;

		public GameObject followCam;

		public GameObject player;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}


		public void OnToggleFlashlight(InputValue value)
		{
			flashlight.SetActive(value.isPressed ? !flashlight.activeSelf : flashlight.activeSelf);
		}

		public void OnToggleFlyMode(InputValue value)
		{
			Debug.Log("toggledfly start");
			flyingCam.SetActive(value.isPressed ? !flyingCam.activeSelf : flyingCam.activeSelf);

			mainCam.SetActive(value.isPressed ? !mainCam.activeSelf : mainCam.activeSelf);
			followCam.SetActive(value.isPressed ? !followCam.activeSelf : followCam.activeSelf);
			player.SetActive(value.isPressed ? !player.activeSelf : player.activeSelf);
			
			/*
			if (value.isPressed)
			{
				!flyingCam.activeSelf;
			}
			else
			{
				flyingCam.activeSelf;
			}
			*/
			//flashlight.SetActive(value.isPressed ? !flashlight.activeSelf : flashlight.activeSelf);
		}

		public void OnToggleSettings(InputValue value)
		{
			pauseMenu.Toggle();
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}