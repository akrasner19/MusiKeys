using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerableKey : MonoBehaviour
{
    public static float TriggerDistance = 0.1f;
    public static float ReleaseBuffer = 0.05f;

    private float audioTriggerDistance = 0.0001f;
    private float filterscale = 0.0f;
    private float filtermin = 330.0f;
    private float filtermax = 16000.0f;
    private float volumemin = 0.01f;
    private float volumemax = 0.8f;
    private float volumescale = 0.0f;
    private float pressedVol = 0.04f;

    private int volExp = 3;
    private int filtExp = 3;

    private int activeFinger = 3;

    //private bool FeedbackMode = false;
    
    /**
     * Type 0 = No sound
     * Type 1 = SOTA Click
     * Type 2 = Atonal Noise
     * Type 3 = MusiKeys
     */
    private int FeedbackType = 0;

    private bool amTriggered = false;
    private bool amRepeatable = false;
    private bool repeatCheckActive = false;

    private BoxCollider triggerCollider;

    private IEnumerator repeatRoutine;

    private Color activeKeyColor;
    private Color originalKeyColor;
    private Color transparentKeyColor;
    private Color pushedKeyColor = new Color(1.0f,0.78f,0.0f);
    private Color shiftedKeyColor = new Color(0.0f, 0.78f, 1.0f);

    private AudioSource keyAudioSource;
    private AudioSource childAudioSource;
    //private AudioSource fbRepSource;
    private AudioLowPassFilter keyLowPass;
    private AudioClip pressClick;
    private AudioClip releaseClick;
    private AudioClip woosh;
    private ONSPAudioSource spatializer;
    private ONSPAudioSource spatializer2;
    private List<AudioClip> fingertones;
    private List<AudioClip> fingerclicksin;
    private List<AudioClip> fingerclicksout;

    private Vector3 originalLocalScale;

    public string keyValue;
    public GameObject myImage;
    public bool hideable = false;
    public bool isEnabled = true;

    private void Awake()
    {
        ManageEvents.manager.onKeyTriggered += hideKey;
        ManageEvents.manager.onShiftActivated += onShiftActivated;
        ManageEvents.manager.onShiftDeactivated += onShiftDeactivated;
        ManageEvents.manager.onSetFeedbackType += onSetFeedbackType;
        ManageEvents.manager.onSetKeyScale += onSetKeyScale;

        originalKeyColor = myImage.GetComponent<Image>().color;
        transparentKeyColor = originalKeyColor;
        //temporary value for transparent for more efficient variable use
        transparentKeyColor.a = 0.1f;
        if (!isEnabled)
        {
            setKeyColor(transparentKeyColor);
            activeKeyColor = transparentKeyColor;
        }
        else
        {
            activeKeyColor = originalKeyColor;
        }
        //real transparent value
        transparentKeyColor.a = 0.2f;

        //add audio system
        keyAudioSource = gameObject.AddComponent<AudioSource>();

        //fbRepSource = gameObject.AddComponent<AudioSource>();
        //fbRepSource.loop = true;
        GameObject aChild = new GameObject();
        aChild = Instantiate(aChild, transform);
        childAudioSource = aChild.AddComponent<AudioSource>();
        childAudioSource.loop = true;

        keyLowPass = aChild.AddComponent<AudioLowPassFilter>();
        keyLowPass.cutoffFrequency = 20.0f;

        originalLocalScale = transform.parent.localScale;

        //load sounds
        if (isEnabled)
        {
            pressClick = Resources.Load("Audio/Tiny Click_Minimal UI Sounds") as AudioClip;
            releaseClick = Resources.Load("Audio/Click 02_Minimal UI Sounds") as AudioClip;
            woosh = Resources.Load("Audio/wooshsound") as AudioClip;//get the right clip loaded

            //fill audio clip lists
            fingertones = new List<AudioClip>();
            fingerclicksin = new List<AudioClip>();
            fingerclicksout = new List<AudioClip>();
            fingertones.Add(Resources.Load("Audio/lpinkytone") as AudioClip);
            fingertones.Add(Resources.Load("Audio/lmidtone") as AudioClip);
            fingertones.Add(Resources.Load("Audio/lindextone") as AudioClip);
            fingertones.Add(Resources.Load("Audio/thumbtone2") as AudioClip);
            fingertones.Add(Resources.Load("Audio/rindextone") as AudioClip);
            fingertones.Add(Resources.Load("Audio/rmidtone") as AudioClip);
            fingertones.Add(Resources.Load("Audio/rpinkytone") as AudioClip);
            fingerclicksin.Add(Resources.Load("Audio/lpinkyclickin") as AudioClip);
            fingerclicksin.Add(Resources.Load("Audio/lmidclickin") as AudioClip);
            fingerclicksin.Add(Resources.Load("Audio/lindexclickin") as AudioClip);
            fingerclicksin.Add(Resources.Load("Audio/thumbclickin") as AudioClip);
            fingerclicksin.Add(Resources.Load("Audio/rindexclickin") as AudioClip);
            fingerclicksin.Add(Resources.Load("Audio/rmidclickin") as AudioClip);
            fingerclicksin.Add(Resources.Load("Audio/rpinkyclickin") as AudioClip);
            fingerclicksout.Add(Resources.Load("Audio/lpinkyclickout") as AudioClip);
            fingerclicksout.Add(Resources.Load("Audio/lmidclickout") as AudioClip);
            fingerclicksout.Add(Resources.Load("Audio/lindexclickout") as AudioClip);
            fingerclicksout.Add(Resources.Load("Audio/thumbclickout") as AudioClip);
            fingerclicksout.Add(Resources.Load("Audio/rindexclickout") as AudioClip);
            fingerclicksout.Add(Resources.Load("Audio/rmidclickout") as AudioClip);
            fingerclicksout.Add(Resources.Load("Audio/rpinkyclickout") as AudioClip);
        }

        spatializer = gameObject.AddComponent<ONSPAudioSource>();
        spatializer2 = aChild.AddComponent<ONSPAudioSource>();

        if (keyValue == "backspace")
        {
            amRepeatable = true;
        }

        //trigger collider stuff
        triggerCollider = gameObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        Vector3 tempVec = new Vector3(0.1f, 0.1f, 0.015f);
        triggerCollider.size = tempVec;
        if (!isEnabled)
        {
            triggerCollider.enabled = false;
        }

        //calculate filter scaling ratio
        //16000 is peak hz
        //330 is min hz
        //
        //Debug.Log("filtermax:" + filtermax);
        //Debug.Log("filtermin:" + filtermin);
        //Debug.Log("TriggerDistance:" + TriggerDistance);
        //Debug.Log("audioTriggerDistance:" + audioTriggerDistance);
        
        //Debug.Log(filterscale);
    }

    // Start is called before the first frame update
    private void Start()
    {
        filterscale = (filtermax - filtermin) / (Mathf.Pow(TriggerDistance, filtExp) - Mathf.Pow(audioTriggerDistance, filtExp));
        volumescale = (volumemax - volumemin) / (Mathf.Pow(TriggerDistance, volExp) - Mathf.Pow(audioTriggerDistance, volExp));
        //ManageEvents.manager.onKeyTriggered += hideKey;
        //ManageEvents.manager.onShiftActivated += onShiftActivated;
        //ManageEvents.manager.onShiftDeactivated += onShiftDeactivated;
        //ManageEvents.manager.onSetFeedbackType += onSetFeedbackType;
        //ManageEvents.manager.onSetKeyScale += onSetKeyScale;


    }

    // Update is called once per frame
    void Update()
    {
        CheckKeyPress();
    }

    private void CheckKeyPress()
    {
        if (isEnabled && transform.localPosition.z > TriggerDistance && !amTriggered)
        {
            amTriggered = true;
            StartCoroutine("KeyPressAnimate");
            ManageEvents.manager.TriggerKey(keyValue ?? "null");
            if (FeedbackType == 3)
            {
                keyAudioSource.PlayOneShot(fingerclicksin[activeFinger]);
                //keyAudioSource.PlayOneShot(pressClick);
            }
            else if (FeedbackType == 1 || FeedbackType == 4)
            {
                keyAudioSource.PlayOneShot(pressClick);
            }
            else if (FeedbackType == 2)
            {
                keyAudioSource.PlayOneShot(fingerclicksin[4]);
            }

            if (amRepeatable)
            {
                repeatRoutine = RepeatTrigger();
                StartCoroutine(repeatRoutine);
            }
        }
        else if (isEnabled && transform.localPosition.z < (TriggerDistance - ReleaseBuffer) && amTriggered)
        {
            if (repeatCheckActive)
            {
                StopCoroutine(repeatRoutine);
                repeatCheckActive = false;
            }
            amTriggered = false;
            KeyUnpressAnimate();
            ManageEvents.manager.UntriggerKey(keyValue ?? "null");
            if (FeedbackType == 3)
            {
                //keyAudioSource.clip = fingerclicksout[activeFinger];
                //keyAudioSource.Play();
                keyAudioSource.PlayOneShot(fingerclicksout[activeFinger]);
            }
            else if (FeedbackType == 1 || FeedbackType == 4)
            {
                keyAudioSource.PlayOneShot(releaseClick);
            }
            else if (FeedbackType == 2)
            {
                keyAudioSource.PlayOneShot(fingerclicksout[4]);
            }
        }

        //feedback mode controls
        if (isEnabled &&
            (FeedbackType == 3 || FeedbackType == 2 || FeedbackType == 4) &&
            !amTriggered &&
            transform.localPosition.z > audioTriggerDistance &&
            transform.localPosition.z < TriggerDistance
            )
        {
            //Debug.Log(((transform.localPosition.z - audioTriggerDistance) * filterscale) + filtermin);
            //Here add volume automation based on key position
            childAudioSource.volume = ((Mathf.Pow(transform.localPosition.z, volExp) -
                Mathf.Pow(audioTriggerDistance, volExp)) * volumescale) + volumemin;
            //audio cutoff automation
            keyLowPass.cutoffFrequency = ((Mathf.Pow(transform.localPosition.z,filtExp) - 
                Mathf.Pow(audioTriggerDistance,filtExp)) * filterscale) + filtermin;
            if (!childAudioSource.isPlaying && !keyAudioSource.isPlaying)
            {
                if (FeedbackType == 3)
                {
                    childAudioSource.clip = fingertones[activeFinger];
                }
                else if (FeedbackType == 4)
                {
                    childAudioSource.clip = woosh;
                }
                else if (FeedbackType == 2)
                {
                    childAudioSource.clip = fingertones[4];
                }
                childAudioSource.Play();
            }
        }
        //else if (isEnabled && 
        //    childAudioSource.isPlaying &&
        //    (amTriggered ||
        //        transform.localPosition.z <= audioTriggerDistance ||
        //        transform.localPosition.z > TriggerDistance
        //        )
        //    )
        //{
        //    childAudioSource.Stop();
        //}
        else if (isEnabled &&
            childAudioSource.isPlaying &&
            transform.localPosition.z <= audioTriggerDistance
            )
        {
            childAudioSource.Stop();
        }
        else if (isEnabled &&
            childAudioSource.isPlaying &&
            amTriggered)
        {
            childAudioSource.volume = pressedVol;
        }
    }

    IEnumerator RepeatTrigger()
    {
        repeatCheckActive = true;
        yield return new WaitForSeconds(1.0f);
        setKeyColor(pushedKeyColor);
        while (amTriggered)
        {
            ManageEvents.manager.TriggerKey(keyValue ?? "null");
            keyAudioSource.PlayOneShot(pressClick);
            yield return new WaitForSeconds(0.15f);
        }
        repeatCheckActive = false;
    }

    IEnumerator KeyPressAnimate()
    {
        setKeyColor(pushedKeyColor);
        yield return new WaitForSeconds(0.1f);
        if (amTriggered)
        {
            setKeyColor(transparentKeyColor);
        }
    }

    private void KeyUnpressAnimate()
    {
        setKeyColor(activeKeyColor);
    }

    private void hideKey(string keyStr)
    {
        if (hideable && keyStr == "hide")
        {
            transform.parent.gameObject.SetActive(false);
        }
    }

    private void setKeyColor(Color color)
    {
        myImage.GetComponent<Image>().color = color;
    }

    private void onShiftActivated()
    {
        activeKeyColor = shiftedKeyColor;
        if (!amTriggered && isEnabled)
        {
            setKeyColor(activeKeyColor);
        }
    }

    private void onShiftDeactivated()
    {
        activeKeyColor = originalKeyColor;
        if (!amTriggered && isEnabled)
        {
            setKeyColor(activeKeyColor);
        }
    }

    //private void onSetFeedbackMode(bool val)
    //{
    //    FeedbackMode = val;
    //}

    private void onSetFeedbackType(int val)
    {
        FeedbackType = val;
    }

    private void onSetKeyScale(float val)
    {
        transform.parent.localScale = new Vector3(originalLocalScale.x * val, originalLocalScale.y * val, originalLocalScale.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Start")
        {
            return;
        }
        //string handedness = other.transform.parent.parent.parent.parent.name;
        bool rightHand = true;
        //if (handedness == "LeftHandAnchor")
        //{
        //    rightHand = false;
        //}
        if (other.name.Substring(2, 1) == "l")
        {
            rightHand = false;
        }

        string nametest = other.name.Substring(4, 1);
        switch (nametest)
        {
            case "t":
                activeFinger = 3;
                break;
            case "i":
                if (rightHand)
                {
                    activeFinger = 4;
                }
                else
                {
                    activeFinger = 2;
                }
                break;
            case "m":
                if (rightHand)
                {
                    activeFinger = 5;
                }
                else
                {
                    activeFinger = 1;
                }
                break;
            case "p":
                if (rightHand)
                {
                    activeFinger = 6;
                }
                else
                {
                    activeFinger = 0;
                }
                break;
            default:
                break;
        }
    }
}
