using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Globalization;
using System.Linq;

public class PanelStateMachine : MonoBehaviour
{
    //text sections to display
    [TextArea]
    public string SelectModePrompt;

    [TextArea]
    public string CodenamePrompt;

    [TextArea]
    public string FreestylePrompt;

    [TextArea]
    public string FTaskPrompt;

    [TextArea]
    public string JTaskPrompt;

    [TextArea]
    public string FFTaskPrompt;

    [TextArea]
    public string JJTaskPrompt;

    [TextArea]
    public string FFFTaskPrompt;

    [TextArea]
    public string AwaitFurtherInstructions;

    [TextArea]
    public string ThankYouMessage;

    //interface components to hide and show
    public GameObject adjustmentKnob;
    public GameObject submitEarlyButton;
    public GameObject topBackButton;
    public GameObject bottomBackButton;
    public GameObject topNextButton;
    public GameObject bottomNextButton;

    public static bool exitTimer = false;

    public static float timerCounter = 0.0f;

    private string nextdisplaystate = "initialize";

    private bool isModeF_;
    private bool isMode_F;

    private TMP_InputField textZone;

    private string Codename;

    private string logText = "";

    private string ERROR_MESSAGE = "ERROR!!! Something went wrong! Please inform the study administrator.";

    private int activeTrialIndex = 0;
    private int activeTaskIndex = 0;

    private AudioClip myclip;
    private AudioClip mysuccessclip;

    private void Awake()
    {
        ManageEvents.manager.onSubmit += progressStateMachine;
        ManageEvents.manager.onLog += onLog;
        myclip = Resources.Load("Audio/VEH3 Percussion 031") as AudioClip;
        mysuccessclip = Resources.Load("Audio/modern_success_sfx") as AudioClip;
    }

    // Start is called before the first frame update
    void Start()
    {
        textZone = GetComponent<TMP_InputField>();
        progressStateMachine("");
    }

    
    public void progressStateMachine(string submission)
    {
        switch (nextdisplaystate)
        {
            case "initialize":
                //hide all buttons
                topBackButton.SetActive(false);
                topNextButton.SetActive(false);
                bottomBackButton.SetActive(false);
                bottomNextButton.SetActive(false);
                //hide submit button
                submitEarlyButton.SetActive(false);

                ManageEvents.manager.setFeedbackType(0);

                string validationResult1 = JSONHandler.study_spec.validateImport();
                string validationResult2 = JSONHandler.preset_text.validateImport();
                if (validationResult1 != "valid")
                {
                    textZone.text = (ERROR_MESSAGE + "\n\n" + validationResult1);
                }
                else if (validationResult2 != "valid")
                {
                    textZone.text = (ERROR_MESSAGE + "\n\n" + validationResult2);
                }
                else if (JSONHandler.study_spec.trial_specs.Count == 0)
                {
                    textZone.text = (ERROR_MESSAGE + "\n\nNo trials loaded.");
                }
                else
                {
                    //set codename
                    Codename = JSONHandler.study_spec.participant;
                    if (JSONHandler.result_log.results.Count == 0)
                    {
                        nextdisplaystate = "start";
                    }
                    else if (JSONHandler.result_log.results.Last().trial_number == 4 &&
                        JSONHandler.result_log.results.Last().task_number == 6)
                    {
                        //set up and send to new error state requesting files be swapped out
                        nextdisplaystate = "errorstate";
                    }
                    else if (JSONHandler.result_log.results.Last().task_number < 5)
                    {
                        //set up and send to tasknum+1 for active trial number
                        activeTaskIndex = JSONHandler.result_log.results.Last().task_number;
                        activeTrialIndex = JSONHandler.result_log.results.Last().trial_number - 1;
                        ManageEvents.manager.setFeedbackType(JSONHandler.study_spec.trial_specs[activeTrialIndex].keyboard_type);
                        nextdisplaystate = "confirmationpause";
                    }
                    else if (JSONHandler.result_log.results.Last().task_number == 5)
                    {
                        //set up and send to tlx for active trial number
                        //make trial number equal trialnum+1 to account for missed log state
                        activeTaskIndex = 0;
                        activeTrialIndex = JSONHandler.result_log.results.Last().trial_number;
                        ManageEvents.manager.setFeedbackType(JSONHandler.study_spec.trial_specs[activeTrialIndex-1].keyboard_type);
                        nextdisplaystate = "tlxcollection";
                    }
                    else if (JSONHandler.result_log.results.Last().task_number == 6)
                    {
                        //set up and send to tasknum+1 for trial number+1
                        //make trial num ++ to account for missed log state
                        activeTaskIndex = 0;
                        activeTrialIndex = JSONHandler.result_log.results.Last().trial_number;
                        ManageEvents.manager.setFeedbackType(JSONHandler.study_spec.trial_specs[activeTrialIndex].keyboard_type);
                        nextdisplaystate = "prepractice";
                    }
                    progressStateMachine("");
                }
                break;
            case "errorstate":
                textZone.text = "ERROR: Please inform the study administrator.\n\nCompleted participant file in use. Please save the files and replace with file for current participant.";
                break;
            case "start":

                //show adjustment handle (starts active)
                adjustmentKnob.SetActive(true);
                //adjustmentKnob.SetActive(true);
                //keep bottom next shown
                bottomNextButton.SetActive(true);
                //set feedbacktype
                ManageEvents.manager.setFeedbackType(0);
                //set text and display codename 
                textZone.text = (JSONHandler.preset_text.position_adjustment + Codename) ?? ERROR_MESSAGE;
                //set next state
                nextdisplaystate = "prepractice";
                break;
            case "prepractice":
                //show adjustment handle (if hidden)
                adjustmentKnob.SetActive(true);
                //show next button in unique position, hide back button
                topBackButton.SetActive(false);
                bottomBackButton.SetActive(false);
                bottomNextButton.SetActive(false);
                //show top next
                topNextButton.SetActive(true);
                //display the explanation
                textZone.text = JSONHandler.preset_text.pre_practices[
                    JSONHandler.study_spec.trial_specs[activeTrialIndex].keyboard_type
                    ].practice_desc;
                //set the next state
                nextdisplaystate = "practicetime";
                //set the feedback type
                ManageEvents.manager.setFeedbackType(JSONHandler.study_spec.trial_specs[activeTrialIndex].keyboard_type);
                break;
            case "practicetime":
                //hide adjustment handle (if shown)
                adjustmentKnob.SetActive(false);
                //show back and submit buttons in unique position
                topBackButton.SetActive(false);
                topNextButton.SetActive(false);
                //show bottom next and back
                bottomNextButton.SetActive(true);
                bottomBackButton.SetActive(true);
                //display the practice prompt
                textZone.text = JSONHandler.study_spec.trial_specs[activeTrialIndex].warm_up;
                //set next state
                nextdisplaystate = "postpractice";
                break;
            case "postpractice":
                //make go back to prepractice on submission == back
                if (submission == "back")
                {
                    nextdisplaystate = "prepractice";
                    progressStateMachine("");
                    break;
                }
                //show adjustment handle (if hidden)
                adjustmentKnob.SetActive(true);
                //show back and next buttons in unique position
                bottomBackButton.SetActive(false);
                bottomNextButton.SetActive(false);
                //show top next and back
                topNextButton.SetActive(true);
                topBackButton.SetActive(true);
                //set text
                textZone.text = JSONHandler.preset_text.post_practice;
                //set next state
                nextdisplaystate = "confirmationpause";
                break;
            case "confirmationpause":
                //make go back to practicetime on submission == back
                if (submission == "back")
                {
                    nextdisplaystate = "practicetime";
                    progressStateMachine("");
                    break;
                }
                //ManageEvents.manager.setFeedbackType(JSONHandler.study_spec.trial_specs[activeTrialIndex].keyboard_type);
                //show adjustment handle (if hidden)
                adjustmentKnob.SetActive(true);
                //hide all buttons
                topBackButton.SetActive(false);
                topNextButton.SetActive(false);
                bottomNextButton.SetActive(false);
                bottomBackButton.SetActive(false);
                //activate the f and j submission
                InputTextController.fjSubmitEnabled = true;
                //display the explanation of f and j stuff
                textZone.text = JSONHandler.preset_text.start_confirmation;
                //set next state
                nextdisplaystate = "threebeeps";
                break;
            case "threebeeps":
                adjustmentKnob.SetActive(false);
                //Disable f j submit
                InputTextController.fjSubmitEnabled = false;
                //set text shown and next state
                textZone.text = "";
                nextdisplaystate = "taskactive";
                //do the thing that calls for 3 beeps
                StartCoroutine(playBoops());
                break;
            case "taskactive":
                //start timer going (both one to count and one to trigger submit)
                StartCoroutine(trialtimer());
                //show the submit early button (somewhere a tad out of the way)
                submitEarlyButton.SetActive(true);
                //display the task
                textZone.text = JSONHandler.study_spec.trial_specs[activeTrialIndex].task_list[activeTaskIndex].stimulus;
                //set next state
                nextdisplaystate = "logandsave";
                break;
            case "logandsave":
                //hide submit early button
                submitEarlyButton.SetActive(false);
                //logging and saving stuff
                logToJSON();

                //play sound for success
                playSuccessChime();

                //show text for success
                textZone.text = JSONHandler.preset_text.task_complete;

                //increment task index
                ++activeTaskIndex;
                //state movement
                if (activeTaskIndex < JSONHandler.study_spec.trial_specs[activeTrialIndex].task_list.Count)
                {
                    nextdisplaystate = "confirmationpause";
                    StartCoroutine(progressInThreeSeconds());
                }
                else
                {
                    //increment trial index
                    ++activeTrialIndex;
                    
                    //reset task index
                    activeTaskIndex = 0;
                    nextdisplaystate = "tlxcollection";
                    StartCoroutine(progressInThreeSeconds());
                }
                break;

            case "tlxcollection":
                //show upper next button
                //hide all other buttons
                topBackButton.SetActive(false);
                topNextButton.SetActive(true);
                bottomNextButton.SetActive(false);
                bottomBackButton.SetActive(false);

                //show adjustment knob
                adjustmentKnob.SetActive(true);

                textZone.text = JSONHandler.preset_text.tlx_answer;

                nextdisplaystate = "preexperience";

                break;

            case "preexperience":
                adjustmentKnob.SetActive(true);
                //show lower next button
                //show lower back button
                //hide all other buttons
                topBackButton.SetActive(false);
                topNextButton.SetActive(false);
                bottomNextButton.SetActive(true);
                bottomBackButton.SetActive(true);

                textZone.text = JSONHandler.preset_text.pre_exp_analysis;

                nextdisplaystate = "experiencetext";

                break;

            case "experiencetext":
                if (submission == "back")
                {
                    nextdisplaystate = "tlxcollection";
                    progressStateMachine("");
                    break;
                }
                //show upper next button
                //show upper back button
                //hide all other buttons
                topBackButton.SetActive(true);
                topNextButton.SetActive(true);
                bottomNextButton.SetActive(false);
                bottomBackButton.SetActive(false);

                adjustmentKnob.SetActive(false);

                textZone.text = JSONHandler.study_spec.trial_specs[activeTrialIndex-1].warm_up;

                //choose which end to go to
                if (activeTrialIndex < JSONHandler.study_spec.trial_specs.Count)
                {
                    nextdisplaystate = "trialend";
                }
                else
                {
                    nextdisplaystate = "studyend";
                }
                break;

            case "trialend":
                if (submission == "back")
                {
                    nextdisplaystate = "preexperience";
                    progressStateMachine("");
                    break;
                }

                //show next hide back (unique spot)
                topBackButton.SetActive(false);
                topNextButton.SetActive(false);
                bottomNextButton.SetActive(true);
                bottomBackButton.SetActive(true);
                //set text
                textZone.text = JSONHandler.preset_text.post_trial;
                //set next state
                nextdisplaystate = "trialendlog";
                break;

            case "trialendlog":
                if (submission == "back")
                {
                    nextdisplaystate = "experiencetext";
                    progressStateMachine("");
                    break;
                }
                //log a task 6 to signify end of trial
                trialEndToJSON();
                //go to prepractice
                nextdisplaystate = "prepractice";
                progressStateMachine("");
                break;

            case "studyend":
                if (submission == "back")
                {
                    nextdisplaystate = "preexperience";
                    progressStateMachine("");
                    break;
                }
                //set text
                textZone.text = JSONHandler.preset_text.thank_you;
                nextdisplaystate = "studyendback";

                //add code to log that task 6 occured
                trialEndToJSON();
                break;

            case "studyendback":
                if (submission == "back")
                {
                    nextdisplaystate = "experiencetext";
                    progressStateMachine("");
                    break;
                }
                nextdisplaystate = "studyend";
                progressStateMachine("");
                break;

            default:
                break;
        }
    }

    //Vars for holding the old log data before using somewhere useful
    private string submissionLog = "";

    private int backspaceLog = 0;

    public void onLog(string submission, int mistakeCount)
    {
        submissionLog = submission;
        backspaceLog = mistakeCount;
        //logText += (submission + "\n\nMistake count: " + mistakeCount.ToString() + "\n\n");
    }

    public void logToJSON()
    {
        TaskResult tr = new TaskResult();
        tr.participant = JSONHandler.study_spec.participant;
        tr.trial_number = JSONHandler.study_spec.trial_specs[activeTrialIndex].trial_number;
        tr.task_number = JSONHandler.study_spec.trial_specs[activeTrialIndex].task_list[activeTaskIndex].task_number;
        tr.keyboard_type = JSONHandler.study_spec.trial_specs[activeTrialIndex].keyboard_type;
        tr.time_elapsed = timerCounter;
        tr.backspace_count = backspaceLog;
        tr.stimulus_text = JSONHandler.study_spec.trial_specs[activeTrialIndex].task_list[activeTaskIndex].stimulus;
        tr.result_text = submissionLog;
        var culture = new CultureInfo("en-US");
        DateTime localTimeStamp = DateTime.Now;
        tr.timestamp = localTimeStamp.ToString(culture);

        JSONHandler.result_log.results.Add(tr);
        JSONHandler.outputResultsToFile();
    }

    public void trialEndToJSON()
    {
        TaskResult tr = new TaskResult();
        tr.participant = JSONHandler.study_spec.participant;
        tr.trial_number = JSONHandler.study_spec.trial_specs[activeTrialIndex-1].trial_number;
        tr.task_number = 6;
        tr.keyboard_type = JSONHandler.study_spec.trial_specs[activeTrialIndex-1].keyboard_type;
        tr.time_elapsed = 0;
        tr.backspace_count = 0;
        tr.stimulus_text = "x End of Trial x";
        tr.result_text = "x End of Trial x";
        var culture = new CultureInfo("en-US");
        DateTime localTimeStamp = DateTime.Now;
        tr.timestamp = localTimeStamp.ToString(culture);

        JSONHandler.result_log.results.Add(tr);
        JSONHandler.outputResultsToFile();
    }

    //deprecated
    public void outputLogFile()
    {
        string _path = Application.persistentDataPath + "/" + Codename + "_logfile.txt";
        //File.WriteAllText(_path, logText);
        File.AppendAllText(_path, logText);
        logText = "";
    }

    //Call me with StartCoroutine
    IEnumerator trialtimer()
    {
        yield return stopwatchTimer(60.0f);
        ManageEvents.manager.TriggerKey("submit");
        ManageEvents.manager.UntriggerKey("submit");
    }

    IEnumerator stopwatchTimer(float dur)
    {
        timerCounter = 0.0f;
        while (timerCounter < dur)
        {
            timerCounter += Time.deltaTime;

            if (exitTimer)
            {
                exitTimer = false;
                yield break;
            }
            yield return null;
        }
    }

    //Play 3 times, once per second, then call after 3rd second
    IEnumerator playBoops()
    {
        var myplayer = gameObject.GetComponent<AudioSource>();
        yield return new WaitForSecondsRealtime(1.0f);
        myplayer.PlayOneShot(myclip);
        simulateTyping("3...   ");
        yield return new WaitForSecondsRealtime(1.0f);
        myplayer.PlayOneShot(myclip);
        simulateTyping("2...   ");
        yield return new WaitForSecondsRealtime(1.0f);
        myplayer.PlayOneShot(myclip);
        simulateTyping("1...   ");
        yield return new WaitForSecondsRealtime(1.0f);
        ManageEvents.manager.TriggerKey("next");
        ManageEvents.manager.UntriggerKey("next");
    }

    IEnumerator progressInThreeSeconds()
    {
        yield return new WaitForSecondsRealtime(3.0f);
        progressStateMachine("");
    }

    public void playSuccessChime()
    {
        var myplayer = gameObject.GetComponent<AudioSource>();
        myplayer.PlayOneShot(mysuccessclip);
    }

    public void simulateTyping(string str)
    {
        foreach (char ch in str)
        {
            ManageEvents.manager.TriggerKey(ch.ToString());
            ManageEvents.manager.UntriggerKey(ch.ToString());
        }
    }
    /**
     * Replace with a copy of this to:
     * Start on a page where you confirm controls, type=1 sounds
     * then give 4 different samples of the old sentence prompts in order
     * sound types going 0-3
     * remove any saving of files
     * 
     * 
     * FOR STUDY VERSION:
     * Make it dump a new file after each 5 rep trial in case the headset turns off or app crashes, 
     * and then you can jump them to where they need to be (build in this resume function to choose where to start off)
     * best way is probably to give it a loop where it endlessly asks for next steps (up to a max of 4) and then you give
     * it a code to choose the audio type for the next run. You tell them to run it in a specific order according to  a script 
     * that you bring in your hands that gives the order for each participant.
     */

    /*
    public void progressStateMachine(string submission)
    {
        switch (displaystate)
        {
            case "start":
                textZone.text = SelectModePrompt;
                displaystate = "selectmode";
                break;
            case "selectmode":
                if (submission == "ff")
                {
                    isModeF_ = true;
                    isMode_F = true;
                    textZone.text = CodenamePrompt;
                    displaystate = "getcodename";
                }
                else if (submission == "fj")
                {
                    isModeF_ = true;
                    isMode_F = false;
                    textZone.text = CodenamePrompt;
                    displaystate = "getcodename";
                }
                else if (submission == "jf")
                {
                    isModeF_ = false;
                    isMode_F = true;
                    textZone.text = CodenamePrompt;
                    displaystate = "getcodename";
                }
                else if (submission == "jj")
                {
                    isModeF_ = false;
                    isMode_F = false;
                    textZone.text = CodenamePrompt;
                    displaystate = "getcodename";
                }
                break;
            case "getcodename":
                Codename = submission;
                textZone.text = FreestylePrompt;
                displaystate = "freestyle";
                if (isModeF_)
                {
                    ManageEvents.manager.setFeedbackType(1);
                }
                else
                {
                    ManageEvents.manager.setFeedbackType(3);
                }
                break;
            case "freestyle":
                textZone.text = AwaitFurtherInstructions;
                displaystate = "prep1";
                break;
            case "prep1":
                if (isModeF_ && isMode_F)
                {
                    onLog("Next submission: SOTA Real",0);
                    textZone.text = FTaskPrompt;
                    ManageEvents.manager.setFeedbackType(1);
                }
                else if (isModeF_ && !isMode_F)
                {
                    onLog("Next submission: SOTA Pseudo", 0);
                    textZone.text = JTaskPrompt;
                    ManageEvents.manager.setFeedbackType(1);
                }
                else if (!isModeF_ && isMode_F)
                {
                    onLog("Next submission: Feedback Real", 0);
                    textZone.text = FFTaskPrompt;
                    ManageEvents.manager.setFeedbackType(3);
                }
                else if (!isModeF_ && !isMode_F)
                {
                    onLog("Next submission: Feedback Pseudo", 0);
                    textZone.text = JJTaskPrompt;
                    ManageEvents.manager.setFeedbackType(3);
                }
                displaystate = "trial1";
                break;
            case "trial1":
                textZone.text = AwaitFurtherInstructions;
                displaystate = "prep2";
                break;
            case "prep2":
                if (isModeF_ && isMode_F)
                {
                    onLog("Next submission: SOTA Pseudo", 0);
                    textZone.text = JTaskPrompt;
                    ManageEvents.manager.setFeedbackType(1);
                }
                else if (isModeF_ && !isMode_F)
                {
                    onLog("Next submission: SOTA Real", 0);
                    textZone.text = FTaskPrompt;
                    ManageEvents.manager.setFeedbackType(1);
                }
                else if (!isModeF_ && isMode_F)
                {
                    onLog("Next submission: Feedback Pseudo", 0);
                    textZone.text = JJTaskPrompt;
                    ManageEvents.manager.setFeedbackType(3);
                }
                else if (!isModeF_ && !isMode_F)
                {
                    onLog("Next submission: Feedback Real", 0);
                    textZone.text = FFTaskPrompt;
                    ManageEvents.manager.setFeedbackType(3);
                }
                displaystate = "trial2";
                break;
            case "trial2":
                textZone.text = FreestylePrompt;
                displaystate = "freestyle2";
                if (isModeF_)
                {
                    ManageEvents.manager.setFeedbackType(3);
                }
                else
                {
                    ManageEvents.manager.setFeedbackType(1);
                }
                break;
            case "freestyle2":
                textZone.text = AwaitFurtherInstructions;
                displaystate = "prep3";
                break;
            case "prep3":
                if (isModeF_ && isMode_F)
                {
                    onLog("Next submission: Feedback Real", 0);
                    textZone.text = FFTaskPrompt;
                    ManageEvents.manager.setFeedbackType(3);
                }
                else if (isModeF_ && !isMode_F)
                {
                    onLog("Next submission: Feedback Pseudo", 0);
                    textZone.text = JJTaskPrompt;
                    ManageEvents.manager.setFeedbackType(3);
                }
                else if (!isModeF_ && isMode_F)
                {
                    onLog("Next submission: SOTA Real", 0);
                    textZone.text = FTaskPrompt;
                    ManageEvents.manager.setFeedbackType(1);
                }
                else if (!isModeF_ && !isMode_F)
                {
                    onLog("Next submission: SOTA Pseudo", 0);
                    textZone.text = JTaskPrompt;
                    ManageEvents.manager.setFeedbackType(1);
                }
                displaystate = "trial3";
                break;
            case "trial3":
                textZone.text = AwaitFurtherInstructions;
                displaystate = "prep4";
                break;
            case "prep4":
                if (isModeF_ && isMode_F)
                {
                    onLog("Next submission: Feedback Pseudo", 0);
                    textZone.text = JJTaskPrompt;
                    ManageEvents.manager.setFeedbackType(3);
                }
                else if (isModeF_ && !isMode_F)
                {
                    onLog("Next submission: Feedback Real", 0);
                    textZone.text = FFTaskPrompt;
                    ManageEvents.manager.setFeedbackType(3);
                }
                else if (!isModeF_ && isMode_F)
                {
                    onLog("Next submission: SOTA Pseudo", 0);
                    textZone.text = JTaskPrompt;
                    ManageEvents.manager.setFeedbackType(1);
                }
                else if (!isModeF_ && !isMode_F)
                {
                    onLog("Next submission: SOTA Real", 0);
                    textZone.text = FTaskPrompt;
                    ManageEvents.manager.setFeedbackType(1);
                }
                displaystate = "trial4";
                break;
            case "trial4":
                textZone.text = ThankYouMessage;
                displaystate = "finished";
                outputLogFile();
                break;
            case "finished":
                break;
            default:
                break;
        }
    }
    */
}
