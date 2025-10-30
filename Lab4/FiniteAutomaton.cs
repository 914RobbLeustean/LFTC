using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    internal class FiniteAutomaton
    {
        private readonly String elemSeparator = ";";
        private Boolean isDeterministic;
        private String initialState;
        private List<String> states;
        private List<String> alphabet;
        private List<String> finalStates;
        private readonly Dictionary<KeyValuePair<object, object>, HashSet<String>> transitions;

        public FiniteAutomaton(string filePath)
        {
            // transitions holds a key-value pair, where the key is made of a key-value pair, containing the first state
            // and the symbol used in the transition;
            // the value is a set where we save the state we're trying to transition to;
            // ex: { (A, b) , { B } } => we go from state A through symbol b to state B;
            transitions = new Dictionary<KeyValuePair<object, object>, HashSet<string>>();
            ReadFromFile(filePath);
        }

        public void ReadFromFile(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    // the first 4 lines of the .txt has the base for how to stages will work;
                    states = new List<string>(reader.ReadLine().Split(new string[] { elemSeparator }, StringSplitOptions.None));
                    initialState = reader.ReadLine();
                    alphabet = new List<string>(reader.ReadLine().Split(new string[] { elemSeparator }, StringSplitOptions.None));
                    finalStates = new List<string>(reader.ReadLine().Split(new string[] { elemSeparator }, StringSplitOptions.None));

                    string transitionLine;
                    while ((transitionLine = reader.ReadLine()) != null)
                    {
                        var transitionComponents = transitionLine.Split(' ');
                        // here, we create the transition by checking if the outer symbols are valid states we can transition between
                        // and the middle symbol is a valid symbol that can make the transition;
                        if (states.Contains(transitionComponents[0]) && states.Contains(transitionComponents[2]) && alphabet.Contains(transitionComponents[1]))
                        {
                            // non-deterministic transitions allowed here; (multiple steps in between states)
                            var transitionStates = new KeyValuePair<object, object>(transitionComponents[0], transitionComponents[1]);

                            if (!transitions.ContainsKey(transitionStates))
                            {
                                var transitionStatesSet = new HashSet<string> { transitionComponents[2] };
                                transitions[transitionStates] = transitionStatesSet;
                            }
                            else
                            {
                                transitions[transitionStates].Add(transitionComponents[2]);
                            }
                        }

                    }
                    // using this function, we check if the transitions have at most one step to make;
                    isDeterministic = CheckIfDeterministic();
                }
            } 
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool CheckIfDeterministic()
        {
            // to check if our transitions are deterministic, we take the values of the set
            // if there's more than one, that means a state is trying to access multiple states using the same symbol
            // so, more than one state;
            return transitions.Values.All(list => list.Count <= 1);
        }

        // getters
        public List<string> GetStates()
        {
            return states;
        }

        public string GetInitialState()
        {
            return initialState;
        }

        public List<string> GetAlphabet()
        {
            return alphabet;
        }

        public List<string> GetFinalStates()
        {
            return finalStates;
        }

        public Dictionary<KeyValuePair<object, object>, HashSet<string>> GetTransitions()
        {
            return transitions;
        }

        // printing all transactions we have
        public string WriteTransitions()
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Transitions: ");
            foreach (var transition in transitions)
            {
                builder.AppendLine($"<{transition.Key.Key},{transition.Key.Value}> -> {string.Join(",", transition.Value)}");
            }
            return builder.ToString();
        }

        // checks the input symbols in the sequence and if they are in a valid order;
        public bool CheckSequence(string sequence)
        {
            // if the initial state is the same as the final one, the sequence can be empty
            // otherwise, invalid sequence
            if (sequence.Length == 0)
                return finalStates.Contains(initialState);

            // this iterates through all characters in the sequence and creates the transition as before;
            // for each key (first state and input symbol), we check if it exists in our transitions;
            // if it's a valid key, we then take the first element from the value (the set) and re-use it
            // as initial state in the next transition;
            // if we can track the transition like this from the initial state to the last, it means
            // the sequence is valid;
            string state = initialState;
            foreach (char c in sequence)
            {
                var key = new KeyValuePair<object, object>(state, c.ToString());
                if (transitions.ContainsKey(key))
                    state = transitions[key].First();
                else
                    return false;
            }

            return finalStates.Contains(state);
        }

    }
}
