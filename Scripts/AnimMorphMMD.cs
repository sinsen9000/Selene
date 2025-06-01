using UnityEngine;
using Cysharp.Threading.Tasks;

public class AnimMorphMMD : MonoBehaviour
{
    /*
    public Config config;
    public MMD4MecanimAnimMorphHelper mMD4MecanimAnimMorphHelper;
    // Update is called once per frame
    async void Update()
    {
        if (config.state != config.response_call) {
            if (config.state=="wait") {
                mMD4MecanimAnimMorphHelper.playingAnimName = "";
                config._anim.SetBool("wait", config.wait_loop); //待機動作の可否を変更
                if (!config.wait_loop) {
                    config.motionID = 0;
                    config._anim.SetInteger("motionID", config.motionID);
                }
                config.response_call = "wait";
            }
            else if (config.state=="sit") {
                config._anim.SetBool("sit", config.is_sitting); //着席動作の可否を変更
                int sit_int = 1;
                if (config.is_sitting) sit_int = 0; //着席時、下半身の通常動作はなし
                config._anim.SetLayerWeight(config._anim.GetLayerIndex("MotionBottom"),sit_int);
                config.response_call = "sit";
            }
        }
        if (!config.is_connect || config.state == "" || config.is_motion) return;

        if(config.motionID != 0) {
            config.is_motion = true;
            config.motion_num = config.motionID;
            if (config.motion_num < 35 && config.motion_num > 40) config._anim.SetBool("wait", false); //待機動作の可否を変更
            string now_mode = config.state;
            # if UNITY_EDITOR
                Debug.Log($"'{now_mode}' motion start.");
            # endif
            await Motion();
            # if UNITY_EDITOR
                Debug.Log($"'{now_mode}' motion end.");
            # endif
            config.is_motion = false;
        }
    }

    private async UniTask<string> Motion()
    {
        config._anim.SetInteger("motionID", config.motion_num); //通常の動作処理
        if (config.illegal_motions.Contains(config.motion_num)) {  //擬似接触を伴う動作処理
            if (config.camera_to_model >= 0.5) {
                while (config.camera_to_model >= 0.5) {
                    /// 
                    /// 擬似接触を伴う動作
                    /// ・手招き動作後、近くに寄るまで動作を待機
                    /// ・次ターン音声認識終了まで待機
                    /// ・システム思考中、ユーザ移動の場合は接触待機を解除
                    ///
                    if (config.move_lock || config.motionID == 0 || !config.is_connect || config.state=="wait") {
                        config.motionID = 0;
                        config._anim.SetInteger("motionID", config.motionID);
                        config.near_bool = false;
                        config.state = "";
                        return "walk.";
                    }
                    await UniTask.Yield();
                }
                config.near_bool = false;
            }
            config._anim.SetInteger("motionID", config.motion_num); //擬似接触を伴う動作処理
        }
        else {
            await WaitAnimState("wait_frame_vmd");
        }
        mMD4MecanimAnimMorphHelper.animName = config.motion_num.ToString();
        if (config.wait_loop) await UniTask.Delay(100);
        else {
            await UniTask.WaitWhile(() => !config._anim.GetCurrentAnimatorStateInfo(1).IsName("StandBy"));
            mMD4MecanimAnimMorphHelper.playingAnimName = "";
        }

        config._anim.SetInteger("motionID", 0);
        config.motionID = 0;
        config.motion_num = 0;
        config.state = "";
        return "done.";
    }
    private async UniTask WaitAnimState(string target_stete_name)
    {
        while (true) {
            if (config._anim.GetCurrentAnimatorStateInfo(1).IsName(target_stete_name)) break; //「A_stans_vmd」越えるまで待機
            await UniTask.Yield();
        }
    }
    */
}
