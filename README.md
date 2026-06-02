# Accelerator Unity Level Production Template

This Unity template starts a game level from zero and keeps the full production
chain inspectable from Unity:

`level design -> whitebox -> concept art -> exploded assets -> 3D/CSG -> placement -> audit`

The repository is intentionally an empty production template. It must not carry
prebuilt scenes, prefabs, meshes, materials, textures, concept images, CSG
proxies, 3D proxies, or previous-run outputs. Every Game Jam run should create a
fresh run/output directory, a fresh Unity scene, and newly generated assets from
the capability chain.

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

Agents should build the production graph using Accelerator capability IDs:

- `gp-level-plan`: create the level plan from a gameplay brief.
- `gp-level-layout`: turn the plan into a spatial floor layout.
- `gp-whitebox`: use Unity REPL agentically to create a Unity whitebox scene,
  source screenshot, and whitebox manifest from the layout.
- `art-beautify`: turn the whitebox screenshot into concept art.
- `art-scene-plan`: extract scene semantics and placement constraints.
- `art-explode`: turn the concept image into an exploded asset sheet.
- `art-cut`: cut part crops from the exploded image.
- `art-pack`: pack part crops into atlas images.
- `art-enhance`: enhance pack images for 3D reconstruction.
- `art-image-to-csg-proxy` or `art-image-to-3d`: generate the CSG proxy or mesh
  from this run's generated images. Do not use preset CSG or 3D proxy assets.
- `art-normalize`: normalize meshes for Unity placement.
- `unity-repl`: apply generated assets to the Unity scene and audit coverage.

## REPL Commands

Open the project in Unity, then create a fresh empty production scene if the
agent needs a scene container before running `gp-whitebox`:

```bash
cargo run -p accelerator-cli -- node run unity-repl \
  --output-dir runs/unity-template-fresh-scene \
  --input 'csharp_code=AcceleratorTemplate.Editor.AcceleratorLevelProductionTemplate.CreateFreshProductionScene()' \
  --option project_root=/path/to/accelerator-unity-level-template
```

Audit the currently open scene after the capability chain has produced and
placed generated assets:

```bash
cargo run -p accelerator-cli -- node run unity-repl \
  --output-dir runs/unity-template-audit \
  --input 'csharp_code=AcceleratorTemplate.Editor.AcceleratorLevelProductionTemplate.AuditActiveScene()' \
  --option project_root=/path/to/accelerator-unity-level-template
```

```text
Accelerator > Level Production > Create Fresh Production Scene
Accelerator > Level Production > Audit Active Scene
```

The template utilities only create empty containers and audit the active scene.
They do not generate seed targets, preset whitebox geometry, placeholder art, CSG
proxies, or meshes. Production usage should run the level, whitebox, art, 3D,
normalization, and Unity placement capabilities for each Game Jam topic from
zero.
