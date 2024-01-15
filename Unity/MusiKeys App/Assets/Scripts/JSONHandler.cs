using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Newtonsoft.Json;
using System.IO;
using System;

public class JSONHandler : MonoBehaviour
{
    public static StudySpec study_spec;
    public static PresetText preset_text;
    public static ResultLog result_log;
    // Start is called before the first frame update
    void Awake()
    {
        string _truepath = Application.persistentDataPath + "/permanent_files/NONE_unity_trials.json";

        string presetpath = Application.persistentDataPath + "/permanent_files/Preset_Texts.json";
        if (File.Exists(presetpath))
        {
            string PresetJSONFromFile = File.ReadAllText(presetpath);
            preset_text = JsonUtility.FromJson<PresetText>(PresetJSONFromFile);
        }
        else
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/permanent_files");
            TextAsset mytxtData2 = (TextAsset)Resources.Load("Preset_Texts");
            string JSONFromFile2 = mytxtData2.text;
            File.WriteAllText(presetpath, JSONFromFile2);
            //also write the backup none file
            TextAsset noneTextData = (TextAsset)Resources.Load("NONE_unity_trials");
            string JSONFromNoneFile = noneTextData.text;
            File.WriteAllText(_truepath, JSONFromNoneFile);
            //load the preset text object
            preset_text = JsonUtility.FromJson<PresetText>(JSONFromFile2);
        }

        string backuppath = Application.persistentDataPath + "/auto_backups";
        if (!Directory.Exists(backuppath))
        {
            Directory.CreateDirectory(backuppath);
        }

        string pNumeral = "";
        
        for (int i = 1; i <= 28; ++i)
        {
            pNumeral = i.ToString("D2");
            string _path = Application.persistentDataPath + "/P" + pNumeral + "_unity_trials.json";
            if (File.Exists(_path))
            {
                _truepath = _path;
                //cheesy way to end loop with technically fewest checks
                i = 30;
            }
        }

        //check to see if logfile with the participant ID already exists
        string newpath = Application.persistentDataPath + "/P" + pNumeral + "_results_log.json";
        if (File.Exists(newpath))
        {
            string LogJSONFromFile = File.ReadAllText(newpath);
            result_log = JsonUtility.FromJson<ResultLog>(LogJSONFromFile);
        }
        else
        {
            result_log = JsonUtility.FromJson<ResultLog>("{\"results\":[]}");
        }

        //always leave a NONE studyspec as the fallback
        //it should include enough info to load and tell that somethings wrong
        string JSONFromFile = File.ReadAllText(_truepath);
        //study_spec = JsonConvert.DeserializeObject<StudySpec>("");
        study_spec = JsonUtility.FromJson<StudySpec>(JSONFromFile);
    }

    public static void outputResultsToFile()
    {
        string resultsToSave = JsonUtility.ToJson(result_log,true);
        string mypath = Application.persistentDataPath + "/" +  
            JSONHandler.study_spec.participant + "_results_log.json";
        string backupPath = Application.persistentDataPath + "/auto_backups/" +
            JSONHandler.study_spec.participant + "_results_log.json";
        File.WriteAllText(mypath, resultsToSave);
        File.WriteAllText(backupPath, resultsToSave);
    }

    /*  void Awake()
    {
        TextAsset mytxtData = (TextAsset)Resources.Load("P08_unity_trials");
        string JSONFromFile = mytxtData.text;
        study_spec = JsonUtility.FromJson<StudySpec>(JSONFromFile);

        TextAsset mytxtData2 = (TextAsset)Resources.Load("Preset_Texts");
        string JSONFromFile2 = mytxtData2.text;
        preset_text = JsonUtility.FromJson<PresetText>(JSONFromFile2);

        study_spec.validateImport();
        preset_text.validateImport();
        //preset_text = JsonConvert.DeserializeObject<PresetText>(JSONFromFile2);
        //Debug.Log(JsonConvert.SerializeObject(preset_text,));
        //Debug.Log(JsonUtility.ToJson(preset_text,true));
        //Debug.Log(JSONFromFile2);
        //Debug.Log(JsonUtility.ToJson(study_spec, true));
        //Debug.Log(JSONFromFile);
    }*/
}

[System.Serializable]
public class StudySpec
{
    public string participant;
    public List<TrialSpec> trial_specs;

    public string validateImport()
    {
        string nullError = "ERROR, loaded item entry empty";
        try
        {
            if (participant == null)
            {
                return nullError;
            }
            foreach (TrialSpec spec in trial_specs)
            {
                if (spec.warm_up == null ||
                    spec.trial_number == 0)
                {
                    return nullError;
                }
                foreach (TaskSpec tspec in spec.task_list)
                {
                    if (tspec.stimulus == null ||
                        tspec.task_number == 0)
                    {
                        return nullError;
                    }
                }
            }
            return "valid";
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}

[System.Serializable]
public class TrialSpec
{
    public int keyboard_type;
    public int trial_number;
    public string warm_up;
    public List<TaskSpec> task_list;
}

[System.Serializable]
public class TaskSpec
{
    public int task_number;
    public string stimulus;
}

[System.Serializable]
public class PresetText
{
    public string position_adjustment;
    public List<TrialDescription> pre_practices;
    public string post_practice;
    public string start_confirmation;
    public string post_trial;
    public string thank_you;
    public string task_complete;
    public string tlx_answer;
    public string pre_exp_analysis;

    public string validateImport()
    {
        string nullError = "ERROR, loaded item entry empty";
        try
        {
            if (position_adjustment == null ||
                post_practice == null ||
                start_confirmation == null ||
                post_trial == null ||
                thank_you == null ||
                tlx_answer == null ||
                pre_exp_analysis == null)
            {
                return nullError;
            }
            foreach (TrialDescription desc in pre_practices)
            {
                if (desc.practice_desc == null)
                {
                    return nullError;
                }
            }
            return "valid";
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}

[System.Serializable]
public class TrialDescription
{
    public string practice_desc;
}

[System.Serializable]
public class ResultLog
{
    public List<TaskResult> results;
}

[System.Serializable]
public class TaskResult
{
    public string participant;
    public int trial_number;
    public int task_number;
    public int keyboard_type;
    public float time_elapsed;
    public int backspace_count;
    public string stimulus_text;
    public string result_text;
    public string timestamp;
}
