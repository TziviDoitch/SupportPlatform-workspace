---
name: new-task
description: Start a new task — create a branch off origin/main in an isolated git worktree and open it in VS Code. Use when the user runs /new-task or asks to start a new task / feature / branch.
---

Get a short kebab-case task name (from the user's args, or ask for one), then run:

```
powershell -NoProfile -ExecutionPolicy Bypass -File .claude/skills/new-task/new-task.ps1 -Task "<task-name>"
```

The script does everything: `git fetch origin main`, adds a worktree at
`../worktrees/<task-name>` on a new branch `<task-name>` based on `origin/main`
(monorepo — one branch covers client + server), then opens it with `code`.

Report the `worktree:` and `branch:` lines it prints. If it errors (worktree or
branch already exists, etc.), show the message and stop — don't try to fix it.
