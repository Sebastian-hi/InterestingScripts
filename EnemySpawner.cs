using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [Header("PREFABS:")]
    [SerializeField] private GameObject _spiderPrefab;
    [SerializeField] private GameObject _ghostPrefab;
    [Space]
    [SerializeField] private GameObject _knightPrefab;
    [SerializeField] private GameObject _golemPrefab;

    [Header("CONFIGS:")]
    [SerializeField] private EnemyConfig[] _spiderConfigs;
    [SerializeField] private EnemyConfig[] _ghostConfigs;
    [SerializeField] private EnemyConfig[] _knightConfigs;
    [SerializeField] private EnemyConfig[] _golemConfigs;

    [Header("OTHER:")]
    private Transform _player;

    private int _pointNumber;

    public enum SpawnTypeNow
    {
        Spider,
        Ghost,
        Knight,
        Golem,
        All
    }

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void Spawn(SpawnTypeNow type, DifficultyEnum difficulty)
    {
        Transform pointRandom = spawnPoints[0+ _pointNumber];

        _pointNumber++;

        if (_pointNumber >= spawnPoints.Length) 
        _pointNumber = 0;

        Vector2 point = new Vector3(pointRandom.position.x + UnityEngine.Random.Range(-1,1), 
        pointRandom.position.y+ UnityEngine.Random.Range(-1,1));

        GameObject prefab = GetPrefab(type);
        EnemyConfig config = GetConfig(type, difficulty);
        GameObject enemyNow = Instantiate(prefab, point, Quaternion.identity);

        Enemy enemy = enemyNow.GetComponent<Enemy>();

        enemy.InitConfig(config, _player);
    }


    private GameObject GetPrefab(SpawnTypeNow type)
    {
        switch (type)
        {
            case SpawnTypeNow.Spider: return _spiderPrefab;
            case SpawnTypeNow.Ghost: return _ghostPrefab;
            case SpawnTypeNow.Knight: return _knightPrefab;
            case SpawnTypeNow.Golem: return _golemPrefab;
        }

        return _spiderPrefab;
    }


    private EnemyConfig GetConfig(SpawnTypeNow type, DifficultyEnum difficulty)
    {
        EnemyConfig[] pool = GetPool(type);

        foreach(var enemy in pool)
        {
            if (enemy.difficulty == difficulty)
            {
                return enemy;
            }
        }

        return pool[0];
        
    }

    private EnemyConfig[] GetPool(SpawnTypeNow type)
    {
        switch (type)
        {
            case SpawnTypeNow.Spider: return _spiderConfigs;
            case SpawnTypeNow.Ghost: return _ghostConfigs;
            case SpawnTypeNow.Knight: return _knightConfigs;
            case SpawnTypeNow.Golem: return _golemConfigs;

            case SpawnTypeNow.All:
                return GetAllConfigs();

            default:
                return _spiderConfigs;
        }
    }


    private EnemyConfig[] GetAllConfigs()
    {
        var list = new List<EnemyConfig>();

        list.AddRange(_spiderConfigs);
        list.AddRange(_ghostConfigs);
        list.AddRange(_knightConfigs);
        list.AddRange(_golemConfigs);

        return list.ToArray();
    }
}