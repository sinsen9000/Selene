using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraDrop : MonoBehaviour
{
    public Config config;
    private Dropdown ddtmp;
    private List<string> device_list = new List<string>();
    void Start()
    {
        ddtmp = GetComponent<Dropdown>(); //DropdownコンポーネントをGet
        foreach (var device in Microphone.devices) {
            //Debug.Log($"Device Name: {device}");
            device_list.Add(device);
        }
        ddtmp.AddOptions(device_list);
    }
    public void OnSelected()
    {
        Debug.Log("gude");
        config.MicDevice = ddtmp.options[ddtmp.value].text; //Dropdownコンポーネントから選択されている文字を取得
        config.microphoneSource.clip = Microphone.Start(config.MicDevice, true, config.MaxSec, 16000);
        config.channel = config.microphoneSource.clip.channels;
        while (Microphone.GetPosition(config.MicDevice) <= 0) {}
        config.microphoneSource.Play(); //フレーム更新開始直後にマイクデバイスをスタートする
    }
}
