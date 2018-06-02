using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colosseum.Match;

namespace Colosseum.Prototypes.AI {
    public class AIPlanner : SingletonInstance<AIPlanner> {
        /// <summary>
        /// Creates a plan for agent
        /// </summary>
        public static bool Plan(AIAgent agent, List<AIAction> availibleActions, Queue<AIAction> outActionSequence) {
            List<AIAction> usableActions = new List<AIAction>(availibleActions);
            //foreach (var action in usableActions)
            //    action.ResetState();

            //usableActions.RemoveAll(action => !action.CheckProceduralPrecondition(agent));
            //if (usableActions.Count == 0)
            //    return false;

            Node startNode = new Node(null, 0, agent.worldState, null);
            List<Node> leaves = new List<Node>();
            bool planBuilt = Instance.CreateGraph(startNode, leaves, usableActions, agent.goalState);
            if (planBuilt) {
                outActionSequence.Clear();
                Queue<AIAction> sequence = Instance.CreateActionSequence(leaves);
                foreach (var action in sequence)
                    outActionSequence.Enqueue(action);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Create the action sequence based on generated graph node leaves
        /// </summary>
        private Queue<AIAction> CreateActionSequence(List<Node> leaves) {
            Queue<AIAction> queue = new Queue<AIAction>();

            if (leaves.Count == 0)
                return queue;

            Node cheapestLeaf = leaves[0];
            for (int i = 0; i < leaves.Count; i++) {
                if (cheapestLeaf.runningCost > leaves[i].runningCost) {
                    cheapestLeaf = leaves[i];
                }
            }

            Node node = cheapestLeaf;
            List<AIAction> actions = new List<AIAction>();

            while (node != null) {
                if (node.action != null)
                    actions.Insert(0, node.action);

                node = node.parent;
            }

            queue = new Queue<AIAction>(actions);
            return queue;
        }

        /// <summary>
        /// Test whether state sequences are equal.
        /// </summary>
        /// <returns></returns>
        private bool StateSequenceEquals(Dictionary<string, object> stateA, Dictionary<string, object> stateB) {
            bool allMatch = true;
            foreach (var v1 in stateA) {
                bool match = false;
                foreach (var v2 in stateB) {
                    if (v2.Equals(v1)) {
                        match = true;
                        break;
                    }
                }
                if (!match)
                    allMatch = false;
            }

            return allMatch;
        }

        /// <summary>
        /// Populate the state with new content
        /// </summary>
        private Dictionary<string, object> PopulateState(Dictionary<string, object> origState, Dictionary<string, object> newContent) {
            Dictionary<string, object> result = new Dictionary<string, object>(origState);
            foreach (var kvp in newContent) {
                result[kvp.Key] = kvp.Value;
            }

            return result;
        }

        /// <summary>
        /// Create the actual node graph based on usable actions and given goal
        /// </summary>
        private bool CreateGraph(Node parent, List<Node> outLeaves, List<AIAction> usableActions, Dictionary<string, object> goal) {
            bool foundOne = false;
            foreach (var action in usableActions) {
                if (StateSequenceEquals(action.preconditions, parent.state)) {
                    Dictionary<string, object> newState = PopulateState(parent.state, action.effects);
                    Node node = new Node(parent, parent.runningCost + action.cost, newState, action);
                    if (StateSequenceEquals(goal, newState)) {
                        outLeaves.Add(node);
                        foundOne = true;
                    }
                    else {
                        List<AIAction> actionSubset = usableActions.FindAll(a => a != action);
                        bool found = CreateGraph(node, outLeaves, actionSubset, goal);
                        if (found)
                            foundOne = true;
                    }
                }
            }

            return foundOne;
        }

        /// <summary>
        /// Represents the graph node
        /// </summary>
        private class Node {
            public Node parent; // the parent of the node
            public float runningCost; // the node running cost
            public Dictionary<string, object> state; // the state of the node
            public AIAction action; // the action that belongs to the node

            public Node(Node parent, float cost, Dictionary<string, object> state, AIAction action) {
                this.parent = parent;
                this.runningCost = cost;
                this.state = state;
                this.action = action;
            }
        }
    }
}