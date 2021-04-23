using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using RabbitMQ.Client;

namespace RedPipes.RabbitMQ
{
    /// <summary>
    /// RabbitMQ diagnostics activity extensions
    /// </summary>
    public static class ActivityExtensions
    {
        /// <summary> RabbitMQ event name prefix for logging </summary>
        public const string EventNamePrefix = "messaging.rabbitmq.event ";

        /// <summary> RabbitMQ init event name prefix for logging </summary>
        public const string InitPrefix = "messaging.rabbitmq.init ";

        /// <summary> Prepends the <see cref="InitPrefix"/> to string <paramref name="s"/> </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string WithRmqInitPrefix(this string s)
        {
            return InitPrefix + s;
        }

        /// <summary> Adds a queue declared event to <see cref="Activity"/> <paramref name="a"/> </summary>
        public static void AddQueueDeclaredEvent(this Activity? a, QueueConfig cfg, QueueDeclareOk result)
        {
            if (a == null) return;

            var tags = new ActivityTagsCollection
            {
                {"queue.name", result.QueueName},
                {"queue.consumer_count", result.ConsumerCount},
                {"queue.message_count", result.MessageCount}
            };

            if (a.IsAllDataRequested)
            {
                tags.Add("queue.exclusive", cfg.Declaration.Exclusive);
                tags.Add("queue.durable", cfg.Declaration.Durable);
                tags.Add("queue.auto_delete", cfg.Declaration.AutoDelete);
                tags.AddWithPrefix("queue.args.", cfg.Declaration.Arguments);
            }
            a.AddEvent(new ActivityEvent(EventNamePrefix + "queue_declare_ok", tags: tags));
        }
      
        /// <summary> Adds a exchange declared event to <see cref="Activity"/> <paramref name="a"/> </summary>
        public static void AddExchangeDeclaredEvent(this Activity? a, ExchangeConfig cfg)
        {
            if (a == null) return;

            var tags = new ActivityTagsCollection
            {
                {"exchange.name", cfg.Name},
                {"exchange.type", cfg.Declaration.Type},
                {"exchange.auto_delete", cfg.Declaration.AutoDelete},
                {"exchange.durable", cfg.Declaration.Durable},
            };

            if (a.IsAllDataRequested)
            {
                tags.AddWithPrefix("exchange.args.", cfg.Declaration.Arguments);
            }
            a.AddEvent(new ActivityEvent(EventNamePrefix + "queue_declare_ok", tags: tags));
        }
       

        /// <summary> Adds a basic consume event to <see cref="Activity"/> <paramref name="a"/> </summary>
        public static void AddBasicConsumeEvent(this Activity? a, QueueConfig cfg, string consumerTag)
        {
            if (a == null) return;

            var tags = new ActivityTagsCollection
            {
                {"queue.name", cfg.Name},
                {"queue.consumer.tag", consumerTag},
            };

            if (a.IsAllDataRequested)
            {
                tags.Add("queue.consumer.auto_ack", cfg.Consumer.AutoAck);
                tags.Add("queue.consumer.exclusive", cfg.Consumer.Exclusive);
                tags.Add("queue.consumer.no_local", cfg.Consumer.NoLocal);
                tags.AddWithPrefix("queue.consumer.args.", cfg.Consumer.Arguments);
            }

            a.AddEvent(new ActivityEvent(EventNamePrefix + "basic_consume", tags: tags));
        }

        /// <summary> Adds the tags in <paramref name="args"/> to the <paramref name="tags"/> while prefixing the key with <paramref name="prefix"/> </summary>
        public static void AddWithPrefix(this ActivityTagsCollection tags, string prefix, IDictionary<string, object>? args)
        {
            if (args == null) return;

            if (args.Count > 0)
            {
                foreach (var (key, val) in args)
                {
                    if (val == null) continue;

                    tags.Add(TagKey(prefix, key), val);
                }
            }
        }

        private static string TagKey(string prefix, string suffix)
        {
            var sb = new StringBuilder(prefix, prefix.Length + suffix.Length);
            suffix = Regex.Replace(suffix, "[^\u0021-\u007E]", "_");
            suffix = Regex.Replace(suffix, "_+", "_").TrimStart('.', '_');
            sb.Append(suffix);
            return sb.ToString();
        }
    }
}
