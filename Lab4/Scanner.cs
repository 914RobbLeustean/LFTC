using Lab3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lab3
{
    internal class Scanner
    {
        private SymbolTable_hashTable constantTable;
        private SymbolTable_hashTable identifierTable;
        private ProgramInternalForm pif;
        private bool lexicallyCorrect;
        private string filePath;
        private bool inQuotations;
        private int paranthesisLayer;
        private int bracketLayer;

        private readonly FiniteAutomaton identifierFA;
        private readonly FiniteAutomaton constantFA;

        private readonly List<string> operators = new List<string>
        {
            "+", "-", "*", "/", "%", "<=", ">=", "==", "!=", "<", ">", "=", "<-", "||", "&&"
        };

        private readonly List<string> separators = new List<string>
        {
            "{", "}", "(", ")", "[", "]", ":", ";", " ", ",", "\t", "\n", "'", "\""
        };

        private readonly List<string> reservedWords = new List<string>
        {
            "read", "write", "if", "else", "for", "while", "int", "string", "char", "return",
            "bool", "real", "declare", "loop", "program", "give", "array", "function", "main",
            "condition", "increment", "call", "with", "noreceive", "receive", "length", "void", "true", "false"
        };

        private readonly List<string> types = new List<string>
        {
            "int", "real", "char", "string", "bool", "void"
        };

        public Scanner(string file)
        {
            this.constantTable = new SymbolTable_hashTable(20);
            this.identifierTable = new SymbolTable_hashTable(20);
            this.pif = new ProgramInternalForm();
            this.lexicallyCorrect = true;
            this.filePath = file;
            this.constantFA = new FiniteAutomaton("FAconst.txt");
            this.identifierFA = new FiniteAutomaton("FAid.txt");
        }

        private string ReadFile()
        {
            StringBuilder fileContent = new StringBuilder();

            try
            {
                using (StreamReader reader = new StreamReader(this.filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        fileContent.AppendLine(line);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found!");
                throw;
            }

            return fileContent.ToString().Replace("\t", "");
        }
        private bool IsArrayOfType(string token)
        {
            foreach (var type in types)
            {
                if (token == $"<{type}>")
                {
                    return true;
                }
            }
            return false;
        }
        private List<string> CreateListOfTokens()
        {
            try
            {
                string content = this.ReadFile();

                // Update the pattern to also match sequences that are not whitespace and are not in separators
                string pattern = $@"\w+|{string.Join("|", separators.Select(Regex.Escape))}|[^\s]+";

                var tokens = Regex.Matches(content, pattern)
                                  .Cast<Match>()
                                  .Select(m => m.Value.Trim())
                                  .Where(t => !string.IsNullOrEmpty(t))
                                  .ToList();

                return tokens;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }

        private int Detect(string token)
        {
            // 1 identifier
            // 2 const
            // 3 separator
            // 4 operator
            // 5 reservedWord

            if (reservedWords.Contains(token) || IsArrayOfType(token))
            {
                return 5;
            }
            if (operators.Contains(token))
            {
                return 4;
            }
            if (separators.Contains(token))
            {
                return 3;
            }
            //if (Regex.IsMatch(token, @"^[-+]?(?=\d|\d+\.\d)(\d+(\.\d+)?|\.\d+)$") || token == "true" || token == "false") // Constant pattern
            //{
            //    return 2;
            //}
            //if (Regex.IsMatch(token, @"^([a-zA-Z]|_)[a-zA-Z_0-9]*$")) // Identifier pattern
            //{
            //    return 1;
            //}
            if (constantFA.CheckSequence(token))
            {
                return 2;
            }
            if (identifierFA.CheckSequence(token))
            {
                return 1;
            }
            return 0;
        }

        public void Scan()
        {
            List<string> tokens = CreateListOfTokens();
            this.paranthesisLayer = 0;
            this.bracketLayer = 0;
            this.inQuotations = false;
            string quotationsString = "";
            int column = -1;

            for(int i = 0; i < tokens.Count;i++)
            {
                column++;
                string token = tokens[i];
                int type = Detect(token);
                switch (type)
                {
                    case 1: //identifier
                        if (this.inQuotations)
                        {
                            quotationsString += token + " ";
                            break;
                        }

                        if (!this.identifierTable.existKey(token))
                        {
                            if (this.types.Contains(tokens[i - 1]) || IsArrayOfType(tokens[i-1]))
                                this.identifierTable.addId(token);
                            else
                            {
                                this.lexicallyCorrect = false;
                                Console.WriteLine("Lexical error on column: " + column + " token: " + token);
                                break; 
                            }
                        }
                            
                        this.pif.addElem("ID", this.identifierTable.getFullPosition(token));
                        break;
                    case 2: //constant
                        if (this.inQuotations)
                        {
                            quotationsString += token + " ";
                            break;
                        }
                        if (!this.constantTable.existKey(token))
                            this.constantTable.addId(token);
                        this.pif.addElem("CONST", this.constantTable.getFullPosition(token));
                        break;
                    case 3: //separator

                        if(token == "\"")
                        {
                            if(inQuotations)
                            {
                                if (!this.constantTable.existKey(quotationsString))
                                    this.constantTable.addId(quotationsString);
                                this.pif.addElem("CONST", this.constantTable.getFullPosition(quotationsString));
                                inQuotations = false;

                            }
                            else
                            {
                                quotationsString = "";
                                inQuotations = true;
                            }
                        }
                        else
                        if(token == "(")
                        {
                            this.paranthesisLayer++;
                        }
                        else
                        if(token == ")")
                        {
                            this.paranthesisLayer--;
                        }
                        else
                        if(token == "{")
                        {
                            this.bracketLayer++;
                        }
                        else
                        if(token == "}")
                        {
                            this.bracketLayer--;
                        }
                        this.pif.addElem("separator: " + token, new PositionInHashTable(0, 0));
                        break;
                    case 4: //operator
                        if (this.inQuotations)
                        {
                            quotationsString += token + " ";
                            break;
                        }

                        this.pif.addElem("operator: " + token, new PositionInHashTable(0, 0));
                        break;
                    case 5: //reserved words
                        if (this.inQuotations)
                        {
                            quotationsString += token + " ";
                            break;
                        }

                        this.pif.addElem("reservedWord: " + token, new PositionInHashTable(0, 0));
                        break;
                    default:
                        if (this.inQuotations)
                        {
                            quotationsString += token + " ";
                            break;
                        }
                        Console.WriteLine($"Something went wrong with token: {token}");
                        this.lexicallyCorrect = false;
                        break;

                }
            }

            if(this.lexicallyCorrect && this.paranthesisLayer == 0 && this.bracketLayer == 0 && !this.inQuotations)
            {
                Console.WriteLine("Lexically correct");
            }
            else
            {
                if (this.paranthesisLayer != 0)
                {
                    Console.WriteLine("Error: Unclosed Paranthesis.");

                }
                if (this.bracketLayer != 0)
                {
                    Console.WriteLine("Error: Unclosed Brackets.");
                }
                if (this.inQuotations)
                {
                    Console.WriteLine("Error: Unclosed Quotes.");
                }
                if (!this.lexicallyCorrect)
                {
                    Console.WriteLine("The program had lexical errors.");
                }
            }
        }

        public ProgramInternalForm getPif()
        {
            return this.pif;
        }

        public SymbolTable_hashTable getIdentifierTable()
        {
            return this.identifierTable;
        }

        public SymbolTable_hashTable getConstantTable()
        {
            return this.constantTable;
        }

    }
}
