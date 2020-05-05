using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace AllSorter.Test
{
    [TestClass]
    public class MethodSorterUnitTests : CodeFixVerifier
    {
        [TestMethod]
        public void TestMethodSortMethods()
        {
            var test = @"public class BClass
    {
        public void BClassMethodC()
        {
            
        }
        public void BClassMethodA()
        {
            
        }
        public void BClassMethodB()
        {
            
        }
    }";

            var fixtest = @"public class BClass
    {
        public void BClassMethodA()
        {
            
        }
        public void BClassMethodB()
        {
            
        }
        public void BClassMethodC()
        {
            
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new UnsortedMethodCodeFix();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UnsortedMethodAnalyzer();
        }
    }
}
