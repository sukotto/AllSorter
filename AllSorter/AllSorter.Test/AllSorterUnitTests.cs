using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace AllSorter.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //Tests CodeFix for sorting classes
        [TestMethod]
        public void TestMethodSortClasses()
        {
            var test = @"using System;

namespace Anteaters
{
    class Program
    {
        static void Main(string[] args)
        {
            
        }
    }

    class Pangolin
    {
        public void PangolinMethod()
        {

        }
    }

    class Echidna
    {
        public void EchidnaMethod()
        {

        }
    }
}";
            
            var fixtest = @"using System;

namespace Anteaters
{

    class Echidna
    {
        public void EchidnaMethod()
        {

        }
    }

    class Pangolin
    {
        public void PangolinMethod()
        {

        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }

        


        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AllSorterCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AllSorterAnalyzer();
        }
    }
}
