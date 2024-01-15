using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputTextController : MonoBehaviour
{
    private TMP_InputField inputZone;
    private bool shiftEnabledL = false;
    private bool shiftEnabledR = false;
    private int mistakeCount = 0;
    private bool shiftActive = false;

    public static bool fjSubmitEnabled = false;
    private bool fSubmitted = false;
    private bool jSubmitted = false;

    // Start is called before the first frame update
    private void Awake()
    {
        ManageEvents.manager.onKeyTriggered += onKeyTriggered;
        ManageEvents.manager.onKeyUntriggered += onKeyUntriggered;
    }

    void Start()
    {
        inputZone = GetComponent<TMP_InputField>();
        inputZone.ActivateInputField();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void onKeyTriggered(string keyStr)
    {
        if (keyStr.Length == 1 && !fjSubmitEnabled)
        {
            inputZone.text += smartCaps(keyStr);
            inputZone.MoveTextEnd(false);
        }
        else if (keyStr == "leftshift")
        {
            shiftEnabledL = true;
        }
        else if (keyStr == "rightshift")
        {
            shiftEnabledR = true;
        }
        else if (keyStr == "backspace")
        {
            string temp = inputZone.text;
            if (temp.Length > 0)
            {
                mistakeCount++;
                inputZone.text = temp.Remove(temp.Length - 1, 1);
                inputZone.MoveTextEnd(false);
            }
        }
        else if (keyStr == "early")
        {
            PanelStateMachine.exitTimer = true;
        }
        else if (keyStr == "submit")
        {
            logText();
            ManageEvents.manager.submit(inputZone.text);
            inputZone.text = "";
            inputZone.MoveTextEnd(false);
        }
        else if (keyStr == "back")
        {
            ManageEvents.manager.submit("back");
            inputZone.text = "";
            inputZone.MoveTextEnd(false);
        }
        else if (keyStr == "next")
        {
            ManageEvents.manager.submit("next");
            inputZone.text = "";
            inputZone.MoveTextEnd(false);
        }
        else if (fjSubmitEnabled)
        {
            if (keyStr == "f")
            {
                fSubmitted = true;
            }
            if (keyStr == "j")
            {
                jSubmitted = true;
            }
            if (fSubmitted && jSubmitted)
            {
                fSubmitted = false;
                jSubmitted = false;
                ManageEvents.manager.submit("");
                inputZone.text = "";
                inputZone.MoveTextEnd(false);
            }
        }
        UpdateGlobalShiftState();
    }

    public void onKeyUntriggered(string keyStr)
    {
        if (keyStr == "leftshift")
        {
            shiftEnabledL = false;
        }
        if (keyStr == "rightshift")
        {
            shiftEnabledR = false;
        }
        UpdateGlobalShiftState();
    }

    private string smartCaps(string keyStr)
    {
        if (shiftEnabledR || shiftEnabledL)
        {
            return keyStr.ToUpper();
        }
        return keyStr;
    }

    private void logText()
    {
        ManageEvents.manager.log(inputZone.text,mistakeCount);
        mistakeCount = 0;
    }

    private void UpdateGlobalShiftState()
    {
        if ((shiftEnabledL || shiftEnabledR) && !shiftActive)
        {
            shiftActive = true;
            ManageEvents.manager.activateShift();
        }
        else if (!(shiftEnabledL || shiftEnabledR) && shiftActive)
        {
            shiftActive = false;
            ManageEvents.manager.deactivateShift();
        }
    }
}
