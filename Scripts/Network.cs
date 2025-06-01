using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// インターネット接続系統スクリプト
/// </summary>
public class Network : MonoBehaviour
{
    
    public Config config;
    public TcpClient client;
    public NetworkStream stream;
    private int port = 12345;
    private int pre_mID = 0;
    /// <summary>
    /// ローカル変数用connect_disp, connect_btn_text。onSocket利用時に更新
    /// </summary>
    private Text connect_disp_local, connect_btn_text_local;
    private bool IsIPAddressValid(string ipAddress)
    {
        if (IPAddress.TryParse(ipAddress, out IPAddress result)) return true; //パースに成功した場合は有効なIPアドレスです
        else return false; //パースに失敗した場合は無効なIPアドレスです
    }

    public async UniTask<bool> onSocket(InputField IP1, InputField IP2, InputField IP3, InputField IP4, Text connect_disp, Text connect_btn_text)
    {
        connect_btn_text_local = connect_btn_text;
        connect_disp_local = connect_disp;
        if (!config.is_connect) {
            config.serverIP = "";
            connect_disp.text = "接続中......";

            if (IP1.text==""||IP1.text==""||IP3.text==""||IP4.text=="") {
                connect_disp.text = "空欄の項目があります";
                return false;
            }
            config.serverIP = string.Format("{0}.{1}.{2}.{3}",IP1.text,IP2.text,IP3.text,IP4.text);
            if (IsIPAddressValid(config.serverIP) == false) {
                connect_disp.text = "無効なIPです（半角数字を入力してください）";
                return false;
            }
            
            try {
                client = new TcpClient();
                var task = client.ConnectAsync(config.serverIP, port);
                if (!task.Wait(2000)){
                    client.Close();
                    throw new SocketException(10060);
                }
            }
            catch (SocketException) {
                connect_disp.text = "接続失敗: " + config.serverIP;
                return false;
            }
            stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;
            while (client.Connected) {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0) {
                    string[] recv_data = Encoding.UTF8.GetString(buffer, 0, bytesRead).Split("//");
                    config.Server_ip = config.serverIP;
                    connect_disp.text = string.Format("接続成功: {0}（{1}）", config.serverIP);
                    #if UNITY_EDITOR
                        Debug.Log(config.Server_ip);
                    # endif
                    break;
                }
            }
            config.time_format += "0";
            config.move_filename = Path.Combine(config.AssetPath,$"position{DateTime.Now:yyyy_MMdd_HHmmss}.txt");
            config.is_connect = true;
            SocketSend("done: IP Connect");
            SocketRecv();
            return true;
        }
        else if (config.is_connect) {
            await SocketClose();
            await UniTask.Yield();
            return false;
        }
        return false;
    }
    /// <summary>
    /// clientシステムからの情報を受信
    /// </summary>
    /// <param name="stete">取得情報の種類（wait=思考待機、voice=音声）</param>
    /// <returns></returns>
    private async void SocketRecv()
    {
        async UniTask state_set(List<string> recv_list, int pre_motion_id = 0)
        {
            if (pre_motion_id == 0) {
                if (int.TryParse(recv_list[1], out int number)) config.motionID = number; //動作IDの取得
                else config.motionID = 0; //入力が不正の場合は強制的に0
            }
            else config.motionID = pre_motion_id;
            config.state = recv_list[0];
            while (config.response_call != config.state) {await UniTask.Delay(1);} //Wait for 100ms
        }

        try {
            while (true){
                stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;
                while (client.Connected) {
                    bytesRead =  await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0) {
                        config.recv_data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        break;
                    }
                }
                //#if UNITY_EDITOR
                Debug.Log($"recv -> {config.recv_data}");
                //#endif
                List<string> recv_list = config.recv_data.Split("//").ToList();

                if (recv_list[0] != "silence"){
                    if (recv_list[0] == "wait") {
                        int number2 = int.Parse(recv_list[2]); //循環有無(1/0 = true/false)
                        if (number2 == 1) config.wait_loop = true;
                        else config.wait_loop = false; //入力が不正の場合は強制的に0(false)
                        await state_set(recv_list);
                    }
                    else if (recv_list[0] == "voice") {
                        config.audioID = recv_list[3]; //音声ファイル名は文字列のため、そのまま使用
                        if (int.TryParse(recv_list[1], out int number)) pre_mID = number; //動作IDの取得
                        else pre_mID = 0; //入力が不正の場合は強制的に0
                        if (config.illegal_motions.Contains(pre_mID) && config.camera_to_model >= 0.5) {
                            config.near_bool = true;
                            config.url = $"http://{config.Server_ip}:4321/wav/thinking/near.wav"; //擬似接触
                        }
                        else {
                            config.near_bool = false;
                            config.url = $"http://{config.Server_ip}:4321/wav/{config.audioID}.wav";
                        }
                        config.state = recv_list[0];
                        await state_set(recv_list);
                    }
                    else if (recv_list[0] == "sit") {
                        int number2 = int.Parse(recv_list[2]);
                        if (number2 == 1) config.is_sitting = true; //循環有無(1/0 = true/false)
                        else config.is_sitting = false; //入力が不正の場合は強制的に0(false)
                        await state_set(recv_list);
                    }
                    else if (recv_list[0] == "end") {
                        await SocketClose();
                        break;
                    }
                    else {
                        if (int.TryParse(recv_list[1], out int number)) config.motionID = number; //動作IDの取得
                        else config.motionID = 0; //入力が不正の場合は強制的に0
                        config.state = recv_list[0];
                    }
                }
                else{
                    config.silence_tho = float.Parse(recv_list[3]); //沈黙時間閾値は文字列のため、float変換
                    #if UNITY_EDITOR
                        Debug.Log($"Silence time: {config.silence_tho}");
                    #endif
                    config.state = recv_list[0];
                }
                SocketSend($"done: {config.recv_data}");
                config.response_call = "";
            }
        }
        catch (Exception ex) {
            //#if UNITY_EDITOR
            Debug.Log("Exception: " + ex.Message);
            //#endif
            await SocketClose();
        }
    }
    /// <summary>
    /// clientシステムへ情報を受信
    /// </summary>
    /// <param name="send_data"></param>
    public async void SocketSend(string send_data){
        try {
            //#if UNITY_EDITOR
            Debug.Log($"send -> {send_data}");
            //#endif
            byte[] data = Encoding.UTF8.GetBytes(send_data);
            stream.Write(data, 0, data.Length);
        }
        catch (Exception ex) {
            //#if UNITY_EDITOR
            Debug.Log("Exception: " + ex.Message);
            //#endif
            await SocketClose();
        }
    }
    public async UniTask SocketClose(){
        config.silence_tho = 0f;
        config.silence_time = 0f;
        client.Close();
        await config.Reset_param();
        connect_disp_local.text ="未接続";
        connect_btn_text_local.text = "接続";
    }
}
