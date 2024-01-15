using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DemoSliderFunctions : MonoBehaviour
{
    public Transform MyKeyboard;
    //public Slider TriggerDistanceSlider;
    //public TextMeshProUGUI TriggerDistanceValue;
    //public Slider ReleaseDistanceSlider;
    //public TextMeshProUGUI ReleaseDistanceValue;
    //public Slider KeyboardScaleSlider;
    //public TextMeshProUGUI KeyboardScaleValue;
    //public Slider KeyScaleSlider;
    //public TextMeshProUGUI KeyScaleValue;
    //public Slider KeyboardAngleSlider;
    //public TextMeshProUGUI KeyboardAngleValue;
    public int KeyboardAngle = 25;
    public float KeyboardScale = 1.0f;
    public float KeyScale = 1.0f;
    private Vector3 KeyboardLocalScale;

    private void Awake()
    {
        setTriggerDistance(0.008f);
        setReleaseBuffer(0.004f);
    }

    // Start is called before the first frame update
    void Start()
    {
        //get necessary starting values to be scaled
        KeyboardLocalScale = MyKeyboard.localScale;
        //set readouts to be the values from manager
        //TriggerDistanceValue.text = TriggerableKey.TriggerDistance.ToString();
        //ReleaseDistanceValue.text = TriggerableKey.ReleaseBuffer.ToString();
        //set readouts to be the given scale values and apply the scale given
        setKeyboardScale(0.86f);
        setKeyScale(0.71f);
        setKeyboardAngle(17);
    }

    //public void setTriggerDistance()
    //{
    //    TriggerableKey.TriggerDistance = TriggerDistanceSlider.value;
    //    TriggerDistanceValue.text = TriggerableKey.TriggerDistance.ToString();
    //}

    //public void setReleaseBuffer()
    //{
    //    TriggerableKey.ReleaseBuffer = ReleaseDistanceSlider.value;
    //    ReleaseDistanceValue.text = TriggerableKey.ReleaseBuffer.ToString();
    //}

    //public void setKeyboardScale()
    //{
    //    MyKeyboard.localScale = new Vector3(KeyboardLocalScale.x*KeyboardScaleSlider.value,
    //                                        KeyboardLocalScale.y,
    //                                        KeyboardLocalScale.z * KeyboardScaleSlider.value);
    //    KeyboardScaleValue.text = KeyboardScaleSlider.value.ToString();
    //}

    //public void setKeyScale()
    //{
    //    ManageEvents.manager.setKeyScale(KeyScaleSlider.value);
    //    KeyScaleValue.text = KeyScaleSlider.value.ToString();
    //}

    //public void setKeyboardAngle()
    //{
    //    MyKeyboard.localEulerAngles = new Vector3(-KeyboardAngleSlider.value, 0, 0);
    //    KeyboardAngleValue.text = KeyboardAngleSlider.value.ToString();
    //}

    public void setTriggerDistance(float value)
    {
        TriggerableKey.TriggerDistance = value;
    }

    public void setReleaseBuffer(float value)
    {
        TriggerableKey.ReleaseBuffer = value;
    }

    public void setKeyboardScale(float value)
    {
        MyKeyboard.localScale = new Vector3(KeyboardLocalScale.x * value,
                                            KeyboardLocalScale.y,
                                            KeyboardLocalScale.z * value);
    }

    public void setKeyScale(float value)
    {
        ManageEvents.manager.setKeyScale(value);
    }

    public void setKeyboardAngle(int value)
    {
        MyKeyboard.localEulerAngles = new Vector3(-value, 0, 0);
    }

}
