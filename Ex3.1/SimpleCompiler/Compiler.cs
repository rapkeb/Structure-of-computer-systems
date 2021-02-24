using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleCompiler
{
    class Compiler
    {
        public static char[] Delimiters = {' ', '\t', '\n', '(', ')', '[', ']', '{', '}' , ',', ';' , '*', '+', '-', '<', '>', '&', '=', '|', '~' };

        public Compiler()
        {
        }

        //reads a file into a list of strings, each string represents one line of code
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



        //Computes the next token in the string s, from the begining of s until a delimiter has been reached. 
        //Returns the string without the token.
        private string Next(string s, char[] aDelimiters, out string sToken, out int cChars)
        {
            cChars = 1;
            sToken = s[0] + "";
            if (aDelimiters.Contains(s[0]))
                return s.Substring(1);
            int i = 0;
            for (i = 1; i < s.Length; i++)
            {
                if (aDelimiters.Contains(s[i]))
                    return s.Substring(i);
                else
                    sToken += s[i];
                cChars++;
            }
            return null;
        }

        //Splits a string into a list of tokens, separated by delimiters
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

        //This is the main method for the Tokenizing assignment. 
        //Takes a list of code lines, and returns a list of tokens.
        //For each token you must identify its type, and instantiate the correct subclass accordingly.
        //You need to identify the token position in the file (line, index within the line).
        //You also need to identify errors, in this assignement - illegal identifier names.
        public List<Token> Tokenize(List<string> lCodeLines)
        {
            List<Token> lTokens = new List<Token>();
            int numOfLine = 0;
            foreach(string line in lCodeLines)
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
                                Operator tmp = new Operator(token[0],numOfLine,placeOfToken);
                                placeOfToken++;
                                lTokens.Add(tmp);
                            }
                            else if(Token.Parentheses.Contains(token[0]))
                            {
                                Parentheses tmp = new Parentheses(token[0], numOfLine, placeOfToken);
                                placeOfToken = placeOfToken + token.Length;
                                lTokens.Add(tmp);
                            }
                            else if(Token.Separators.Contains(token[0]))
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
                            if(Token.Statements.Contains(token))
                            {
                                Statement tmp = new Statement(token,numOfLine,placeOfToken);
                                placeOfToken = placeOfToken + token.Length;
                                lTokens.Add(tmp);
                            }
                            else if(Token.VarTypes.Contains(token))
                            {
                                VarType tmp = new VarType(token, numOfLine, placeOfToken);
                                placeOfToken = placeOfToken + token.Length;
                                lTokens.Add(tmp);
                            }
                           else if (Token.Constants.Contains(token))
                            {
                                Constant tmp = new Constant(token,numOfLine,placeOfToken);
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
                                if(char.IsLetter(token[0]))
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

    }
}

