using UnityEngine;

public class SE : MonoBehaviour
{
    private int pre_num, se_num;
    private AudioSource[] audio_source;
    // Start is called before the first frame update
    void Start()
    {
        audio_source = gameObject.GetComponents<AudioSource>();
        pre_num = 0;
    }

    /// <summary>
    /// 歩行音の付与。対象のアニメーションのeventsに上記関数名を登録することで使用可能。
    /// </summary>
    public void WalkSE(){
        
        if(se_num > 0) pre_num = se_num; //直前の歩行音Noを保存
        se_num += 1; //歩行音Noを1つ進める
        if (pre_num == 4) se_num = 1; //直前の歩行音Noが4番の時は1番に変更
        audio_source[0].PlayOneShot((AudioClip)Resources.Load("SE/Walk/walk"+se_num.ToString()));
    }

    public void VmdSE(string se_name){
        audio_source[0].PlayOneShot((AudioClip)Resources.Load("motion/"+se_name)); //音声ファイル更新
    }
}