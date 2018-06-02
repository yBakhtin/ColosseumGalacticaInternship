using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Colosseum.Management {
    public class Sector {
        public string name;
        public AudienceSpecies species;
        public PopulationDensity populationDensity;
        public string history;
        public bool isLocked;


        public void UnlockEvents() {
            // TODO: Handle additional events here
            throw new System.NotImplementedException();
        }
    }
}
