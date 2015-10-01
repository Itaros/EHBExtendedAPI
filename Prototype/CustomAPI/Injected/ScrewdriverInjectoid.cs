using GameWorld2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prototype.CustomAPI.Injected
{
    public class ScrewdriverInjectoid
    {

        

        public static void RemoveWatchdog(Computer pTarget)
        {
            if (pTarget != null)
            {
                pTarget.maxExecutionTime = -2F;
            }
        }

        public static void SetWatchdog(Computer pTarget, float seconds)
        {
            if (pTarget != null)
            {
                pTarget.maxExecutionTime = seconds;
            }
        }

        public static void ResetWatchdog(Computer pTarget)
        {
            if (pTarget != null)
            {
                if (pTarget.masterProgram != null)
                {
                    pTarget.masterProgram.executionTime = 0F;
                }
            }
        }

    }
}
