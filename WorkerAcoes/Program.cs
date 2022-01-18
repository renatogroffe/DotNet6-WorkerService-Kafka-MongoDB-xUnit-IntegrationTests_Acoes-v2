using WorkerAcoes.Data;
using WorkerAcoes;
using WorkerAcoes.Extensions;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Assume o uso do Apache Kafka e desta aplicação em modo testes
        // quando não houver um password de acesso definido
        if (KafkaExtensions.ExecutingTests(hostContext.Configuration))
            KafkaExtensions.CheckTopicForTests(hostContext.Configuration);

        services.AddSingleton<AcoesRepository>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();