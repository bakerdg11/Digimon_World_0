using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwap : MonoBehaviour
{
    [Header("References")]
    public BasePlayerController playerController;

    [Header("Character List (Order Matters)")]
    public CharacterDefinition[] characters;
    // Index 0 = Player 1
    // Index 1 = Player 2

    private int currentIndex = 0;

    void Start()
    {
        if (playerController != null && characters.Length > 0)
        {
            currentIndex = 0;
            playerController.ApplyCharacterDefinition(characters[currentIndex]);
        }
    }

    void Update()
    {
        // Press 1 → First character
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SwapToIndex(0);
        }

        // Press 2 → Second character
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SwapToIndex(1);
        }
    }

    private void SwapToIndex(int index)
    {
        if (index < 0 || index >= characters.Length)
            return;

        if (index == currentIndex)
            return;

        currentIndex = index;
        playerController.ApplyCharacterDefinition(characters[currentIndex]);
    }
}