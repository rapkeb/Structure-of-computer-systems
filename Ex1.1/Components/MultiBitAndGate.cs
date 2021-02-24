﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components
{
    //Multibit gates take as input k bits, and compute a function over all bits - z=f(x_0,x_1,...,x_k)
    class MultiBitAndGate : MultiBitGate
    {
        //your code here

        public MultiBitAndGate(int iInputCount)
            : base(iInputCount)
        {
            //your code here

        }


        public override bool TestGate()
        {
            throw new NotImplementedException();
        }
    }
}
