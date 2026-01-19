using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour,  IDamageable
{
    [Header("TURRET ROTATE")]
    [SerializeField] private Transform _turretRotateObj;
    [SerializeField] private Transform _turretUpGun;
    private float _rotationSpeedDegrees = 90f;
    private Vector3 _targetHorizontal;
    private Vector3 _targetFull;
    private Quaternion _targetRot;
    private Quaternion _targetUpRot;

    [Header("TURRET ATTACK")]
    [SerializeField] private int _hpTurret = 100;
    [SerializeField] private int _attackDamage = 25;
    public LayerMask hitLayers;
    [SerializeField] private Transform _bulletSpawner;
    [SerializeField] private float _attackRange = 150f;
    [SerializeField] private GameObject _shootFX;
    [SerializeField] private GameObject _shootMuzzleFX;
    [SerializeField] private float _plusHeightAim = 1.2f;
    private Transform _player;
    private bool _isShooting = false;
    private bool _playerInZone = false;
    private RaycastHit _raycastHit;
    private Vector3 _originPos;
    private Vector3 _directionShoot;

    [Header("TURRET DEATH")]
    [SerializeField] private bool _instantDeath;
    [SerializeField] private ParticleSystem _deathBoomFX;
    [SerializeField] private ParticleSystem _deathFireFX;

    [Header("TURRET SOUNDS")]
    private AudioSource _shootHitSource;
    [SerializeField] private AudioSource _shootMissSource;
    [SerializeField] private AudioSource _deathTurretSource;
    [SerializeField] private AudioSource _deathStartFireSource;
    [SerializeField] private AudioSource _deathFireSource;
    [SerializeField] private AudioClip[] _audioClipsShootHit;

    public event Action OnDeath;


    private Animator _animator;
    private bool _isDead = false;

    private int _randomChanceMiss;
    private int _randomNumberForHit;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _shootHitSource = GetComponent<AudioSource>();

        if(_deathBoomFX != null) _deathBoomFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if(_deathFireFX != null) _deathFireFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }


    private void Update()
    {
        if (_playerInZone && _player != null && !_isDead)
        {
            if (_instantDeath)
            {
                TakeDamage(200);
            }
            
            RotateToPlayer();
            CanShoot();
        }
    }

    private void RotateToPlayer()
    {
        _targetHorizontal = _player.position;
        _targetHorizontal.y = _turretRotateObj.position.y; // bez naclonov
        _targetRot = Quaternion.LookRotation(_targetHorizontal - _turretRotateObj.position, _turretRotateObj.up);
        _turretRotateObj.rotation = Quaternion.RotateTowards(_turretRotateObj.rotation, _targetRot, _rotationSpeedDegrees * Time.deltaTime);


        _targetFull = _player.position + Vector3.up * _plusHeightAim;
        _targetUpRot = Quaternion.LookRotation(_targetFull - _turretUpGun.position, _turretUpGun.up); // local up
        _turretUpGun.rotation = Quaternion.RotateTowards(_turretUpGun.rotation, _targetUpRot, _rotationSpeedDegrees * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_isDead)
        {
           _animator.SetTrigger("DetectPlayer");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !_isDead)
        {
            _playerInZone = true;
            _player = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !_isDead)
        {
            _playerInZone = false;
            _animator.SetBool("LookAtPlayer", false);
            _player = null;
        }
    }

    private void CanShoot()
    {
        _animator.SetBool("LookAtPlayer", true);

        if (!_isShooting && !_isDead)
        {
            _isShooting = true;
            StartCoroutine(ShootPlayer());
        }
    }

    private IEnumerator ShootPlayer()
    {
        yield return new WaitForSeconds(1.5f);

        if (_isDead)
        {
            yield break;
        }

        if (_player != null && !Managers.Game.onCutScene)
        {
            _animator.SetTrigger("Shoot");
            ShootTurrel();
        }

        yield return new WaitForSeconds(1.5f);
        _isShooting = false;
    }

    private void ShootTurrel()
    {
        if (_player == null) return;

        _originPos = _bulletSpawner.position;
        
        _directionShoot = (_player.position + Vector3.up * _plusHeightAim - _originPos).normalized; // shoot middle height hero
        GameObject activeMuzzlefX = Instantiate(_shootMuzzleFX, _bulletSpawner.position, _bulletSpawner.rotation);
        Destroy(activeMuzzlefX, 6f);

        if (Physics.Raycast(_originPos, _directionShoot, out _raycastHit, _attackRange, hitLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawRay(_originPos, _directionShoot * _attackRange, Color.red);

            if (ChanceMissYes())
            {
                _randomNumberForHit = UnityEngine.Random.Range(-6, 7);
                _raycastHit.point = new Vector3(_raycastHit.point.x +_randomNumberForHit, _raycastHit.point.y, _raycastHit.point.z + _randomNumberForHit);
            }
            else
            {
                Managers.Audio.ShootSoundInArray(_shootHitSource, _audioClipsShootHit);
                if (_raycastHit.collider.gameObject.CompareTag("Player"))
                {
                    PlayerHealth playerHealth =_raycastHit.collider.gameObject.GetComponentInParent<PlayerHealth>();
                    playerHealth.TakeDamage(_attackDamage);
                    Debug.Log("Turrel: Hit player!");
                }
                else Debug.Log("Turrel Attack! Hit: " + _raycastHit.collider.name);
            }

            GameObject activeShootEffect =  Instantiate(_shootFX, _raycastHit.point, Quaternion.identity);
            Destroy(activeShootEffect, 3f);

        }
        else
        {
            Debug.DrawRay(_originPos, _directionShoot * _attackRange, Color.red);
            Debug.Log("Turrel: missing");
        }
    }


    private bool ChanceMissYes() 
    {
        _randomChanceMiss = UnityEngine.Random.Range(0, 5);

        if (_randomChanceMiss == 4)
        {
            _shootMissSource.Play();
            Debug.Log("Turrel: Miss!");
            return true;
        } 
        else return false;
    }

    public void TakeDamage(int damage)
    {
        _hpTurret -= damage;

        if (_hpTurret <= 0 && !_isDead)
        {
            _isDead = true;
            Debug.Log("Turret destroy!");
            StartCoroutine(DeathTurret());
        }
    }

    private IEnumerator DeathTurret()
    {
        _deathTurretSource.Play();
        OnDeath?.Invoke();
        StartCoroutine(DeathTurretSounds());
        yield return new WaitForSeconds(0.5f);

        _animator.SetTrigger("Death");

        yield return new WaitForSeconds(1f);

        _deathFireFX.Play();
        Destroy(_deathBoomFX, 2f);
        yield return new WaitForSeconds(7f);
        var emission = _deathFireFX.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(1f);

        yield return new WaitForSeconds(15f);
        Destroy(_deathFireFX.gameObject);
        _deathFireSource.Stop();

    }

    private IEnumerator DeathTurretSounds()
    {
        _deathBoomFX.Play();
        yield return new WaitForSeconds(0.2f);
        _deathStartFireSource.Play();

        yield return new WaitForSeconds(1f);

        _deathFireSource.Play();
    }
}
