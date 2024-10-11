using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Button rollButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Image diceImage;
    [SerializeField] private Image chipImage;
    [SerializeField] private Transform chipTransform;
    [SerializeField] private Transform[] boardPositions;

    [SerializeField] private AudioSource audioSource;


    private bool canRoll;
    private bool canMove;

    private int lastRollResult = 0;
    private int currentPosition = 0;
    private Sprite[] diceSprites;
    private Sprite[] diceAnimationSprites;

    private Animator animator;

    private void Start()
    {
        canRoll = true;
        animator = diceImage.GetComponent<Animator>();
        rollButton.onClick.AddListener(RollDice);
        resetButton.onClick.AddListener(ResetChip);
        LoadAddressableAssets();
    }

    private void LoadAddressableAssets()
    {
        // Load static dice face sprites
        Addressables.LoadAssetAsync<Sprite>("DieFace").Completed += OnDiceSpriteSheetLoaded;

        // Load chip sprite
        Addressables.LoadAssetAsync<Sprite>("ChipSprite").Completed += OnChipSpriteLoaded;
    }

    private void OnDiceSpriteSheetLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Sprite spriteSheet = handle.Result;
            diceSprites = SplitSpriteSheet(spriteSheet, 6); // Assuming 6 sprites in the sheet
        }
        else
        {
            Debug.LogError("Failed to load DiceSpriteSheet");
        }
    }

    private void OnChipSpriteLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            chipImage.sprite = handle.Result;
        }
        else
        {
            Debug.LogError("Failed to load ChipSprite");
        }
    }

    private Sprite[] SplitSpriteSheet(Sprite spriteSheet, int spriteCount)
    {
        Sprite[] sprites = new Sprite[spriteCount];
        float spriteWidth = spriteSheet.texture.width / spriteCount;
        float spriteHeight = spriteSheet.texture.height;

        for (int i = 0; i < spriteCount; i++)
        {
            sprites[i] = Sprite.Create(spriteSheet.texture,
                new Rect(i * spriteWidth, 0, spriteWidth, spriteHeight),
                new Vector2(0.5f, 0.5f));
        }

        return sprites;
    }

    private void RollDice()
    {
        // Start the dice roll animation and provide the callback to run after the animation finishes
        if(canRoll)
            StartCoroutine(RollDiceAnimation(OnDiceAnimationComplete));
    }

    private IEnumerator RollDiceAnimation(Action callback)
    {
        audioSource.Play();
        canRoll = false;
        diceImage.gameObject.SetActive(true);
        animator.enabled = true;
        animator.SetBool("isRoll", true);
        yield return new WaitForSeconds(0.05f);

        // Call the callback once the animation finishes
        callback?.Invoke();
    }
    
    private void OnDiceAnimationComplete()
    {
        // After the animation completes, get the random number from the API
        StartCoroutine(GetRandomNumberFromAPI());
    }

    private IEnumerator GetRandomNumberFromAPI()
    {
        yield return ApiManager.instance.GetRandomNumber(OnRandomNumberReceived);
    }

    private void OnRandomNumberReceived(int result)
    {
        lastRollResult = result;
        Debug.Log($"Dice roll result: {lastRollResult}");

        if (lastRollResult >= 1 && lastRollResult <= diceSprites.Length)
        {
            animator.SetBool("isRoll", false);
            animator.enabled = false;

            diceImage.sprite = diceSprites[lastRollResult - 1]; // Adjust for 0-based index
            canMove = true;
        }
        else
        {
            Debug.LogError($"Invalid roll result: {lastRollResult}. Expected 1-{diceSprites.Length}");
        }
    }

    public void MoveChip()
    {
        if (lastRollResult > 0 && canMove)
        {
            // Start the step-by-step movement using DOTween
            StartCoroutine(MoveChipStepByStep(lastRollResult));
        }
    }

    private IEnumerator MoveChipStepByStep(int steps)
    {
        canMove = false;
        int targetPosition = currentPosition + steps;

        // Ensure targetPosition doesn't exceed board bounds
        if (targetPosition >= boardPositions.Length)
        {
            targetPosition = boardPositions.Length - 1;
        }

        // Move step by step
        for (int i = currentPosition + 1; i <= targetPosition; i++)
        {
            // Tween the chip to the next board position
            chipTransform.DOMove(boardPositions[i].position, 0.5f).SetEase(Ease.Linear);

            // Wait until the movement to the current position is complete
            yield return new WaitForSeconds(0.8f); // Adjust time to control movement speed
        }

        // Update the current position to the final target position
        currentPosition = targetPosition;

        // Allow rolling the dice again
        canRoll = true;
        diceImage.gameObject.SetActive(false); // Hide the dice after movement
    }

    private void ResetChip()
    {
        currentPosition = 0;
        chipTransform.position = boardPositions[0].position;
        lastRollResult = 0;

        canRoll = true;
        diceImage.gameObject.SetActive ( false );
        Debug.Log("Reset chip position");
    }

    private void OnDestroy()
    {
        // Release the loaded assets
        if (diceSprites != null)
        {
            foreach (var sprite in diceSprites)
            {
                Addressables.Release(sprite);
            }
        }
        if (diceAnimationSprites != null)
        {
            foreach (var sprite in diceAnimationSprites)
            {
                Destroy(sprite);
            }
        }
        Addressables.Release(chipImage.sprite);
        Debug.Log("Released addressable assets");
    }
}
