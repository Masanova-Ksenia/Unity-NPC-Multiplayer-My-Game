using UnityEngine;

public class AttackIndicator : MonoBehaviour
{
    public GameObject indicatorCircle; // Круг под ногами игрока
    public float attackRange = 3f;

    void Update()
    {
        if (indicatorCircle != null)
        {
            // Показывать радиус атаки
            indicatorCircle.transform.localScale = Vector3.one * attackRange * 2;
        }
    }
}