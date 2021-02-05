﻿using ChatCore.Utilities;
using SongRequestManagerV2.Bots;
using SongRequestManagerV2.Models;
using SongRequestManagerV2.Utils;
using SiraUtil;

namespace SongRequestManagerV2.Installer
{
    public class SRMAppInstaller : Zenject.Installer
    {
        public override void InstallBindings()
        {
            Container.BindFactory<QueueLongMessage, QueueLongMessage.QueueLongMessageFactroy>().AsCached();
            Container.BindFactory<SongRequest, SongRequest.SongRequestFactory>().AsCached();
            Container.BindFactory<ParseState, ParseState.ParseStateFactory>().AsCached();
            Container.BindFactory<SRMCommand, SRMCommand.SRMCommandFactory>().AsCached();
            Container.BindFactory<JSONObject, string, string, SongMap, SongMap.SongMapFactory>().AsCached();
            Container.Bind<MapDatabase>().AsSingle();
            Container.Bind<RequestManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<CommandManager>().AsSingle();
            this.Container.BindFactory<DynamicText, DynamicText.DynamicTextFactory>().AsCached();
            this.Container.BindInterfacesAndSelfTo<ChatManager>().AsSingle();
            this.Container.BindInterfacesAndSelfTo<StringNormalization>().AsSingle();
            this.Container.BindInterfacesAndSelfTo<NotifySound>().FromNewComponentOnNewGameObject("NotifySound").AsSingle();
            this.Container.Bind<ListCollectionManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<RequestBot>().AsSingle();
        }
    }
}
