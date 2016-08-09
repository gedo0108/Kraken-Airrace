using System;
using System.Collections;
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

        // Adds the controler module
        void Start()
        {
            Vessel v = GetComponent<Vessel>();
            controller = v.rootPart.AddModule(typeof(Controller).Name) as Controller;
        }

        // Gets called when someone passes a target
        public void Trigger(AirraceTargetModule target)
        {
            if (target.order == race.position && race.targets.Contains(target))
            {
                race.position = target.order;
                StartCoroutine(StopHighlight(target.part));
                if (target.modus == "Start")
                {
                    raceStart = DateTime.Now;
                    isRacing = true;
                    race.targets[target.order].part.SetHighlightColor(new Color32(122, 122, 178, 178));
                    race.targets[target.order].part.SetHighlight(true, true);
                }
                else if (target.modus == "Ziel")
                {
                    race.time = DateTime.Now - raceStart;
                    isRacing = false;
                    isEnabled = false;
                }
                else if (target.modus == "Start+Ziel")
                {
                    if (target.rounds == race.rounds)
                    {
                        race.time = DateTime.Now - raceStart;
                        isRacing = false;
                        isEnabled = false;
                    }
                    else
                    {
                        race.rounds++;
                        race.position = 1;
                        race.targets[0].part.SetHighlightColor(new Color32(122, 122, 178, 178));
                        race.targets[0].part.SetHighlight(true, true);
                    }
                }
            }
        }

        private IEnumerator StopHighlight(Part part)
        {
            part.SetHighlight(false, false);
            part.SetHighlightColor(new Color32(122, 178, 122, 178));
            part.SetHighlight(true, true);
            yield return new WaitForSeconds(5f);
            part.SetHighlight(false, false);
        }

        // Interface for the vessel while 
        public class Controller : PartModule
        {
            [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "Zeit")]
            public String timePassed;

            // The driver
            public AirraceDriver driver;

            [KSPEvent(guiActive = true, active = true, guiActiveEditor = false, guiActiveUncommand = false, guiActiveUnfocused = false, guiName = "Start")]
            private void Enable()
            {
                driver.isEnabled = !driver.isEnabled;
                Events["Enable"].guiName = driver.isEnabled ? "Start" : "Stop";

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
                    race.targets[0].part.SetHighlightColor(new Color32(122, 122, 178, 178));
                    race.targets[0].part.SetHighlight(true, true);
                }
                else
                {
                    driver.race = null;
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