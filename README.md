# Accelerator Unity Level Production Template

This Unity template starts a game level from zero and keeps the full production
chain inspectable from Unity:

`level design -> whitebox -> concept art -> image-to-3D -> placement -> audit`

It includes the Unity REPL toolchain in `Packages/manifest.json`:

- `com.lambda-labs.unity-repl`
- `com.lambda-labs.unity-agent-input`
- `com.lambda-labs.unity-agent-vision`

## Agent Setup

This template is preconfigured with the Unity REPL skill installed from
`LambdaLabsHQ/unity-repl` for both Codex and Claude Code:

```text
.agents/skills/unity-repl/SKILL.md
.claude/skills/unity-repl/SKILL.md
```

Agent runtimes that scan those project skill directories can load Unity REPL
directly from the template without a manual registration step.

After cloning the repo, open it in Unity and wait for package import to finish.
The skill will resolve the REPL package from either:

```text
Packages/com.lambda-labs.unity-repl
Library/PackageCache/com.lambda-labs.unity-repl@*
```

Verify the REPL through the project skill by evaluating:

```csharp
Application.unityVersion
```

## Unity Version

The template does not intentionally depend on a specific Unity editor version.
Use the Unity version standardized by your project.

Unity still records the editor that last saved a project in
`ProjectSettings/ProjectVersion.txt`; if you open the project with a different
editor, Unity may prompt to upgrade project metadata. That file is not a product
requirement. This repository's scripts avoid Unity-version-specific gameplay APIs
where practical and are designed around standard Editor primitives plus Unity
REPL.

## Capability Chain

The template exports a graph using Accelerator capability IDs:

- `gp-level-plan`: create the level plan from a gameplay brief.
- `gp-level-layout`: turn the plan into a spatial floor layout.
- `gp-whitebox`: create a Unity whitebox scene and source whitebox screenshot.
- `2d-prop-ref`: generate concept reference images for each whitebox target.
- `art-image-to-3d`: generate raw GLB meshes from concept images.
- `art-normalize`: normalize meshes for Unity placement.
- `unity-repl`: apply generated assets to the Unity scene and audit coverage.

## REPL Commands

Open the project in Unity, then export the from-zero production graph:

```bash
cargo run -p accelerator-cli -- node run unity-repl \
  --output-dir runs/unity-template-graph \
  --input 'csharp_code=AcceleratorTemplate.Editor.AcceleratorLevelProductionTemplate.ExportFromZeroProductionGraph()' \
  --option project_root=/path/to/accelerator-unity-level-template
```

Create the template whitebox scene from the default seed brief:

```bash
cargo run -p accelerator-cli -- node run unity-repl \
  --output-dir runs/unity-template-whitebox \
  --input 'csharp_code=AcceleratorTemplate.Editor.AcceleratorLevelProductionTemplate.GenerateSeedWhitebox()' \
  --option project_root=/path/to/accelerator-unity-level-template
```

Apply generated placeholders and audit placement coverage:

```bash
cargo run -p accelerator-cli -- node run unity-repl \
  --output-dir runs/unity-template-placeholders \
  --input 'csharp_code=AcceleratorTemplate.Editor.AcceleratorLevelProductionTemplate.ApplyGeneratedAssetPlaceholders()' \
  --option project_root=/path/to/accelerator-unity-level-template
```

```text
Accelerator > Level Production > Export From-Zero Graph
Accelerator > Level Production > Generate Seed Whitebox
Accelerator > Level Production > Apply Generated Placeholders
Accelerator > Level Production > Audit Placements
```

`GenerateSeedWhitebox()` is a local deterministic stand-in for the first
`gp-whitebox` run. Production usage should call the exported graph's level design
and whitebox capabilities, then use `unity-repl` to apply the generated manifest
or place provider-generated meshes into the same target slots.
