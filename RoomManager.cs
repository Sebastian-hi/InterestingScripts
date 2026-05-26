using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private bool _isDarkLevel = false;
    [SerializeField] private Light2D _globalLight;
    [SerializeField] private float _lightChangeDuration = 2f;
    [SerializeField] private GameObject _portalEnd;
    [SerializeField] private Transform _pointPortalEnd;

    [SerializeField] private Transform _spawnPlayerPoint;
    private GameObject _player;
    private Light2D _playerLight;

    [Header("STAGE 1:")]
    [SerializeField] private EnemySpawner _spawnerStage1;
    [SerializeField] private List<WaveSpawnData> _wave1;

    [Header("STAGE 2:")]
    [SerializeField] private EnemySpawner _spawnerStage2;
    [SerializeField] private List<WaveSpawnData> _wave2;
    [SerializeField] private Color _colorStage2;

    [Header("STAGE 3:")]
    [SerializeField] private EnemySpawner _spawnerStage3;
    [SerializeField] private List<WaveSpawnData> _wave3;
    [SerializeField] private Color _colorStage3;

    [Header("STAGE BOSS:")]
    [SerializeField] private EnemySpawner _spawnerBossStage;
    [SerializeField] private List<WaveSpawnData> _bossWave;
    [SerializeField] private Color _colorBossStage;


    [Header("STAGE UI:")]
    [SerializeField] private GameObject _stage1Image;
    [SerializeField] private GameObject _stage2Image;
    [SerializeField] private GameObject _stage3Image;
    [SerializeField] private GameObject _stageBossStage;

    [SerializeField] private bool _isBossSpawner = false;

    private bool _firstWaveStart = false;
    private bool _secondWaveStart = false;
    private bool _thirdWaveStart = false;
    private bool _bossWaveStart = false;
    private bool _allWavesDone = false;
    private int _aliveEnemies;
    private Color _defaultLightColor;
    private bool _waveFinishedSpawning = false;

    private void OnEnable()
    {
        Enemy.OnEnemyDead += EnemyDead;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDead -= EnemyDead;
    }

    private void Start()
    {
        _stage1Image.SetActive(false);
        _stage2Image.SetActive(false);
        _stage3Image.SetActive(false);



        _player = GameObject.FindGameObjectWithTag("Player");
        _player.transform.position = _spawnPlayerPoint.position;

        _playerLight = _player.GetComponentInChildren<Light2D>(true);

        if (_isDarkLevel)
        {
            _player.GetComponent<PlayerMoveAttack>().LightOn();
            _globalLight.intensity = 0.05f;
        }
        
        _defaultLightColor = GetCurrentLight().color;

        Invoke(nameof(StartStage1), 3f);
    }

    private IEnumerator SpawnWave(EnemySpawner spawner, List<WaveSpawnData> wave, GameObject stageImage) 
    {
        _waveFinishedSpawning = false;
        StartCoroutine(ShowStageImage(stageImage));

        foreach (var spawnData in wave)
        {
            for (int i = 0; i < spawnData.count; i++)
            {
                _aliveEnemies++;
                spawner.Spawn(spawnData.enemyType,spawnData.difficulty);

                yield return new WaitForSeconds(spawnData.delayBetweenSpawns);
            }
        }

        _waveFinishedSpawning = true;
    }

    private Light2D GetCurrentLight()
    {
        if (_isDarkLevel) return _playerLight;
        else return _globalLight;
    }
    
    private void StartStage1()
    {
        StartCoroutine(SpawnWave(_spawnerStage1, _wave1, _stage1Image));
        _firstWaveStart = true;
    }

    private void StartStage2()
    {
       StartCoroutine(ChangeColorLight(_colorStage2));
       StartCoroutine(SpawnWave(_spawnerStage2, _wave2, _stage2Image));

        _secondWaveStart = true;
    }

    private void StartStage3()
    {
        StartCoroutine(ChangeColorLight(_colorStage3));
        StartCoroutine(SpawnWave(_spawnerStage3, _wave3, _stage3Image));

        _thirdWaveStart = true;
    }

    private void StartBossStage()
    {
        StartCoroutine(ChangeColorLight(_colorBossStage));
        StartCoroutine(SpawnWave(_spawnerBossStage, _bossWave, _stageBossStage));

        _bossWaveStart = true;
    }

    private void NextWave()
    {
        if (_firstWaveStart && !_secondWaveStart)
        {
            Invoke(nameof(StartStage2), 3f);
        }
        else if (_secondWaveStart && !_thirdWaveStart)
        {
            Invoke(nameof(StartStage3), 3f);
        }
        else if (_thirdWaveStart && _isBossSpawner &&  !_bossWaveStart)
        {
            Invoke(nameof(StartBossStage), 3f);
        }
        else if (_allWavesDone)
        {
            Invoke(nameof(FinishRoom), 1f);
        }
    }

    private void EnemyDead()
    {
        _aliveEnemies--;

        if (_aliveEnemies <= 0 && _waveFinishedSpawning)
        {
            if (_thirdWaveStart && !_isBossSpawner)
            {
                _allWavesDone = true;
            }
            else if (_isBossSpawner && _bossWaveStart)
            {
                _allWavesDone = true;
            }

            NextWave();
        }
    }


    private IEnumerator ShowStageImage(GameObject image)
    {
        image.SetActive(true);

        yield return new WaitForSeconds(3f);

        image.SetActive(false);
    }

    private IEnumerator ChangeColorLight(Color targetColor)
    {
        Light2D currentLight = GetCurrentLight();
        Color startColor = currentLight.color;

        float time = 0f;

        while (time < _lightChangeDuration)
        {
            time += Time.deltaTime;
            float t = time / _lightChangeDuration;
            currentLight.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        currentLight.color = targetColor;
    }

    private void FinishRoom()
    {
        Debug.Log("All waves is done");
        StartCoroutine(ChangeColorLight(_defaultLightColor));
        Instantiate(_portalEnd, _pointPortalEnd.position, Quaternion.identity);
    }
}