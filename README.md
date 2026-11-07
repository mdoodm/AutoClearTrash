# AutoClearTrash

Runs the game's `cleartrash` console command automatically every 3 minutes while you are in a loaded save.

## Install
r2modman: import `mdoodm-AutoClearTrash-1.0.0.zip` with **Import local mod**, or drop `AutoClearTrash.dll` into the profile's `Mods` folder. Requires MelonLoader 0.7.3 (IL2CPP, game 0.4.7).

## Settings (`UserData/MelonPreferences.cfg`, section `[AutoClearTrash]`)
| Setting | Default | Meaning |
|---|---|---|
| Enabled | true | Run automatically. |
| IntervalMinutes | 3 | Real-time minutes between runs (minimum 0.25). The first run happens one interval after the save loads. |
| Command | `cleartrash` | Any console command, as typed in the console. |
| HostOnly | true | Only run as host / single-player. |
| Verbose | true | Log each run to `MelonLoader/Latest.log`. |

## How it works
Every frame it checks: in the `Main` scene, local player exists, save finished loading, and (by default) you are the host. When the interval elapses it calls `Il2CppScheduleOne.Console.SubmitCommand(command)` — the same path as typing the command in the game's console. The timer pauses while loading and resets on each save load.

Built with AI assistance (Claude).
