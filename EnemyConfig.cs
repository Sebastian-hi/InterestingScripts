using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("STATS:")]
    public float health;
    public float speed;
    public float attackCooldown;
    public float damage;
    public Vector3 scale;
    public float attackDistance;
    public float timeDestroyAfterDeath;

    [Header("COLOR:")]
    public Color color;

    [Header("TYPE:")]
    public EnemyType type;

    [Header("DIFFICULTY:")]
    public DifficultyEnum difficulty;

    public enum EnemyKind
    {
        Spider,
        Ghost,
        Knight,
        Golem,
    }

    public enum EnemyType
    {
        Weak,
        Fast,
        Special,
        Boss
    }

    public GameObject dropPrefab;
}
