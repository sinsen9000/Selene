using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class WavPlay : MonoBehaviour
{
    public Config config;
    public Network network;
    public UnityWebRequest www;
    public AudioSource[] audio_source;

    // Update is called once per frame
    async void Update()
    {
        if (config.Is_playing || config.state != "voice") return; //状態が「voice」で、音声再生していない場合に処理開始
        config.response_call = "voice";
        config.Is_playing = true;
        if (config.url.Contains("thinking")) {
            /// 
            /// ＜音声フィラーの取り扱い＞
            /// 音声フィラーと本音声はペアとなっているため、フィラー〜本音声間のUpdate処理は一時停止する
            /// 思考動作かどうかの判別方法：URL名に「thinking」があるかどうか。フィラー音声フォルダは音声フォルダの下階層にあるため
            /// 
            if (!config.url.Contains("near")) { 
                string filename = await Play(config.url);
                while (config.url==filename) await UniTask.Yield(); //先にフィラー音声が再生終了した場合は次の音声URLが来るまで待機
            }

            config.response_call = "voice";
            if (config.url.Contains("near")) { //擬似接触誘導動作時の音声
                string filename = await Play(config.url);
                while (config.camera_to_model >= 0.5) { //先にフィラー音声が再生終了した場合は次の音声URLが来るまで待機
                    if (config.near_bool == false) {
                        config.Is_playing = false;
                        config.is_system_respose = false;
                        config.state = "";
                        config.time_format += "¥n";
                        #if UNITY_EDITOR
                            Debug.Log("Voice all talked!!");
                        # endif
                        return;
                    }
                    await UniTask.Yield();
                }
                config.url = $"http://{config.Server_ip}:4321/wav/{config.audioID}.wav";
            }
        }

        await GetAudioClip(config.url); //本音声の再生
        config.time_format += "¥n";
        #if UNITY_EDITOR
            Debug.Log("Voice all talked!!");
        # endif
        if (config.is_goodbye) await network.SocketClose(); //終了処理は音声再生が終わってから
    }

    private async UniTask WaitForAudioToStop(AudioSource audioSource, string playing_file)
    {
        while (audioSource.isPlaying) {
            if (config.is_recording || config.url != playing_file) {
                audio_source[0].Stop();
                break;
            }
            await UniTask.Yield(); //Wait for 100ms
        }
    }
    async UniTask<string> Play(string url)
    {
        string filename = url;
        Debug.Log(url);
        www = UnityWebRequestMultimedia.GetAudioClip(filename, AudioType.WAV);
        ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;
        www.timeout = 5;
        await www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError) {
            #if UNITY_EDITOR
                Debug.Log(www.error);
            # endif
            await UniTask.Delay(2000);
        }
        else{
            config.time_format += ","+Time.time.ToString();
            audio_source[0].clip = DownloadHandlerAudioClip.GetContent(www);
            audio_source[0].Play(); //発声処理
            await WaitForAudioToStop(audio_source[0], filename);
        }
        return filename;
    }
    async UniTask GetAudioClip(string filename)
    {
        await Play(filename);
        // 発声終了処理
        audio_source[0].clip = null;
        await UniTask.Delay(500);
        config.Is_playing = false;
        config.state = "";
        config.is_system_respose = false;
    }
}
