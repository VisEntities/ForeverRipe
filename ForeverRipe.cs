/*
 * Copyright (C) 2024 Game4Freak.io
 * This mod is provided under the Game4Freak EULA.
 * Full legal terms can be found at https://game4freak.io/eula/
 */

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Forever Ripe", "VisEntities", "1.1.0")]
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

            [JsonProperty("Plant Short Prefab Names")]
            public List<string> PlantShortPrefabNames { get; set; }
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

            PrintWarning("Config update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                PlantShortPrefabNames = new List<string>
                {
                    "corn.entity",
                    "hemp.entity",
                    "pumpkin.entity",
                    "potato.entity"
                }
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

        private object OnGrowableStateChange(GrowableEntity growableEntity, PlantProperties.State state)
        {
            if (growableEntity != null && growableEntity.planter != null && state == PlantProperties.State.Dying && growableEntity.harvests < growableEntity.Properties.maxHarvests)
            {
                if (_config.PlantShortPrefabNames.Contains(growableEntity.ShortPrefabName))
                {
                    BasePlayer owner = FindById(growableEntity.OwnerID);
                    if (owner != null && PermissionUtil.HasPermission(owner, PermissionUtil.USE))
                    {
                        growableEntity.ChangeState(PlantProperties.State.Ripe, resetAge: false);
                        growableEntity.InitializeHealth(1000f, 1000f);
                        return true;
                    }
                }
            }

            return null;
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

        public static BasePlayer FindById(ulong playerId)
        {
            return RelationshipManager.FindByID(playerId);
        }

        #endregion Helper Functions
    }
}