# Workflow Tips

## Git config (local repo)

```bash
git config pull.rebase true
git config rebase.autoStash true
git config commit.template .gitmessage
```

With auto-stash, `git pull` stashes unstaged changes, rebases, then pops them back safely.

## Branch workflow (recommended)

- Work on feature branches: `git checkout -b feature/<name>`
- Commit small and often
- Push branch: `git push -u origin feature/<name>`
- Create PR to main
- Avoid long-lived direct commits to main when collaborating

## Quick commands

- Stash manually: `git stash push -u -m "wip"`
- Restore: `git stash pop`
- See what's changed: `git status --short; git diff --stat`
- Rebase current on main: `git fetch && git rebase origin/main`

## For AI agents

- Keep AGENTS.md, docs/INIT_PROMPT.md, opencode_setup.md aligned with project direction
- Never commit secrets, never copy competitor full content
- Mark UNVERIFIED/LEGAL-REVIEW as needed
- Smallest diffs, plan before code
