using System.Collections.Generic;
using System.Threading;
using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Core.Scripts.GameModes;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.CampfireGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.Players.Default;
using UnityEngine;
using UnityEngine.Pool;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes
{
    public class CrossRoadGameMode :  GameMode<bool>
    {
        private SceneReference sceneToLoad;
        public IEnumerable<IPlayer> Players => players.Values;
        public int PlayerCount => players.Count;

        private Dictionary<int, IPlayer> players;
        
        public NpcManager NpcManager { get; private set; }
        
        public CrossRoadGameMode(SceneReference sceneToLoad)
        {
            this.sceneToLoad = sceneToLoad;
        }
        
        protected override async Awaitable Initialize(CancellationToken token)
        {
            sceneToLoad ??= GameSceneSettings.Current.Game;
            await GameController.GameSceneController.LoadSceneWithLoadingScreen(sceneToLoad);

            players = DictionaryPool<int, IPlayer>.Get();
            
            players.Add(0, new DefaultPlayer(GameMetricsSettings.Current.PlayerData));
            
            //creation du manager npc unique au jeu 
            NpcManager = new NpcManager();
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            // créer un context propre au gamemode
            // ici le NpcManager rempli est mit dans le context 
            CrossRoadGameModeContext context = new CrossRoadGameModeContext(this);
            
            //fading screen
            await GameController.GameSceneController.HideLoadingScreen();
            
            //création de la phase de combat
            var battlePhase = new BattlePhase(context);
            await battlePhase.Run();

            //creation de la phase de feu de camp
            var campfirePhase = new CampfirePhase(context);
            await campfirePhase.Run();
            
            var dialoguePhase = new DialoguePhase(context);
            var dialogueResult = await dialoguePhase.Run();
            
            return dialogueResult.value;
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            DictionaryPool<int, IPlayer>.Release(players);

            return base.Dispose(token);
        }
    }
}