﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Iimages.IStore
{
    public static class SotreCenter
    {

        static SotreCenter()
        {
            StoreFactories = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(e => e.GetInterfaces().Contains(typeof(IStoreFactory)))
                .Select(e => Activator.CreateInstance(e))
                .OfType<IStoreFactory>()
                .ToList();
        }

        public static void Initialize(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            var stores = StoreFactories.SelectMany(e => e.GetStores(app, configuration)).ToList();
            Config = configuration;
            Stores = stores.ToDictionary(e => e.Type, e => e);
            Supports = stores.Select(e => new SupportType(e)).OrderBy(e => e.Index).ToList();
            NSFW = configuration.GetSection("GLOBAL").GetValue<bool>("NSFW");
            COMPRESS = configuration.GetSection("GLOBAL").GetValue<bool>("COMPRESS");
            NSFWHOST = configuration.GetSection("GLOBAL").GetValue<string>("NSFWHOST");
            if (NSFW)
            {
                NSFWCHECK = new StoreCheckNsfwWarperResetAPI(app, env, configuration, httpClientFactory);
            }
            if (COMPRESS)
            {
                //  StoreCompress = new StoreCompressTinyWarper(app, env, configuration, httpClientFactory);
                StoreCompress = new StoreCompressPngQuantWarper(app, configuration);
            }
        }

        public static IConfiguration Config { get; private set; }

        public static bool NSFW { get; private set; }

        public static string NSFWHOST { get; private set; }

        public static bool COMPRESS { get; private set; }

        public static List<IStoreFactory> StoreFactories { get; private set; }

        public static Dictionary<string, IDataStore> Stores { get; private set; }

        public static List<SupportType> Supports { get; private set; }

        public static IStoreCheck NSFWCHECK { get; private set; }

        public static IStoreCompress StoreCompress { get; private set; }

    }
}
