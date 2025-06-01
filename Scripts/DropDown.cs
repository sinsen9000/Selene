using UnityEngine;
using UnityEngine.UI;

public class DropDown : MonoBehaviour
{
    public string DropDownValue;
    public void OnSelected()
    {
        Dropdown ddtmp;
        ddtmp = GetComponent<Dropdown>(); //DropdownコンポーネントをGet
        DropDownValue = ddtmp.options[ddtmp.value].text; //Dropdownコンポーネントから選択されている文字を取得
    }
}
