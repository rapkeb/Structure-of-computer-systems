using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompiler
{
    public class Compiler
    {

        private Dictionary<string, int> m_dSymbolTable;
        private int m_cLocals;
        public static char[] Delimiters = { ' ', '\t', '\n', '(', ')', '[', ']', '{', '}', ',', ';', '*', '+', '-', '<', '>', '&', '=', '|', '~' };

        public Compiler()
        {
            m_dSymbolTable = new Dictionary<string, int>();
            m_cLocals = 0;

        }

        public List<string> Compile(string sInputFile)
        {
            List<string> lCodeLines = ReadFile(sInputFile);
            List<Token> lTokens = Tokenize(lCodeLines);
            TokensStack sTokens = new TokensStack();
            for (int i = lTokens.Count - 1; i >= 0; i--)
                sTokens.Push(lTokens[i]);
            JackProgram program = Parse(sTokens);
            return null;
        }

        private JackProgram Parse(TokensStack sTokens)
        {
            JackProgram program = new JackProgram();
            program.Parse(sTokens);
            return program;
        }

        public List<string> Compile(List<string> lLines)
        {

            List<string> lCompiledCode = new List<string>();
            foreach (string sExpression in lLines)
            {
                List<string> lAssembly = Compile(sExpression);
                lCompiledCode.Add("// " + sExpression);
                lCompiledCode.AddRange(lAssembly);
            }
            return lCompiledCode;
        }



        public List<string> ReadFile(string sFileName)
        {
            StreamReader sr = new StreamReader(sFileName);
            List<string> lCodeLines = new List<string>();
            while (!sr.EndOfStream)
            {
                lCodeLines.Add(sr.ReadLine());
            }
            sr.Close();
            return lCodeLines;
        }


        public List<Token> Tokenize(List<string> lCodeLines)
        {
            List<Token> lTokens = new List<Token>();
            int numOfLine = 0;
            foreach (string line in lCodeLines)
            {
                int placeOfToken = 0;
                List<string> tokens = Split(line, Delimiters);
                if (line.Contains("//"))
                {
                }
                else
                {
                    int help;
                    foreach (string token in tokens)
                    {
                        if (int.TryParse(token, out help))
                        {
                            Number tmp = new Number(token, numOfLine, placeOfToken);
                            placeOfToken = placeOfToken + token.Length;
                            lTokens.Add(tmp);
                        }
                        else if (token.Length == 1 & token != " " & token != "\t")
                        {
                            if (Token.Operators.Contains(token[0]))
                            {
                                Operator tmp = new Operator(token[0], numOfLine, placeOfToken);
                                placeOfToken++;
                                lTokens.Add(tmp);
                            }
                            else if (Token.Parentheses.Contains(token[0]))
                            {
                                Parentheses tmp = new Parentheses(token[0], numOfLine, placeOfToken);
                                placeOfToken = placeOfToken + token.Length;
                                lTokens.Add(tmp);
                            }
                            else if (Token.Separators.Contains(token[0]))
                            {
                                Separator tmp = new Separator(token[0], numOfLine, placeOfToken);
                                placeOfToken = placeOfToken + token.Length;
                                lTokens.Add(tmp);
                            }
                            else if (!char.IsDigit(token[0]) & token[0] != '#')
                            {
                                Identifier tmp = new Identifier(token, numOfLine, placeOfToken);
                                placeOfToken = placeOfToken + token.Length;
                                lTokens.Add(tmp);
                            }
                            else
                            {
                                Token tmp = new Token();
                                tmp.Line = numOfLine;
                                tmp.Position = placeOfToken;
                                placeOfToken = placeOfToken + token.Length;
                                lTokens.Add(tmp);
                                throw new SyntaxErrorException("problem", tmp);
                            }
                        }
                        else if (token.Equals(" "))
                        {
                            placeOfToken++;
                        }
                        else if (token.Equals("\t"))
                        {
                            placeOfToken++;
                        }
                        else
                        {
                            if (Token.Statements.Contains(token))
                            {
                                Statement tmp = new Statement(token, numOfLine, placeOfToken);
                                placeOfToken = placeOfToken + token.Length;
                                lTokens.Add(tmp);
                            }
                            else if (Token.VarTypes.Contains(token))
                            {
                                VarType tmp = new VarType(token, numOfLine, placeOfToken);
                                placeOfToken = placeOfToken + token.Length;
                                lTokens.Add(tmp);
                            }
                            else if (Token.Constants.Contains(token))
                            {
                                Constant tmp = new Constant(token, numOfLine, placeOfToken);
                                placeOfToken = placeOfToken + token.Length;
                                lTokens.Add(tmp);
                            }
                            else
                            {
                                /* bool isValid = false;  
                                 for(int i = 1; i<token.Length & !isValid; i++)
                                 {
                                     if (char.IsLetter(token[i]))
                                         isValid = true;
                                 }*/
                                if (char.IsLetter(token[0]))
                                {
                                    Identifier tmp = new Identifier(token, numOfLine, placeOfToken);
                                    placeOfToken = placeOfToken + token.Length;
                                    lTokens.Add(tmp);
                                }
                                else
                                {
                                    Token tmp = new Token();
                                    tmp.Line = numOfLine;
                                    tmp.Position = placeOfToken;
                                    placeOfToken = placeOfToken + token.Length;
                                    lTokens.Add(tmp);
                                    throw new SyntaxErrorException("problem", tmp);
                                }
                            }
                        }
                    }
                }
                numOfLine++;
            }
            //throw new Exception(string.Join("\n", lCodeLines.ToArray()));
            return lTokens;
        }
        private List<string> Split(string s, char[] aDelimiters)
        {
            List<string> lTokens = new List<string>();
            while (s.Length > 0)
            {
                string sToken = "";
                int i = 0;
                for (i = 0; i < s.Length; i++)
                {
                    if (aDelimiters.Contains(s[i]))
                    {
                        if (sToken.Length > 0)
                            lTokens.Add(sToken);
                        lTokens.Add(s[i] + "");
                        break;
                    }
                    else
                        sToken += s[i];
                }
                if (i == s.Length)
                {
                    lTokens.Add(sToken);
                    s = "";
                }
                else
                    s = s.Substring(i + 1);
            }
            return lTokens;
        }
    }
}
