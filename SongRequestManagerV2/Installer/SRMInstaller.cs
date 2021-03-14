﻿using SiraUtil;
using SongRequestManagerV2.Bots;
using SongRequestManagerV2.Views;
using Zenject;

namespace SongRequestManagerV2.Installers
{
    public class SRMInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindFactory<KEYBOARD, KEYBOARD.KEYBOARDFactiry>().AsCached();


            this.Container.Bind<SongListUtils>().AsCached();

            this.Container.BindInterfacesAndSelfTo<RequestBotListView>().FromNewComponentAsViewController().AsSingle();
            this.Container.BindInterfacesAndSelfTo<KeyboardViewController>().FromNewComponentAsViewController().AsSingle();
            this.Container.BindInterfacesAndSelfTo<RequestFlowCoordinator>().FromNewComponentOnNewGameObject(nameof(RequestFlowCoordinator)).AsSingle();
            this.Container.BindInterfacesAndSelfTo<SRMButton>().FromNewComponentAsViewController().AsSingle().NonLazy();
        }
    }
}
