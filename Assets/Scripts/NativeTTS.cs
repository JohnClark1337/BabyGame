using UnityEngine;
using System;

public class NativeTTS : MonoBehaviour
{
    private static NativeTTS _instance;
    private AndroidJavaObject _tts;
    private bool _isInitialized;
    private bool _isAndroid;
    private string _queuedText;

    public static bool IsInitialized => _instance != null && _instance._isInitialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        if (_instance != null) return;
        var go = new GameObject("NativeTTS");
        DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        _instance = go.AddComponent<NativeTTS>();
    }

    void Awake()
    {
        _isAndroid = Application.platform == RuntimePlatform.Android;
        if (_isAndroid)
            Initialize();
    }

    void Initialize()
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var listener = new TTSOnInitListener(status =>
                {
                    using (var ttsClass = new AndroidJavaClass("android.speech.tts.TextToSpeech"))
                    {
                        var success = ttsClass.GetStatic<int>("SUCCESS");
                        _isInitialized = status == success;
                    }

                    if (_isInitialized)
                    {
                        using (var localeClass = new AndroidJavaClass("java.util.Locale"))
                        using (var us = localeClass.GetStatic<AndroidJavaObject>("US"))
                            _tts.Call("setLanguage", us);

                        if (!string.IsNullOrEmpty(_queuedText))
                        {
                            SpeakInternal(_queuedText);
                            _queuedText = null;
                        }
                    }
                });
                _tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, listener);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"NativeTTS init failed: {e.Message}");
        }
    }

    public static void Speak(string text)
    {
        if (_instance == null) return;
        _instance.SpeakInternal(text);
    }

    public static void Stop()
    {
        if (_instance != null && _instance._tts != null && _instance._isInitialized)
            _instance._tts.Call("stop");
    }

    void SpeakInternal(string text)
    {
        if (!_isAndroid || _tts == null || string.IsNullOrEmpty(text)) return;

        if (!_isInitialized)
        {
            _queuedText = text;
            return;
        }

        try
        {
            using (var ttsClass = new AndroidJavaClass("android.speech.tts.TextToSpeech"))
            {
                var flush = ttsClass.GetStatic<int>("QUEUE_FLUSH");
                _tts.Call("speak", text, flush, null, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"NativeTTS speak failed: {e.Message}");
        }
    }

    void OnDestroy()
    {
        if (_tts == null) return;
        try
        {
            _tts.Call("stop");
            _tts.Call("shutdown");
        }
        catch { }
        _tts = null;
        _isInitialized = false;
    }

    private class TTSOnInitListener : AndroidJavaProxy
    {
        private readonly Action<int> _onInit;

        public TTSOnInitListener(Action<int> onInit)
            : base("android.speech.tts.TextToSpeech$OnInitListener")
        {
            _onInit = onInit;
        }

        void onInit(int status)
        {
            _onInit?.Invoke(status);
        }
    }
}
