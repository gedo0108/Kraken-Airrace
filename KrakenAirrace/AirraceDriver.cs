using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UnityEngine;

namespace KrakenAirrace
{



    public class AirraceDriver : VesselModule
    {

        // Whether a race has been started
        public Boolean isRacing;

        // Wheter training mode is active
        public Boolean isTraining = false;

        public Dictionary<TimeSpan, Int32> times = new Dictionary<TimeSpan, Int32>();

        // Whether the driver can start a race
        public Boolean isEnabled;

        // Time when the race has started
        public Double raceStart;

        // Best time 
        public TimeSpan record = new TimeSpan(0);



        // The current airrace
        public Airrace race;

        // Controller for the race
        public Controller controller;

        public PartSelector next;
        public PartSelector done;
        public Dictionary<Part, PartSelector> all = new Dictionary<Part, PartSelector>();

        // Audio stuff
        public AudioClip correct = GameDatabase.Instance.GetAudioClip("krakenairrace/Sounds/correct");
        public AudioSource audiosource = new AudioSource();


        // Adds the controller module
        public override void OnLoadVessel()

        {

            if (!audiosource)
            {
                audiosource = gameObject.AddComponent<AudioSource>();
                audiosource.reverbZoneMix = 1;
                audiosource.bypassListenerEffects = true;
                audiosource.minDistance = 10000;
                audiosource.maxDistance = 10000;
                audiosource.priority = 10;
                audiosource.dopplerLevel = 0;
                audiosource.spatialBlend = 1;
                audiosource.rolloffMode = AudioRolloffMode.Linear;
            }


            if (!(vessel.name.Contains("Pylon")))
            {
                controller = (Controller)vessel.rootPart.AddModule(typeof(Controller).Name);
                controller.driver = this;
            }


        }
        // Gets called when someone passes a target
        public void Trigger(AirraceTargetModule target)
        {
            audiosource.transform.position = vessel.rootPart.transform.position;
            if (target.order == race?.position && race.targets.Contains(target))
            {

                race.position++;
                next.Dismiss();
                all.Clear();
                
                ScreenMessages.PostScreenMessage("Pylone " + target.order + " passiert (Zeit: " + controller.timePassed + ")", 3f, ScreenMessageStyle.UPPER_CENTER);

                if (race.targets.Count != (Int32)target.order && all.ContainsKey(race.targets[(Int32)target.order].part))
                {
                    all[race.targets[(Int32)target.order].part].Dismiss();
                    all.Remove(race.targets[(Int32)target.order].part);
                }
       
                all.Add(target.part, PartSelector.Create(target.part, p => { }, XKCDColors.GrassyGreen, XKCDColors.GrassyGreen));

                
                if (target.modus == "Start")
                {
                    raceStart = Planetarium.GetUniversalTime();
                    isRacing = true;
                    next = PartSelector.Create(race.targets[(Int32)target.order].part, p => { }, XKCDColors.BrightAqua, XKCDColors.BrightAqua);
                    ScreenMessages.PostScreenMessage("Nächste Pylone: " + race.targets[(Int32)target.order].part.vessel.vesselName, 5f, ScreenMessageStyle.UPPER_CENTER);

                }
                else if (target.modus == "Ziel")
                {
                    ScreenMessages.PostScreenMessage("Zielpylone passiert (Zeit: " + controller.timePassed + ") @" + System.Math.Round(controller.vessel.speed, 1) + "m/s", 100f, ScreenMessageStyle.LOWER_CENTER);

                    race.time = TimeSpan.FromSeconds(Planetarium.GetUniversalTime() - raceStart);
                    controller.qc = (Int32)(race.time.TotalMilliseconds * 0.1337);

                    times.Add(race.time, controller.qc);
                    record = times.Keys.Min();
    
                        isRacing = false;
                    Vessel v = GetComponent<Vessel>();
                    foreach (Part p in v.parts)
                        all.Add(p, PartSelector.Create(p, pa => { }, XKCDColors.GrassyGreen, XKCDColors.GrassyGreen));

                    if (isTraining)
                        controller.Enable();
                }
                else if (target.modus == "Start+Ziel")
                {
                    if (target.rounds == race.rounds)
                    {         
                        ScreenMessages.PostScreenMessage("Zielpylone passiert (Zeit: " + controller.timePassed + ") @" + System.Math.Round(controller.vessel.speed, 1) + "m/s", 10f, ScreenMessageStyle.LOWER_CENTER);

                        race.time = TimeSpan.FromSeconds(Planetarium.GetUniversalTime() - raceStart);
                        controller.qc = (Int32)(race.time.TotalMilliseconds * 0.1337);
                        times.Add(race.time,controller.qc);
                        record = times.Keys.Min();
                        isRacing = false;
                        Vessel v = GetComponent<Vessel>();
                        foreach (Part p in v.parts)
                            all.Add(p, PartSelector.Create(p, pa => { }, XKCDColors.GrassyGreen, XKCDColors.GrassyGreen));
                        if (isTraining)
                            controller.Enable();
                    }
                    else
                    {
                        if (race.rounds == 0)
                        {
                            raceStart = Planetarium.GetUniversalTime();
                            isRacing = true;
                        }
                        race.rounds++;
                        next = PartSelector.Create(race.targets[(Int32)target.order].part, p => { }, XKCDColors.BrightAqua, XKCDColors.BrightAqua);

                        
                        ScreenMessages.PostScreenMessage("Nächste Pylone: " + (string)next.Host.vessel.vesselName, 5f, ScreenMessageStyle.UPPER_CENTER);
                        
                    }
                }
                else if (race.targets.Count == (Int32)target.order && race.targets[0].modus == "Start+Ziel")
                {

#if debug
                    all[race.targets[0].part].Dismiss();
                    all.Remove(race.targets[0].part);
                    foreach (AirraceTargetModule art in race.targets)
                    { all.Remove(art.part);
                       // all.Add(p, PartSelector.Create(p, pa => { }, XKCDColors.GrassyGreen, XKCDColors.GrassyGreen));

                    }

#endif 
                    all.Clear();
                    next = PartSelector.Create(race.targets[0].part, p => { }, XKCDColors.BrightAqua, XKCDColors.BrightAqua);
                    ScreenMessages.PostScreenMessage("Nächste Pylone: " + race.targets[0].part.vessel.vesselName, 5f, ScreenMessageStyle.UPPER_CENTER);
                    
                    race.position = 1;
                }
                else
                {
                    next = PartSelector.Create(race.targets[(Int32)target.order].part, p => { }, XKCDColors.BrightAqua, XKCDColors.BrightAqua);
                    ScreenMessages.PostScreenMessage("Nächste Pylone: " + (string)next.Host.vessel.vesselName, 5f, ScreenMessageStyle.UPPER_CENTER);
                    
                }
                if (!isTraining)
                audiosource.PlayOneShot(correct);
            }
            
            
        }

        // Interface for the vessel while 
        public class Controller : PartModule
        {


            [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "Zeit")]
            public String timePassed = "00:00:0000";

            [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "QC")]
            public Int32 qc = 0;


            // The driver
            public AirraceDriver driver;

            [KSPEvent(guiActive = true, active = true, guiActiveEditor = false, guiActiveUncommand = false, guiActiveUnfocused = false, guiName = "Start")]
            public void Enable()
            {
                driver.isEnabled = !driver.isEnabled;
                Events["Enable"].guiName = !driver.isEnabled ? "Start" : "Stop";
                Fields["timePassed"].guiActive = driver.isEnabled;

                // Create the Race
                if (driver.isEnabled)
                {
                    Airrace race = new Airrace
                    {
                        amount = FlightGlobals.Vessels.Count(v => v.FindPartModulesImplementing<AirraceTargetModule>().Count != 0),
                        position = 1,
                        rounds = 0,
                        targets = FlightGlobals.Vessels.SelectMany(v => v.FindPartModulesImplementing<AirraceTargetModule>()).OrderBy(m => m.order).ToList()
                    };
                    qc = 0;
                    timePassed = "00:00:0000";
                    driver.race = race;
                    ScreenMessages.PostScreenMessage("Jeb ist am Drücker. Flieg durch die erste Pylone!", 5f, ScreenMessageStyle.UPPER_CENTER);
                    driver.next = PartSelector.Create(race.targets[0].part, p => { }, XKCDColors.BrightAqua, XKCDColors.BrightAqua);
                }
                else
                {
                    
                    driver.race = null;
                    foreach (PartSelector s in driver.all.Values)
                        s.Dismiss();
                    driver.all.Clear();
                    driver.isRacing = false;
                    ScreenMessages sm = (ScreenMessages)GameObject.FindObjectOfType(typeof(ScreenMessages));
                    foreach (ScreenMessage msg in sm.ActiveMessages.ToList<ScreenMessage>())
                        ScreenMessages.RemoveMessage(msg);
                   
                    if (driver.isTraining)
                    {
                        driver.isEnabled = false;
                        Enable();
                    }
                }
            }

            public override void OnUpdate()
            {
                if (!driver.isRacing) return;
                TimeSpan time = TimeSpan.FromSeconds(Planetarium.GetUniversalTime() - driver.raceStart);
                timePassed = $"{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds:0000}";
            }


            void OnGUI()
            {
                GUI.skin.label.fontSize = 18;
                GUI.skin.label.fontStyle = FontStyle.Bold;
                GUI.skin.button.fontSize = 16;
                GUI.skin.box.fontSize = 16;
                GUI.Box(new Rect(5, 40, 240, 170), new GUIContent("[KAR]Kraken Airrace v0.3.1"));
                GUI.Label(new Rect(18, 75, 240, 30), "Zeit: " + timePassed);
                GUI.Label(new Rect(18, 95, 240, 30), "QC: " + qc.ToString());
                if (GUI.Button(new Rect(65, 125, 120, 30), !driver.isEnabled ? "START" : "STOP"))
                    Enable();
                if (GUI.Button(new Rect(65, 160, 120, 30), !driver.isTraining ? "RACE" : "TRAIN"))
                    driver.isTraining = !driver.isTraining;

                if (driver.isTraining) { 
                if (driver.times.Count != 0)
                    {
                    int guii = 260;
                    GUI.Label(new Rect(15, guii, 230, 30), "Rekord: " + String.Format("{0:D2}:{1:D2}:{2:D4}", driver.record.Minutes, driver.record.Seconds, driver.record.Milliseconds));
                    guii = guii + 30;
                    int i = 1;

                    foreach (KeyValuePair<TimeSpan,Int32> t in driver.times)
                        {
                        
                        GUI.Label(new Rect(15, guii, 350, 25), "[" + i + "] " + String.Format("{0:D2}:{1:D2}:{2:D4}", t.Key.Minutes, t.Key.Seconds, t.Key.Milliseconds + " - GC: " + t.Value.ToString()));
                            guii = guii + 25;
                            i++;
                        }
                    
                     }
                    GUI.skin.label.fontSize = 32;
                    GUI.Label(new Rect(Screen.width - 310, Screen.height - 120, 300, 50), System.Math.Round(vessel.speed, 0) + "m/s");
                    GUI.skin.label.fontSize = 18;
                    GUI.Label(new Rect(Screen.width - 310, Screen.height - 90, 300, 50), System.Math.Round(vessel.speed * 3.6, 0) + "km/h");
                }





#if DEBUG
            

                if (driver.isEnabled)
                {
                    GUI.Label(new Rect(220, 85, 600, 30), "Pos: " + driver.race.position + "Modus: " + driver.race.targets[0].modus );
                    GUI.Label(new Rect(220, 115, 800, 30), "Amount: " + driver.race.amount + " | Rounds: " + driver.race.rounds + " | Targets: " + driver.race.targets.Count);
                }    
                
                                if (e.origin == e.origin.vessel.rootPart)
                {

                    driver.race = null;
                }          
#endif
            }

        }

        
    }






}