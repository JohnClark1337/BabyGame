using UnityEngine;

public class ScreenTimeout : MonoBehaviour
{
    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
