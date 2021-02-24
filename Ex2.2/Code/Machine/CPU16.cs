using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleComponents;

namespace Machine
{
    public class CPU16 
    {
        //this "enum" defines the different control bits names
        public const int J3 = 0, J2 = 1, J1 = 2, D3 = 3, D2 = 4, D1 = 5, C6 = 6, C5 = 7, C4 = 8, C3 = 9, C2 = 10, C1 = 11, A = 12, X2 = 13, X1 = 14, Type = 15;

        public int Size { get; private set; }

        //CPU inputs
        public WireSet Instruction { get; private set; }
        public WireSet MemoryInput { get; private set; }
        public Wire Reset { get; private set; }

        //CPU outputs
        public WireSet MemoryOutput { get; private set; }
        public Wire MemoryWrite { get; private set; }
        public WireSet MemoryAddress { get; private set; }
        public WireSet InstructionAddress { get; private set; }

        //CPU components
        private ALU m_gALU;
        private Counter m_rPC;
        private MultiBitRegister m_rA, m_rD;
        private BitwiseMux m_gAMux, m_gMAMux;
        private AndGate and_D, and_MW, and_JM0, and_JM1, and_JM2,and_JMP;
        private OrGate or_A,or_JMP;
        private NotGate not_Zero, not_Neg, not_A;
        private Wire J_0, JGT, JEQ, JGE, JLT, JNE, JLE, J_1,wi1,wi2;
        private WireSet JMP;
        //private Wire wi1, wi2;

        //here we initialize and connect all the components, as in Figure 5.9 in the book
        public CPU16()
        {
            wi1 = new Wire();
            wi2 = new Wire();
            and_D = new AndGate();
            and_MW = new AndGate();
            and_JM0 = new AndGate();
            and_JM1 = new AndGate();
            and_JM2 = new AndGate();
            and_JMP = new AndGate();
            not_Zero = new NotGate();
            not_Neg = new NotGate();
            not_A = new NotGate();
            J_0 = new Wire();
            JGT = new Wire();
            JEQ = new Wire();
            JGE = new Wire();
            JLT = new Wire();
            JNE = new Wire();
            JLE = new Wire();
            J_1 = new Wire();
            JMP = new WireSet(3);
            or_A = new OrGate();
            or_JMP = new OrGate();

            Size =  16;

            Instruction = new WireSet(Size);
            MemoryInput = new WireSet(Size);
            MemoryOutput = new WireSet(Size);
            MemoryAddress = new WireSet(Size);
            InstructionAddress = new WireSet(Size);
            MemoryWrite = new Wire();
            Reset = new Wire();

            m_gALU = new ALU(Size);
            m_rPC = new Counter(Size);
            m_rA = new MultiBitRegister(Size);
            m_rD = new MultiBitRegister(Size);

            m_gAMux = new BitwiseMux(Size);
            m_gMAMux = new BitwiseMux(Size);

            m_gAMux.ConnectInput1(Instruction);
            m_gAMux.ConnectInput2(m_gALU.Output);

            m_rA.ConnectInput(m_gAMux.Output);

            m_gMAMux.ConnectInput1(m_rA.Output);
            m_gMAMux.ConnectInput2(MemoryInput);
            m_gALU.InputY.ConnectInput(m_gMAMux.Output);

            m_gALU.InputX.ConnectInput(m_rD.Output);

            m_rD.ConnectInput(m_gALU.Output);

            MemoryOutput.ConnectInput(m_gALU.Output);
            MemoryAddress.ConnectInput(m_rA.Output);

            InstructionAddress.ConnectInput(m_rPC.Output);
            m_rPC.ConnectInput(m_rA.Output);
            m_rPC.ConnectReset(Reset);

            //now, we call the code that creates the control unit
            ConnectControls();
        }

        //add here components to implement the control unit 
        private BitwiseMultiwayMux m_gJumpMux;//an example of a control unit compnent - a mux that controls whether a jump is made
        

        private void ConnectControls()
        {
            //1. connect control of mux 1 (selects entrance to register A)
            m_gAMux.ConnectControl(Instruction[Type]);
            //2. connect control to mux 2 (selects A or M entrance to the ALU)
            m_gMAMux.ConnectControl(Instruction[A]);
            //3. consider all instruction bits only if C type instruction (MSB of instruction is 1)

            //4. connect ALU control bits
            m_gALU.ZeroX.ConnectInput(Instruction[C1]);
            m_gALU.NotX.ConnectInput(Instruction[C2]);
            m_gALU.ZeroY.ConnectInput(Instruction[C3]);
            m_gALU.NotY.ConnectInput(Instruction[C4]);
            m_gALU.F.ConnectInput(Instruction[C5]);
            m_gALU.NotOutput.ConnectInput(Instruction[C6]);
            //5. connect control to register D (very simple)
            and_D.ConnectInput1(Instruction[D2]);
            and_D.ConnectInput2(Instruction[Type]);
            m_rD.Load.ConnectInput(and_D.Output);
            //6. connect control to register A (a bit more complicated)
            not_A.ConnectInput(Instruction[Type]);
            or_A.ConnectInput1(not_A.Output);
            or_A.ConnectInput2(Instruction[D1]);
            m_rA.Load.ConnectInput(or_A.Output);
            //7. connect control to MemoryWrite
            and_MW.ConnectInput1(Instruction[D3]);
            and_MW.ConnectInput2(Instruction[Type]);
            MemoryWrite.ConnectInput(and_MW.Output);
            //8. create inputs for jump mux
            not_Zero.ConnectInput(m_gALU.Zero);
            not_Neg.ConnectInput(m_gALU.Negative);
            and_JMP.ConnectInput1(not_Zero.Output);
            and_JMP.ConnectInput2(not_Neg.Output);
            //not3.ConnectInput(m_gALU.Negative);
            or_JMP.ConnectInput2(m_gALU.Negative);
            or_JMP.ConnectInput1(m_gALU.Zero);
            //not4.ConnectInput(m_gALU.Zero);
            //or2.ConnectInput1(m_gALU.Zero);
            //or2.ConnectInput2(m_gALU.Negative);

            //9. connect jump mux (this is the most complicated part)
            m_gJumpMux = new BitwiseMultiwayMux(1, 3);
            J_0.ConnectInput(wi1);
            m_gJumpMux.Inputs[0][0].ConnectInput(J_0);
            JGT.ConnectInput(and_JMP.Output);
            m_gJumpMux.Inputs[1][0].ConnectInput(JGT);
            JEQ.ConnectInput(m_gALU.Zero);
            m_gJumpMux.Inputs[2][0].ConnectInput(JEQ);
            JGE.ConnectInput(not_Neg.Output);
            m_gJumpMux.Inputs[3][0].ConnectInput(JGE);
            JLT.ConnectInput(m_gALU.Negative);
            m_gJumpMux.Inputs[4][0].ConnectInput(JLT);
            JNE.ConnectInput(not_Zero.Output);
            m_gJumpMux.Inputs[5][0].ConnectInput(JNE);
            JLE.ConnectInput(or_JMP.Output);
            m_gJumpMux.Inputs[6][0].ConnectInput(JLE);
            wi2.Value = 1;
            J_1.ConnectInput(wi2);
            m_gJumpMux.Inputs[7][0].ConnectInput(J_1);
            and_JM2.ConnectInput1(Instruction[Type]);
            and_JM2.ConnectInput2(Instruction[J1]);
            and_JM1.ConnectInput1(Instruction[Type]);
            and_JM1.ConnectInput2(Instruction[J2]);
            and_JM0.ConnectInput1(Instruction[Type]);
            and_JM0.ConnectInput2(Instruction[J3]);
            JMP[2].ConnectInput(and_JM2.Output);
            JMP[1].ConnectInput(and_JM1.Output);
            JMP[0].ConnectInput(and_JM0.Output);
            m_gJumpMux.ConnectControl(JMP);
            //10. connect PC load control
            m_rPC.ConnectLoad(m_gJumpMux.Output[0]);
        }

        public override string ToString()
        {
            return "A=" + m_rA + ", D=" + m_rD + ", PC=" + m_rPC + ",Ins=" + Instruction;
        }

        private string GetInstructionString()
        {
            if (Instruction[Type].Value == 0)
                return "@" + Instruction.GetValue();
            return Instruction[Type].Value + "XX " +
               "a" + Instruction[A] + " " +
               "c" + Instruction[C1] + Instruction[C2] + Instruction[C3] + Instruction[C4] + Instruction[C5] + Instruction[C6] + " " +
               "d" + Instruction[D1] + Instruction[D2] + Instruction[D3] + " " +
               "j" + Instruction[J1] + Instruction[J2] + Instruction[J3];
        }

        //use this function in debugging to print the current status of the ALU. Feel free to add more things for printing.
        public void PrintState()
        {
            Console.WriteLine("CPU state:");
            Console.WriteLine("PC=" + m_rPC + "=" + m_rPC.Output.GetValue());
            Console.WriteLine("A=" + m_rA + "=" + m_rA.Output.GetValue());
            Console.WriteLine("D=" + m_rD + "=" + m_rD.Output.GetValue());
            Console.WriteLine("Ins=" + GetInstructionString());
            Console.WriteLine("ALU=" + m_gALU);
            Console.WriteLine("inM=" + MemoryInput);
            Console.WriteLine("outM=" + MemoryOutput);
            Console.WriteLine("addM=" + MemoryAddress);
        }
    }
}
