using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colosseum.Match;

namespace Colosseum.Prototypes.AI {
    public class AIAction {
        public string name; // the name of the action
        public float cost; // the cost of the action at which the agent will determine whether not or use the action
        public bool isAborted; // is the action aborted
        public Dictionary<string, object> effects = new Dictionary<string, object>(); // effects of the action (used for planning)
        public Dictionary<string, object> preconditions = new Dictionary<string, object>(); // preconditions of the action (used for planning)
        public List<System.Func<bool>> procedualPreconditions; // procedural preconditions of the actions
        public IEnumerator action; // callback which will be exectued by the agent
    }

}