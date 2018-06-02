using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colosseum.Match;

namespace Colosseum.Prototypes.AI {
    public class AIAction {
        public string name;
        public float cost;
        public bool isAborted;
        public Dictionary<string, object> effects = new Dictionary<string, object>();
        public Dictionary<string, object> preconditions = new Dictionary<string, object>();
        public List<System.Func<bool>> procedualPreconditions;
        public IEnumerator action;
    }

}