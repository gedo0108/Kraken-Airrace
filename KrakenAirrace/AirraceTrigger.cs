using System;
using UnityEngine;

namespace KrakenAirrace
{
    public class AirraceTrigger : MonoBehaviour
    {
        public Action<Collider> callback;

        // Gets called when something enters the 
        void OnTriggerEnter(Collider other)
        {
            callback?.Invoke(other);
        }
    }
}