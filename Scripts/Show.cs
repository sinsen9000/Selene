using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

class Show : MonoBehaviour
{
    public DialogShow GuiObj;
    public Config config;
    public Move MovObj;
    public Text post, roca, state, dis_cap;
    private Vector3 camera_posi, camera_deg;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;
        config.is_finish = true;
        post = post.GetComponent<Text> ();
        roca = roca.GetComponent<Text> ();
        state = state.GetComponent<Text> ();
        dis_cap = dis_cap.GetComponent<Text> ();
    }
    // Update is called once per frame
    void Update()
    {
        if (config.is_finish) {
            camera_posi = MovObj.mainCamera.transform.position;
            camera_deg = MovObj.mainCamera.transform.eulerAngles;
            post.text = "位置："+camera_posi.ToString();
            roca.text = "角度："+camera_deg.y.ToString();
        }
        if (Input.GetMouseButtonDown(0) && !config.is_finish){
            GuiObj.DialogOpen();
            config.is_finish=true;
        }
    }
    private async void onCalibration()
    {
        if (config.is_getting) return;
        config.is_getting = true;
        await GetDistance(state, dis_cap);
    }

    float CalculateMedian(Queue<float> queue)
    {
        
        List<float> list = queue.ToList();
        list.Sort();
        int count = list.Count;
        int middle = count / 2;

        if (count % 2 == 1) {
            return list[middle]; //データ数が奇数の場合
        }
        else {
            return (list[middle - 1] + list[middle]) / 2; //データ数が偶数の場合
        }
    }

    private async UniTask GetDistance(Text caption, Text value)
    {
        caption.text = "右方向に回ってください......";
        List<Vector3> orbit = new List<Vector3>{camera_posi};
        List<float> distance = new List<float>();
        
        float startTime = Time.time;
        while (Time.time - startTime < 3f) {
            Vector3 get = camera_posi;
            orbit.Add(get);
            await UniTask.Yield(); // 1フレーム待つ
        }
        foreach (Vector3 i in orbit){
            distance.Add(Mathf.Sqrt(Mathf.Pow(i.x-config.user.x,2)+Mathf.Pow(i.z-config.user.z,2)));
        }
        config.to_user = distance.Average();
        value.text = config.to_user.ToString();
        config.is_getting=false;
        if (config.to_user.ToString() != "NaN") state.text = "完了";
        else{
            value.text = "0";
            state.text = "失敗";
        }
        config.threshold = CalculateMedian(config.volume_list) + 20;
    }
}
