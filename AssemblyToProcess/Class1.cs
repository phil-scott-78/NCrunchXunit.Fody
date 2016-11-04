using NCrunch.Framework;
using Shouldly;
using Xunit;

namespace AssemblyToProcess
{
    [ExclusivelyUses("dummy")]
    public class ForceNCrunch
    {
        
    }

    public class ClassWithFacts
    {
        [Fact]
        public void MyUnitTest()
        {
            (2 + 2).ShouldBe(4);
        }
    }

    public class ClassWithNoFacts
    {
        public int SomeFunction()
        {
            return 2 + 2;
        }
    }

    public class ClassWithTheory
    {
        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(6)]
        public void SomeTheory(int i)
        {
            (i + 1).ShouldBeGreaterThan(0);
        }
    }

    [Collection("SomeCollection")]
    public class ClassWithCollection1
    {
        [Fact]
        public void SomeUnitTest()
        {
            (2 + 2).ShouldBe(4);
        } 
    }

    [Collection("SomeCollection")]
    public class ClassWithCollection2
    {
        [Fact]
        public void SomeUnitTest()
        {
            (2 + 2).ShouldBe(4);
        }
    }
}
