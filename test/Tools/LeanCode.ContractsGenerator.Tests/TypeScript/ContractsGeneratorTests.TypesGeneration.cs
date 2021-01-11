using Xunit;
using static LeanCode.ContractsGenerator.Tests.TypeScript.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
{
    public class ContractsGeneratorTests_TypesGeneration
    {
        [Fact]
        public void Int_is_resolved_to_number()
        {
            var generator = CreateTsGeneratorFromClass("public int TestVar { get; set; }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("TestVar: number;", contracts);
        }

        [Fact]
        public void Nullable_property_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromClass("public Nullable<int> TestVar { get; set; }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("TestVar?: number | null;", contracts);
        }

        [Fact]
        public void Nullable_property_with_question_mark_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromClass("public int? TestVar { get; set; }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("TestVar?: number | null;", contracts);
        }

        [Fact]
        public void Nullable_reference_type_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromClass("public string? TestVar { get; set; }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("TestVar?: string | null;", contracts);
        }

        [Fact]
        public void Reference_type_marked_with_CanBeNullAttribute_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromClass("[CanBeNull] public string TestVar { get; set; }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("TestVar?: string | null;", contracts);
        }

        [Fact]
        public void Array_is_resolved_to_array()
        {
            var generator = CreateTsGeneratorFromClass("public int[] TestArray { get; set; }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("TestArray: number[];", contracts);
        }

        [Fact]
        public void List_is_resolved_to_array()
        {
            var generator = CreateTsGeneratorFromClass("public List<int> TestArray { get; set; }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("TestArray: number[];", contracts);
        }

        [Fact]
        public void HashSet_is_resolved_to_array()
        {
            var generator = CreateTsGeneratorFromClass("public HashSet<int> TestArray { get; set; }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("TestArray: number[];", contracts);
        }

        [Fact]
        public void Dictionary_is_resolved_to_proper_object()
        {
            var generator = CreateTsGeneratorFromClass("public Dictionary<string, int> TestDictionary { get; set; }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Matches("TestDictionary: { \\[[a-zA-Z]+: string\\]: number };", contracts);
        }
    }
}
