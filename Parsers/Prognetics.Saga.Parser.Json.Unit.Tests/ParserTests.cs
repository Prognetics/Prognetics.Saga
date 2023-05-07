using NSubstitute;
using Prognetics.Saga.Parsers.Core.Abstract;
using Prognetics.Saga.Parsers.Core.Model;
using Xunit;

namespace Prognetics.Saga.Parser.Json.Unit.Tests
{
    public class ParserTests
    {
        [Fact]
        public async void ShouldReadFromFile()
        {
            var reader = Substitute.For<ITransactionsLedgerReader>();
            reader.GetSource().Returns(ConfigurationSource.File);

            var parser = new Parser(reader);

            await parser.GetTransactionsAsync();

            await reader.Received().ReadFromFileAsync();
        }

        [Fact]
        public async void ShouldReadFromNetwork()
        {
            var reader = Substitute.For<ITransactionsLedgerReader>();
            reader.GetSource().Returns(ConfigurationSource.Network);

            var parser = new Parser(reader);

            await parser.GetTransactionsAsync();

            await reader.Received().ReadFromInternetAsync();
        }

        [Fact]
        public async void ShouldThrowExceptionIfSourceNotSet()
        {
            var reader = Substitute.For<ITransactionsLedgerReader>();
            reader.GetSource().Returns((ConfigurationSource)(-1));

            var parser = new Parser(reader);

            await Assert.ThrowsAsync<FileLoadException>(async () => await parser.GetTransactionsAsync());
        }
    }
}
