using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class LookUp : MonoBehaviour
{
    public Config config;
    public Move move;
    public float HeadWeight = 0.3f, EyeWeight = 0.2f;
    private bool _is_look = false;
    private float weigth = 0.5f;
    private List<int> exclusion_motion_list = new List<int>{39,40,84,85,86,90,114,131};
    private bool look{
        get { return _is_look; }
        set {
            if (_is_look != value) {
                _is_look = value;
                OnMotionChanged();
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        look = config.is_motion;
    }

    // Update is called once per frame
    void Update()
    {
        look = config.is_look;
    }

    /// <summary>
    /// 通常時のLookUp処理
    /// ユーザが話していない場合は発動せず、ユーザ会話中やモーションがなかった場合のシステム発話での発動を想定
    /// 頭=0.5、目=0.2の重みでモデルがユーザの方に向くが、移動中は頭=0.8fで推移
    /// </summary>
    private void SetLook()
    {
        if (config.move_lock) HeadWeight = 0.5f;
        config._anim.SetLookAtWeight(1f, 0f, HeadWeight, EyeWeight, 0.5f); //ターゲットへの向きの重みを設定
        config._anim.SetLookAtPosition(move.mainCamera.transform.position); //ターゲットの位置を設定
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (move.objectIsInCameraView) {
            //Debug.Log(config.motion_num);
            if (config.wait_loop) {return;}
            else if (exclusion_motion_list.Contains(config.motion_num)) {
                config._anim.SetLookAtWeight(1f, 0f, 0f, 1f, 1f); //思考動作・擬似接触を伴う動作は、LookAt処理全てを停止
                config._anim.SetLookAtPosition(move.mainCamera.transform.position);
            }
            else SetLook();
        }
    }

    /// <summary>
    /// LookUp処理使用時にスムーズに移行するための処理
    /// 使用する際は重みを0.1ずつ上昇させ1.0に、解除の際は0.1ずつ減少させ0.0にする
    /// 発声処理でモデルが準備モーション以外の時（ID＝0以外）の場合は省略
    /// </summary>
    async void OnMotionChanged()
    {
        if (config.Is_playing && config.motionID != 0) return;
        if (_is_look) {
            for (int i = 1; i <= 10; i++) {
                await UniTask.Delay((int)(Time.deltaTime*1000));
                weigth = 0.1f * i;
            }
            if (weigth > 1f) weigth = 1f;
        }
        else {
            for (int i = 1; i <= 10; i++) {
                await UniTask.Delay((int)(Time.deltaTime*1000));
                weigth = 1f - 0.1f * i;
            }
            if (weigth < 0f) weigth = 0f;
        }
    }
}
