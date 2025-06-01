using UnityEngine;

[ExecuteAlways]
public class SampleGUI : MonoBehaviour
{
    private Rect m_windowRect = new Rect(0, 0, 120, 200); //ウィンドウの初期位置・大きさ
    private int m_windowId = 0; //ウィンドウ固有のID
    private string m_windowTitle = "status"; //ウィンドウの題名
    public Config config;
    private bool isFirst = true;
    private Color defaultLabelColor;

    private void colorLabel(string text, Color color) {
        GUIStyle style = GUI.skin.label;
        GUIStyleState styleState = new GUIStyleState();
        styleState.textColor = color;
        style.normal = styleState;
        style.fontSize = 20;

        GUILayout.Label(text, style);

        GUIStyleState styleState2 = new GUIStyleState();
        styleState2.textColor = this.defaultLabelColor;
        style.normal = styleState2;
        style.fontSize = 20;
    }

    public void OnGUI()
    {
        m_windowRect = GUI.Window(m_windowId, m_windowRect, (id) => 
        {
            if (isFirst) {
                this.defaultLabelColor = GUI.skin.label.normal.textColor;
            }
            this.isFirst = false;

            // ここにGUIの中身を記述する
            GUILayout.Label($"音量: \n{config.maxVolume}"); //音量表示用テキスト
            if (config.silence_time!=0f) {
                colorLabel($"沈黙時間: \n{config.silence_time}", new Color(0.0f, 1.0f, 0.0f)); //沈黙時間（計測中）
            }
            else {
                colorLabel($"沈黙時間: \n{config.silence_time}", new Color(1.0f, 0.0f, 0.0f)); //沈黙時間（計測停止中）
            }

            if (config.audio_process) {
                colorLabel($"録音中......", new Color(0.0f, 0.0f, 1.0f)); //録音中（青）
            }
            else if (!config.is_system_respose && config.silence_time!=0f) {
                colorLabel($"会話可能", new Color(0.0f, 1.0f, 0.0f)); //会話可能（緑）
            }
            else {
                colorLabel($"会話不可", new Color(1.0f, 0.0f, 0.0f)); //会話不可（赤）
            }

            // これを一番下に書くと，ドラッグでウィンドウを動かせるようになる
            GUI.DragWindow();
        }, m_windowTitle);
    }
}
