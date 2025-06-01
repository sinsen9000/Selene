using System.Collections;
using UnityEngine;
using UniVRM10;

public class EyeBlinkVRM : MonoBehaviour
{
    [SerializeField] private Vrm10Instance vrmInstance;
    private Vrm10RuntimeExpression vrmRuntimeExpression;
    private bool isPlus = true, DelayTime = false;
    private float BrinkWeight = 0f;

    // Start is called before the first frame update
    void Start()
    {
        vrmRuntimeExpression = vrmInstance.Runtime.Expression;
    }

    // Update is called once per frame
    void Update()
    {
        if(isPlus) BrinkWeight += Time.deltaTime * 7.5f;
        else       BrinkWeight -= Time.deltaTime * 7.5f;
        Brink();

        if (BrinkWeight < 0f){
            BrinkWeight = 0f;
            Brink();
            if (!DelayTime){
                DelayTime = true;
                StartCoroutine(DelayCoroutine());
            }
        }
        else if(BrinkWeight > 1f){
            BrinkWeight = 1f;
            Brink();
            isPlus = false;
        }
    }

    private IEnumerator DelayCoroutine() //コルーチン本体
    {
        float random_time = Random.Range(0.5f,5.0f);
        yield return new WaitForSeconds(random_time);
        isPlus = true;
        DelayTime = false;
    }
    /// <summary>
    /// 瞬き処理のメイン（VRM）
    /// </summary>
    private void Brink(){
        vrmRuntimeExpression.SetWeight(ExpressionKey.Blink, BrinkWeight);
    }
}
