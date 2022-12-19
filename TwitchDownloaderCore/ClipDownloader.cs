﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TwitchDownloaderCore.Options;
using TwitchDownloaderCore.TwitchObjects.Gql;

namespace TwitchDownloaderCore
{
    public sealed class ClipDownloader
    {
        private readonly ClipDownloadOptions downloadOptions;

        public ClipDownloader(ClipDownloadOptions DownloadOptions)
        {
            downloadOptions = DownloadOptions;
        }

        public async Task DownloadAsync(CancellationToken cancellationToken = new())
        {
            List<GqlClipTokenResponse> taskLinks = await TwitchHelper.GetClipLinks(downloadOptions.Id);

            string downloadUrl = "";

            foreach (var quality in taskLinks[0].data.clip.videoQualities)
            {
                if (quality.quality + "p" + (quality.frameRate.ToString() == "30" ? "" : quality.frameRate.ToString()) == downloadOptions.Quality)
                    downloadUrl = quality.sourceURL;
            }

            if (downloadUrl == "")
                downloadUrl = taskLinks[0].data.clip.videoQualities.First().sourceURL;

            downloadUrl += "?sig=" + taskLinks[0].data.clip.playbackAccessToken.signature + "&token=" + HttpUtility.UrlEncode(taskLinks[0].data.clip.playbackAccessToken.value);
            
            cancellationToken.ThrowIfCancellationRequested();

            using (WebClient client = new WebClient())
                await client.DownloadFileTaskAsync(downloadUrl, downloadOptions.Filename);
        }
    }
}
