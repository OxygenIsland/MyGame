using UnityEngine;

namespace StarWorld.FrameWork
{
    public class WaitingForJobDone : CustomYieldInstruction
    {
        private bool isDone = false;
        public override bool keepWaiting => !isDone;

        public void Done() { isDone = true; }

        public bool success;
    }
}
