using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using Confluent.Kafka;
using MongoDB.Driver;
using WorkerAcoes.IntegrationTests.Models;
using WorkerAcoes.IntegrationTests.Documents;

namespace WorkerAcoes.IntegrationTests;

public class TestesIntegracaoWorkerAcoes
{
    private const string COD_CORRETORA = "00000";
    private const string NOME_CORRETORA = "Corretora Testes";
    private static IConfiguration Configuration { get; }

    static TestesIntegracaoWorkerAcoes()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.json")
            .AddEnvironmentVariables().Build();
    }

    private readonly ITestOutputHelper _output;

    public TestesIntegracaoWorkerAcoes(ITestOutputHelper output)
    {
        // Capturing Output - xUnit:
        // https://xunit.net/docs/capturing-output
        _output = output;
    }

    [Theory]
    [InlineData("ABCD", 100.98)]
    [InlineData("EFGH", 200.9)]
    [InlineData("IJKL", 1_400.978)]
    public void TestarWorkerService(string codigo, double valor)
    {
        _output.WriteLine($"Sistema operacional: {RuntimeInformation.OSDescription}");
        
        var broker = Configuration["ApacheKafka:Broker"];
        var topic = Configuration["ApacheKafka:Topic"];
        _output.WriteLine($"Tópico: {topic}");

        var cotacaoAcao = new Acao()
        {
            Codigo = codigo,
            Valor = valor,
            CodCorretora = COD_CORRETORA,
            NomeCorretora = NOME_CORRETORA
        };
        var conteudoAcao = JsonSerializer.Serialize(cotacaoAcao);
        _output.WriteLine($"Dados: {conteudoAcao}");

        var configKafka = new ProducerConfig
        {
            BootstrapServers = broker
        };

        using (var producer = new ProducerBuilder<Null, string>(configKafka).Build())
        {
            var result = producer.ProduceAsync(
                topic,
                new Message<Null, string>
                { Value = conteudoAcao }).Result;

            _output.WriteLine(
                $"Apache Kafka - Envio para o tópico {topic} concluído | " +
                $"{conteudoAcao} | Status: { result.Status.ToString()}");
        }

        _output.WriteLine("Aguardando o processamento do Worker...");
        Thread.Sleep(
            Convert.ToInt32(Configuration["IntervaloProcessamento"]));

        var mongoDBConnection = Configuration["MongoDBConnection"];

        var mongoDatabase = Configuration["MongoDatabase"];
        _output.WriteLine($"MongoDB Database: {mongoDatabase}");

        var mongoCollection = Configuration["MongoCollection"];
        _output.WriteLine($"MongoDB Collection: {mongoCollection}");

        var acaoDocument = new MongoClient(mongoDBConnection)
            .GetDatabase(mongoDatabase)
            .GetCollection<AcaoDocument>(mongoCollection)
            .Find(h => h.Codigo == codigo).SingleOrDefault();

        _output.WriteLine("Analisar dados processados pelo Worker...");

        acaoDocument.Should().NotBeNull();
        acaoDocument.Codigo.Should().Be(codigo);
        acaoDocument.Valor.Should().Be(valor);
        acaoDocument.CodCorretora.Should().Be(COD_CORRETORA);
        acaoDocument.NomeCorretora.Should().Be(NOME_CORRETORA);
        acaoDocument.HistLancamento.Should().NotBeNullOrWhiteSpace();
        acaoDocument.DataReferencia.Should().NotBeNullOrWhiteSpace();
    }
}