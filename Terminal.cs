using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terminal : MonoBehaviour
{
    private AudioSource _audioSource;
    private BoxCollider _collider;

    public static int activatedStage1 = 0;
    public static int needActivatedStage1 = 3;

    [SerializeField] private bool _isFinishTerminal = false;

    public static int needActivatedStage2 = 1;
    public static int activatedStage2 = 0;


    public static bool terminalFirstStageDone = false;
    public static bool terminalSecondStageDone = false;

    public event Action OnTerminalActivated;
    public static event Action OnAnyTerminalActivated;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _collider = GetComponent<BoxCollider>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKey(KeyCode.E))
        {
            if (_isFinishTerminal)
            {
                Debug.Log("Terminal Second Stage IS DONE");
                activatedStage2++;
                terminalSecondStageDone = true;
            }
            else
            {
                _audioSource.Play();
                activatedStage1++;
                Debug.Log($"Сейчас: {activatedStage1} / {needActivatedStage1}");

                if (activatedStage1 >= needActivatedStage1)
                {
                    terminalFirstStageDone = true;
                    Debug.Log("Terminal first stage IS DONE");
                }
            }

            OnTerminalActivated?.Invoke();
            OnAnyTerminalActivated?.Invoke();

            _collider.enabled = false;
        }
    }




}
