using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MichelinMonthlyFileTesters
{
    class Program
    {
        static void Main(string[] args)
        {

            FunctionTools.ConsoleSettings();

            // Michelin Monthly File Tester -----------------------------
            MonthlyFileTesters.MichelinMonthlyFileCleaner();

        }
    }
}
