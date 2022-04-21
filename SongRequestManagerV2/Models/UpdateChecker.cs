﻿using IPA.Loader;
using SongRequestManagerV2.Interfaces;
using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SongRequestManagerV2.Models
{
    internal class UpdateChecker : IInitializable, IUpdateChecker
    {
        private Hive.Versioning.Version _gameVersion;
        private const string s_latest = "/releases";
        private static readonly Regex s_zipFileRegex = new Regex(@"bs([0-9]+\.){2}[0-9]+");
        private static readonly string s_pending = Path.Combine(Environment.CurrentDirectory, "IPA", "Pending");
        public Hive.Versioning.Version CurrentLatestVersion { get; private set; } = new Hive.Versioning.Version(0, 0, 0);
        public string DownloadURL { get; private set; } = "";
        public bool AnyUpdate { get; private set; } = false;
        public void Initialize()
        {
            this._gameVersion = new Hive.Versioning.Version(Application.version);
        }
        public async Task<bool> CheckUpdate(PluginMetadata metadata)
        {
            var apiRoot = metadata.PluginSourceLink?.ToString()?.Replace(@"github.com", @"api.github.com/repos");
            var version = metadata.HVersion;
            this.AnyUpdate = false;
            var res = await WebClient.GetAsync($"{apiRoot}{s_latest}", CancellationToken.None);
            if (res == null || !res.IsSuccessStatusCode) {
                return false;
            }
            var releases = res.ConvertToJsonNode();
            if (releases == null) {
                return false;
            }
            foreach (var releseJson in releases.AsArray.Children) {
                var asaets = releseJson["assets"].AsArray;
                var tag = "";
                foreach (var item in asaets.Children) {
                    if (item["content_type"].Value != "application/x-zip-compressed") {
                        continue;
                    }
                    var fileVersion = s_zipFileRegex.Match(item["name"].Value).Value.Replace("bs", "");
                    if (string.IsNullOrEmpty(fileVersion)) {
                        continue;
                    }
                    var fileBSVersion = new Hive.Versioning.Version(fileVersion);
                    if (this._gameVersion < fileBSVersion) {
                        continue;
                    }
                    this.DownloadURL = item["browser_download_url"].Value;
                    tag = releseJson["tag_name"].Value;
                    break;
                }
                if (string.IsNullOrEmpty(tag)) {
                    continue;
                }
                var latestVersion = new Hive.Versioning.Version(tag);
                if (latestVersion <= version) {
                    return false;
                }
                else {
                    this.CurrentLatestVersion = latestVersion;
                    this.AnyUpdate = true;
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> UpdateMod()
        {
            if (!this.AnyUpdate || string.IsNullOrEmpty(this.DownloadURL)) {
                return true;
            }
            var zip = await WebClient.GetAsync(this.DownloadURL, CancellationToken.None);
            if (zip == null || !zip.IsSuccessStatusCode) {
                return false;
            }
            var tmpFolder = Path.GetTempPath();
            tmpFolder = Path.Combine(tmpFolder, Guid.NewGuid().ToString());
            try {
                using (var ms = new MemoryStream(zip.ContentToBytes()))
                using (var archive = new ZipArchive(ms)) {
                    archive.ExtractToDirectory(tmpFolder);
                }
                this.CopyDirectory(tmpFolder, s_pending);
                Directory.Delete(tmpFolder, true);
            }
            catch (Exception e) {
                Logger.Error(e);
                return false;
            }
            return true;
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            if (string.IsNullOrEmpty(sourceDir) || string.IsNullOrEmpty(destDir) || !Directory.Exists(sourceDir)) {
                return;
            }
            if (!Directory.Exists(destDir)) {
                Directory.CreateDirectory(destDir);
            }
            foreach (var file in Directory.EnumerateFiles(sourceDir)) {
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
            }
            foreach (var dir in Directory.EnumerateDirectories(sourceDir)) {
                this.CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
            }
        }
    }
}
