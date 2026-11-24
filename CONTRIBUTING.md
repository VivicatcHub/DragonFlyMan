# Contributing to DragonFlyMan

Thanks for your interest! 🎉

## Workflow

> [!IMPORTANT]
> Please **open a pull request** — don't push directly to `main`.

1. Fork the repo and create a branch: `git checkout -b feat/my-feature`
2. Open the project with **Unity `6000.4.6f1`** (via Unity Hub → *Add project from disk*).
3. Make your changes and check that the game still runs:
   - open `Assets/Scenes/ScenePreparation.unity` and press **Play**;
   - play at least one full phase (Preparation → Results → next phase).
4. Commit following the convention below.
5. Open a pull request describing **what** you changed and **why**.

> [!NOTE]
> Don't commit build outputs (`Build/`, `BuildWeb/`) or Unity caches (`Library/`, `Temp/`): they are ignored on purpose.

## Commit convention

This project uses [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<optional scope>): <short description>
```

| Type       | Use for                                   |
| ---------- | ----------------------------------------- |
| `feat`     | a new feature                             |
| `fix`      | a bug fix                                 |
| `docs`     | documentation only                        |
| `style`    | formatting, no code change                |
| `refactor` | code change that is neither fix nor feat  |
| `perf`     | performance improvement                   |
| `test`     | adding or fixing tests                    |
| `chore`    | tooling, dependencies, config, assets     |

Examples:

```
feat(merge): allow merging level 8 insects
fix(ui): reset the Next Phase button label
docs(readme): update screenshots
```

## Reporting a bug

Open an issue with steps to reproduce, what you expected and what happened.
