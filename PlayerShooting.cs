using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Ak-47")] 
    [SerializeField] private GameObject _ak47;
    [SerializeField] private Transform _muzzle;

    [Header("Input")]
    [SerializeField] private KeyCode _reloadButton = KeyCode.R;

    [Header("Parameters")]
    [SerializeField] private int _baseDamageAk47 = 10;
    [Range(1,5)]
    [SerializeField] private int _chanceRicochet = 1;
    [SerializeField] private int _bulletsAll = 120;
    [SerializeField] private float _maxShootDistance = 100f;
    [SerializeField] private float _fireRate = 1f; // speed shoot
    [SerializeField] private float _timeReload = 1.5f;
    public LayerMask layerMask;

    [Header("FX objects")]
    [SerializeField] private GameObject _shootEffect;
    [SerializeField] private GameObject _shootMuzzleFX;

    [Header("Sounds")]
    [SerializeField] private AudioSource _shootAudioSource;
    [SerializeField] private AudioSource _shootMISSAudioSource;
    [SerializeField] private AudioSource _shootNOBulletsAudioSource;
    [SerializeField] private AudioSource _reloadAudioSource;

    [SerializeField] private AudioClip _shootClip;
    [SerializeField] private AudioClip _shootMissSand;
    [SerializeField] private AudioClip[] _shootMissMetal;

    [SerializeField] private Animator _animatorAk47;
    private int _bulletsInMagAk47 = 30;
    private int _startBullets = 30;
    private int _bulletsNow;
    private float _timer = 0f;
    private Camera _playerCamera;
    private Ray _ray;
    private RaycastHit _hit;
    private bool _isReloading = false;
    private int _bonusDamage;
    private int _randomNumberMissMetal;



    private void OnEnable()
    {
        Messenger.AddListener(GameEvent.PLAYER_STATS_UPDATED, UpdateStats);
    }

    private void OnDisable()
    {
        Messenger.RemoveListener(GameEvent.PLAYER_STATS_UPDATED, UpdateStats);
    }

    private void Start()
    {
        _playerCamera = GetComponentInChildren<Camera>();
        _bulletsNow = _startBullets;
    }

    private void Update()
    {
        if (Managers.Player.PlayerAlive && !Managers.Game.onCutScene)
        {
            _timer += Time.deltaTime;

            if (Input.GetMouseButton(0) && _timer > _fireRate && !_isReloading)
            {
                if (_bulletsNow >= 1)
                {
                    Shoot();
                }
                else
                {
                    _shootNOBulletsAudioSource.Play();
                }

                _timer = 0f;
            }

            if (Input.GetKeyDown(_reloadButton) && !_isReloading)
            {
                Debug.Log("Player reloading starting..");
                StartCoroutine(Reloading());
            }
        }
    }

    private void Shoot()
    {
        _bulletsNow -=1;
        _animatorAk47.SetTrigger("Shoot");
        ShootMuzzleFX();
        _shootAudioSource.PlayOneShot(_shootClip);

        _ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);

        if (Physics.Raycast(_ray, out _hit, _maxShootDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            ShootEffect();
            Debug.Log("Player attack:" + _hit.collider.gameObject.name);

            if (_hit.collider.CompareTag("Enemy"))
            {
                IDamageable damageable = _hit.collider.GetComponentInParent<Turret>();

                if (!ChanceRicochetYes()) 
                {
                    damageable?.TakeDamage(_baseDamageAk47 + _bonusDamage);
                }
                
            }
            else if (_hit.collider.CompareTag("Terrain"))
            {
               StartCoroutine(ShootDelaySoundTerrain(0.05f));
            }
            else if (_hit.collider.CompareTag("Metal"))
            {
                ChanceRicochetYes();
            }
        }
    }

    private bool ChanceRicochetYes() 
    {
        _randomNumberMissMetal = Random.Range(_chanceRicochet, 10);

        if (_randomNumberMissMetal == 9)
        {
            Managers.Audio.ShootSoundInArray(_shootMISSAudioSource, _shootMissMetal);
            Debug.Log("Ricochet!");
            return true;
        } 
        else return false;
    }

    private IEnumerator ShootDelaySoundTerrain(float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        _shootMISSAudioSource.PlayOneShot(_shootMissSand);
    }

    private void ShootEffect()
    {
        GameObject activeEffect = Instantiate(_shootEffect, _hit.point, Quaternion.LookRotation(_hit.normal));
        Destroy(activeEffect, 5f);
    }

    private void ShootMuzzleFX()
    {
        GameObject activeMuzzleFX = Instantiate(_shootMuzzleFX, _muzzle.transform.position, _muzzle.transform.rotation, _muzzle);
        Destroy(activeMuzzleFX, 2f);
    }


    private IEnumerator Reloading()
    {
        if(_bulletsAll <= 0)
        {
            yield return "No bulllets";
        }

        _isReloading = true;
        _animatorAk47.SetTrigger("Reloading");
        _reloadAudioSource.Play();
        Debug.Log("Reloading!");

        yield return new WaitForSeconds(_timeReload);
        Debug.Log("Reloaded.");

        if (_bulletsAll < _bulletsInMagAk47)
        {
            _bulletsNow = _bulletsAll;
            _bulletsAll -= _bulletsAll;
        }
        else
        {
            _bulletsAll -= _bulletsInMagAk47;
            _bulletsNow = _bulletsInMagAk47;
        }
        
        _bulletsNow = _startBullets;
        _isReloading = false;
        _reloadAudioSource.Stop();
    }

    private void UpdateStats()
    {
        SetDamageBonus(Managers.Player.DamageBonus);
    }

    public void SetDamageBonus(int value)
    {
        _bonusDamage = value;
        Debug.Log($"Bonus damage: {_bonusDamage}, all damage: {_baseDamageAk47 + _bonusDamage}.");
    }
}