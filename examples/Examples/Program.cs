using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using IContainer = Autofac.IContainer;

namespace RedPipes
{
    class Program
    {
        private readonly Dictionary<string, IPipe<string[]>> _examples;

        static async Task Main(string[] args)
        {
            await using var container = ConfigureContainer();
            var program = container.Resolve<Program>();
            try
            {
                await program.Run(args);
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync(e.ToString());
                await program.PrintHelp();
            }
        }

        public Program(IEnumerable<IPipe<string[]>> examples)
        {
            _examples = examples.ToDictionary(x => TrimPrefix(x.GetType().FullName??"", "RedPipes."), StringComparer.OrdinalIgnoreCase);
        }

        private async Task Run(string[] args)
        {
            var exampleName = args.FirstOrDefault() ?? "";
            if (_examples.TryGetValue(exampleName, out var pipe))
            {
                await pipe.Execute(Context.Background, args.Skip(1).ToArray());
            }
            else
            {
                await PrintHelp();
            }
        }

        private async Task PrintHelp()
        {
            var appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "dotnet run";
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync($"Usage: {appName} <example name> [ arg0[ arg1[ arg 2...]]]");
            await Console.Out.WriteLineAsync("   or: dotnet run -- <example name> [ arg0[ arg1[ arg 2...]]]");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("  available examples:");
            await Console.Out.WriteLineAsync();
            var names = _examples.Keys.OrderBy(x => x).ToList();
            var maxLength = names.Max(n => n.Length);
            var format = $"  {{0,-{maxLength}}}  : {{1}}";

            foreach (var name in names)
                await Console.Out.WriteLineAsync(string.Format(format, name, GetDescription(name)));

            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync();
        }

        private string GetDescription(string name)
        {
            string description = "no description available";
            if (_examples.TryGetValue(name, out var value))
            {
                description = value?.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description ?? description;
            }

            return description;
        }

        private static IContainer ConfigureContainer()
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterAssemblyTypes(typeof(Program).Assembly)
                .AssignableTo<IPipe<string[]>>()
                .AsImplementedInterfaces()
                .AsSelf();
            builder.RegisterType<Program>();

            var disposable = builder.Build();
            return disposable;
        }

        private static string TrimPrefix(string s, string prefix)
        {
            if (s.StartsWith(prefix))
                return s[prefix.Length..^prefix.Length];
            return s;
        }

    }

}
