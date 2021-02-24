using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompiler
{
    public class BinaryOperationExpression : Expression
    {
        public string Operator { get;  set; }
        public Expression Operand1 { get;  set; }
        public Expression Operand2 { get;  set; }

        public override string ToString()
        {
            return "(" + Operand1 + " " + Operator + " " + Operand2 + ")";
        }

        public override void Parse(TokensStack sTokens)
        {
            Expression tmp = Expression.Create(sTokens);
            tmp.Parse(sTokens);
            Operand1 = tmp;
            Token tWhile = sTokens.Pop();
            if (!(tWhile is Operator))
                throw new SyntaxErrorException("Expected function received: " + tWhile, tWhile);
            // Operator = char.Parse(((Operator)tWhile).Name);
            Expression tmp1 = Expression.Create(sTokens);
            tmp.Parse(sTokens);
            Operand2 = tmp1;

        }
    }
}
