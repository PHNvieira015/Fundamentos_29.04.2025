using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    private int pos = 2; // Start in middle lane (position 2)
    float NewXPos = 0f;
    private bool SwipeLeft;
    private bool SwipeRight;
    private bool SwipeUp;
    private bool SwipeDown;

    public float XValue = 2.5f; // Default lane width if not set in Inspector
    [SerializeField] private float smoothTime = 0.3f; // Smoothing time (lower = faster)
    [SerializeField] private GameObject characterToMove; // Character to move and rotate, set in inspector
    [SerializeField] private float rotationAmount = 30f; // Maximum rotation amount
    private Vector3 currentVelocity = Vector3.zero; // Current velocity (used by SmoothDamp)
    public Animator animator; // Reference to the animator component
    [SerializeField] private float jumpHeight = 1.5f; // Made positive
    [SerializeField] private float jumpDuration = 0.5f; // Made positive
    private bool isJumping = false;
    private bool isSliding = false;
    private float jumpStartTime;
    private float startY; //Starting Y position
    private float slideStartTime;
    private GameManager GM;

    [Header("Mobile Input Settings")]
    [SerializeField] private bool useMobileInput = false;
    [SerializeField] private float swipeThreshold = 50f;
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool isTouching = false;

    void Start()
    {
        pos = 2;
        GM = GameObject.FindObjectOfType<GameManager>();

        if (Application.isMobilePlatform)
        {
            useMobileInput = true;
        }

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
            if (useMobileInput)
            {
                MobileInputManager();
            }
            else
            {
                DesktopInputManager();
            }

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

    void DesktopInputManager()
    {
        SwipeLeft = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        SwipeRight = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
        SwipeUp = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space);
        SwipeDown = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftControl);

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

    void MobileInputManager()
    {
        // Reset swipe flags
        SwipeLeft = false;
        SwipeRight = false;
        SwipeUp = false;
        SwipeDown = false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    isTouching = true;
                    break;

                case TouchPhase.Ended:
                    if (isTouching)
                    {
                        endTouchPosition = touch.position;
                        DetectSwipe();
                        isTouching = false;
                    }
                    break;

                case TouchPhase.Canceled:
                    isTouching = false;
                    break;
            }
        }

#if UNITY_EDITOR
        if (useMobileInput)
        {
            if (Input.GetMouseButtonDown(0))
            {
                startTouchPosition = Input.mousePosition;
                isTouching = true;
            }
            else if (Input.GetMouseButtonUp(0) && isTouching)
            {
                endTouchPosition = Input.mousePosition;
                DetectSwipe();
                isTouching = false;
            }
        }
#endif

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
    }

    void DetectSwipe()
    {
        Vector2 swipeVector = endTouchPosition - startTouchPosition;
        float swipeDistance = swipeVector.magnitude;

        // Check if swipe distance meets threshold
        if (swipeDistance < swipeThreshold)
        {
            return; // Too short to be a swipe
        }

        swipeVector.Normalize();

        // Determine swipe direction
        // Horizontal swipe (left/right)
        if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
        {
            if (swipeVector.x > 0)
            {
                SwipeRight = true;
            }
            else
            {
                SwipeLeft = true;
            }
        }
        else
        {
            if (swipeVector.y > 0)
            {
                SwipeUp = true;
            }
            else
            {
                SwipeDown = true;
            }
        }
    }

    void Move()
    {
        // Calculate target X position based on lane position (pos)
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
            startY = characterToMove.transform.position.y;
        }
    }

    void UpdateJump()
    {
        float jumpProgress = (Time.time - jumpStartTime) / jumpDuration;

        if (jumpProgress >= 1.0f)
        {
            isJumping = false;
            Vector3 landedPosition = characterToMove.transform.position;
            landedPosition.y = startY;
            characterToMove.transform.position = landedPosition;
            return;
        }

        // Parabolic jump calculation (Corrected)
        float normalizedHeight = -4 * jumpHeight * (jumpProgress - 0.5f) * (jumpProgress - 0.5f) + jumpHeight;

        // Apply the height to the character
        Vector3 newPos = characterToMove.transform.position;
        newPos.y = startY + normalizedHeight;
        characterToMove.transform.position = newPos;
    }

    void Slide()
    {
        if (!isSliding)
        {
            isSliding = true;
            slideStartTime = Time.time;
            if (animator != null)
            {
                animator.SetTrigger("Slide");
                Debug.Log("Slide triggered!");
            }
        }
    }

    void UpdateSliding()
    {
        if (!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.LeftControl))
        {
            isSliding = false;
            if (animator != null)
            {
                animator.SetBool("StopSlide", true);
            }
        }
        else
        {
            Vector3 newPosition = characterToMove.transform.position;
            newPosition.y = startY * 0.5f;
            characterToMove.transform.position = newPosition;
        }
    }

    private void SetValue(int value)
    {
        if (value < 1)
            pos = 1;
        else if (value > 3)
            pos = 3;
        else
            pos = value;
    }
}