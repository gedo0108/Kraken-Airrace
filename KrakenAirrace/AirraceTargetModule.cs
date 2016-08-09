using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KrakenAirrace
{
    public class AirraceTargetModule : PartModule
    {
        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Modus", isPersistant = true)]
        [UI_ChooseOption(scene = UI_Scene.Flight, options = new[] {"Start", "Ziel", "Normal", "Start+Ziel"})]
        public String modus;

        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "Runden", isPersistant = true)]
        [UI_FloatRange(scene = UI_Scene.Flight, minValue = 1, maxValue = 20, stepIncrement = 1)]
        public Int32 rounds;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Stelle", isPersistant = true)]
        [UI_FloatRange(scene = UI_Scene.Flight, minValue = 1, maxValue = 30, stepIncrement = 1)]
        public Int32 order;

        [KSPField(isPersistant = false)]
        public String triggerTransform;

        // The race driver
        public AirraceDriver driver;

        public override void OnStart(StartState state)
        {
            driver = vessel.vesselModules.FirstOrDefault(m => m.GetType() == typeof(AirraceDriver)) as AirraceDriver;
            Transform trigger = part.FindModelTransform(triggerTransform);
            trigger.gameObject.AddComponent<AirraceTrigger>().callback = collider => { driver.Trigger(this); };
        }

        public override void OnUpdate()
        {
            Fields["rounds"].guiActive = modus == "Start+Ziel";
        }
    }
}
