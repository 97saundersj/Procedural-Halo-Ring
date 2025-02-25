using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(Rigidbody))]
public class FlyCamera : MonoBehaviour
{
    public float acceleration = 50; // how fast you accelerate
    public float accSprintMultiplier = 4; // how much faster you go when "sprinting"
    public float lookSensitivity = 1; // mouse look sensitivity
    public float dampingCoefficient = 5; // how quickly you break to a halt after you stop your input

    public bool focusOnEnable = true; // whether or not to focus and lock cursor immediately on enable

    public float mouseMovementThreshold = 20f; // Define a threshold for mouse movement

    public float boostTimeToMax = 3f; // Time in seconds to reach max speed when boosting

    public PauseMenu pauseMenu;
    public GameObject mainCam;
	public GameObject flyingCam;
	public GameObject followCam;
	public GameObject player;

    private float currentBoostMultiplier = 1f; // Current boost multiplier

    Vector3 velocity; // current velocity

    Rigidbody rb; // Reference to the Rigidbody

    static bool Focused
    {
        get => Cursor.lockState == CursorLockMode.Locked;
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = value == false;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Disable gravity if you want the camera to fly
        rb.isKinematic = false; // Ensure the Rigidbody is not kinematic
    }

    void OnEnable()
    {
        if (focusOnEnable) Focused = true;
    }

    void OnDisable() => Focused = false;

    void Update()
    {
        // Check if the game is paused
        if (Time.timeScale == 0f)
        {
            Focused = false;
            return;
        }

        // Input
        if (Focused)
            UpdateInput();
        else if (Input.GetMouseButtonDown(0))
            Focused = true;

        // Update the boost multiplier
        UpdateBoostMultiplier();

        // Physics
        velocity = Vector3.Lerp(velocity, Vector3.zero, dampingCoefficient * Time.deltaTime);
        rb.velocity = velocity;
    }

    void UpdateInput()
    {
        // Position
        velocity += GetAccelerationVector() * Time.deltaTime;

        // Rotation
        Vector2 mouseDelta = lookSensitivity * new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));

        // Ignore large changes in mouse movement
        if (mouseDelta.magnitude > mouseMovementThreshold)
        {
            mouseDelta = Vector2.zero;
        }

        // Add arrow key input for rotation
        if (Input.GetKey(KeyCode.LeftArrow))
            mouseDelta.x -= lookSensitivity;
        if (Input.GetKey(KeyCode.RightArrow))
            mouseDelta.x += lookSensitivity;
        if (Input.GetKey(KeyCode.UpArrow))
            mouseDelta.y -= lookSensitivity;
        if (Input.GetKey(KeyCode.DownArrow))
            mouseDelta.y += lookSensitivity;

        // Add Q and E key input for rotation
        float roll = 0f;
        if (Input.GetKey(KeyCode.Q))
            roll += lookSensitivity;
        if (Input.GetKey(KeyCode.E))
            roll -= lookSensitivity;

        Quaternion rotation = transform.localRotation;
        Quaternion horiz = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
        Quaternion vert = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);
        Quaternion rollRotation = Quaternion.AngleAxis(roll, Vector3.forward);
        transform.localRotation = rotation * horiz * vert * rollRotation;

        // Leave cursor lock
        if (Input.GetKeyDown(KeyCode.Escape))
            Focused = false;
    }

    void UpdateBoostMultiplier()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // Gradually increase the boost multiplier
            currentBoostMultiplier = Mathf.MoveTowards(currentBoostMultiplier, accSprintMultiplier, Time.deltaTime * (accSprintMultiplier - 1) / boostTimeToMax);
        }
        else
        {
            // Reset the boost multiplier when not boosting
            currentBoostMultiplier = 1f;
        }
    }

    Vector3 GetAccelerationVector()
    {
        Vector3 moveInput = default;

        void AddMovement(KeyCode key, Vector3 dir)
        {
            if (Input.GetKey(key))
                moveInput += dir;
        }

        AddMovement(KeyCode.W, Vector3.forward);
        AddMovement(KeyCode.S, Vector3.back);
        AddMovement(KeyCode.D, Vector3.right);
        AddMovement(KeyCode.A, Vector3.left);
        AddMovement(KeyCode.Space, Vector3.up);
        AddMovement(KeyCode.LeftControl, Vector3.down);
        Vector3 direction = transform.TransformVector(moveInput.normalized);

        // Use the current boost multiplier
        return direction * acceleration * currentBoostMultiplier;
    }

    #if ENABLE_INPUT_SYSTEM

		public void OnToggleFlyMode(InputValue value)
		{
			Debug.Log("toggledfly cam");
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
}