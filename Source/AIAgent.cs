﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colosseum.Match;

namespace Colosseum.Prototypes.AI {
    public class AIAgent {
        //private List<AIAction> availibleActions = new List<AIAction>();
        //private Coroutine currentPlan;

        public Dictionary<string, object> worldState = new Dictionary<string, object>(); // the world state of the agent which is used for planning
        public Dictionary<string, object> goalState = new Dictionary<string, object>(); // the current goal state of the agent
        //public bool IsBusy { get { return currentPlan != null; } }

        /// <summary>
        /// The agent will perform the actions step by step described in the action sequence.
        /// </summary>
        public IEnumerator PerformActionSequence(Queue<AIAction> actionSequence) {
            Queue<AIAction> sequence = new Queue<AIAction>(actionSequence);
            bool goalReached = true;

            while (sequence.Count != 0) {
                AIAction action = sequence.Dequeue();
                yield return action.action;
            }

            //while (sequence.Count > 0) {
            //    //Debug.Log("forever running");
            //    AIAction current = sequence.Peek();
            //    yield return StartCoroutine(current.Perform(this));
            //    if (current.IsAborted) {
            //        goalReached = false;
            //        //OnPlanAborted(current);
            //        break;
            //    }
            //    else {
            //        sequence.Dequeue();
            //    }
            //}

            //if (goalReached)
            //    OnGoalReached();

            //currentPlan = null;
        }
    }

}
