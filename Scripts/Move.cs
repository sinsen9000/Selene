using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Move : MonoBehaviour
{
    public class Complex {
        public float x;
        public float y;
        public Complex(float x, float y) {
            this.x = x;
            this.y = y;
        }
        public static Complex operator *(Complex a, Complex b) {
            return new Complex(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
        }
    }

    public Camera mainCamera;
    //public GameObject Capsule1, Capsule2, Capsule3;
    public GameObject model_target;
    public CameraIn camera_in;
    public DropDown DropObj;
    public Config config;
    private Vector2 direction;
    /// <summary>
    /// ARカメラの位置
    /// </summary>
    private Vector3 camera_rig;
    private List<Vector3> pre_posi=new List<Vector3>();
    private float distance, distance_value, error_x, error_z, temp_x, temp_z;
    private float _arctan, angle, trans_x, trans_z, decision_arctan, threshold=0.8f, move_r=0f, pre_move_r=0f, _error=0.05f, drop_value = 0f;
    private int num=0, _myVariable, adjust_hash, distance_hash, camera_to_model_hash, walk_tf_hash;
    /// <summary>
    /// 移動状態のフィードバック。0 = 停止中, 1 = 歩行中
    /// </summary>
    private int adjust {
        get { return _myVariable; }
        set
        {
            if (_myVariable != value)
            {
                _myVariable = value;
                OnMyVariableChanged();
            }
        }
    }
    public bool objectIsInCameraView=false;
    public float v_arctan;

    /// <summary>
    /// モデルの指定
    /// </summary>
    /// <param name="model_name"></param>
    private void ModelChange(string model_name = "")
    {
        GameObject camera_target;
        if (transform.Find($"{model_name}/U_Char/U_Char_0") != null) {
            camera_target = transform.Find($"{model_name}/U_Char/U_Char_0").gameObject; //mmd用
        }
        else if (transform.Find($"{model_name}/Face") != null) {
            camera_target = transform.Find($"{model_name}/Face").gameObject; //vrm用
        }
        else if (transform.Find($"{model_name}/mesh_root/skin") != null){
            camera_target = transform.Find($"{model_name}/mesh_root/skin").gameObject; //unitychan用
        }
        else {
            return;
        } 
        model_target = transform.Find(model_name).gameObject;
        config._anim = model_target.GetComponent<Animator>();
        camera_in = camera_target.GetComponent<CameraIn>();
    }
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        ModelChange("Sample");
        config._anim.SetLayerWeight(config._anim.GetLayerIndex("MotionBottom"),1);
        
        adjust_hash = Animator.StringToHash("adjust");
        distance_hash = Animator.StringToHash("distance");
        camera_to_model_hash = Animator.StringToHash("camera_to_model");
        walk_tf_hash = Animator.StringToHash("walk_tf");
        config._anim.SetBool("listen", true);
        //Capsule1 = GameObject.Find("Capsule1");
        //Capsule2 = GameObject.Find("Capsule2");
        //Capsule3 = GameObject.Find("Capsule3");

        // カメラの位置を取得し、関連する値に情報を入力
        config.to_user=1f;
        camera_rig = mainCamera.transform.position;
        Vector3 camera_r = mainCamera.transform.eulerAngles; //ARカメラの角度
        config.user = new Vector3(camera_rig.x-config.to_user*Mathf.Sin(camera_r.x*Mathf.Deg2Rad),camera_rig.y,camera_rig.z-config.to_user*Mathf.Cos(camera_r.z*Mathf.Deg2Rad));
        config.camera_stop = config.user; //最後に停止した位置
        pre_posi.Add(config.user); //ずらした後のカメラ位置の履歴
        DropObj.DropDownValue = "前方";

        v_arctan = 0f*Mathf.Deg2Rad;
        config.camera_p = new Vector3(config.user.x+drop_value*Mathf.Sin(v_arctan),config.user.y,config.user.z+drop_value*Mathf.Cos(v_arctan));
        direction = new Vector2(drop_value*Mathf.Sin(v_arctan),drop_value*Mathf.Cos(v_arctan));
        config.camera_to_model = Mathf.Sqrt((transform.position.x-camera_rig.x)*(transform.position.x-camera_rig.x)
                            +(transform.position.z-camera_rig.z)*(transform.position.z-camera_rig.z));
        angle = transform.eulerAngles.y;
        distance_value = -0.5f*0.05f;
        StartCoroutine(DegreeMove());
        StartCoroutine(Direction());
    }

    // Update is called once per frame
    void Update()
    {
        config.model = transform.position;
        camera_rig = mainCamera.transform.position;
        if ((camera_rig.x-config.user.x)*(camera_rig.x-config.user.x) + (camera_rig.z-config.user.z)*(camera_rig.z-config.user.z) > config.to_user*config.to_user){
            Vector3 user_posi = new Vector3(camera_rig.x+config.to_user*Mathf.Sin(decision_arctan*Mathf.Deg2Rad),
                                            camera_rig.y,
                                            camera_rig.z+config.to_user*Mathf.Cos(decision_arctan*Mathf.Deg2Rad));
            config.user += new Vector3(user_posi.x-config.user.x, 0, user_posi.z-config.user.z) * Time.deltaTime;
        }
        //Capsule3.transform.position = new Vector3(user.x, 0, user.z);
        
        objectIsInCameraView = camera_in.isVisible;
        var temp = DistanceAjust(config.model, config.camera_p);
        distance = temp.dis;
        error_x = temp.th_x;
        error_z = temp.th_z;
        
        if (!config.move_lock){
            if (0.8f+_error < DistanceDerive(config.model, config.camera_p) && DropObj.DropDownValue != "停止"){
                config._anim.SetBool(walk_tf_hash, true);
                config._anim.SetLayerWeight(config._anim.GetLayerIndex("MotionBottom"),0);
                config.move_lock = true; //移動処理への移行
                num = 0;
                adjust = 1;
            }
            else if (objectIsInCameraView){
                _arctan = Mathf.Atan2(config.model.z-camera_rig.z, config.model.x-camera_rig.x);
                angle = -(90 + _arctan*Mathf.Rad2Deg); //カメラ（＝ユーザ）の方に向く
            }
        }
    }
    /// <summary>
    /// 移動処理（目標点への最短移動。FixedUpdate処理）
    /// </summary>
    void FixedUpdate()
    {
        if(config.move_lock) {
            _arctan = Mathf.Atan2(config.model.z-(config.camera_p.z+error_z),config.model.x-(config.camera_p.x+error_x));
            angle = -(90 + _arctan*Mathf.Rad2Deg);
            trans_x = distance_value*num*distance * Mathf.Cos(_arctan);
            trans_z = distance_value*num*distance * Mathf.Sin(_arctan);
            Vector3 _new = new Vector3(trans_x, 0, trans_z) * Time.deltaTime;
            transform.localPosition += _new;
            num+=1;
            if (distance<=0.01f && adjust==1){
                config.camera_stop = config.user;
                config.move_lock=false;
                adjust = 0;
                config._anim.SetBool(walk_tf_hash, false);
                config._anim.SetLayerWeight(config._anim.GetLayerIndex("MotionBottom"),1);
            }
        }
        model_target.transform.position = transform.position;
        if (config.is_connect) File.AppendAllText(config.move_filename, $"{camera_rig.x},{camera_rig.z},{model_target.transform.position.x},{model_target.transform.position.z}\n");
        config._anim.SetFloat(distance_hash, distance + threshold - _error);
        config._anim.SetFloat(camera_to_model_hash, config.camera_to_model);
        if (pre_posi.Count == 20) pre_posi.RemoveAt(0);
        pre_posi.Add(config.camera_p);
    }

    /// <summary>
    /// モデルの振り向きを自然にする
    /// </summary>
    private IEnumerator DegreeMove() {
        while(true){
            var target_angle = transform.eulerAngles;
            transform.rotation = Quaternion.Slerp(Quaternion.Euler(target_angle.x,target_angle.y,target_angle.z), Quaternion.Euler(0f,angle,0f), 0.1f);
            model_target.transform.rotation = transform.rotation;
            yield return null;
        }
    }
    /// <summary>
    /// camera_pの導出
    /// </summary>
    private IEnumerator Direction() {
        Vector3 pre_posi = config.user;
        decision_arctan = v_arctan;
        float CameraToModel_x, CameraToModel_z;
        while(true){
            Vector3 now_posi = config.user; //この時点でのユーザ位置を取得
            var delta = now_posi-pre_posi;
            if (delta != Vector3.zero) {
                var rocation = Quaternion.LookRotation(delta,Vector3.up);
                v_arctan = rocation.eulerAngles.y;
                decision_arctan = v_arctan;
            }
            //Debug.Log(decision_arctan);
            if (DropObj.DropDownValue == "前方") drop_value = config.to_user + 0.3f;//0.5f;
            else drop_value = 0f;
            direction = new Vector2(drop_value*Mathf.Sin(decision_arctan * Mathf.Deg2Rad),drop_value*Mathf.Cos(decision_arctan * Mathf.Deg2Rad));
            config.camera_p = new Vector3(now_posi.x+direction.x,now_posi.y,now_posi.z+direction.y);
            CameraToModel_x = config.model.x-mainCamera.transform.position.x;
            CameraToModel_z = config.model.z-mainCamera.transform.position.z;
            config.camera_to_model = Mathf.Sqrt(CameraToModel_x*CameraToModel_x+CameraToModel_z*CameraToModel_z);
            yield return null;
            pre_posi = now_posi;
        }
    }
    private float DistanceDerive(Vector3 target, Vector3 _base){
        temp_x = target.x - _base.x;
        temp_z = target.z - _base.z;
        return Mathf.Sqrt(temp_x*temp_x+temp_z*temp_z);
    }
    /// <summary>
    /// 進む目標点を決定。目標点はユーザ進行方向左右２箇所にあり、モデルは最短距離を選択する
    /// </summary>
    /// <param name="target_p"></param>
    /// <param name="base_p"></param>
    /// <returns></returns>
    private (float dis, float th_x, float th_z) DistanceAjust(Vector3 target_p, Vector3 base_p) {
        temp_x = target_p.x - base_p.x;
        temp_z = target_p.z - base_p.z;
        pre_move_r = Mathf.Atan2(base_p.x-pre_posi[0].x,-(base_p.z-pre_posi[0].z)) * Mathf.Rad2Deg;
        if (pre_move_r != 180) move_r = (int)pre_move_r;
        var comp_p = ComplexDegree(move_r,  1, 1, 1);
        var comp_m = ComplexDegree(move_r, -1, 1, 1);
        Vector3 plus  = new Vector3(Mathf.Sqrt((temp_x-comp_p._x)*(temp_x-comp_p._x)+(temp_z-comp_p._z)*(temp_z-comp_p._z)), comp_p._x, comp_p._z);
        Vector3 minus = new Vector3(Mathf.Sqrt((temp_x-comp_m._x)*(temp_x-comp_m._x)+(temp_z-comp_m._z)*(temp_z-comp_m._z)), comp_m._x, comp_m._z);
        //Capsule1.transform.position = new Vector3(camera_p.x+plus[1], mainCamera.transform.position.y, camera_p.z+plus[2]); //new Vector3(transform.position.x,transform.position.y,transform.position.z);
        //Capsule2.transform.position = new Vector3(camera_p.x+minus[1], mainCamera.transform.position.y, camera_p.z+minus[2]);
        if(plus[0] < minus[0]) return (plus[0] , plus[1] , plus[2]); //カメラの左右のうち近い方にオブジェクトを移動する
        else return (minus[0], minus[1], minus[2]);
    }
    /// <summary>
    /// 候補点を決めるための複素数計算。カメラ進行方向を正面とし、左右90度の座標を導出
    /// </summary>
    /// <param name="rocate"></param>
    /// <param name="value"></param>
    /// <param name="direc_x"></param>
    /// <param name="direc_z"></param>
    /// <returns></returns>
    private (float _x, float _z) ComplexDegree (float rocate, float value, float direc_x=1f, float direc_z=1f){
        var comp = new Complex(threshold * Mathf.Cos((rocate+90*direc_x)*Mathf.Deg2Rad), threshold * Mathf.Sin((rocate+90*direc_z)*Mathf.Deg2Rad));
        var _comp = comp * new Complex(0, value);
        var rad = Mathf.Atan2(_comp.y, _comp.x);
        return (threshold * Mathf.Cos(rad), threshold * Mathf.Sin(rad));
    }
    void OnMyVariableChanged()
    {
        config._anim.SetInteger(adjust_hash, (int)adjust);
    }
}
