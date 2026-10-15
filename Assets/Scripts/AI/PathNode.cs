using UnityEngine;

public class PathNode
{
    public bool isWalkable;       // Чи немає на цьому вузлі стіни
    public Vector3 worldPosition; // Позиція у світових координатах
    public int gridX;            // Індекс X у масиві
    public int gridY;            // Індекс Y у масиві

    public int gCost; // Вартість від старту до вузла
    public int hCost; // Евристична оцінка до цілі
    public PathNode parent; // Для відновлення шляху

    // === HEATMAP (Теплова карта) ===
    public float heatValue; // Рівень "теплоти" перебування гравця

    public int FCost => gCost + hCost;

    public PathNode(bool isWalkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.isWalkable = isWalkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
        this.heatValue = 0f;
    }

    public void AddHeat(float amount, float maxHeat)
    {
        heatValue = Mathf.Clamp(heatValue + amount, 0f, maxHeat);
    }

    public void CoolDown(float decayAmount)
    {
        heatValue = Mathf.Max(0f, heatValue - decayAmount);
    }
}