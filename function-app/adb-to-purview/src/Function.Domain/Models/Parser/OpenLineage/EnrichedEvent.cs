using System;
using System.Collections.Generic;
using Function.Domain.Models.Adb;

namespace Function.Domain.Models.OL
{
    public class EnrichedEvent
    {
        public Event? OlEvent = null;
        public AdbRoot? AdbRoot = null;
        public AdbRoot? AdbParentRoot = null;
        public bool IsInteractiveNotebook = false;
        public EnrichedEvent(Event olEvent, AdbRoot? adbRoot, AdbRoot? adbParentRoot)
        {
            OlEvent = olEvent;
            AdbRoot = adbRoot;
            AdbParentRoot = adbParentRoot;
        }
    }
}