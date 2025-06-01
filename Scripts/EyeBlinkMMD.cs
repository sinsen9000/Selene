using System.Collections;
using UnityEngine;
public class EyeBlinkMMD : MonoBehaviour
{
    /*
    private MMD4MecanimMorphHelper morphScript;
    private bool isPlus = true, DelayTime = false;

    // Start is called before the first frame update
    void Start()
    {
        morphScript = GetComponent<MMD4MecanimMorphHelper>();
        morphScript.morphName = "まばたき";
    }

    // Update is called once per frame
    void Update()
    {
        if(isPlus) morphScript.morphWeight += Time.deltaTime * 7.5f;
        else morphScript.morphWeight -= Time.deltaTime * 7.5f;

        if (morphScript.morphWeight < 0){
            morphScript.morphWeight = 0;
            if (!DelayTime){
                DelayTime = true;
                StartCoroutine(DelayCoroutine());
            }
        }
        else if(morphScript.morphWeight > 1){
            morphScript.morphWeight = 1;
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
    */
}
