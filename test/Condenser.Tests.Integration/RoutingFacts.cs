﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using CondenserDotNet.Core;
using CondenserDotNet.Core.DataContracts;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server;
using CondenserDotNet.Server.Builder;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Routes;
using CondenserDotNet.Server.RoutingTrie;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class RoutingFacts
    {
        private const string UrlPrefix = "urlprefix-";

        [Fact]
        public async Task CanWeFindARouteAndGetAPage()
        {
            var informationService = new InformationService
            {
                Address = "www.google.com",
                Port = 80
            };

            var router = BuildRouter();
            var service = new Service(null, null, null);
            await service.Initialise("service1", "node1", new[] { UrlPrefix + "/search" }, "www.google.com", 80);
            router.AddNewService(service);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/search";

            var routedService = router.GetServiceFromRoute(context.Request.Path, out string matchedPath);
            await routedService.CallService(context);

            Assert.Equal(200, context.Response.StatusCode);
        }

        private CustomRouter BuildRouter()
        {
            Func<ChildContainer<IService>> createNode = () =>
            {
                var randomRoutingStrategy = new RandomRoutingStrategy<IService>();
                return new ChildContainer<IService>(new DefaultRouting<IService>(new[] { randomRoutingStrategy },
                    null));
            };
            var data = new RoutingData(new RadixTree<IService>(createNode));
            return new CustomRouter(null, data);
        }
    }
}