using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject _playerCanvasCrosshair;
    [SerializeField] private GameObject _menuCanvas;
    [Header("CROSSHAIR:")]
    [SerializeField] private Image _crosshair;
    [SerializeField] private Sprite _crosshairBase;
    [SerializeField] private Sprite _crosshairHit;
    [Header("WEAPON HUD:")]
    [SerializeField] private TextMeshProUGUI _bulletsHUDText;
    [SerializeField] private GameObject _reloadHUDText;
    [SerializeField] private GameObject _ricochetImage;

    [Header("HEALTH HUD:")]
    [SerializeField] private TextMeshProUGUI _healthHUDText;

    [Header("BOSS HUD:")]
    [SerializeField] private GameObject _HPBoss;
    [SerializeField] private TextMeshProUGUI _hpBossText;

    [Header("TASK 1 WHITE TERMINALS:")]
    [SerializeField] private GameObject _task1;
    [SerializeField] private TextMeshProUGUI _task1Text;
    [Header("TASK 2 CAMERAS ON BOSS:")]
    [SerializeField] private GameObject _task2;
    [SerializeField] private TextMeshProUGUI _task2Text;
    [Header("TASK 3 MAIN TERMINAL:")]
    [SerializeField] private GameObject _task3;
    [SerializeField] private TextMeshProUGUI _task3Text;
    [Header("TASK 4 CILYNDER & BOSS TURRET:")]
    [SerializeField] private GameObject _task4_1;
    [SerializeField] private TextMeshProUGUI _task4Text1;
     [SerializeField] private GameObject _task4_2;
    [SerializeField] private TextMeshProUGUI _task4Text2;
    [Header("TASK 5 TAKE A PRESENT:")]
    [SerializeField] private GameObject _task5_1;
    [SerializeField] private GameObject _task5_2;
    [Header("TASK 6 FINAL ROUND:")]
    [SerializeField] private GameObject _task6_1;
    [SerializeField] private GameObject _task6_2;

    private PlayerShooting _playerShooting;
    private PlayerHealth _playerHealth;
    private CameraBoss[] _cameraBoss;
    private BossMapTurret[] _bossMapTurret;

    private Coroutine _hitCoroutine;
    private Coroutine _ricochetCoroutine;

    private bool _showTaskHUD = false;
    private bool _showTask2HUD = false;
    private bool _showLastTask = false;
    private bool _showUltraWeapon = false;

    private void Start()
    {
        _playerShooting = GetComponent<PlayerShooting>();
        if (_playerShooting != null) 
        {
            _playerShooting.OnEnemyHit += EnemyHit;
            _playerShooting.OnBulletsHUDUpdate += UpdateBulletsHUD;
            _playerShooting.OnRicochet += RicochetHUD;
        }
        else Debug.LogError("Script PlayerShooting not found in parent");


        _playerHealth = GetComponent<PlayerHealth>();
        if (_playerHealth != null) 
        {
            _playerHealth.OnHealthUpdate += UpdateHealthHUD;
        }
        else Debug.LogError("Script PlayerHealth not found in parent");

        _cameraBoss = FindObjectsOfType<CameraBoss>();
        foreach (var camera in _cameraBoss)
        {
            camera.OnDestroyCamera += UpdateTaskHUD;
        }

        _bossMapTurret = FindObjectsOfType<BossMapTurret>();
        foreach (var bossTurret in _bossMapTurret)
        {
            bossTurret.OnDestroyBossTurret += UpdateTaskHUD;
        }

        _playerHealth.OnRespawn += UpdateHealthHUD;
        _playerHealth.OnRespawn += RefreshUI;

        Terminal.OnAnyTerminalActivated += UpdateTaskHUD;

        Boss.OnBossHPUpdate += UpdateBossHPHUD;

        ResetAllTasks();

        ResetAllTasks();
        _HPBoss.SetActive(false);

        CylinderBoss.OnAnyCylinderDestroyed += SafeUpdateTaskHUD;
        
        _crosshair.sprite = _crosshairBase;

        _ricochetImage.SetActive(false);
        _menuCanvas.SetActive(false);
        _HPBoss.SetActive(false);
        _reloadHUDText.SetActive(false);

        _task1.SetActive(false);
        _task2.SetActive(false);
        _task3.SetActive(false);
        _task4_1.SetActive(false);
        _task4_2.SetActive(false);
        _task5_1.SetActive(false);
        _task5_2.SetActive(false);
        _task6_1.SetActive(false);
        _task6_2.SetActive(false);
    }
    private void Update()
    {
        CheckCutScene();
        
        if (StartSceneTrigger.heroIsReadyFightBoss && !_showTaskHUD)
        {
            _task1.SetActive(true);
            _showTaskHUD = true;
            UpdateTaskHUD();
            _HPBoss.SetActive(true);
        }
        else if (Terminal.terminalSecondStageDone && !_showTask2HUD)
        {
            _task1.SetActive(false);
            _task2.SetActive(false);
            _task3.SetActive(false);
            _task4_1.SetActive(true);
            _task4_2.SetActive(true);
            _showTask2HUD = true;
            UpdateTaskHUD();
        }

        if (Boss.thirdStageStarting && !_showLastTask) 
        {
            _showLastTask = true;
            UpdateTaskHUD();
        }

        if (UltraWeaponBox.openedUltraWeapon && !_showUltraWeapon)
        {
            _showUltraWeapon = true;
            _task1.SetActive(false);
            _task2.SetActive(false);
            _task3.SetActive(false);
            _task4_1.SetActive(false);
            _task4_2.SetActive(false);
            _task5_1.SetActive(false);
            _task5_2.SetActive(false);
            _task6_1.SetActive(true);
            _task6_2.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !Managers.Game.onCutScene && Managers.Player.PlayerAlive)
        {
            PlayerMoveController playerMoveController = GetComponent<PlayerMoveController>();
            playerMoveController.StopAnimations();
            bool isActive = !_menuCanvas.activeSelf;
            _menuCanvas.SetActive(isActive);

            Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isActive;

            PlayerShooting shooting = GetComponent<PlayerShooting>();
            if (shooting != null) shooting.enabled = !isActive;

            PlayerMoveController moveController = GetComponent<PlayerMoveController>();
            if (moveController != null) moveController.enabled = !isActive;
        }
    }

    private void CheckCutScene()
    {
        if (Managers.Game.onCutScene || !Managers.Player.PlayerAlive) _playerCanvasCrosshair.SetActive(false);
        else _playerCanvasCrosshair.SetActive(true);
    }

    private void EnemyHit()
    {
        if (_hitCoroutine != null) StopCoroutine(_hitCoroutine);
        _hitCoroutine = StartCoroutine(ChangeCrossHairHit());
    }

    private IEnumerator ChangeCrossHairHit()
    {
        _crosshair.sprite = _crosshairHit;
        
        yield return new WaitForSeconds(0.1f);

        _crosshair.sprite = _crosshairBase;
        _hitCoroutine = null;
    }

    private void RicochetHUD()
    {
        if (_ricochetCoroutine != null) StopCoroutine(_ricochetCoroutine);
        _ricochetCoroutine = StartCoroutine(ShowRicochetImage());
    }

    private IEnumerator ShowRicochetImage()
    {
        _ricochetImage.SetActive(true);

        yield return new WaitForSeconds(0.4f);

        _ricochetImage.SetActive(false);
    }

    private void UpdateBulletsHUD()
    {
        _bulletsHUDText.text = $"{_playerShooting.bulletsNow}/{_playerShooting.bulletsAll}";

        if (_playerShooting.bulletsNow <= 0 && _playerShooting.bulletsAll >= 1)
        _reloadHUDText.SetActive(true);
        else _reloadHUDText.SetActive(false);
        
    }

    private void UpdateHealthHUD()
    {
        _healthHUDText.text = $"{_playerHealth.curHealth}/{Managers.Player.MaxHealth}";
    }

    private void UpdateTaskHUD()
    {
        ResetAllTasks();
        if (Managers.Game.onCutScene || !Managers.Player.PlayerAlive)
        {
            ResetAllTasks();
            return;
        }

        if (!StartSceneTrigger.heroIsReadyFightBoss)
        {
            ResetAllTasks();
            _HPBoss.SetActive(false);
            return;
        }

        // STAGE 1
        if (!Terminal.terminalFirstStageDone)
        {
            _task1.SetActive(true);
            _task1Text.text = $"{Terminal.activatedStage1}/{Terminal.needActivatedStage1}";
            UpdateBossHPHUD();
            return;
        }

        // STAGE 1.5 CAMERAS
        if (!Boss.allCamerasDestroy)
        {
            _task2.SetActive(true);
            _task2Text.text = $"{Boss.camerasDeathCount}/{Boss.requiredCameras.Length}";
            UpdateBossHPHUD();
            return;
        }

        // STAGE 2 MAIN TERMINAL
        if (!Terminal.terminalSecondStageDone)
        {
            _task3.SetActive(true);
            _task3Text.text = $"{Terminal.activatedStage2}/{Terminal.needActivatedStage2}";
            UpdateBossHPHUD();
            return;
        }

        // STAGE 2
        if (!Boss.thirdStageStarting)
        {
            _task4_1.SetActive(true);
            _task4_2.SetActive(true);
            _task4Text1.text = $"{Boss.bossTurretDestroyCount}/{Boss.requiredBossTurrets.Length}";
            _task4Text2.text = $"{Boss.cilinderBossDestroyCount}/{Boss.requiredBossCylinder.Length}";
            UpdateBossHPHUD();
            return;
        }

        if (!UltraWeaponBox.openedUltraWeapon && Boss.thirdStageStarting)
        {
            _task4_1.SetActive(false);
            _task4_2.SetActive(false);
            _task5_1.SetActive(true);
            _task5_2.SetActive(true);
        }
        
        if (UltraWeaponBox.openedUltraWeapon)
        {
            _task5_1.SetActive(false);
            _task5_2.SetActive(false);
            _task6_1.SetActive(true);
            _task6_2.SetActive(true);
        }

        // STAGE 3 - FINAL ROUND
        if (Managers.Game.playerWin)
        {
            _task6_1.SetActive(false);
            _task6_2.SetActive(false);
            _HPBoss.SetActive(false);
        }
    }

    private void UpdateBossHPHUD()
    {
        _hpBossText.text = $"{Boss.hpBoss} / 1000";
    }

    private void SafeUpdateTaskHUD()
    {
        if (this == null || gameObject == null) return;

        ResetAllTasks();
        UpdateTaskHUD();
    }

    private void OnDestroy()
    {
        if (_playerShooting != null)
        {
            _playerShooting.OnEnemyHit -= EnemyHit;
            _playerShooting.OnBulletsHUDUpdate -= UpdateBulletsHUD;
        } 
        if (_playerHealth != null) 
        {
            _playerHealth.OnHealthUpdate -= UpdateHealthHUD;
        }
        _cameraBoss = FindObjectsOfType<CameraBoss>();
        foreach (var camera in _cameraBoss)
        {
            camera.OnDestroyCamera -= UpdateTaskHUD;
        }

        _bossMapTurret = FindObjectsOfType<BossMapTurret>();
        foreach (var bossTurret in _bossMapTurret)
        {
            bossTurret.OnDestroyBossTurret -= UpdateTaskHUD;
        }

        
        Terminal.OnAnyTerminalActivated -= UpdateTaskHUD;
        Boss.OnBossHPUpdate -= UpdateBossHPHUD;
        CylinderBoss.OnAnyCylinderDestroyed -= SafeUpdateTaskHUD;
    }


    public void Bind(PlayerHealth newHealth)
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnHealthUpdate -= UpdateHealthHUD;
        }

        _playerHealth = newHealth;

        _playerHealth.OnHealthUpdate += UpdateHealthHUD;
        UpdateHealthHUD();
    }

    private void ResetAllTasks()
    {
        _task1.SetActive(false);
        _task2.SetActive(false);
        _task3.SetActive(false);
        _task4_1.SetActive(false);
        _task4_2.SetActive(false);
        _task5_1.SetActive(false);
        _task5_2.SetActive(false);
        _task6_1.SetActive(false);
        _task6_2.SetActive(false);
    }

    private void RefreshUI()
    {
        ResetAllTasks();

        if (!StartSceneTrigger.heroIsReadyFightBoss)
            return;

        UpdateTaskHUD();
    }
}
