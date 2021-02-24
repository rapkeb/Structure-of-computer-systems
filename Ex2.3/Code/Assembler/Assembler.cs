using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assembler
{
    public class Assembler
    {
        private const int WORD_SIZE = 16;

        private Dictionary<string, int[]> m_dControl, m_dJmp, m_dDest; //these dictionaries map command mnemonics to machine code - they are initialized at the bottom of the class

        //more data structures here (symbol map, ...)
        private Dictionary<string, int> symboleTable;
        private Dictionary<string, int> labels;
        private int place = 16;
        public Assembler()
        {
            InitCommandDictionaries();
        }

        //this method is called from the outside to run the assembler translation
        public void TranslateAssemblyFile(string sInputAssemblyFile, string sOutputMachineCodeFile)
        {
            //read the raw input, including comments, errors, ...
            StreamReader sr = new StreamReader(sInputAssemblyFile);
            List<string> lLines = new List<string>();
            while (!sr.EndOfStream)
            {
                lLines.Add(sr.ReadLine());
            }
            sr.Close();
            //translate to machine code
            List<string> lTranslated = TranslateAssemblyFile(lLines);
            //write the output to the machine code file
            StreamWriter sw = new StreamWriter(sOutputMachineCodeFile);
            foreach (string sLine in lTranslated)
                sw.WriteLine(sLine);
            sw.Close();
        }

        //translate assembly into machine code
        private List<string> TranslateAssemblyFile(List<string> lLines)
        {
            //implementation order:
            //first, implement "TranslateAssemblyToMachineCode", and check if the examples "Add", "MaxL" are translated correctly.
            //next, implement "CreateSymbolTable", and modify the method "TranslateAssemblyToMachineCode" so it will support symbols (translating symbols to numbers). check this on the examples that don't contain macros
            //the last thing you need to do, is to implement "ExpendMacro", and test it on the example: "SquareMacro.asm".
            //init data structures here 

            //expand the macros
            List<string> lAfterMacroExpansion = ExpendMacros(lLines);

            //first pass - create symbol table and remove lable lines
            CreateSymbolTable(lAfterMacroExpansion);

            //second pass - replace symbols with numbers, and translate to machine code
            List<string> lAfterTranslation = TranslateAssemblyToMachineCode(lAfterMacroExpansion);
            symboleTable.Clear();
            labels.Clear();
            place = 16;
            return lAfterTranslation;
        }


        //first pass - replace all macros with real assembly
        private List<string> ExpendMacros(List<string> lLines)
        {
            //You do not need to change this function, you only need to implement the "ExapndMacro" method (that gets a single line == string)
            List<string> lAfterExpansion = new List<string>();
            for (int i = 0; i < lLines.Count; i++)
            {
                //remove all redudant characters
                string sLine = CleanWhiteSpacesAndComments(lLines[i]);
                if (sLine == "")
                    continue;
                //if the line contains a macro, expand it, otherwise the line remains the same
                List<string> lExpanded = ExapndMacro(sLine);
                //we may get multiple lines from a macro expansion
                foreach (string sExpanded in lExpanded)
                {
                    lAfterExpansion.Add(sExpanded);
                }
            }
            return lAfterExpansion;
        }

        //expand a single macro line
        private List<string> ExapndMacro(string sLine)
        {
            List<string> lExpanded = new List<string>();

            if (IsCCommand(sLine))
            {
                string sDest, sCompute, sJmp;
                GetCommandParts(sLine, out sDest, out sCompute, out sJmp);
                //your code here - check for indirect addessing and for jmp shortcuts
                //read the word file to see all the macros you need to support
                if(sDest.Equals("") & sJmp.Equals(""))
                {
                    lExpanded.Add("@" + sCompute.Substring(0,sCompute.Length-2));
                    lExpanded.Add("M=M" + sCompute[sCompute.Length - 1] + "1");
                }
                else if (m_dDest.ContainsKey(sDest) & sJmp.Equals("") & !m_dControl.ContainsKey(sCompute))
                {
                    lExpanded.Add("@" + sCompute);
                    if (sDest.Equals("D"))
                    {
                        if (sCompute[0] > 0 & sCompute[0] < 10)
                            lExpanded.Add("D=A");
                        else
                        {
                            lExpanded.Add("D=M");
                        }
                }
                }
                else if(!m_dDest.ContainsKey(sDest) & sJmp.Equals("") & m_dControl.ContainsKey(sCompute))
                {
                    lExpanded.Add("@" + sDest);
                    lExpanded.Add("M=" + sCompute);
                }
                else if (!m_dDest.ContainsKey(sDest) & sJmp.Equals("") & !m_dControl.ContainsKey(sCompute))
                {
                    int i;
                    lExpanded.Add("@" + sCompute);
                    if (int.TryParse(sCompute,out i))
                    {
                        lExpanded.Add("D=A");
                        lExpanded.Add("@" + sDest);
                        lExpanded.Add("M=D");
                    }
                    else
                    {
                        lExpanded.Add("D=M");
                        lExpanded.Add("@" + sDest);
                        lExpanded.Add("M=D");
                    }
                }
                else if(sJmp != "")
                {
                    if (sJmp.Contains(":"))
                    {
                        lExpanded.Add("@" + sJmp.Substring(sJmp.IndexOf(':') + 1));
                        lExpanded.Add(sCompute + ";" + sJmp.Substring(0, sJmp.IndexOf(":")));
                    }
                    else
                    {
                        if (sCompute.Equals(";"))
                            lExpanded.Add("0;" + sJmp);
                    }
                }
                else if (m_dDest.ContainsKey(sDest) & sJmp.Equals("") & m_dControl.ContainsKey(sCompute))
                {
                }
            }
            if (lExpanded.Count == 0)
                lExpanded.Add(sLine);
            return lExpanded;
        }

        //second pass - record all symbols - labels and variables
        private void CreateSymbolTable(List<string> lLines)
        {
              InitSymbolTable();
            string sLine = "";
            int help = 0;
            for (int i = 0; i < lLines.Count; i++)
            {
                sLine = lLines[i];
                if (IsLabelLine(sLine))
                {
                    if (sLine.Length > 2)
                    {
                        char unique = char.Parse(sLine.Substring(1, 1));
                        if ((unique > 64 & unique < 91) | (unique > 96 & unique < 123))
                        {
                            //record label in symbol table
                            //do not add the label line to the result
                            if (!labels.ContainsKey(sLine.Substring(1, sLine.Length - 2)))
                            {
                                //symboleTable.Add(sLine.Substring(1, sLine.Length - 2), i);
                                labels.Add(sLine.Substring(1, sLine.Length - 2), i-help);
                                help++;
                            }
                            else
                            throw new FormatException("Label is already defined");
                        }
                        else
                            throw new FormatException("Illegal label");
                    }
                }
                else if (IsACommand(sLine))
                {
                    //may contain a variable - if so, record it to the symbol table (if it doesn't exist there yet...)
                    string tmp = sLine.Substring(1);
                    char unique = char.Parse(sLine.Substring(1, 1));
                    if (!symboleTable.ContainsKey(tmp) & ((unique > 64 & unique < 91) | (unique > 96 & unique < 123)))
                    {
                        while (symboleTable.ContainsValue(place))
                            place++;
                        symboleTable.Add(tmp, place);
                        place++;
                    }
                }
                else if (IsCCommand(sLine))
                {
                    //do nothing here
                }
                else
                    throw new FormatException("Cannot parse line " + i + ": " + lLines[i]);
            }

        }

        //third pass - translate lines into machine code, replacing symbols with numbers
        private List<string> TranslateAssemblyToMachineCode(List<string> lLines)
        {
            string sLine = "";
            List<string> lAfterPass = new List<string>();
            for (int i = 0; i < lLines.Count; i++)
            {
                sLine = lLines[i];
                if (IsACommand(sLine))
                {
                    string tmp;
                    //translate an A command into a sequence of bits
                    if (!labels.ContainsKey(sLine.Substring(1)) & !symboleTable.ContainsKey(sLine.Substring(1)))
                    {
                        tmp = sLine.Substring(1);
                        tmp = ToBinary(int.Parse(tmp));
                        lAfterPass.Add(tmp);
                    }
                    else
                    {
                        if (!labels.ContainsKey(sLine.Substring(1)))
                        {
                            int curr = symboleTable[sLine.Substring(1)];
                            tmp = ToBinary(curr);
                            lAfterPass.Add(tmp);
                        }
                        else
                        {
                            int curr = labels[sLine.Substring(1)];
                            tmp = ToBinary(curr);
                            lAfterPass.Add(tmp);
                        }
                    }
                }
                else if (IsCCommand(sLine))
                {
                    string sDest, sControl, sJmp;
                    GetCommandParts(sLine, out sDest, out sControl, out sJmp);
                    //translate an C command into a sequence of bits
                    //take a look at the dictionaries m_dControl, m_dJmp, and where they are initialized (InitCommandDictionaries), to understand how to you them here
                    string tmp = "111";
                    tmp = tmp + ToString(m_dControl[sControl]);
                    tmp = tmp + ToString(m_dDest[sDest]);
                    tmp = tmp + ToString(m_dJmp[sJmp]);
                    lAfterPass.Add(tmp);
                }
                else { }
                //throw new FormatException("Cannot parse line " + i + ": " + lLines[i]);
            }
            return lAfterPass;
        }

        //helper functions for translating numbers or bits into strings
        private string ToString(int[] aBits)
        {
            string sBinary = "";
            for (int i = 0; i < aBits.Length; i++)
                sBinary += aBits[i];
            return sBinary;
        }

        private string ToBinary(int x)
        {
            string sBinary = "";
            for (int i = 0; i < WORD_SIZE; i++)
            {
                sBinary = (x % 2) + sBinary;
                x = x / 2;
            }
            return sBinary;
        }


        //helper function for splitting the various fields of a C command
        private void GetCommandParts(string sLine, out string sDest, out string sControl, out string sJmp)
        {
            if (sLine.Contains('='))
            {
                int idx = sLine.IndexOf('=');
                sDest = sLine.Substring(0, idx);
                sLine = sLine.Substring(idx + 1);
            }
            else
                sDest = "";
            if (sLine.Contains(';'))
            {
                int idx = sLine.IndexOf(';');
                sControl = sLine.Substring(0, idx);
                sJmp = sLine.Substring(idx + 1);

            }
            else
            {
                sControl = sLine;
                sJmp = "";
            }
        }

        private bool IsCCommand(string sLine)
        {
            return !IsLabelLine(sLine) && sLine[0] != '@';
        }

        private bool IsACommand(string sLine)
        {
            return sLine[0] == '@';
        }

        private bool IsLabelLine(string sLine)
        {
            if (sLine.StartsWith("(") && sLine.EndsWith(")"))
                return true;
            return false;
        }

        private string CleanWhiteSpacesAndComments(string sDirty)
        {
            string sClean = "";
            for (int i = 0; i < sDirty.Length; i++)
            {
                char c = sDirty[i];
                if (c == '/' && i < sDirty.Length - 1 && sDirty[i + 1] == '/') // this is a comment
                    return sClean;
                if (c > ' ' && c <= '~')//ignore white spaces
                    sClean += c;
            }
            return sClean;
        }


        private void InitCommandDictionaries()
        {
            labels = new Dictionary<string, int>();

            m_dControl = new Dictionary<string, int[]>();

            m_dControl["0"] = new int[] { 0, 1, 0, 1, 0, 1, 0 };
            m_dControl["1"] = new int[] { 0, 1, 1, 1, 1, 1, 1 };
            m_dControl["-1"] = new int[] { 0, 1, 1, 1, 0, 1, 0 };
            m_dControl["D"] = new int[] { 0, 0, 0, 1, 1, 0, 0 };
            m_dControl["A"] = new int[] { 0, 1, 1, 0, 0, 0, 0 };
            m_dControl["!D"] = new int[] { 0, 0, 0, 1, 1, 0, 1 };
            m_dControl["!A"] = new int[] { 0, 1, 1, 0, 0, 0, 1 };
            m_dControl["-D"] = new int[] { 0, 0, 0, 1, 1, 1, 1 };
            m_dControl["-A"] = new int[] { 0, 1, 1, 0, 0, 1, 1 };
            m_dControl["D+1"] = new int[] { 0, 0, 1, 1, 1, 1, 1 };
            m_dControl["A+1"] = new int[] { 0, 1, 1, 0, 1, 1, 1 };
            m_dControl["D-1"] = new int[] { 0, 0, 0, 1, 1, 1, 0 };
            m_dControl["A-1"] = new int[] { 0, 1, 1, 0, 0, 1, 0 };
            m_dControl["D+A"] = new int[] { 0, 0, 0, 0, 0, 1, 0 };
            m_dControl["A+D"] = new int[] { 0, 0, 0, 0, 0, 1, 0 };
            m_dControl["D-A"] = new int[] { 0, 0, 1, 0, 0, 1, 1 };
            m_dControl["A-D"] = new int[] { 0, 0, 0, 0, 1, 1, 1 };
            m_dControl["D&A"] = new int[] { 0, 0, 0, 0, 0, 0, 0 };
            m_dControl["D|A"] = new int[] { 0, 0, 1, 0, 1, 0, 1 };

            m_dControl["A&D"] = new int[] { 0, 0, 0, 0, 0, 0, 0 };
            m_dControl["A|D"] = new int[] { 0, 0, 1, 0, 1, 0, 1 };

            m_dControl["M"] = new int[] { 1, 1, 1, 0, 0, 0, 0 };
            m_dControl["!M"] = new int[] { 1, 1, 1, 0, 0, 0, 1 };
            m_dControl["-M"] = new int[] { 1, 1, 1, 0, 0, 1, 1 };
            m_dControl["M+1"] = new int[] { 1, 1, 1, 0, 1, 1, 1 };
            m_dControl["M-1"] = new int[] { 1, 1, 1, 0, 0, 1, 0 };
            m_dControl["D+M"] = new int[] { 1, 0, 0, 0, 0, 1, 0 };
            m_dControl["M+D"] = new int[] { 1, 0, 0, 0, 0, 1, 0 };
            m_dControl["D-M"] = new int[] { 1, 0, 1, 0, 0, 1, 1 };
            m_dControl["M-D"] = new int[] { 1, 0, 0, 0, 1, 1, 1 };
            m_dControl["D&M"] = new int[] { 1, 0, 0, 0, 0, 0, 0 };
            m_dControl["D|M"] = new int[] { 1, 0, 1, 0, 1, 0, 1 };

            m_dControl["M&D"] = new int[] { 1, 0, 0, 0, 0, 0, 0 };
            m_dControl["M|D"] = new int[] { 1, 0, 1, 0, 1, 0, 1 };

            m_dJmp = new Dictionary<string, int[]>();

            m_dJmp[""] = new int[] { 0, 0, 0 };
            m_dJmp["JGT"] = new int[] { 0, 0, 1 };
            m_dJmp["JEQ"] = new int[] { 0, 1, 0 };
            m_dJmp["JGE"] = new int[] { 0, 1, 1 };
            m_dJmp["JLT"] = new int[] { 1, 0, 0 };
            m_dJmp["JNE"] = new int[] { 1, 0, 1 };
            m_dJmp["JLE"] = new int[] { 1, 1, 0 };
            m_dJmp["JMP"] = new int[] { 1, 1, 1 };

            m_dDest = new Dictionary<string, int[]>();

            m_dDest[""] = new int[] { 0, 0, 0 };
            m_dDest["M"] = new int[] { 0, 0, 1 };
            m_dDest["A"] = new int[] { 1, 0, 0 };
            m_dDest["D"] = new int[] { 0, 1, 0 };
            m_dDest["AD"] = new int[] { 1, 1, 0 };
            m_dDest["AMD"] = new int[] { 1, 1, 1 };
            m_dDest["MD"] = new int[] { 0, 1, 1 };
            m_dDest["AM"] = new int[] { 1, 0, 1 };

        }

         private void InitSymbolTable()
         {
             symboleTable = new Dictionary<string, int>();

             symboleTable.Add("R0", 0);
             symboleTable.Add("R1", 1);
             symboleTable.Add("R2", 2);
             symboleTable.Add("R3", 3);
             symboleTable.Add("R4", 4);
             symboleTable.Add("R5", 5);
             symboleTable.Add("R6", 6);
             symboleTable.Add("R7", 7);
             symboleTable.Add("R8", 8);
             symboleTable.Add("R9", 9);
             symboleTable.Add("R10", 10);
             symboleTable.Add("R11", 11);
             symboleTable.Add("R12", 12);
             symboleTable.Add("R13", 13);
             symboleTable.Add("R14", 14);
             symboleTable.Add("R15", 15);
             symboleTable.Add("SCREEN", 16384);
             symboleTable.Add("KBD", 24576);
         }
    }
}
