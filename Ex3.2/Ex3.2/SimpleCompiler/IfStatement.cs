using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompiler
{
    public class IfStatement : StatetmentBase
    {
        public Expression Term { get; private set; }
        public List<StatetmentBase> DoIfTrue { get; private set; }
        public List<StatetmentBase> DoIfFalse { get; private set; }

        public override void Parse(TokensStack sTokens)
        {
            Token tWhile = sTokens.Pop();
            if (!(tWhile is Statement) || ((Statement)tWhile).Name != "if")
                throw new SyntaxErrorException("Expected function received: " + tWhile, tWhile);
            Token tPar = sTokens.Pop();
            if (!(tPar is Parentheses) || ((Parentheses)tPar).Name != '(')
                throw new SyntaxErrorException("Expected var type, received " + tPar, tPar);
            while (sTokens.Count > 0 && !(sTokens.Peek() is Parentheses))
            {
                Expression local = Expression.Create(sTokens);
                //We call the Parse method of the VarDeclaration, which is responsible to parsing the elements of the variable declaration
                local.Parse(sTokens);
                Term = local;
            }
            Token tPar1 = sTokens.Pop();
            if (!(tPar1 is Parentheses) || ((Parentheses)tPar1).Name != ')')
                throw new SyntaxErrorException("Expected function received: " + tPar1, tPar1);
            Token tPar2 = sTokens.Pop();
            if (!(tPar2 is Parentheses) || ((Parentheses)tPar1).Name != '{')
                throw new SyntaxErrorException("Expected function received: " + tPar2, tPar2);
            while (sTokens.Count > 0 && !(sTokens.Peek() is Parentheses))
            {
                StatetmentBase state = StatetmentBase.Create(sTokens.Peek());
                //We call the Parse method of the VarDeclaration, which is responsible to parsing the elements of the variable declaration
                state.Parse(sTokens);
                DoIfTrue.Add(state);
            }
            Token tPar3 = sTokens.Pop();
            if (!(tPar3 is Parentheses) || ((Parentheses)tPar1).Name != '}')
                throw new SyntaxErrorException("Expected function received: " + tPar3, tPar3);
            if (sTokens.Count > 0 && sTokens.Peek() is Statement && ((Statement)sTokens.Pop()).Name.Equals("else"))
            {
                Token tPar4 = sTokens.Pop();
                if (!(tPar4 is Parentheses) || ((Parentheses)tPar4).Name != '{')
                    throw new SyntaxErrorException("Expected function received: " + tPar4, tPar4);
                while (sTokens.Count > 0 && !(sTokens.Peek() is Parentheses))
                {
                    StatetmentBase state = StatetmentBase.Create(sTokens.Peek());
                    //We call the Parse method of the VarDeclaration, which is responsible to parsing the elements of the variable declaration
                    state.Parse(sTokens);
                    DoIfFalse.Add(state);
                }
                Token tPar5 = sTokens.Pop();
                if (!(tPar5 is Parentheses) || ((Parentheses)tPar1).Name != '}')
                    throw new SyntaxErrorException("Expected function received: " + tPar5, tPar5);
            }
        }

        public override string ToString()
        {
            string sIf = "if(" + Term + "){\n";
            foreach (StatetmentBase s in DoIfTrue)
                sIf += "\t\t\t" + s + "\n";
            sIf += "\t\t}";
            if (DoIfFalse.Count > 0)
            {
                sIf += "else{";
                foreach (StatetmentBase s in DoIfFalse)
                    sIf += "\t\t\t" + s + "\n";
                sIf += "\t\t}";
            }
            return sIf;
        }

    }
}
