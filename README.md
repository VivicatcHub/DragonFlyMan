# 🐜 DragonFlyMan

**Bug Merge** — a small merge & risk game with ants and dragonflies, made in one weekend.

[![Play on itch.io](https://img.shields.io/badge/Play_on-itch.io-FA5C5C?logo=itchdotio&logoColor=white)](https://vivicaty.itch.io/dragonflyman)
[![ARCADIA JAM #4](https://img.shields.io/badge/Game_jam-ARCADIA_JAM_%234-8A2BE2)](https://itch.io/jam/utc-arcadia-jam-4)
![Unity](https://img.shields.io/badge/Unity-6000.4.6f1-000000?logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)

> [!NOTE]
> My first Unity game, made in **2 days** for the [ARCADIA JAM #4](https://itch.io/jam/utc-arcadia-jam-4) (theme: **“Bug”**).
> It is **unfinished**: the focus was on the core gameplay, not on polish.

![Preparation phase: the 3×3 insect grid, the three paths and the Send button](public/dragonflyman.png)

---

## 🎮 Play

👉 **[Play in your browser on itch.io](https://vivicaty.itch.io/dragonflyman)** (HTML5, no install).

## 🛠️ Run locally

**Requirements:** [Unity Hub](https://unity.com/download) with **Unity `6000.4.6f1`**.

1. Clone the repo:
   ```sh
   git clone https://github.com/VivicatcHub/DragonFlyMan.git
   ```
2. In Unity Hub: **Add → Add project from disk** → select the `DragonFlyMan` folder (the first import takes a few minutes).
3. Open `Assets/Scenes/ScenePreparation.unity` and press **▶ Play**.

To build: **File → Build Profiles** → pick **Web** or **Linux** → **Build**.

---

## 📖 How to play

Each **phase** has two steps.

**1. Preparation**
- You get a new **level 1 ant** on your 3×3 grid.
- **Merge:** drop an insect on another one of the **same type and level** to get one insect of the next level (max level 8).
- **Inspect:** drop an insect on the white cell to see its type and level.
- **Send:** drag insects onto the three **paths** (at least one insect), then press **Send**.

**2. Results** — each path has a random type:

| Path | What happens |
| ---- | ------------ |
| 🎁 **Object** | Hides an insect of level 1–8. If the total level you sent is high enough, you get it for sure; each missing level halves your chance. |
| ⚔️ **Enemy** | Your insects attack first. If their total level is at least the enemy's level, the enemy **joins your grid**. Otherwise it strikes back and your insects come back **wounded** (they lose levels, minimum 1). |
| 🪤 **Trap** | Each insect has a chance to come back unharmed (better with a higher total level); otherwise it **loses 1 level**. |

Then drag every insect (and your rewards) back to your grid and press **Next Phase**.

> [!TIP]
> The first 4 phases are easier: enemies are level 1 and there are no traps.

> [!WARNING]
> Insects always come back from a path. They can only be lost if your 3×3 grid is **full** at the end of a phase.

## 🧬 Evolution chains

Each insect type has 8 levels:

![Dragonfly evolution chain, from level 1 to level 8](public/dragonflyman2.png)
![Ant evolution chain, from level 1 to level 8](public/dragonflyman3.png)

---

## 🚧 Limitations

- No game over: insects can't die, so there is no real way to lose.
- No tutorial, no save system, limited balancing.
- Basic UI and animations.

## 🎨 Credits

- Game design & code: [Vivicaty](https://github.com/VivicatcHub)
- Graphics: AI-generated
- Music: a track from the *Mario Kart* series, © Nintendo, used for the jam only (not covered by the license)

## 📜 License

Code under the [Vivicaty attribution license](LICENSE.md): free to reuse for any purpose, including commercial, as long as you credit **Vivicaty**.
See [CONTRIBUTING.md](CONTRIBUTING.md) to contribute.
