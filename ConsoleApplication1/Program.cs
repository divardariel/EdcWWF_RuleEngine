using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var vC = new ValidationClass();
            ExternalRuleClientController eRCC = new ExternalRuleClientController();
            eRCC.LoadRuleSet("RuleSet1");
            eRCC.ExecuteRuleSet(vC);

            Console.WriteLine(vC.minorVersion);
            Console.ReadLine();

            eRCC.LoadRuleSet("RuleSet1");
            eRCC.ExecuteRuleSet(vC);

            Console.ReadLine();
            Console.WriteLine(vC.minorVersion);

            Console.ReadLine();

        }
    }
}
