using System;
using System.Collections.Generic;

namespace SimpleCompiler
{
    public class FunctionCallExpression : Expression
    {
        public string FunctionName { get; private set; }
        public List<Expression> Args { get; private set; }

        public override void Parse(TokensStack sTokens)
        {
            Token FName = sTokens.Pop();
            if (!(FName is Identifier))
                throw new SyntaxErrorException("Expected function name, received " + FName, FName);
            FunctionName = ((Identifier)FName).Name;
            Token par = sTokens.Pop();
            if (!(par is Parentheses) || ((Parentheses)par).Name != '(')
                throw new SyntaxErrorException("Expected function name, received " + par, par);
            while (sTokens.Count > 0 && !(sTokens.Peek() is Parentheses))//)
            {
                Expression tmp = Expression.Create(sTokens);
                tmp.Parse(sTokens);
                Args.Add(tmp);
                //If there is a comma, then there is another argument
                if (sTokens.Count > 0 && sTokens.Peek() is Separator)//,
                    sTokens.Pop();
            }
            Token par1 = sTokens.Pop();
            if (!(par1 is Parentheses) || ((Parentheses)par).Name != ')')
                throw new SyntaxErrorException("Expected function name, received " + par1, par1);
        }

        public override string ToString()
        {
            string sFunction = FunctionName + "(";
            for (int i = 0; i < Args.Count - 1; i++)
                sFunction += Args[i] + ",";
            if (Args.Count > 0)
                sFunction += Args[Args.Count - 1];
            sFunction += ")";
            return sFunction;
        }
    }
}