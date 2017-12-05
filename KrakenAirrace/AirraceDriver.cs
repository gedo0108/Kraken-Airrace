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

        // Whether the driver can start a race
        public Boolean isEnabled;

        // Time when the race has started
        public Double raceStart;

        // The current airrace
        public Airrace race;

        // Controller for the race
        public Controller controller;

        public PartSelector next;
        public PartSelector done;
        public Dictionary<Part, PartSelector> all = new Dictionary<Part, PartSelector>();

        // Adds the controller module
        public override void OnLoadVessel()
        {
            if (!(vessel.name.Contains("Pylon")))
            {
                controller = (Controller)vessel.rootPart.AddModule(typeof(Controller).Name);
                controller.driver = this;
               
            }


        }

        // Gets called when someone passes a target
        public void Trigger(AirraceTargetModule target)
        {


            if (target.order == race?.position && race.targets.Contains(target))
            {
                race.position++;
                next.Dismiss();
                foreach (ScreenMessage msg in ScreenMessages.Instance.ActiveMessages)
                    ScreenMessages.RemoveMessage(msg);
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
                    isRacing = false;
                    Vessel v = GetComponent<Vessel>();
                    foreach (Part p in v.parts)
                        all.Add(p, PartSelector.Create(p, pa => { }, XKCDColors.GrassyGreen, XKCDColors.GrassyGreen));

                }
                else if (target.modus == "Start+Ziel")
                {
                    if (target.rounds == race.rounds)
                    {
                        ScreenMessages.PostScreenMessage("Zielpylone passiert (Zeit: " + controller.timePassed + ") @" + System.Math.Round(controller.vessel.speed, 1) + "m/s", 100f, ScreenMessageStyle.LOWER_CENTER);
                        race.time = TimeSpan.FromSeconds(Planetarium.GetUniversalTime() - raceStart);
                        controller.qc = (Int32)(race.time.TotalMilliseconds * 0.1337);
                        isRacing = false;
                        Vessel v = GetComponent<Vessel>();
                        foreach (Part p in v.parts)
                            all.Add(p, PartSelector.Create(p, pa => { }, XKCDColors.GrassyGreen, XKCDColors.GrassyGreen));   
                       
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
                        ScreenMessages.PostScreenMessage("Nächste Pylone: " + race.targets[(Int32)target.order].part.vessel.vesselName, 5f, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
                else if (race.targets.Count == (Int32)target.order && race.targets[0].modus == "Start+Ziel")
                {
                    all[race.targets[0].part].Dismiss();
                    all.Remove(race.targets[0].part);

                    next = PartSelector.Create(race.targets[0].part, p => { }, XKCDColors.BrightAqua, XKCDColors.BrightAqua);
                    ScreenMessages.PostScreenMessage("Nächste Pylone: " + race.targets[0].part.vessel.vesselName, 5f, ScreenMessageStyle.UPPER_CENTER);
                    race.position = 1;
                }
                else
                {
                    next = PartSelector.Create(race.targets[(Int32)target.order].part, p => { }, XKCDColors.BrightAqua, XKCDColors.BrightAqua);
                    ScreenMessages.PostScreenMessage("Nächste Pylone: " + race.targets[(Int32)target.order].part.vessel.vesselName, 5f, ScreenMessageStyle.UPPER_CENTER);
                }

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
                    timePassed = "00:00:0000";
                    Debug.Log("[KAR] Race started");

                }

            }

            public override void OnUpdate()
            {
                if (!driver.isRacing) return;
                TimeSpan time = TimeSpan.FromSeconds(Planetarium.GetUniversalTime() - driver.raceStart);
                timePassed = $"{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds:0000}";
            }

#if DEBUG
            void onJointBreak(EventReport e)
            {
                if (e.origin == e.origin.vessel.rootPart)
                {
                    if (driver.isRacing)
                        Enable();
                        ScreenMessages.PostScreenMessage("Disqualifiziert. Grund: Du bist gecrasht.", 5f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
#endif

            void OnGUI()
            {
                GUI.skin.label.fontSize = 16;
                GUI.skin.button.fontSize = 16;
                GUI.skin.box.fontSize = 16;
                GUI.Box(new Rect(5, 40, 240, 130), new GUIContent("[KAR]Kraken Airrace v0.3.1"));
                GUI.Label(new Rect(15, 75, 240, 30), "Zeit: " + (string)timePassed);
                GUI.Label(new Rect(15, 95, 240, 30), "QC: " + qc.ToString());
                if (GUI.Button(new Rect(65, 125, 120, 30), !driver.isEnabled ? "Start" : "Stop"))
                    Enable();
#if debug
                if (driver.isEnabled)
                {
                    GUI.Label(new Rect(220, 85, 600, 30), "Pos: " + driver.race.position + "Modus: " + driver.race.targets[0].modus );
                    GUI.Label(new Rect(220, 115, 800, 30), "Amount: " + driver.race.amount + " | Rounds: " + driver.race.rounds + " | Targets: " + driver.race.targets.Count);
                }              
#endif
            }

        }

        
    }






}