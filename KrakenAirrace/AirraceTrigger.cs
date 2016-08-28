using System;
using UnityEngine;

namespace KrakenAirrace
{
    public class AirraceTrigger : MonoBehaviour
    {
        public Action<Collider> callback;

#if DEBUG
        private static Boolean ten = true;
        void Update()
        {
            if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(ten ? KeyCode.F10 : KeyCode.F11))
            {
                ten = !ten;
                callback?.Invoke(null);
            }
        }
#else
        // Gets called when something enters the 
        void OnTriggerEnter(Collider other)
        {
            if (Part.GetComponentUpwards<Part>(other.gameObject))
                callback?.Invoke(other);
        }
#endif
    }
}