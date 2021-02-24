using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompiler
{
    public class LetStatement : StatetmentBase
    {
        public string Variable { get; set; }
        public Expression Value { get; set; }

        public override string ToString()
        {
            return "let " + Variable + " = " + Value + ";";
        }

        public override void Parse(TokensStack sTokens)
        {
            //First, we remove the "Let" token
            Token tRet = sTokens.Pop();//Let
            //Now, we create the correct Expression type based on the top token in the stack
            Token tid1 = sTokens.Pop();
            if (!(tid1 is Identifier))
                throw new SyntaxErrorException("Expected function received: " + tid1, tid1);
            Variable = ((Identifier)tid1).Name;
            //We transfer responsibility of the parsing to the created expression
            Token tid2 = sTokens.Pop();
            if (!(tid2 is Symbol) || ((Symbol)tid2).Name != '=')
                throw new SyntaxErrorException("Expected function received: " + tid2, tid2);
            Expression tmp = Expression.Create(sTokens);
            tmp.Parse(sTokens);
            Value = tmp;
            //After the expression was parsed, we expect to see ;
            Token tEnd = sTokens.Pop();//;
        }

    }
}
