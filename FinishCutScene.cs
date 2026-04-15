using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishCutScene : MonoBehaviour
{
    [SerializeField] private GameObject _playerFinish;
    [SerializeField] private GameObject _boss;
    [SerializeField] private float _timeCutScene = 15f;
    [SerializeField] private float _timeToStartSalute = 5f;
    [SerializeField] private GameObject[] _deathBossFX;
    [SerializeField] private GameObject _wallsBoss;
    [SerializeField] private GameObject _finishTitle;

    [Header("AudioSources")]
    [SerializeField] private AudioSource _deathBoss1;
    [SerializeField] private AudioSource _deathBoss2;
    [SerializeField] private AudioSource _finishMusic;

    private bool _startFinishCutScene;
    private GameObject _player;

    private void Start()
    {
        _playerFinish.SetActive(false);
        _finishTitle.SetActive(false);

        foreach (GameObject FX in _deathBossFX)
        {
            FX.SetActive(false);
        }
    }

    private void Update()
    {
        if (Managers.Game.playerWin && !_startFinishCutScene)
        {
            _startFinishCutScene = true;
            StartCoroutine(StartFinishCutScene());
        }
    }

    private IEnumerator StartFinishCutScene()
    {
        Managers.Game.onCutScene = true;
        _deathBoss1.Play();

        yield return new WaitForSeconds(2f);

        _deathBoss2.Play();

        yield return new WaitForSeconds(1f);

        _deathBoss2.Play();

        yield return new WaitForSeconds(2f);

        _player = GameObject.FindGameObjectWithTag("Player");
        _player.SetActive(false);
        Destroy(_wallsBoss);
        _playerFinish.SetActive(true);

        _boss.GetComponent<Boss>().enabled = false;

        _finishMusic.Play();

        yield return new WaitForSeconds(_timeToStartSalute);

        _playerFinish.GetComponent<Animator>().SetTrigger("Salute");

        _finishTitle.SetActive(true);

        foreach (GameObject FX in _deathBossFX)
        {
            FX.SetActive(true);
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(_timeCutScene);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Managers.Game.onCutScene = false;

        SceneManager.LoadScene("MainMenu");
    }
}
