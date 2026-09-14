# Validated Unity development setup

Validated on 14 September 2026 using Unity 6000.3.6f1.

Open this repository's `unity` folder in Unity Hub and Bezi. Keep the local clone outside OneDrive.

## Included setup

- Unity AI Assistant 2.19.0-pre.2 and its resolved package dependencies.
- Embedded Bezi Plugin 0.115.12, with its asset metadata.
- Unity project settings and generated asset metadata for reproducible imports.
- Missing Vertical, Submit and Cancel mappings required by the runtime UI.

Codex's local Unity relay configuration and application sign-ins are machine-specific and are not stored in this repository. Each developer must connect their own tools to their local `unity` folder.

## Validation

- Unity imported and compiled `Assets/Scenes/Main.unity`.
- Play Mode created Logan, the runtime level and main camera.
- The official Unity relay returned the active project path, scene, version and real Console logs.
- Bezi and Unity both showed green connection indicators after plugin installation.
- All 10 JavaScript tests passed: `node --test tests/animations.test.cjs`.

The Main scene is intentionally empty while editing: the bootstrap script creates the game when Play Mode starts. A saved editable level has not been created.

## Remaining checks

Full Unity gameplay, browser/controller playthroughs, physical tablet tests and platform builds remain pending. Bezi's GitHub endpoint is configured locally, but authenticated repository access from Bezi has not been verified.

Unity Assistant logged Account API and entitlement warnings; the tested local relay still worked. Historical missing-Submit errors may remain in the Console; subsequent Play Mode reads of Vertical, Submit and Cancel succeeded after the mapping fix.
