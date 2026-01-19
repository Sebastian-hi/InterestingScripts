using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerManager))]
[RequireComponent(typeof(AudioManager))]
[RequireComponent(typeof(BattleManager))]
[RequireComponent(typeof(MissionManager))]

public class Managers : MonoBehaviour
{
    public static PlayerManager Player { get; private set; }

    public static AudioManager Audio { get; private set; }
    public static BattleManager Battle { get; private set; }
    public static MissionManager Mission { get; private set; }

    private List<IGameManager> startSequence; // only ��� ��������� ���������


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        //Data = GetComponent<DataManager>(); ������!

        Player = GetComponent<PlayerManager>();
        Audio = GetComponent<AudioManager>();
        Battle = GetComponent<BattleManager>();
        Mission = GetComponent<MissionManager>();

        startSequence = new List<IGameManager>
        {
            Player,
            Audio,
            Battle,
            Mission,
        };

        StartCoroutine(StartManagers());
    }

    private IEnumerator StartManagers()
    {
        foreach (IGameManager manager in startSequence)
        {
            manager.Startup();
        }

        yield return null;

        int numModules = startSequence.Count;
        int numReady = 0;

        while (numReady < numModules)
        {
            int lastReady = numReady;
            numReady = 0;

            foreach (IGameManager manager in startSequence)
            {
                if (manager.status == ManagerStatus.Started)
                {
                    numReady++;
                }
            }

            if (numReady > lastReady)
            {
                Debug.Log($"Progress: {numReady}/{numModules}.");
            }
            yield return null;
        }

        Debug.Log("All managers started up");
    }
}