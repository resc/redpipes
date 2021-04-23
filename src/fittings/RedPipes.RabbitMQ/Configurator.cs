using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;
using RedPipes.OpenTelemetry.Tracing;

namespace RedPipes.RabbitMQ
{
    /// <summary> RabbitMQ configuration entry point </summary>
    public static class Configurator
    {
        /// <summary> Uses RabbitMQ messages as the input for a pipe </summary>
        public static IBuilder<RabbitMqConfig, Receive> UseRabbitMQ(this IBuilderProvider builderProviderFactory)
        {
            if (Equals(null, builderProviderFactory))
                throw new ArgumentNullException(nameof(builderProviderFactory));

            return builderProviderFactory.For(new Builder());
        }

        sealed class Builder : Configuration.Builder, IBuilder<RabbitMqConfig, Receive>
        {
            public Task<IPipe<RabbitMqConfig>> Build(IPipe<Receive> next)
            {
                // TODO implement construction and configuration of the new pipe segment here
                var pipe = new Pipe(next);
                return Task.FromResult((IPipe<RabbitMqConfig>)pipe);
            }
        }

        sealed class Pipe : AsyncDefaultBasicConsumer, IPipe<RabbitMqConfig>
        {
            private readonly IPipe<Receive> _next;
            private readonly SemaphoreSlim _done;

            public Pipe(IPipe<Receive> next)
            {
                _next = next;
                _done = new SemaphoreSlim(0, 1);
            }

            public async Task Execute(IContext ctx, RabbitMqConfig config)
            {
                var factory = new ConnectionFactory
                {
                    Uri = config.Connection.Uri,
                    ClientProvidedName = config.Connection.ClientProvidedName,
                    ClientProperties = config.Connection.ClientProperties == null ? null : new Dictionary<string, object>(config.Connection.ClientProperties),
                    DispatchConsumersAsync = true,
                };

                config.Connection.ConfigureFactory?.Invoke(factory);
                using var connection = factory.CreateConnection();
                config.Connection.Configure?.Invoke(connection);
                using var model = connection.CreateModel();
                config.Model.Configure?.Invoke(model);
                ExchangeDeclare(config, model);
                QueueDeclare(config, model);
                BasicConsume(config, model);
                await _done.WaitAsync(ctx.Token);
            }

            private void BasicConsume(RabbitMqConfig config, IModel model)
            {
                using var basicConsume = ActivitySources.Default.StartActivity("basic_consume".WithRmqInitPrefix());
                foreach (var q in config.Model.Queues)
                {
                    var consumerTag = model.BasicConsume(q.Name, q.Consumer.AutoAck, q.Consumer.ConsumerTag, q.Consumer.NoLocal, q.Consumer.Exclusive, q.Consumer.Arguments, this);
                    basicConsume?.AddBasicConsumeEvent(q, consumerTag);
                }
            }

            private static void QueueDeclare(RabbitMqConfig config, IModel model)
            {
                using var queueDeclare = ActivitySources.Default.StartActivity("queue_declare".WithRmqInitPrefix());
                foreach (var q in config.Model.Queues)
                {
                    try
                    {
                        var result = model.QueueDeclare(q.Name, q.Declaration.Durable, q.Declaration.Exclusive, q.Declaration.AutoDelete, q.Declaration.Arguments);
                        queueDeclare?.AddQueueDeclaredEvent(q, result);
                    }
                    catch (Exception ex)
                    {
                        queueDeclare?.RecordException(ex);
                        throw;
                    }
                }
            }

            private static void ExchangeDeclare(RabbitMqConfig config, IModel model)
            {
                using var exchangeDeclare = ActivitySources.Default.StartActivity("exchange_declare".WithRmqInitPrefix());
                foreach (var e in config.Model.Exchanges)
                {
                    try
                    {
                        model.ExchangeDeclare(e.Name, e.Declaration.Type, e.Declaration.Durable, e.Declaration.AutoDelete, e.Declaration.Arguments);
                        exchangeDeclare?.AddExchangeDeclaredEvent(e);
                    }
                    catch (Exception ex)
                    {
                        exchangeDeclare?.RecordException(ex);
                        throw;
                    }
                }
            }


            ///<summary>Fires when the server confirms successful consumer cancelation.</summary>
            public override async Task HandleBasicCancelOk(string consumerTag)
            {
                await base.HandleBasicCancelOk(consumerTag).ConfigureAwait(false);
            }

            ///<summary>Fires when the server confirms successful consumer registration.</summary>
            public override async Task HandleBasicConsumeOk(string consumerTag)
            {
                await base.HandleBasicConsumeOk(consumerTag).ConfigureAwait(false);
            }

            public override async Task OnCancel(params string[] consumerTags)
            {
                await base.OnCancel(consumerTags);
                _done.Release();
            }

            ///<summary>Fires the Received event.</summary>
            public override async Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
            {
                // No need to call base, it's empty.
                var background = Context.Background;
                var receive = new Receive(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
                using var basicDeliver = ActivitySources.Default.StartActivity("basic_deliver".WithRmqInitPrefix());
                try
                {
                    await _next.Execute(background, receive);
                }
                catch (Exception ex)
                {
                    basicDeliver?.RecordException(ex);
                    throw;
                }
            }

            ///<summary>Fires the Shutdown event.</summary>
            public override async Task HandleModelShutdown(object model, ShutdownEventArgs reason)
            {
                await base.HandleModelShutdown(model, reason).ConfigureAwait(false);
            }
            
            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                if (visitor.AddEdge(this, _next, (Keys.Name, "Next")))
                    _next.Accept(visitor);
            }
        }
    }
}
