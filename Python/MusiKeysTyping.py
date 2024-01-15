from kivy.app import App
from kivy.clock import Clock
from kivy.uix.gridlayout import GridLayout
from kivy.uix.label import Label
from kivy.uix.textinput import TextInput
from kivy.properties import ColorProperty
from kivy.core.audio import SoundLoader
import json
import datetime
import os.path

typables = set(['e','r','t','y','u','i','d','f','g','h','j','k','c','v','b','n','m',',',' '])
myfontsize = 40
sidepadding = 56

class StateMachine:
    nextState = "start"
    timerActive = False
    activeTimer = None
    activeTicker = None
    timerDuration = 60
    timerCount = 0
    taskIndex = 0

    def __init__(self, mkInput, refInput):
        self.mkInput = mkInput
        self.refInput = refInput
        self.mkInput.focus = True
        #TODO - ADD CHECK FOR PXX ID files-----------------------------------------------------------------!
        #create the two JSON objects
        specjsonreadpath = "Trial Resources/permanent_files/NONE_python_trials.json"
        for n in range(1,31):
            numStr = str(n)
            numStr = numStr.rjust(2,"0")
            testpath = "Trial Resources/P" + numStr + "_python_trials.json"
            if os.path.exists(testpath):
                specjsonreadpath = testpath
                break
        presetjsonreadpath = "Trial Resources/permanent_files/Preset_Texts.json"
        #boopreadpath = "Trial Resources/permanent_files/VEH3 Percussion 031.wav"
        #boopreadpath = "Trial Resources/permanent_files/VEH3-Percussion-031.ogg"
        boopreadpath = "Trial Resources/permanent_files/VEH3-Percussion-031.mp3"
        successreadpath = "Trial Resources/permanent_files/modern_success_sfx.mp3"
        self.boopSound = SoundLoader.load(boopreadpath)
        self.successSound = SoundLoader.load(successreadpath)
        with open(specjsonreadpath,"r") as specjsonloadfile:
            self.tspec = json.load(specjsonloadfile)
        with open(presetjsonreadpath,"r") as presetjsonloadfile:
            self.ptext = json.load(presetjsonloadfile)
        self.JSONLog = {}
        self.JSONLog["results"] = []


    def runStateMachine(self,flagStr=""):
        if self.timerActive:
            self.activeTimer.cancel()
            self.timerActive = False
            self.activeTicker.cancel()
            #set text prompt state to next waiting state
            #log things
            self.runStateMachine()

        elif self.nextState == "start":
            self.mkInput.goBackEnabled = False
            codename = self.tspec["participant"]
            self.refInput.text = self.ptext["position_adjustment"] + codename
            self.mkInput.text = ""
            self.nextState = "prepractice"

        elif self.nextState == "prepractice":
            self.mkInput.goBackEnabled = False
            self.refInput.text = self.ptext["pre_practices"][0]["practice_desc"]
            self.mkInput.text = ""
            self.nextState = "practicetime"

        elif self.nextState == "practicetime":
            self.mkInput.goBackEnabled = True
            self.refInput.text = self.tspec["trial_specs"][0]["warm_up"]
            self.mkInput.text = ""
            self.nextState = "postpractice"

        elif self.nextState == "postpractice":
            if flagStr == "back":
                self.nextState = "prepractice"
                self.runStateMachine()
            else:
                self.mkInput.goBackEnabled = True
                self.refInput.text = self.ptext["post_practice"]
                self.mkInput.text = ""
                self.nextState = "confirmationpause"

        elif self.nextState == "confirmationpause":
            if flagStr == "back":
                self.nextState = "practicetime"
                self.runStateMachine()
            else:
                self.mkInput.enterActive = True
                self.mkInput.goBackEnabled = False
                self.mkInput.fjSubmitEnabled = True
                self.refInput.text = self.ptext["start_confirmation"]
                self.mkInput.text = ""
                self.nextState = "threebeeps"

        elif self.nextState == "threebeeps":
            self.mkInput.fjSubmitEnabled = False
            self.mkInput.enterActive = False
            self.refInput.text = ""
            self.mkInput.text = ""
            self.nextState = "taskactive"
            self.mkInput.backspaceCount = 0
            self.scheduleBoops()

        elif self.nextState == "taskactive":
            self.mkInput.enterActive = True
            self.refInput.text = self.tspec["trial_specs"][0]["task_list"][self.taskIndex]["stimulus"]
            self.mkInput.text = ""
            self.nextState = "logandsave"
            self.timerActive = True
            self.startTimer()

        elif self.nextState == "logandsave":
            self.mkInput.enterActive = False
            self.logToJSON()
            
            self.successSound.play()

            self.refInput.text = self.ptext["task_complete"]
            self.mkInput.text = ""

            self.taskIndex += 1
            if self.taskIndex < len(self.tspec["trial_specs"][0]["task_list"]):
                self.nextState = "confirmationpause"
                Clock.schedule_once(self.callableSM,3)
            else:
                self.nextState = "tlxcollection"
                Clock.schedule_once(self.callableSM,3)

        elif self.nextState == "tlxcollection":
            self.mkInput.enterActive = True
            self.mkInput.goBackEnabled = False
            self.refInput.text = self.ptext["tlx_answer"]
            self.mkInput.text = ""
            self.nextState = "preexperience"

        elif self.nextState == "preexperience":
            self.mkInput.goBackEnabled = True
            self.refInput.text = self.ptext["pre_exp_analysis"]
            self.mkInput.text = ""
            self.nextState = "experiencetext"

        elif self.nextState == "experiencetext":
            if flagStr == "back":
                self.nextState = "tlxcollection"
                self.runStateMachine()
            else:
                self.mkInput.goBackEnabled = True
                self.refInput.text = self.tspec["trial_specs"][0]["warm_up"]
                self.mkInput.text = ""
                self.nextState = "trialend"

        elif self.nextState == "trialend":
            if flagStr == "back":
                self.nextState = "preexperience"
                self.runStateMachine()
            else:
                self.mkInput.goBackEnabled = True
                self.refInput.text = self.ptext["post_trial"]
                self.mkInput.text = ""
                self.nextState = "finalgoback"

        elif self.nextState == "finalgoback":
            if flagStr == "back":
                self.nextState = "experiencetext"
                self.runStateMachine()
            else:
                self.nextState = "trialend"
                self.runStateMachine()

        else:
            return

    def logToJSON(self):
        dataToLog = {}
        dataToLog["participant"] = self.tspec["participant"]
        dataToLog["trial_number"] = self.tspec["trial_specs"][0]["trial_number"]
        dataToLog["task_number"] = self.tspec["trial_specs"][0]["task_list"][self.taskIndex]["task_number"]
        dataToLog["keyboard_type"] = self.tspec["trial_specs"][0]["keyboard_type"]
        dataToLog["time_elapsed"] = self.timerCount
        dataToLog["backspace_count"] = self.mkInput.backspaceCount
        dataToLog["stimulus_text"] = self.tspec["trial_specs"][0]["task_list"][self.taskIndex]["stimulus"]
        dataToLog["result_text"] = self.mkInput.text
        dataToLog["timestamp"] = str(datetime.datetime.now())

        self.JSONLog["results"].append(dataToLog)
        dumppath = "Trial Resources/" + self.tspec["participant"] + "_python_results_log.json"
        with open(dumppath,"w") as dumpfile:
            json.dump(self.JSONLog,dumpfile,indent="\t")
        

    def startTimer(self):
        self.timerActive = True
        self.timerCount = 0
        self.activeTimer = Clock.schedule_once(self.timerCompleted,self.timerDuration)
        self.activeTicker = Clock.schedule_interval(self.tickTimer,0.1)


    def timerCompleted(self, dt):
        self.timerActive = False
        self.activeTicker.cancel()
        self.runStateMachine()

    def tickTimer(self,interval):
        self.timerCount += interval

    def scheduleBoops(self):
        Clock.schedule_once(self.playBoop,1)
        Clock.schedule_once(self.playBoop,2)
        Clock.schedule_once(self.playBoop,3)
        Clock.schedule_once(self.callableSM,4)

    def playBoop(self,dt):
        self.boopSound.play()
        self.mkInput.text += (str(4-round(dt)) + "...   ")

    def callableSM(self,dt):
        self.runStateMachine()


class MusiKeysInput(TextInput):

    backspaceCount = 0
    #add something to send a signal when enter detected
    myStateMachine = None

    goBackEnabled = False
    enterActive = True

    fjSubmitEnabled = False

    fpressed = False
    jpressed = False


    def insert_text(self, substring, from_undo=False):
        s = ""
        if self.fjSubmitEnabled:
            s = ""
            if substring == "f":
                self.fpressed = True
            if substring == "j":
                self.jpressed = True
            if self.fpressed and self.jpressed:
                self.myStateMachine.runStateMachine()
                self.fpressed = False
                self.jpressed = False
        elif substring in typables:
            s = substring
        elif substring == '=' and self.goBackEnabled:
            s = ""
            self.myStateMachine.runStateMachine(flagStr="back")
        elif substring == '\n' and self.enterActive:
            s = ""
            self.myStateMachine.runStateMachine()
        else:
            s = ""
        return super().insert_text(s, from_undo=from_undo)

    def do_backspace(self, from_undo=False, mode='bkspc'):
        self.backspaceCount += 1
        return super().do_backspace(from_undo=False, mode='bkspc')


class MainScreen(GridLayout):

    def __init__(self, **kwargs):
        super(MainScreen, self).__init__(**kwargs)
        self.cols = 2
        self.previewtext = TextInput(disabled=True,font_size=myfontsize,padding=[sidepadding,6,sidepadding,6])
        self.previewtext.text="""
            Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Neque laoreet suspendisse interdum consectetur libero id faucibus. Vitae et leo duis ut diam quam nulla porttitor. Magna eget est lorem ipsum dolor sit amet. In cursus turpis massa tincidunt. Duis at consectetur lorem donec massa sapien faucibus. Duis tristique sollicitudin nibh sit amet. Fusce ut placerat orci nulla pellentesque dignissim enim sit amet. Cursus in hac habitasse platea dictumst quisque sagittis purus sit. Fermentum posuere urna nec tincidunt praesent semper feugiat nibh. Non diam phasellus vestibulum lorem sed risus ultricies.
            """
        self.add_widget(self.previewtext)
        self.resulttext = MusiKeysInput(multiline=True,font_size=myfontsize,cursor_color='black',padding=[sidepadding,6,sidepadding,6])
        self.add_widget(self.resulttext)

        self.stateMachine = StateMachine(self.resulttext,self.previewtext)
        self.resulttext.myStateMachine = self.stateMachine

        self.stateMachine.runStateMachine()
        


class MyApp(App):

    def build(self):
        return MainScreen()


if __name__ == '__main__':
    MyApp().run()