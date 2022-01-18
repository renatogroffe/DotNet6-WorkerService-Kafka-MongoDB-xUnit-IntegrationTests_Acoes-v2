using System.Text.Json;
using Confluent.Kafka;
using WorkerAcoes.Data;
using WorkerAcoes.Models;
using WorkerAcoes.Validators;
using WorkerAcoes.Extensions;

namespace WorkerAcoes;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly AcoesRepository _repository;
    private readonly IConsumer<Ignore, string> _consumer;

    public Worker(ILogger<Worker> logger, IConfiguration configuration,
        AcoesRepository repository)
    {
        _logger = logger;
        _configuration = configuration;
        _repository = repository;
        _consumer = KafkaExtensions.CreateConsumer(configuration);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string topico = _configuration["ApacheKafka:Topic"];

        _logger.LogInformation($"Topic = {topico}");
        _logger.LogInformation($"Group Id = {_configuration["ApacheKafka:GroupId"]}");
        _logger.LogInformation("Aguardando mensagens...");
        _consumer.Subscribe(topico);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Run(() =>
            {
                var result = _consumer.Consume(stoppingToken);
                var dadosAcao = result.Message.Value;
                
                _logger.LogInformation(
                    $"[{_configuration["ApacheKafka:GroupId"]} | Nova mensagem] " +
                    dadosAcao);

                ProcessarAcao(dadosAcao);
            });
        }
    }

    private void ProcessarAcao(string dados)
    {
        Acao? acao;            
        try
        {
            acao = JsonSerializer.Deserialize<Acao>(dados,
                new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch
        {
            acao = null;
        }

        if (acao is not null &&
            new AcaoValidator().Validate(acao).IsValid)
        {
            _repository.Save(acao);
            _logger.LogInformation("Ação registrada com sucesso!");
        }
        else
        {
            _logger.LogError("Dados inválidos para a Ação");
        } 
    }
}