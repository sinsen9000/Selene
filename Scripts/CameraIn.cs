// カメラ判定用スクリプト
using UnityEngine;

public class CameraIn : MonoBehaviour
{
    public bool isVisible = false;

    void OnBecameVisible() //可視状態になった時に呼ばれる
    {
        isVisible = true;
    }
    void OnBecameInvisible() //非可視状態になった時に呼ばれる
    {
        isVisible = false;
    }
}
