# Accelerator Unity Level Production Template

This Unity template starts a game level from zero and keeps the full production
chain inspectable from Unity:

`level design -> whitebox -> concept art -> image-to-3D -> placement -> audit`

It includes the Unity REPL toolchain in `Packages/manifest.json`:

- `com.lambda-labs.unity-repl`
- `com.lambda-labs.unity-agent-input`
- `com.lambda-labs.unity-agent-vision`

## Agent Setup

After cloning the repo, open it in Unity and wait for package import to finish.
Then register the Unity REPL skill from the imported package:

```bash
npx skills add ./Packages/com.lambda-labs.unity-repl
```

If that command fails because Node.js or `npx` is unavailable, use your agent
runtime's skill installer with this skill file:

```text
./Packages/com.lambda-labs.unity-repl/.agents/skills/unity-repl/SKILL.md
```

Some Unity installs resolve Git packages into `Library/PackageCache` instead of
materializing them under `Packages`. If `./Packages/com.lambda-labs.unity-repl`
does not exist after import, locate the package directory:

```bash
find Library/PackageCache -maxdepth 1 -type d -name 'com.lambda-labs.unity-repl@*'
```

Then run `npx skills add` on that package directory, or pass its
`.agents/skills/unity-repl/SKILL.md` file to your agent runtime's skill
installer.

Verify the REPL through the skill by evaluating:

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
