using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    [Header("Energy Settings")]
    [SerializeField] private int maxEnergy = 100;
    public int MaxEnergy => maxEnergy;

    // Energy per character
    private readonly Dictionary<CharacterDefinition, int> _energyByCharacter = new();

    // Make sure this is never null
    public UnityEvent<int, int> OnEnergyChanged = new UnityEvent<int, int>();

    private BasePlayerController player;

    private void EnsurePlayer()
    {
        if (player == null)
            player = GetComponent<BasePlayerController>();
    }

    private void Start()
    {
        EnsurePlayer();
        if (player == null) return;

        EnsureEntry(player.currentCharacter);
        RaiseChanged();
    }

    private void EnsureEntry(CharacterDefinition def)
    {
        if (def == null) return;
        if (!_energyByCharacter.ContainsKey(def))
            _energyByCharacter[def] = 0;
    }

    public int GetEnergy(CharacterDefinition def)
    {
        EnsureEntry(def);
        return def != null && _energyByCharacter.TryGetValue(def, out var v) ? v : 0;
    }

    public bool IsEnergyFull(CharacterDefinition def)
    {
        return GetEnergy(def) >= maxEnergy;
    }

    public bool IsCurrentEnergyFull()
    {
        EnsurePlayer();
        if (player == null || player.currentCharacter == null) return false;
        return IsEnergyFull(player.currentCharacter);
    }

    public void AddEnergy(int amount)
    {
        if (amount <= 0) return;

        EnsurePlayer();
        if (player == null) return;

        var def = player.currentCharacter;
        if (def == null) return;

        EnsureEntry(def);

        _energyByCharacter[def] = Mathf.Clamp(_energyByCharacter[def] + amount, 0, maxEnergy);
        RaiseChanged();
    }

    public bool TrySpendEnergy(int amount)
    {
        EnsurePlayer();
        if (player == null) return false;

        var def = player.currentCharacter;
        if (def == null) return false;

        EnsureEntry(def);

        if (_energyByCharacter[def] < amount)
            return false;

        _energyByCharacter[def] -= amount;
        RaiseChanged();
        return true;
    }

    public void SpendAllEnergy()
    {
        EnsurePlayer();
        if (player == null) return;

        var def = player.currentCharacter;
        if (def == null) return;

        EnsureEntry(def);
        _energyByCharacter[def] = 0;
        RaiseChanged();
    }

    // Call this when character swaps
    public void OnCharacterChanged(CharacterDefinition newDef)
    {
        EnsurePlayer();
        EnsureEntry(newDef);
        RaiseChanged();
    }

    private void RaiseChanged()
    {
        EnsurePlayer();
        if (player == null) return;

        var def = player.currentCharacter;
        if (def == null) return;

        OnEnergyChanged.Invoke(GetEnergy(def), maxEnergy);
    }
}
