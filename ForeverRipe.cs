using Newtonsoft.Json;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Forever Ripe", "VisEntities", "1.0.0")]
    [Description("Stops plants from dying by keeping them always ripe.")]
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
        }

        private void Unload()
        {
            _config = null;
            _plugin = null;
        }

        private void OnGrowableStateChange(GrowableEntity growableEntity, PlantProperties.State state)
        {
            if (growableEntity != null && growableEntity.planter != null && state == PlantProperties.State.Dying)
            {
                if (_config.PlantShortPrefabNames.Contains(growableEntity.ShortPrefabName))
                {
                    growableEntity.ChangeState(PlantProperties.State.Ripe, resetAge: true);
                }
            }
        }

        #endregion Oxide Hooks
    }
}