﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Web.App.Hypernova;

namespace Web.App
{
    public class StoryController: Controller
    {
        private readonly HypernovaClient _hypernovaClient;
        private readonly HypernovaFileCache _hypernovaFileCache;
        private readonly string _contentRoot;
        private readonly string _ampPagesCacheName;

        public StoryController(ILogger<StoryController> logger, IHostingEnvironment env, IHttpClientFactory httpClientFactory, IOptions<HypernovaSettings> options)
        {
            _hypernovaClient = new HypernovaClient(logger, env, httpClientFactory, options);
            _hypernovaFileCache = new HypernovaFileCache(logger, env, options);
            _contentRoot = env.ContentRootPath;
            var settings = options.Value;
            _ampPagesCacheName = settings.AmpPagesCacheName;
        }

        public async Task<IActionResult> ArtistStory(string artistId)
        {
            var artistJson = FindArtist(artistId);
            if (artistJson == null)
            {
                return NotFound();
            }

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            artistJson.Add(new JProperty("baseUrl", baseUrl));
            
            var cacheItemName = $"ArtistStory_{artistId}.html";
            IHtmlContent hypernovaResult;
            ActionResult result = _hypernovaFileCache.GetCachedActionResult(this, _ampPagesCacheName, cacheItemName);
            if (result == null)
            {
                hypernovaResult = await _hypernovaClient.React("pwa:HypernovaArtistStory", artistJson.ToString());

                result = _hypernovaFileCache.StoreAndGetActionResult(this, _ampPagesCacheName, cacheItemName, hypernovaResult.ToString());
            }

            return result;
        }

        public ActionResult ArtistStoryBookend(string artistId)
        {
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            var allArtistIds = GetAllArtistIds();
            allArtistIds.Remove(artistId);

            var components = new List<AmpStoryBookendComponent>();
            components.Add(new AmpStoryBookendComponent
            {
                type = "heading",
                text = new string[] { "Other artists" }
            });

            foreach (var id in allArtistIds)
            {
                var artistJson = FindArtist(id);
                components.Add(new AmpStoryBookendComponent
                {
                    type = "landscape",
                    title = artistJson.Property("cover_artistname").Value.ToString(),
                    image = $"{baseUrl}/artists/{id}/cover.jpg",
                    url = $"{baseUrl}/Story/ArtistStory?artistId={id}"
                });
            }

            var curArtistJson = FindArtist(artistId);
            var curArtistName = curArtistJson.Property("cover_artistname").Value.ToString();

            var ampStoryBookend = new AmpStoryBookend
            {
                bookendVersion = "v1.0",
                components = components.ToArray(),
                shareProviders = new object[]
                {
                    "email",
                    "whatsapp",
                    new AmpStoryBookendShareProvider
                    {
                        provider = "twitter",
                        text = $"The story of {curArtistName} - {baseUrl}/Story/ArtistStory?artistId={artistId}"
                    }
                }
            };
            return Json(ampStoryBookend);
        }

        private JObject FindArtist(string artistId) {
            var artistDataFile = Path.Combine(_contentRoot, $@"ClientApp\public\artists\{artistId}\data.json");
            if (!System.IO.File.Exists(artistDataFile))
            {
                return null;
            }

            var jsonString = System.IO.File.ReadAllText(artistDataFile);
            var json = JObject.Parse(jsonString);
            json.Add(new JProperty("id", artistId));
            return json;
        }

        private List<string> GetAllArtistIds()
        {
            var artistIds = new List<string>();
            var artistRoot = Path.Combine(_contentRoot, @"ClientApp\public\artists");
            var directories = Directory.GetDirectories(artistRoot);
            foreach (string dir in directories)
            {
                artistIds.Add(dir.Remove(0, artistRoot.Length + 1));
            }
            return artistIds;
        }
    }
}
