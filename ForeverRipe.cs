/*
 * Copyright (C) 2024 Game4Freak.io
 * This mod is provided under the Game4Freak EULA.
 * Full legal terms can be found at https://game4freak.io/eula/
 */

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Forever Ripe", "VisEntities", "1.2.0")]
    [Description("Stops plants from dying by keeping them in a ripe state all the time.")]
    public class ForeverRipe : RustPlugin
    {
        #region Fields

        private static ForeverRipe _plugin;
        private static Configuration _config;

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Whitelisted Growables (leave empty to apply to all)")]
            public List<string> WhitelistedGrowables { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();

            if (string.Compare(_config.Version, Version.ToString()) < 0)
                UpdateConfig();

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        private void UpdateConfig()
        {
            PrintWarning("Config changes detected! Updating...");

            Configuration defaultConfig = GetDefaultConfig();

            if (string.Compare(_config.Version, "1.0.0") < 0)
                _config = defaultConfig;

            if (string.Compare(_config.Version, "1.2.0") < 0)
                _config.WhitelistedGrowables = defaultConfig.WhitelistedGrowables;

            PrintWarning("Config update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                WhitelistedGrowables = new List<string>()
            };
        }

        #endregion Configuration

        #region Oxide Hooks

        private void Init()
        {
            _plugin = this;
            PermissionUtil.RegisterPermissions();
        }

        private void Unload()
        {
            _config = null;
            _plugin = null;
        }

        private object OnGrowableStateChange(GrowableEntity growable, PlantProperties.State state)
        {
            if (growable == null || growable.planter == null || state != PlantProperties.State.Dying || growable.harvests >= growable.Properties.maxHarvests)
                return null;

            bool allowAll = _config.WhitelistedGrowables == null || _config.WhitelistedGrowables.Count == 0;
            if (!allowAll && !_config.WhitelistedGrowables.Contains(growable.ShortPrefabName))
                return null;

            BasePlayer owner = FindPlayerById(growable.OwnerID);
            if (owner == null || !PermissionUtil.HasPermission(owner, PermissionUtil.USE))
                return null;

            growable.ChangeState(PlantProperties.State.Ripe, resetAge: false);
            growable.InitializeHealth(1000f, 1000f);
            return true;
        }

        #endregion Oxide Hooks

        #region Permissions

        public static class PermissionUtil
        {
            public const string USE = "foreverripe.use";
            private static readonly List<string> _permissions = new List<string>
            {
                USE
            };

            public static void RegisterPermissions()
            {
                foreach (var permission in _permissions)
                {
                    _plugin.permission.RegisterPermission(permission, _plugin);
                }
            }

            public static bool HasPermission(BasePlayer player, string permissionName)
            {
                return _plugin.permission.UserHasPermission(player.UserIDString, permissionName);
            }
        }

        #endregion Permissions

        #region Helper Functions

        public static BasePlayer FindPlayerById(ulong playerId)
        {
            return RelationshipManager.FindByID(playerId);
        }

        #endregion Helper Functions
    }
}