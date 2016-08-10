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
        public DateTime raceStart;

        // The current airrace
        public Airrace race;

        // Whether the UI is enabled
        public Boolean showUI;

        // Controller for the race
        public Controller controller;

        public PartSelector next;
        public PartSelector done;
        public List<PartSelector> all = new List<PartSelector>();

        // Adds the controler module
        void Start()
        {
            Vessel v = GetComponent<Vessel>();
            controller = (Controller)v.rootPart.AddModule(typeof(Controller).Name);
            controller.driver = this;
        }

        // Gets called when someone passes a target
        public void Trigger(AirraceTargetModule target)
        {
            if (target.order == race?.position && race.targets.Contains(target))
            {
                race.position++;
                DestroyImmediate(next);
                all.Add(PartSelector.Create(target.part, p => { }, XKCDColors.GrassyGreen, XKCDColors.GrassyGreen));
                if (target.modus == "Start")
                {
                    raceStart = DateTime.Now;
                    isRacing = true;
                    next = PartSelector.Create(race.targets[(Int32)target.order].part, p => { }, XKCDColors.BrightAqua, XKCDColors.BrightAqua);
                }
                else if (target.modus == "Ziel")
                {
                    race.time = DateTime.Now - raceStart;
                    isRacing = false;
                    isEnabled = false;
                    Vessel v = GetComponent<Vessel>();
                    foreach (Part p in v.parts)
                        all.Add(PartSelector.Create(p, pa => { }, XKCDColors.GrassyGreen, XKCDColors.GrassyGreen));
                }
                else if (target.modus == "Start+Ziel")
                {
                    if (target.rounds == race.rounds)
                    {
                        race.time = DateTime.Now - raceStart;
                        isRacing = false;
                        isEnabled = false;
                        Vessel v = GetComponent<Vessel>();
                        foreach (Part p in v.parts)
                            all.Add(PartSelector.Create(p, pa => { }, XKCDColors.GrassyGreen, XKCDColors.GrassyGreen));
                    }
                    else
                    {
                        race.rounds++;
                        race.position = 1;
                        next = PartSelector.Create(race.targets[0].part, p => { }, XKCDColors.BrightAqua, XKCDColors.BrightAqua);
                    }
                }
            }
        }

        // Interface for the vessel while 
        public class Controller : PartModule
        {
            [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "Zeit")]
            public String timePassed = "00:00:00";

            // The driver
            public AirraceDriver driver;

            [KSPEvent(guiActive = true, active = true, guiActiveEditor = false, guiActiveUncommand = false, guiActiveUnfocused = false, guiName = "Start")]
            private void Enable()
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
                        rounds = 1,
                        targets = FlightGlobals.Vessels.SelectMany(v => v.FindPartModulesImplementing<AirraceTargetModule>()).OrderBy(m => m.order).ToList()
                    };
                    driver.race = race;
                    driver.next = PartSelector.Create(race.targets[0].part, p => { }, XKCDColors.BrightAqua, XKCDColors.BrightAqua);
                }
                else
                {
                    driver.race = null;
                    foreach (PartSelector s in driver.all)
                        s.Dismiss();
                    driver.all.Clear();
                }

            }

            [KSPEvent(guiActive = true, active = true, guiActiveEditor = false, guiActiveUncommand = false, guiActiveUnfocused = false, guiName = "UI zeigen")]
            private void UI()
            {
                driver.showUI = !driver.showUI;
                Events["UI"].guiName = !driver.showUI ? "UI zeigen" : "UI verstecken";
            }

            public override void OnUpdate()
            {
                if (!driver.isRacing) return;
                TimeSpan time = (DateTime.Now - driver.raceStart);
                timePassed = $"{(int) time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
            }
        }
    }
}