﻿using ChatCore.Interfaces;
using ChatCore.Services;
using ChatCore.Services.Twitch;
using ChatCore.Utilities;
using SongRequestManagerV2.Bots;
using SongRequestManagerV2.Models;
using SongRequestManagerV2.Statics;
using SongRequestManagerV2.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;

namespace SongRequestManagerV2.Interfaces
{
    public interface IRequestBot
    {
        ChatServiceMultiplexer MultiplexerInstance { get; }
        
        SongListUtils SongListUtils { get; }
        StringNormalization Normalize { get; }
        MapDatabase MapDatabase { get; }
        SongRequest Currentsong { get; set; }
        TwitchService TwitchService { get; }
        ListCollectionManager ListCollectionManager { get; }
        bool RefreshQueue { get; set; }

        event Action RecevieRequest;
        event Action DismissRequest;
        event Action<bool> RefreshListRequest;
        event Action<Color> ChangeButtonColor;
        bool HasRights(ISRMCommand botcmd, IChatUser user, CmdFlags flags);
        string QueueMessage(bool QueueState);
        List<JSONObject> ReadJSON(string path);
        void SetRequestStatus(int index, RequestStatus status, bool fromHistory = false);
        void Shuffle<T>(List<T> list);
        void WriteJSON(string path, List<JSONObject> objs);
        void WriteQueueStatusToFile(string status);
        Task Addsongs(ParseState state);
        Task AddsongsFromnewest(ParseState state);
        void Addtolist(IChatUser requestor, string request);
        string AddToTop(ParseState state);
        string Backup();
        string BackupStreamcore(ParseState state);
        Task Ban(ParseState state);
        void Blacklist(int index, bool fromHistory, bool skip);
        void BlockedUserList(IChatUser requestor, string request);
        string ChatMessage(ParseState state);
        string ClearDuplicateList(ParseState state);
        string ClearEvents(ParseState state);
        void ClearList(IChatUser requestor, string request);
        void Clearqueue(IChatUser requestor, string request);
        void ClearSearch(KEYBOARD.KEY key);
        void ClearSearches();
        string CloseQueue(ParseState state);
        void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target);
        bool CreateMD5FromFile(string path, out string hash);
        string CreateMD5FromString(string input);
        SongRequest DequeueRequest(int index, bool updateUI = true);
        void DequeueRequest(SongRequest request, bool updateUI = true);
        string DequeueSong(ParseState state);
        bool DoesContainTerms(string request, ref string[] terms);
        string EventIn(ParseState state);
        string Every(ParseState state);
        bool Filtersong(JSONObject song);
        string GetBeatSaverId(string request);
        TimeSpan GetFileAgeDifference(string filename);
        string GetGCCount(ParseState state);
        Task GetPPData();
        string GetRating(ref JSONObject song, bool mode = true);
        List<JSONObject> GetSongListFromResults(JSONNode result, string SearchString, ref string errorMessage, SongFilter filter = (SongFilter)(-1), string sortby = "-rating", int reverse = 1);
        string GetStarRating(ref JSONObject song, bool mode = true);
        string IsRequestInQueue(string request, bool fast = false);
        string Listaccess(ParseState state);
        void ListList(IChatUser requestor, string request);
        Task Makelistfromsearch(ParseState state);
        void MapperAllowList(IChatUser requestor, string request);
        void MapperBanList(IChatUser requestor, string request);
        bool Mapperfiltered(JSONObject song, bool white);
        string ModAdd(ParseState state);
        void MoveRequestPositionInQueue(IChatUser requestor, string request, bool top);
        void MoveRequestToBottom(IChatUser requestor, string request);
        void MoveRequestToTop(IChatUser requestor, string request);
        void MSD(KEYBOARD.KEY key);
        bool MyChatMessageHandler(IChatMessage msg);
        void Newest(KEYBOARD.KEY key);
        void Next();
        void OpenList(IChatUser requestor, string request);
        string OpenQueue(ParseState state);
        void Parse(IChatUser user, string request, CmdFlags flags = CmdFlags.None, string info = "");
        void Process(int index, bool fromHistory);
        string ProcessSongRequest(ParseState state);
        void QueueChatMessage(string message);
        string Queueduration();
        string Queuelist(ParseState state);
        string QueueLottery(ParseState state);
        void QueueSong(ParseState state, JSONObject song);
        string QueueStatus(ParseState state);
        IEnumerator ReadArchive(ParseState state);
        string Readdeck(ParseState state);
        void ReadRemapList();
        void RefreshSongQuere();
        IEnumerator RefreshSongs(ParseState state);
        void Remap(IChatUser requestor, string request);
        void RemoveFromlist(IChatUser requestor, string request);
        string Restoredeck(ParseState state);
        void RunScript(IChatUser requestor, string request);
        void RunStartupScripts();
        IEnumerator SaveSongDatabase(ParseState state);
        void ScheduledCommand(string command, ElapsedEventArgs e);
        void Search(KEYBOARD.KEY key);
        IChatUser SerchCreateChatUser();
        IEnumerator SetBombState(ParseState state);
        void Showlists(IChatUser requestor, string request);
        string ShowSongLink(ParseState state);
        void Skip(int index, RequestStatus status = RequestStatus.Skipped);
        string SongMsg(ParseState state);
        string SongSearchFilter(JSONObject song, bool fast = false, SongFilter filter = (SongFilter)(-1));
        void ToggleQueue(IChatUser requestor, string request, bool state);
        void Unban(IChatUser requestor, string request);
        void UnfilteredSearch(KEYBOARD.KEY key);
        void UnloadList(IChatUser requestor, string request);
        void Unmap(IChatUser requestor, string request);
        string Unqueuelist(ParseState state);
        void UpdateRequestUI(bool writeSummary = true);
        string Variable(ParseState state);
        void WhiteList(IChatUser requestor, string request);
        string Who(ParseState state);
        void Writedeck(IChatUser requestor, string request);
        void Writelist(IChatUser requestor, string request);
        void WriteQueueSummaryToFile();
        void WriteRemapList();
        void WrongSong(IChatUser requestor, string request);
    }
}
