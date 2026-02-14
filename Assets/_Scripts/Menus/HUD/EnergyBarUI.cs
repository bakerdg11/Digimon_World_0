using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Slider slider;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (stats == null)
            stats = FindFirstObjectByType<PlayerStats>();
    }

    private void OnEnable()
    {
        if (stats != null)
            stats.OnEnergyChanged.AddListener(UpdateBar);
    }

    private void OnDisable()
    {
        if (stats != null)
            stats.OnEnergyChanged.RemoveListener(UpdateBar);
    }

    private void UpdateBar(int energy, int max)
    {
        slider.maxValue = max;
        slider.value = energy;
    }
}
