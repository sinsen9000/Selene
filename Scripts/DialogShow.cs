using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;

public class DialogShow : MonoBehaviour
{
    [SerializeField] private Animator _animator; //アニメーター
    [SerializeField] private int _layer; //アニメーターコントローラーのレイヤー(通常は0)
    private static readonly int ParamIsOpen = Animator.StringToHash("IsOpen"); //IsOpenフラグ(アニメーターコントローラー内で定義したフラグ)
    private bool IsOpen => gameObject.activeSelf; //ダイアログは開いているかどうか
    private bool IsTransition { get; set; }
    public Config config;
    public Network network;
    public TcpClient client;
    public Button connect_btn;
    public InputField IP1,IP2,IP3,IP4;
    public Text text, connect_btn_text;
    
    void Start()
    {
        connect_btn_text = connect_btn.GetComponentInChildren<Text>();
    }

    /// <summary>
    /// 「接続ボタン」の押下
    /// </summary>
    public async void onConnectBtn() 
    {
        IP1 = IP1.GetComponent<InputField> ();
        IP2 = IP2.GetComponent<InputField> ();
        IP3 = IP3.GetComponent<InputField> ();
        IP4 = IP4.GetComponent<InputField> ();
        text = text.GetComponent<Text> ();
        bool is_socket_completeed = await network.onSocket(IP1,IP2,IP3,IP4,text,connect_btn_text);
        if (is_socket_completeed) {
            DialogClose();
            connect_btn_text.text = "解除";
        }
    }
    /// <summary>
    /// ダイアログを開く
    /// </summary>
    public void DialogOpen()
    {
        if (IsOpen || IsTransition) return; //不正操作防止
        gameObject.SetActive(true); //パネル自体をアクティブにする
        config.is_finish=true;
        _animator.SetBool(ParamIsOpen, config.is_finish); //IsOpenフラグをセット
        WaitAnimation("Shown").Forget(); //アニメーション待機
    }
    /// <summary>
    /// ダイアログを閉じる
    /// </summary>
    public void DialogClose()
    {
        if (!IsOpen || IsTransition || config.is_getting) return; //不正操作防止
        config.is_finish=false;
        _animator.SetBool(ParamIsOpen, config.is_finish); //IsOpenフラグをクリア
        WaitAnimation("Hidden", () => gameObject.SetActive(false)).Forget(); //アニメーション待機し、終わったらパネル自体を非アクティブにする
    }

    /// <summary>
    /// 開閉アニメーションの待機処理
    /// </summary>
    /// <param name="stateName">監視対象アニメーション名</param>
    /// <param name="onCompleted">???</param>
    private async UniTask WaitAnimation(string stateName, UnityAction onCompleted = null)
    {
        IsTransition = true;
        await UniTask.WaitUntil(() => { //ステートが変化し、アニメーションが終了するまでループ
            var state = _animator.GetCurrentAnimatorStateInfo(_layer);
            return state.IsName(stateName) && state.normalizedTime >= 1;
        });
        onCompleted?.Invoke();
        IsTransition = false;
    }

    async void OnDestroy()
    {
        if (client != null && client.Connected) await network.SocketClose(); //アプリケーションが終了する前にソケットを閉じます
    }
    async void OnApplicationQuit()
    {
        if (client != null && client.Connected) await network.SocketClose(); //アプリケーションが終了する前にソケットを閉じます
    }
}
