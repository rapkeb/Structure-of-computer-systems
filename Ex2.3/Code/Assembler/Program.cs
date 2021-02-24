using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembler
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembler a = new Assembler();
            //to run tests, call the "TranslateAssemblyFile" function like this:
            string sourceFileLocation = "C:\\Users\\97254\\Desktop\\macroTest.asm";
            string destFileLocation = "C:\\Users\\97254\\Desktop\\לימודים\\שנה ב\\סמסטר א\\מבנה מערכות מחשוב\\Ex2.3\\Code\\Assembly examples\\add.mc";
            a.TranslateAssemblyFile(sourceFileLocation, destFileLocation);
            //a.TranslateAssemblyFile(@"Add.asm", @"Add.mc");
        }
    }
}
