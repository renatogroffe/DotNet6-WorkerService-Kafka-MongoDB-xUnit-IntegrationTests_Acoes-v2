using Confluent.Kafka;

namespace WorkerAcoes.Extensions;

public static class KafkaExtensions
{
    public static bool ExecutingTests(IConfiguration configuration) =>
        String.IsNullOrWhiteSpace(configuration["ApacheKafka:Password"]);

    public static void CheckTopicForTests(IConfiguration configuration)
    {
        var configKafka = new ProducerConfig
        {
            BootstrapServers = configuration["ApacheKafka:Broker"]
        };

        using (var producer = new ProducerBuilder<Null, string>(configKafka).Build())
        {
            string topic = configuration["ApacheKafka:Topic"];
            var result = producer.ProduceAsync(
                topic,
                new Message<Null, string>
                { Value = $"Mensagem de teste enviada para o tópico {topic} - DESCONSIDERAR" }
                ).Result;

            Console.WriteLine(
                $"Envio de mensagem inicial de teste para o tópico {topic} do Apache Kafka concluído | " +
                $"Status: { result.Status.ToString()}");
        }
    }

    public static IConsumer<Ignore, string> CreateConsumer(
        IConfiguration configuration)
    {
        if (!ExecutingTests(configuration))
            return new ConsumerBuilder<Ignore, string>(
                new ConsumerConfig()
                {
                    BootstrapServers = configuration["ApacheKafka:Broker"],
                    SecurityProtocol = SecurityProtocol.SaslSsl,
                    SaslMechanism = SaslMechanism.Plain,
                    SaslUsername = configuration["ApacheKafka:Username"],
                    SaslPassword = configuration["ApacheKafka:Password"],
                    GroupId = configuration["ApacheKafka:GroupId"],
                    AutoOffsetReset = AutoOffsetReset.Earliest
                }).Build();
        else
            return new ConsumerBuilder<Ignore, string>(
                new ConsumerConfig()
                {
                    BootstrapServers = configuration["ApacheKafka:Broker"],
                    GroupId = configuration["ApacheKafka:GroupId"],
                    AutoOffsetReset = AutoOffsetReset.Earliest
                }).Build();
    }
}