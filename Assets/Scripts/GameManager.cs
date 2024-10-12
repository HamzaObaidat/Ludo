using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Button rollButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button moveButton;
    [SerializeField] private Image diceImage;
    [SerializeField] private Image chipImage;
    [SerializeField] private GameObject winEffect;
    [SerializeField] private Transform[] boardPositions; // Array of positions representing the Ludo board


    private int homeIndex;  // Index of the starting position (chip's home)
    private int goalIndex;  // Index of the final goal position

    private bool canRoll;  // Controls whether the dice can be rolled
    private bool canMove;  // Controls whether the chip can move
    private bool gameOver; // Game over state (not currently used)

    private int lastRollResult = 0;  // Stores the last dice roll result
    private int currentPosition = 0; // Keeps track of the chip's current position on the board
    private Sprite[] diceSprites;    // Array to store the individual dice face sprites

    private Animator animator;


    
    private void Start()
    {
        canRoll = true;
        animator = diceImage.GetComponent<Animator>(); // Get the Animator attached to the dice Image

        // Register button click event listeners
        rollButton.onClick.AddListener(RollDice);
        moveButton.onClick.AddListener(MoveChip);
        resetButton.onClick.AddListener(ResetChip);

        // Load images (dice faces and chip) using Unity Addressables system
        LoadAddressableAssets();

        // Define home and goal positions based on the board array
        homeIndex = 0;
        goalIndex = boardPositions.Length - 1;

    }


    #region LoadSprites
    // Load addressable assets (sprites for dice and chip)
    private void LoadAddressableAssets()
    {
        // Load dice face spritesheet
        Addressables.LoadAssetAsync<Sprite>("DieFace").Completed += OnDiceSpriteSheetLoaded;

        // Load chip sprite
        Addressables.LoadAssetAsync<Sprite>("ChipSprite").Completed += OnChipSpriteLoaded;

    }

    // Callback for when the dice spritesheet is successfully loaded
    private void OnDiceSpriteSheetLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Sprite spriteSheet = handle.Result;
            diceSprites = SplitSpriteSheet(spriteSheet, 6); // Assuming the spritesheet contains 6 faces of a dice
        }
        else
        {
            Debug.LogError("Failed to load DiceSpriteSheet");
        }
    }


    // Callback for when the chip sprite is successfully loaded
    private void OnChipSpriteLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            chipImage.sprite = handle.Result; // Assign the loaded sprite to the chip
        }
        else
        {
            Debug.LogError("Failed to load ChipSprite");
        }
    }

    // Split the loaded dice spritesheet into individual dice face sprites
    private Sprite[] SplitSpriteSheet(Sprite spriteSheet, int spriteCount)
    {
        Sprite[] sprites = new Sprite[spriteCount]; // Create an array to hold individual dice faces
        float spriteWidth = spriteSheet.texture.width / spriteCount; // Calculate width of each sprite
        float spriteHeight = spriteSheet.texture.height; // Use the full height of the sprite sheet

        // Loop through the spritesheet and extract individual dice faces
        for (int i = 0; i < spriteCount; i++)
        {
            sprites[i] = Sprite.Create(spriteSheet.texture,
                new Rect(i * spriteWidth, 0, spriteWidth, spriteHeight),
                new Vector2(0.5f, 0.5f));
        }

        return sprites; // Return the array of individual dice faces
    }
    #endregion

    #region Handle Dice
    // Handles dice rolling when Roll button is clicked
    private void RollDice()
    {
        if (canRoll) // Ensure rolling is allowed
        {
            SetButtonsInteractable(false, false, false); // Disable all buttons during the dice roll
            StartCoroutine(RollDiceAnimation());
        }
    }

    private IEnumerator RollDiceAnimation()
    {
        AudioManager.Instance.Play("DiceRoll");
        canRoll = false;
        diceImage.gameObject.SetActive(true);
        animator.enabled = true;
        animator.SetTrigger("Roll");

        yield return new WaitForSeconds(.5f);  // Adjust timing as needed

        animator.enabled = false;
        yield return StartCoroutine(GetRandomNumber());
    }

    // Coroutine to fetch a random number from an online API
    private IEnumerator GetRandomNumber()
    {
        yield return ApiManager.Instance.GetRandomNumber(OnRandomNumberReceived); // Call API and pass the result to the callback
    }

    // Callback for when a random number (dice roll result) is received from the API
    private void OnRandomNumberReceived(int result)
    {
        lastRollResult = result; // Store the roll result
        Debug.Log($"Dice roll result: {lastRollResult}");

        if (lastRollResult >= 1 && lastRollResult <= diceSprites.Length)
        {
            // Update the dice face to the corresponding sprite based on the roll result
            animator.SetBool("isRoll", false); // Stop the roll animation
            animator.enabled = false; // Disable the animator after rolling
            diceImage.sprite = diceSprites[lastRollResult - 1]; // Update dice image to show rolled number


            // Handle game logic when the chip is at home
            if (currentPosition == homeIndex)
            {
                if (lastRollResult != 6) // If the result isn't 6, the chip can't move out of home
                {
                    AudioManager.Instance.Play("Warning"); // Play warning sound
                    canRoll = true; // Allow another roll
                    canMove = false; // Disable chip movement
                    SetButtonsInteractable(true, false, true); // Enable Roll and Reset buttons
                }
                else
                {
                    canMove = true; // Chip can move if 6 is rolled
                    SetButtonsInteractable(false, true, true); // Enable Roll and Reset buttons

                }
            }
            // Prevent movement if the dice roll would exceed the goal
            else if (currentPosition + lastRollResult > goalIndex)
            {
                AudioManager.Instance.Play("Warning"); // Play warning sound
                Debug.Log("Need to roll again.");
                canRoll = true; // Allow another roll
                canMove = false; // Disable chip movement
                SetButtonsInteractable(true, false, true); // Enable Roll and Reset buttons
            }
            else
            {
                SetButtonsInteractable(false, true, true); // Enable Move button
                canMove = true; // Chip can move normally
            }
        }
        else
        {
            Debug.LogError($"Invalid roll result: {lastRollResult}. Expected 1-{diceSprites.Length}");
        }
    }

    #endregion

    /*    #region Handle Chip
        // Handle chip movement when Move button is clicked
        private void MoveChip()
        {
            if (lastRollResult > 0 && canMove) // Ensure movement is allowed
            {
                SetButtonsInteractable(false, false, false); // Disable all buttons during chip movement

                // Handle special case where chip leaves home after rolling a 6
                if (currentPosition == homeIndex && lastRollResult == 6)
                {
                    StartCoroutine(MoveChipStepByStep(1)); // Move chip out of home
                    Debug.Log("Chip exited home and moved to start point.");
                }
                else
                {
                    StartCoroutine(MoveChipStepByStep(lastRollResult)); // Move chip based on dice roll
                }
            }
        }

        // Coroutine to move the chip step by step
        private IEnumerator MoveChipStepByStep(int steps)
        {
            canMove = false; // Disable further movement during animation
            int targetPosition = currentPosition + steps; // Calculate target position

            // Move the chip to each board position step by step
            for (int i = currentPosition + 1; i <= targetPosition; i++)
            {
                AudioManager.Instance.PlayWithCooldown("ChipMove", 0.1f); // Play chip movement sound with cooldown
                chipImage.transform.DOMove(boardPositions[i].position, 0.2f).SetEase(Ease.Linear); // Animate chip movement
                yield return new WaitForSeconds(0.3f); // Wait before moving to the next position
            }

            currentPosition = targetPosition; // Update the chip's current position

            // Check if the chip has reached the goal
            if (currentPosition == goalIndex)
            {
                AudioManager.Instance.Play("GameOver"); // Play game over sound
                Debug.LogError("Game Over");
                WinEffect();
                SetButtonsInteractable(false, false, true); // Enable only the Reset button
            }
            else
            {
                canRoll = true; // Allow dice rolling after movement
                diceImage.gameObject.SetActive(false); // Hide dice image
                SetButtonsInteractable(true, false, true); // Enable Roll and Reset buttons
            }
        }
    */
    #region Handle Chip
    // Handle chip movement when Move button is clicked
    private void MoveChip()
    {
        if (lastRollResult > 0 && canMove) // Ensure movement is allowed
        {
            SetButtonsInteractable(false, false, false); // Disable all buttons during chip movement

            // Handle special case where chip leaves home after rolling a 6
            if (currentPosition == homeIndex && lastRollResult == 6)
            {
                StartCoroutine(MoveChipStepByStep(1)); // Move chip out of home
                Debug.Log("Chip exited home and moved to start point.");
            }
            else
            {
                StartCoroutine(MoveChipStepByStep(lastRollResult)); // Move chip based on dice roll
            }
        }
    }

    // Coroutine to move the chip step by step
    private IEnumerator MoveChipStepByStep(int steps)
    {
        canMove = false; // Disable further movement during animation
        int targetPosition = currentPosition + steps; // Calculate target position

        // Move the chip to each board position step by step
        for (int i = currentPosition + 1; i <= targetPosition; i++)
        {
            AudioManager.Instance.PlayWithCooldown("ChipMove", 0.1f); // Play chip movement sound with cooldown

            // Animate chip movement with ease and duration customization
            chipImage.transform.DOMove(boardPositions[i].position, 0.3f)
                .SetEase(Ease.InOutQuad) // Smoother easing for movement
                .OnComplete(() => Debug.Log("Movement to position " + i + " complete.")); // Callback when movement finishes

            // Wait a bit before moving to the next position
            yield return new WaitForSeconds(0.25f); // Reduce delay for smoother flow
        }

        currentPosition = targetPosition; // Update the chip's current position

        // Check if the chip has reached the goal
        if (currentPosition == goalIndex)
        {
            AudioManager.Instance.Play("GameOver"); // Play game over sound
            Debug.LogError("Game Over");
            winEffect.SetActive(true);
            SetButtonsInteractable(false, false, true); // Enable only the Reset button
        }
        else
        {
            canRoll = true; // Allow dice rolling after movement
            diceImage.gameObject.SetActive(false); // Hide dice image
            SetButtonsInteractable(true, false, true); // Enable Roll and Reset buttons
        }
    }

    // Handle resetting the chip to the start position when Reset button is clicked
    private void ResetChip()
    {
        winEffect.SetActive(false);

        currentPosition = 0; // Reset chip position
        chipImage.transform.position = boardPositions[0].position; // Move chip back to the start position
        lastRollResult = 0; // Reset last roll result
        canRoll = true; // Allow rolling again
        diceImage.gameObject.SetActive(false); // Hide dice image
        SetButtonsInteractable(true, false, true); // Enable Roll and Reset buttons
        Debug.Log("Reset chip position");
    }
    #endregion


    // Set the interactable states of the buttons
    private void SetButtonsInteractable(bool roll, bool move, bool reset)
    {
        rollButton.interactable = roll;  // Enable or disable Roll button
        moveButton.interactable = move;  // Enable or disable Move button
        resetButton.interactable = reset; // Enable or disable Reset button
    }

    // Clean up addressable assets when the game object is destroyed
    private void OnDestroy()
    {
        Addressables.Release(chipImage.sprite); // Release the loaded chip sprite
        //Addressables.Release(winImage.sprite); // Release the loaded chip sprite
        Debug.Log("Released addressable assets");
    }
}
