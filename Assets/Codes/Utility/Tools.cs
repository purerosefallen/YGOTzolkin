using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace YGOTzolkin.Utility
{
    static class Tools
    {
        public static void BindEvent(GameObject control, EventTriggerType type, Action<BaseEventData> action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = type
            };
            entry.callback.AddListener(new UnityAction<BaseEventData>(action));
            control.GetComponent<EventTrigger>().triggers.Add(entry);
        }

        public static T LoadResource<T>(string name)
        {
            return UnityEngine.Object.Instantiate(Resources.Load(name) as GameObject).GetComponent<T>();
        }
    }
}
