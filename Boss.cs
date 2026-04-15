using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Video;

public class Boss : MonoBehaviour, IDamageable
{
    [Header("Base:")]
    public static float hpBoss = 1000;
    [SerializeField] private GameObject[] _damageFX;
    [SerializeField] private float _addRotate = 10;

    [Header("STAGE 1")]
    [SerializeField] private GameObject _oldItems;
    [SerializeField] private GameObject _enemyFirstStage;
    [SerializeField] private GameObject _itemsFirstStage;


    [Header("STAGE 2")]
    [SerializeField] private GameObject _enemySecondStage;
    [SerializeField] private GameObject _itemsSecondStage;
    [SerializeField] private Transform _spawnStage2;
    [SerializeField] private Camera _cameraStage2Anim;
    [SerializeField] private Animator _animatorFakeGround;
    [SerializeField] private AudioSource _audioSourceStartStage2;

    [SerializeField] private GameObject _moveWallDeath;
    [SerializeField] private Camera _cameraStage2MoveWall;
    [SerializeField] private GameObject _checkPointStage2;
    [SerializeField] private GameObject _checkPoint2Stage2;

    [Header("STAGE 3")]
    public static CylinderBoss[] requiredBossCylinder;
    public static BossMapTurret[] requiredBossTurrets;
    [SerializeField] private GameObject _shieldWallBoss;
    [SerializeField] private GameObject _ultraWeaponBox;
    [SerializeField] private Camera _cameraUltraWeapon;
    [SerializeField] private GameObject[] _cubeParts;

    private Renderer[] _cubeRenderers;
    private Color[] _startCubeColors;
    private Color _endColor = Color.red;



    private int _FXFirstWave = 4;
    private int _FXSecondWave = 9;

    public static CameraBoss[] requiredCameras;
    public static int camerasDeathCount = 0;
    public static int bossTurretDestroyCount = 0;

    public static int cilinderBossDestroyCount = 0;

    private Camera _mainCamera;

    private bool _firstStageStart = false;

    public static bool allCamerasDestroy = false;
    private bool _allBossTurretDestroy = false;
    private bool _secondStageStarting = false;
    public static bool thirdStageStarting = false;

    private bool _finalStageStarting = false;

    private GameObject _player;

    private BossRotate _bossRotate;
    public static event Action OnBossHPUpdate;

    public static bool spawnEnemy1Stage = false;

    private bool _isWin = false;

    private void Awake()
    {
        PrepareToPlayer(); // Off all

        _cubeRenderers = new Renderer[_cubeParts.Length];
        _startCubeColors = new Color[_cubeParts.Length];

        for (int i = 0; i < _cubeParts.Length; i++)
        {
            _cubeRenderers[i] = _cubeParts[i].GetComponent<Renderer>();
            _startCubeColors[i] = _cubeRenderers[i].material.color;
        }
    }

    private void OnEnable()
    {
        foreach (var camera in requiredCameras)
        {
            if (camera != null)
                camera.OnDestroyCamera += OnCamerasDeath;
        }

        foreach (var bossTurret in requiredBossTurrets)
        {
            if (bossTurret != null)
                bossTurret.OnDestroyBossTurret += OnBossTurretDestroy;
        }

        CylinderBoss.OnAnyCylinderDestroyed += OnBossCylinderDestroy;
    }

    private void OnDisable()
    {
         foreach (var camera in requiredCameras)
        {
            if (camera != null)
                camera.OnDestroyCamera -= OnCamerasDeath;
        }

        foreach (var bossTurret in requiredBossTurrets)
        {
            if (bossTurret != null)
                bossTurret.OnDestroyBossTurret -= OnBossTurretDestroy;
        }

        CylinderBoss.OnAnyCylinderDestroyed -= OnBossCylinderDestroy;
    }

    private void Update()
    {
        // First Stage
        if (spawnEnemy1Stage && !_firstStageStart)
        {
            _firstStageStart = true;
            _enemyFirstStage.SetActive(true);
            _itemsFirstStage.SetActive(true);
            Destroy(_oldItems);
        }   

        // Second Stage
        if (allCamerasDestroy && !_secondStageStarting)
        {
            _secondStageStarting = true;
            StartCoroutine(SecondStageStart());
            _bossRotate.AddRotateBoss(_addRotate);
        }

        // Third Stage 
        if (! thirdStageStarting && _allBossTurretDestroy)
        {
            thirdStageStarting = true;
            StartCoroutine(ThirdStage());
        }

        // Final Stage
        if (UltraWeaponBox.openedUltraWeapon && !_finalStageStarting)
        {
            _finalStageStarting = true;
            _shieldWallBoss.SetActive(true);

            foreach (GameObject cubeParts in _cubeParts)
            {
                cubeParts.tag = "Boss";
            }
        }

        if (Managers.Game.playerWin && !_isWin)
        {
            _isWin = true;
            _enemySecondStage.SetActive(false);
            _moveWallDeath.SetActive(false);
        }
    }

    private IEnumerator ThirdStage()
    {
        _bossRotate.AddRotateBoss(_addRotate);
        yield return new WaitForSeconds(2f);
        _audioSourceStartStage2.Play();
        yield return new WaitForSeconds(2f);

        _ultraWeaponBox.SetActive(true);
        Managers.Game.onCutScene = true;

        _mainCamera = Camera.main;
        _mainCamera.enabled = false;
        _cameraUltraWeapon.enabled = true;

        yield return new WaitForSeconds(4f);

        _cameraUltraWeapon.enabled = false;
        _mainCamera.enabled = true;

        Managers.Game.onCutScene = false;

    }

    private void OnCamerasDeath() // action event
    {
        camerasDeathCount++;

        if (camerasDeathCount >= requiredCameras.Length)
        {
            for (int i = 0; i < _FXFirstWave; i++)
            {
                _damageFX[i].SetActive(true);
            }
            
            hpBoss -= 200;
            Debug.Log("HP BOSS: " + hpBoss);
            allCamerasDestroy = true;
        }
    }

    private void OnBossCylinderDestroy()
    {
        requiredBossTurrets[cilinderBossDestroyCount].ShieldOff();
        
        cilinderBossDestroyCount++;
    }

    private void OnBossTurretDestroy()
    {
        bossTurretDestroyCount++;

        if (bossTurretDestroyCount >= requiredBossTurrets.Length)
        {
            for (int i = _FXFirstWave; i < _FXSecondWave; i++)
            {
                _damageFX[i].SetActive(true);
            }

            hpBoss -= 300;
            Debug.Log("HP BOSS: " + hpBoss);
            _allBossTurretDestroy = true;
        }
    }

    private IEnumerator SecondStageStart()
    {
        yield return new WaitForSeconds(2f);
        _audioSourceStartStage2.Play();
        yield return new WaitForSeconds(2f);

        // CutScene Stage2
        Managers.Game.onCutScene = true;
        _mainCamera = Camera.main;
        _mainCamera.enabled = false;
        _cameraStage2Anim.enabled = true;
        _animatorFakeGround.enabled = true;

        _player = GameObject.FindGameObjectWithTag("Player");
        _player.GetComponent<CharacterController>().enabled = false;
        _player.transform.position = _spawnStage2.position;
        _player.GetComponent<CharacterController>().enabled = true;
        _checkPointStage2.SetActive(true);
        _checkPoint2Stage2.SetActive(true);

        yield return new WaitForSeconds(5f);

        Destroy(_enemyFirstStage);
        Destroy(_itemsFirstStage);

        _cameraStage2Anim.enabled = false;
        _cameraStage2MoveWall.enabled = true;


        _moveWallDeath.SetActive(true);

        _enemySecondStage.SetActive(true);
        _itemsSecondStage.SetActive(true);

        yield return new WaitForSeconds(4f);

        _cameraStage2MoveWall.enabled = false;
        _mainCamera.enabled = true;
        Managers.Game.onCutScene = false;
    }


    private void PrepareToPlayer()
    {
        requiredCameras = GetComponentsInChildren<CameraBoss>();
        requiredBossCylinder = GetComponentsInChildren<CylinderBoss>();
        requiredBossTurrets = GetComponentsInChildren<BossMapTurret>();
        _bossRotate = GetComponentInChildren<BossRotate>();

        cilinderBossDestroyCount = 0;
        bossTurretDestroyCount = 0;
        camerasDeathCount = 0;

        foreach (var FX in _damageFX)
        {
            if (FX != null)
            {
                FX.SetActive(false);
            }
        }
        _enemyFirstStage.SetActive(false);
        _enemySecondStage.SetActive(false);
        _itemsFirstStage.SetActive(false);
        _itemsSecondStage.SetActive(false);


        _checkPointStage2.SetActive(false);
        _checkPoint2Stage2.SetActive(false);

       _moveWallDeath.SetActive(false);

        _animatorFakeGround.enabled = false;  
        _cameraStage2Anim.enabled = false;
        _cameraStage2MoveWall.enabled = false;
        _cameraUltraWeapon.enabled = false;
        _shieldWallBoss.SetActive(false);
        _ultraWeaponBox.SetActive(false);

        foreach (GameObject cubeParts in _cubeParts)
        {
            cubeParts.tag = "Untagged";
        }
    }

    public void TakeDamage(int damage)
    {
        hpBoss -= damage;
        _bossRotate.AddRotateBoss(2f);

        float hpPercent = Mathf.Clamp01(hpBoss / 1000f);

    
        for (int i = 0; i < _cubeRenderers.Length; i++)
        {
            if (_cubeRenderers[i] != null)
            {
                _cubeRenderers[i].material.color = Color.Lerp(_endColor, _startCubeColors[i], hpPercent);
            }
        }

        if (hpBoss <= 0)
        {
            hpBoss = 0;
            Managers.Game.playerWin = true;
            Managers.Game.onCutScene = true;
            Debug.Log("Игрок выиграл!!");
        }

        OnBossHPUpdate?.Invoke();
    }
}
