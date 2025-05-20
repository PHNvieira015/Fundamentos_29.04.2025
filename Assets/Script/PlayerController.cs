using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private int pos = 2; // Start in middle lane (position 2)
    float NewXPos = 0f;
    private bool SwipeLeft;
    private bool SwipeRight;
    private bool SwipeUp;
    private bool SwipeDown;

    public float XValue = 2.5f; // Default lane width if not set in Inspector

    [SerializeField] private float smoothTime = 0.3f; // Tempo de suavização (menor = mais rápido)

    [SerializeField] private GameObject characterToMove; // Character to move and rotate, set in inspector
    [SerializeField] private float rotationAmount = 30f; // Maximum rotation amount

    private Vector3 currentVelocity = Vector3.zero; // Velocidade atual (usada pelo SmoothDamp)

    public Animator animator; // Reference to the animator component

    [SerializeField] private float jumpHeight = 1.5f; //  Made positive

    [SerializeField] private float jumpDuration = 0.5f; // Made positive

    private bool isJumping = false;
    private bool isSliding = false;
    private float jumpStartTime;
    private float startY; //Starting Y position
    private float slideStartTime;
    private GameManager GM;


    void Start()
    {
        // Initialize position to middle lane
        pos = 2;
        GM = GameObject.FindAnyObjectByType<GameManager>();

        if (characterToMove == null)
        {
            Debug.LogError("Character to Move is not assigned in the Inspector!");
            enabled = false;
        }
        startY = characterToMove.transform.position.y; // Store initial Y
    }

    void Update()
    {
        if (GM.canMoveRoad)
        {
            InputManager();
            Move();
            if (isJumping)
            {
                UpdateJump();
            }
            if (isSliding)
            {
                UpdateSliding();
            }
        }
    }

    void InputManager()
    {
        SwipeLeft = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        SwipeRight = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
        SwipeUp = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space);
        SwipeDown = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftControl); // Changed to GetKey

        if (SwipeLeft)
        {
            SetValue(pos - 1);
        }
        else if (SwipeRight)
        {
            SetValue(pos + 1);
        }
        else if (SwipeUp)
        {
            Jump();
        }
        else if (SwipeDown)
        {
            Slide();
        }

        Debug.Log("Current Lane: " + pos);
    }

    void Move()
    {
        // Calculate target X position based on lane position (pos)
        // Assuming lanes are at X positions: -XValue, 0, XValue
        if (pos == 1)
            NewXPos = -XValue;
        else if (pos == 2)
            NewXPos = 0f;
        else if (pos == 3)
            NewXPos = XValue;

        // Get current position of the character
        Vector3 currentPos = characterToMove.transform.position;

        // Create target position with only the X value changing
        Vector3 targetPos = new Vector3(NewXPos, currentPos.y, currentPos.z);

        // Calculate how far we are from target X position
        float xDifference = targetPos.x - currentPos.x;

        // Apply rotation in SAME direction as movement
        // When moving right (positive xDifference), rotate right (positive angle)
        float rotationValue = Mathf.Clamp(xDifference * 15f, -rotationAmount, rotationAmount);
        Quaternion targetRotation;

        if (Mathf.Abs(xDifference) > 0.1f)
        {
            // We're moving sideways, apply rotation
            targetRotation = Quaternion.Euler(0, rotationValue, 0);
        }
        else
        {
            // We've reached the target X, return to forward facing
            targetRotation = Quaternion.identity;
        }

        // Apply rotation to the character
        characterToMove.transform.rotation = Quaternion.Slerp(
            characterToMove.transform.rotation,
            targetRotation,
            Time.deltaTime * 5f);

        // Smoothly move the character toward the target X position
        characterToMove.transform.position = Vector3.SmoothDamp(
            currentPos,
            targetPos,
            ref currentVelocity,
            smoothTime);
    }

    void Jump()
    {
        if (!isJumping)
        {
            // Trigger the jump animation
            if (animator != null)
            {
                animator.SetTrigger("Jump");
                Debug.Log("Jump triggered!");
            }
            isJumping = true;
            jumpStartTime = Time.time;
            startY = characterToMove.transform.position.y; // Store this here!

        }
    }
    void UpdateJump()
    {
        float jumpProgress = (Time.time - jumpStartTime) / jumpDuration;

        if (jumpProgress >= 1.0f)
        {
            isJumping = false;
            // Optionally, you might want to snap the character back to startY here.
            Vector3 landedPosition = characterToMove.transform.position;
            landedPosition.y = startY;
            characterToMove.transform.position = landedPosition;
            return;
        }

        // Parabolic jump calculation (Corrected)
        float normalizedHeight = -4 * jumpHeight * (jumpProgress - 0.5f) * (jumpProgress - 0.5f) + jumpHeight;

        // Apply the height to the character
        Vector3 newPos = characterToMove.transform.position;
        newPos.y = startY + normalizedHeight; // Use startY
        characterToMove.transform.position = newPos;
    }



    void Slide()
    {
        if (!isSliding) // Start sliding only if not already sliding
        {
            isSliding = true;
            slideStartTime = Time.time; // Record when sliding started
            // Trigger the slide animation
            if (animator != null)
            {
                animator.SetTrigger("Slide");
                Debug.Log("Slide triggered!");
            }
        }
        //removed the Input.GetKey check from here.
    }

    void UpdateSliding()
    {
        if (!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.LeftControl)) // Stop sliding
        {
            isSliding = false;
            //  Optionally, reset any slide-specific properties (like scale) here.
            if (animator != null)
            {
                animator.SetBool("StopSlide",true);
            }
        }
        else
        {
            // Apply sliding movement.  For example, you might lower the character's Y position:
            Vector3 newPosition = characterToMove.transform.position;
            newPosition.y = startY * 0.5f;  // Example: Slide to half the original height
            characterToMove.transform.position = newPosition;
            // Or you might increase the Z scale to make it appear longer:
            //Vector3 newScale = characterToMove.transform.localScale;
            //newScale.z = 2f; // Example: Double the Z scale
            //characterToMove.transform.localScale = newScale;
        }
    }
    private void SetValue(int value)
    {
        // Apply boundaries: can't go lower than 1 or higher than 3
        if (value < 1)
            pos = 1;
        else if (value > 3)
            pos = 3;
        else
            pos = value;
    }
}