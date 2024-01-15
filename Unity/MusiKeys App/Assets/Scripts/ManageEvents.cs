using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageEvents : MonoBehaviour
{
    public static ManageEvents manager; 

    private void Awake()
    {
        manager = this;
    }

    private void Start()
    {
        //OVRManager.suggestedCpuPerfLevel = OVRManager.ProcessorPerformanceLevel.Boost;
    }

    public event Action<string> onKeyTriggered;
    public void TriggerKey(string myKey)
    {
        onKeyTriggered?.Invoke(myKey);
        //Debug.Log("Triggered Key!");
    }

    public event Action<string> onKeyUntriggered;
    public void UntriggerKey(string myKey)
    {
        onKeyUntriggered?.Invoke(myKey);
        //Debug.Log("Released Key!");
    }

    public event Action<string> onSubmit;
    public void submit(string myStr)
    {
        onSubmit?.Invoke(myStr);
    }

    public event Action<string,int> onLog;
    public void log(string myStr,int mistakes)
    {
        onLog?.Invoke(myStr,mistakes);
    }

    public event Action onShiftActivated;
    public void activateShift()
    {
        onShiftActivated?.Invoke();
    }

    public event Action onShiftDeactivated;
    public void deactivateShift()
    {
        onShiftDeactivated?.Invoke();
    }

    //public event Action<bool> onSetFeedbackMode;
    //public void setFeedbackMode(bool val)
    //{
    //    onSetFeedbackMode?.Invoke(val);
    //}

    public event Action<int> onSetFeedbackType;
    public void setFeedbackType(int val)
    {
        onSetFeedbackType?.Invoke(val);
    }

    public event Action<float> onSetKeyScale;
    public void setKeyScale(float val)
    {
        onSetKeyScale?.Invoke(val);
    }
}
