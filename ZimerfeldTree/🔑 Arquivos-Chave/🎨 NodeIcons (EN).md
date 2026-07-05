---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [arquivo, icones, gdi, resources, imagelist]
arquivo: src/GitExtensions.ZimerfeldTree/NodeIcons.cs
---

# NodeIcons.cs

16×16 tree icons — embedded PNGs with a GDI+ drawn fallback. ~381 lines.

**Path:** `src/GitExtensions.ZimerfeldTree/NodeIcons.cs`

---

## How it works

- Generates/loads the 16×16 icons of the tree's `ImageList`. Indices: **0–4** generic, **5–7** sections, **8–15** GitFlow/leaf.
- **`LoadEmbedded`** reads the resource by `GitExtensions.ZimerfeldTree.Resources.<file>` and resizes it to 16×16. If missing/unreadable, it falls back to the **GDI+ backup glyph** → the build never breaks for a missing image (each `<EmbeddedResource>` is `Condition="Exists(...)"`).
- **Remote group (`origin`)** uses `Resources\origin.png` (rocket) via `NodeIcons.Remote`, mapped in `GetFolderIconIndex`.
- **Develop (index 9)** uses `Resources\develop.png`, fallback `Wrench()`.

## Node PNGs (Resources/)

`master.png`, `develop.png`, `feature.png`, `folha.png`, `release.png`, `origin.png`, `remote-branch.png`, `tag.png` + the `ctx-*.png` of the context menu. `ctx-pull`/`ctx-push` (blue ↓ arrow / green ↑ arrow) also feed the Pull/Push buttons.

## Related

- [[🌿 BranchNode (EN)|BranchNode]]
- [[🖼️ PluginIcon (EN)|PluginIcon]]
- [[🪟 BranchHierarchyForm (EN)|BranchHierarchyForm]]
