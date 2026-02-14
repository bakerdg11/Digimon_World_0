using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] private int energyAmount = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerStats>() is PlayerStats stats)
        {
            stats.AddEnergy(energyAmount);
            Destroy(gameObject);
        }
    }
}
