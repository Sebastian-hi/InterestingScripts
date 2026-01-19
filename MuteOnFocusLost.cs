using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuteOnFocusLost : MonoBehaviour
{
    private void OnApplicationFocus(bool hasFocus)
    {
        AudioListener.pause = !hasFocus;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        AudioListener.pause = pauseStatus;
    }
}
