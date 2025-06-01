using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(AudioSource))]
public class MicIn : MonoBehaviour
{
    public readonly int SAMPLE_RATE = 16000; //サンプリング周波数
    [SerializeField] private readonly float dB_Min= -80.0f, dB_Max = -0.0f; //このdBでlevelMeter表示の下限に到達する, このdBでlevelMeter表示の上限に到達する
    public float aveAmp=0f, modified_dB=0f, rag = 0f; //現在のdB値
    public bool is_voice = false, is_IEnumerator = false;
    public Config config;
    private float[] samples;
    
    void Start()
    {
        #if UNITY_EDITOR
            config.AssetPath = Application.streamingAssetsPath;
        #elif UNITY_IOS
            config.AssetPath = Application.persistentDataPath;
        #elif UNITY_ANDROID
            config.AssetPath = Application.persistentDataPath;
        # endif
        config.microphoneSource = GetComponent<AudioSource>();
        config.MicDevice = null;
        config.microphoneSource.clip = Microphone.Start(config.MicDevice, true, config.MaxSec, SAMPLE_RATE);
        config.channel = config.microphoneSource.clip.channels;
        while (Microphone.GetPosition(config.MicDevice) <= 0) {}
        config.microphoneSource.Play(); //フレーム更新開始直後にマイクデバイスをスタートする
    }

    void Update()
    {
        rag = Time.deltaTime;
        if (!config.is_system_respose && !config.is_recording && config.is_connect) {
            config.silence_time += rag;
        }
        if (!config.microphoneSource.isPlaying || config.Is_playing) return;

        samples = new float[(int)(rag * SAMPLE_RATE)];
        config.microphoneSource.GetOutputData(samples, config.channel); //音データの取得
        if (!is_IEnumerator) UniTask.Create(async () => {await AudioToVol();});
    }

    void OnDestroy()
    {
        Microphone.End(Microphone.devices[0]);
    }
    void OnApplicationQuit()
    {
        Microphone.End(Microphone.devices[0]);
    }

    /// <summary>
    /// 取得した音声情報を音量に変換（-80db~0db）
    /// </summary>
    private async UniTask AudioToVol()
    {
        is_IEnumerator = true;
        if (samples.Length > 0) {
            aveAmp = samples.Average(s => Mathf.Abs(s)); //バッファ内の平均振幅を取得（絶対値を平均する）
            modified_dB = 20.0f * Mathf.Log10(aveAmp); //入力されたdBをdB_MaxとdBMin値で切り捨て
            if (modified_dB > dB_Max) { modified_dB = dB_Max; }
            else if (modified_dB < dB_Min) { modified_dB = dB_Min; }
            config.maxVolume = Mathf.Clamp(modified_dB, -800f, 0f);//値が超過/下回ると、自動で最大/最小に調整
            config.volume_list.Enqueue(config.maxVolume);
            if (config.volume_list.Count > 20) config.volume_list.Dequeue();
        }
        await UniTask.Yield();
        is_IEnumerator = false;
    }
}
