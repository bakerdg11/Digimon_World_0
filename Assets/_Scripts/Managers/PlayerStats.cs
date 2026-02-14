using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    [Header("Energy")]
    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private int energy = 0;

    public int Energy => energy;
    public int MaxEnergy => maxEnergy;
    public bool IsEnergyFull => energy >= maxEnergy;

    public UnityEvent<int, int> OnEnergyChanged;

    void Start()
    {
        OnEnergyChanged?.Invoke(energy, maxEnergy);
    }

    public void AddEnergy(int amount)
    {
        if (amount <= 0) return;

        int old = energy;
        energy = Mathf.Clamp(energy + amount, 0, maxEnergy);

        if (energy != old)
            OnEnergyChanged?.Invoke(energy, maxEnergy);
    }

    public bool TrySpendEnergy(int amount)
    {
        if (energy < amount) return false;
        energy -= amount;
        OnEnergyChanged?.Invoke(energy, maxEnergy);
        return true;
    }

    public void SetEnergyFull()
    {
        energy = maxEnergy;
        OnEnergyChanged?.Invoke(energy, maxEnergy);
    }


}
