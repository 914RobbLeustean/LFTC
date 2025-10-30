
using Lab3;
using System;

namespace Lab3
{
    public static class Program
    {
        private static void PrintToFile(string filePath, object obj)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine(obj);
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
            }
        }
        private static void Run(string filePath)
        {
            Scanner scanner = new Scanner(filePath);
            scanner.Scan();
            PrintToFile(filePath.Replace(".txt", "IT.txt"), scanner.getIdentifierTable());
            PrintToFile(filePath.Replace(".txt", "CT.txt"), scanner.getConstantTable());
            PrintToFile(filePath.Replace(".txt", "PIF.txt"), scanner.getPif());
        }
        private static void PrintMenu()
        {
            Console.WriteLine("1. Print states, alphabet, initial state, final state, transitions.");
            Console.WriteLine("2. Print if it's deterministic.");
            Console.WriteLine("3. Check if sequence is accepted by DFA.");
        }
        private static void OptionsForDFA()
        {
            FiniteAutomaton finiteAutomaton = new FiniteAutomaton("FA.txt");

            Console.WriteLine("FA read from file.");
            PrintMenu();
            Console.WriteLine("Your option: ");
            int option = Convert.ToInt32(Console.ReadLine());

            while (option != 0)
            {
                switch (option)
                {
                    case 1:
                        Console.WriteLine("States: ");
                        Console.WriteLine(string.Join(" ", finiteAutomaton.GetStates()));

                        Console.WriteLine("Alphabet: ");
                        Console.WriteLine(string.Join(" ", finiteAutomaton.GetAlphabet()));

                        Console.WriteLine("Initial state: ");
                        Console.WriteLine(finiteAutomaton.GetInitialState());

                        Console.WriteLine("Final states: ");
                        Console.WriteLine(string.Join(" ", finiteAutomaton.GetFinalStates()));

                        Console.WriteLine(finiteAutomaton.WriteTransitions());

                        break;

                    case 2:
                        Console.WriteLine("Is deterministic?");
                        Console.WriteLine(finiteAutomaton.CheckIfDeterministic());
                        break;

                    case 3:
                        Console.WriteLine("Enter your sequence: ");
                        string sequence = Console.ReadLine();

                        if (finiteAutomaton.CheckSequence(sequence))
                            Console.WriteLine("Sequence is valid");
                        else
                            Console.WriteLine("Invalid sequence");
                        break;

                    default:
                        Console.WriteLine("Invalid command!");
                        break;
                }
                Console.WriteLine();
                PrintMenu();
                Console.WriteLine("Your option: ");
                option = Convert.ToInt32(Console.ReadLine());
            }
        }


        public static void RunScanner()
        {
            Run("p1.txt");
            Run("p2.txt");
            Run("p3.txt");
            Run("p1err.txt");
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("1. FA");
            Console.WriteLine("2. Scanner");
            Console.WriteLine("Your option: ");
            int option = Convert.ToInt32(Console.ReadLine());

            switch (option)
            {
                case 1:
                    OptionsForDFA();
                    break;
                case 2:
                    RunScanner();
                    break;

                default:
                    Console.WriteLine("Invalid command!");
                    break;
            }

        }
    }
}
