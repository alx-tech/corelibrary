using Xunit;
using static LeanCode.ContractsGenerator.Tests.TypeScript.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
{
    public class ContractsGeneratorTests_TypesExclusion
    {
        [Theory]
        [InlineData("class")]
        [InlineData("interface")]
        [InlineData("struct")]
        [InlineData("enum")]
        public void Excludes_top_level_types_with_exclude_attribute_keeps_those_without(string type)
        {
            var code = $@"
            [ExcludeFromContractsGeneration]
            public {type} ToExclude {{}}

            public {type} ToKeep {{}}
            ";

            var generator = CreateTsGeneratorFromNamespace(code);
            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.DoesNotContain("ToExclude", contracts);
            Assert.Contains("ToKeep", contracts);
        }

        [Fact]
        public void Excludes_nested_classes_with_attribute_and_their_children()
        {
            var code = @"
            public class Root
            {
                [ExcludeFromContractsGeneration]
                public class Inner
                {
                    public class MoreInner
                    {}
                }
            }";

            var generator = CreateTsGeneratorFromNamespace(code);
            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.DoesNotContain("Inner", contracts);
            Assert.DoesNotContain("MoreInner", contracts);
            Assert.Contains("Root", contracts);
        }

        [Fact]
        public void Excludes_properties_with_attribute()
        {
            var code = @"
            public interface IEntitiesRelated
            {
                public int[] Included { get; }

                [ExcludeFromContractsGeneration]
                public int[] Excluded { get; }
            }";

            var generator = CreateTsGeneratorFromNamespace(code);
            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("Included", contracts);
            Assert.DoesNotContain("Excluded", contracts);
        }
    }
}
