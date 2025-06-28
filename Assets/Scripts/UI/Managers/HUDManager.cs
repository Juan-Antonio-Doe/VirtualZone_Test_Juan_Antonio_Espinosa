using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour {

    // Check if the input field starts with a negative sign and remove it
    public void ValueChanged(InputField input)
    {
        string txt = input.text;
        if (txt.Length > 0 && txt[0] == '-')
            input.text = txt[1..];
    }

}