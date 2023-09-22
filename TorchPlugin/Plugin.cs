using System;
using System.Collections.Generic;
using NLog;
using Sandbox;
using Sandbox.Game;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Torch;
using Torch.API;
using VRage.Game.ModAPI;

namespace AutoPromoTorchFixed
{
    public class AutoPromoTorchFixed : TorchPluginBase
    {
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            torch.GameStateChanged += new TorchGameStateChangedDel(this.Torch_GameStateChanged);
        }

        private void Torch_GameStateChanged(MySandboxGame game, TorchGameState newState)
        {
            if (newState == TorchGameState.Loaded)
            {
                if (MyAPIGateway.Session != null && MyAPIGateway.Session.IsServer)
                {
                    MyVisualScriptLogicProvider.PlayerSpawned = (SingleKeyPlayerEvent)Delegate.Combine(MyVisualScriptLogicProvider.PlayerSpawned, new SingleKeyPlayerEvent(this.PlayerSpawned));
                }
            }
            else if (newState == TorchGameState.Unloading && MyAPIGateway.Session != null && MyAPIGateway.Session.IsServer)
            {
                MyVisualScriptLogicProvider.PlayerSpawned = (SingleKeyPlayerEvent)Delegate.Remove(MyVisualScriptLogicProvider.PlayerSpawned, new SingleKeyPlayerEvent(this.PlayerSpawned));
            }
        }

        private void PlayerSpawned(long playerId)
        {
            all_players.Clear();
            MyAPIGateway.Multiplayer.Players.GetPlayers(all_players, null);
            foreach (IMyPlayer myPlayer in all_players)
            {
                if (myPlayer.IdentityId == playerId && myPlayer.PromoteLevel != MyPromoteLevel.SpaceMaster && myPlayer.PromoteLevel != MyPromoteLevel.Admin)
                {
                    MySession mySession = MyAPIGateway.Session as MySession;
                    if (mySession != null && !promoted_players.Contains(myPlayer))
                    {
                        mySession.SetUserPromoteLevel(myPlayer.SteamUserId, MyPromoteLevel.Admin);
                        Log.Info($"Promoted {myPlayer.DisplayName} to {myPlayer.PromoteLevel}");
                        promoted_players.Add(myPlayer);
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private List<IMyPlayer> all_players = new List<IMyPlayer>();
        private List<IMyPlayer> promoted_players = new List<IMyPlayer>();
    }
}
