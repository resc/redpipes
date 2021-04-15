using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using RabbitMQ.Client;

namespace RedPipes.RabbitMQ
{
    public static class ActivityExtensions
    {
        private const string EventNamePrefix = "messaging.rabbitmq.event ";
        private const string InitPrefix = "messaging.rabbitmq.init ";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string WithRmqInitPrefix(this string s)
        {
            return InitPrefix + s;
        }

        public static void AddQueueDeclaredEvent(this Activity a, QueueConfig cfg, QueueDeclareOk result)
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

        public static void AddExchangeDeclaredEvent(this Activity a, ExchangeConfig cfg)
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
        public static void AddBasicConsumeEvent(this Activity a, QueueConfig cfg, string consumerTag)
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
