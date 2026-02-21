using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwap : MonoBehaviour
{
    [Header("References")]
    public BasePlayerController playerController;

    [Header("Character List (Order Matters)")]
    public List<CharacterDefinition> characters = new List<CharacterDefinition>();
    // Index 0 = Player 1
    // Index 1 = Player 2

    private int currentIndex = 0;

    void Start()
    {
        if (playerController != null && characters.Count > 0)
        {
            currentIndex = 0;
            playerController.ApplyCharacterDefinition(characters[currentIndex]);
        }
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) SwapToIndex(0);
        else if (kb.digit2Key.wasPressedThisFrame) SwapToIndex(1);
        else if (kb.digit3Key.wasPressedThisFrame) SwapToIndex(2);
        else if (kb.digit4Key.wasPressedThisFrame) SwapToIndex(3);
        else if (kb.digit5Key.wasPressedThisFrame) SwapToIndex(4);
        else if (kb.digit6Key.wasPressedThisFrame) SwapToIndex(5);
        else if (kb.digit7Key.wasPressedThisFrame) SwapToIndex(6);
        else if (kb.digit8Key.wasPressedThisFrame) SwapToIndex(7);
        else if (kb.digit9Key.wasPressedThisFrame) SwapToIndex(8);
    }

    private void SwapToIndex(int index)
    {
        if (index < 0 || index >= characters.Count)
            return;

        if (index == currentIndex)
            return;

        currentIndex = index;
        playerController.ApplyCharacterDefinition(characters[currentIndex]);
    }

    public void AddCharacterToRoster(CharacterDefinition newChar)
    {
        if (newChar == null) return;

        // prevent duplicates
        if (characters.Contains(newChar))
        {
            Debug.Log($"{newChar.displayName} already in roster.");
            return;
        }

        characters.Add(newChar);
        Debug.Log($"Added {newChar.displayName} to roster. Total now: {characters.Count}");

        // Optional: auto-swap to the new character
        // currentIndex = characters.Count - 1;
        // playerController.ApplyCharacterDefinition(characters[currentIndex]);
    }
}