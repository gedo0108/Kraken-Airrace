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
        public String modus = "Normal";

        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "Runden", isPersistant = true)]
        [UI_FloatRange(scene = UI_Scene.Flight, minValue = 1, maxValue = 20, stepIncrement = 1)]
        public Single rounds = 1;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Stelle", isPersistant = true)]
        [UI_FloatRange(scene = UI_Scene.Flight, minValue = 1, maxValue = 30, stepIncrement = 1)]
        public Single order = 1;

        [KSPField(isPersistant = false)]
        public String triggerTransform;

        // The race driver
        public AirraceDriver driver;

        public override void OnStart(StartState state)
        {
            driver = FlightGlobals.ActiveVessel.vesselModules.FirstOrDefault(m => m.GetType() == typeof(AirraceDriver)) as AirraceDriver;
#if DEBUG



            Transform trigger = part.partTransform;
#else
            Transform trigger = part.FindModelTransform(triggerTransform);
            
#endif
            trigger.gameObject.AddOrGetComponent<AirraceTrigger>().callback = collider => { driver.Trigger(this); };
            trigger.gameObject.GetComponentInChildren<MeshCollider>().isTrigger = true;
            
            Debug.Log("[KAR] " + this.name + " created");
            
        }

        public override void OnUpdate()
        {
            Fields["rounds"].guiActive = modus == "Start+Ziel";
        }
    }
}
