﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server.Builder;
using CondenserDotNet.Server.Routes;
using CondenserDotNet.Server.RoutingTrie;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCondenser(this IServiceCollection self)
        {
            return AddCondenser(self, "localhost", 8500);
        }

        private static IServiceCollection AddCondenser(this IServiceCollection self, string agentAddress, int agentPort)
        {
            var config = new CondenserConfiguration
            {
                AgentPort = agentPort,
                AgentAddress = agentAddress
            };
            self.AddSingleton(config);
            self.AddTransient<Service>();
            self.AddSingleton<Func<IConsulService>>(x => x.GetService<Service>);
            self.AddSingleton<CurrentState>();
            self.AddTransient<IRoutingStrategy<IService>, RandomRoutingStrategy<IService>>();
            self.AddTransient<IRoutingStrategy<IService>, RoundRobinRoutingStrategy<IService>>();
            self.AddSingleton<IDefaultRouting<IService>, DefaultRouting<IService>>();
            self.AddSingleton<Func<string, HttpClient>>(s => new HttpClient());
            Func<ChildContainer<IService>> factory = () =>
            {
                var randomRoutingStrategy = new RandomRoutingStrategy<IService>();
                return new ChildContainer<IService>(new DefaultRouting<IService>(new[] { randomRoutingStrategy }, null));
            };
            self.AddSingleton(new RoutingData(new RadixTree<IService>(factory)));
            self.AddSingleton<CustomRouter>();
            self.AddSingleton<RoutingHost>();
            return self;
        }

        public static IServiceCollection AddCondenser(this IServiceCollection self, string agentAddress, int agentPort,
            IHealthConfig health, IRoutingConfig routingConfig, IHttpClientConfig httpClientConfig)
        {
            var config = new CondenserConfiguration
            {
                AgentPort = agentPort,
                AgentAddress = agentAddress
            };

            self.AddSingleton(config);
            self.AddSingleton(health);
            self.AddSingleton(routingConfig);
            self.AddSingleton(httpClientConfig);

            self.AddSingleton<RoutingData>();
            self.AddSingleton<IService, HealthRouter>();
            self.AddSingleton<IService, RouteSummary>();
            self.AddSingleton<IService, TreeRouter>();
            self.AddSingleton<IService, ChangeRoutingStrategy>();
            self.AddSingleton<IService, ServerStatsRoute>();
            self.AddTransient<IRoutingStrategy<IService>, RandomRoutingStrategy<IService>>();
            self.AddTransient<IRoutingStrategy<IService>, RoundRobinRoutingStrategy<IService>>();
            self.AddSingleton<IDefaultRouting<IService>, DefaultRouting<IService>>();

            self.AddTransient<ChildContainer<IService>>();
            self.AddSingleton<CurrentState>();
            self.AddSingleton<CustomRouter>();
            self.AddSingleton<RoutingHost>();
            self.AddSingleton<RadixTree<IService>>();

            self.AddTransient<Service>();
            self.AddSingleton<Func<IConsulService>>(x => x.GetService<Service>);
            self.AddSingleton<Func<ChildContainer<IService>>>(x => x.GetService<ChildContainer<IService>>);
            return self;
        }

        public static IConfigurationBuilder AddCondenserWithBuilder(this IServiceCollection self)
        {
            return new ConfigurationBuilder(self);
        }
    }
}
