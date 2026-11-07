using System;
using MelonLoader;
using UnityEngine;
using AutoClearTrash;

[assembly: MelonInfo(typeof(AutoClearTrashMod), "AutoClearTrash", "1.0.0", "mdoodm")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace AutoClearTrash
{
    /// <summary>
    /// Runs a console command (default "cleartrash") every N real-time minutes while in a loaded save.
    /// Uses the game's own console: Il2CppScheduleOne.Console.SubmitCommand(string).
    /// </summary>
    public class AutoClearTrashMod : MelonMod
    {
        MelonPreferences_Entry<bool> _enabled, _hostOnly, _verbose;
        MelonPreferences_Entry<float> _interval;
        MelonPreferences_Entry<string> _command;

        bool _inMain;
        float _next = -1f;
        int _runs;

        public override void OnInitializeMelon()
        {
            var cat = MelonPreferences.CreateCategory("AutoClearTrash", "Auto Clear Trash");
            _enabled = cat.CreateEntry("Enabled", true, description: "Run the command automatically.");
            _interval = cat.CreateEntry("IntervalMinutes", 3f, description: "Real-time minutes between runs (minimum 0.25).");
            _command = cat.CreateEntry("Command", "cleartrash", description: "Console command to run (same text you would type in the console).");
            _hostOnly = cat.CreateEntry("HostOnly", true, description: "Only run when you are the host / single-player (clients cannot clear world trash).");
            _verbose = cat.CreateEntry("Verbose", true, description: "Log each run.");
            LoggerInstance.Msg($"init: Enabled={_enabled.Value} Interval={_interval.Value}min Command='{_command.Value}' HostOnly={_hostOnly.Value}");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            _inMain = sceneName == "Main";
            _next = -1f; // start the timer once the player exists
            if (_verbose.Value) LoggerInstance.Msg($"scene '{sceneName}' -> {(_inMain ? "timer armed" : "timer idle")}");
        }

        public override void OnUpdate()
        {
            if (!_enabled.Value || !_inMain) return;
            float interval = Math.Max(0.25f, _interval.Value) * 60f;
            float now = Time.unscaledTime;

            if (!Ready()) { _next = -1f; return; }
            if (_next < 0f) { _next = now + interval; if (_verbose.Value) LoggerInstance.Msg($"save loaded; first run in {interval / 60f:0.##} min"); return; }
            if (now < _next) return;

            _next = now + interval;
            Run();
        }

        bool Ready()
        {
            try
            {
                if (Il2CppScheduleOne.PlayerScripts.Player.Local == null) return false;
                var lm = Il2CppScheduleOne.Persistence.LoadManager.Instance;
                if (lm != null && lm.IsLoading) return false;
                if (_hostOnly.Value && !Il2CppFishNet.InstanceFinder.IsServer) return false;
                return true;
            }
            catch { return false; }
        }

        void Run()
        {
            string cmd = (_command.Value ?? "").Trim();
            if (cmd.Length == 0) return;
            try
            {
                Il2CppScheduleOne.Console.SubmitCommand(cmd);
                _runs++;
                if (_verbose.Value) LoggerInstance.Msg($"ran '{cmd}' (#{_runs})");
            }
            catch (Exception e)
            {
                LoggerInstance.Error($"running '{cmd}' failed: {e.Message}");
            }
        }
    }
}
