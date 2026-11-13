# Sasogine Mobile (experimental)

The template creates a shared Sasogine game app with Android and iOS entry points.
`--MobilePlatform` selects Android, iOS or both; Android is the default.

**Important:** This is a platform integration starter, not a verified turnkey mobile build.
The Sasogine 0.14.1-alpha package list currently has no dedicated Sasogine Android/iOS backend package;
validate compatible MonoGame framework assembly references and platform lifecycle before publishing.
Install workloads with `dotnet workload install android ios` (iOS builds require a suitable Apple toolchain).
Android package identifiers / iOS bundle IDs, icons and signing need project-specific values.

Graphics: `--MobileGraphics OpenGLES` is supported; Vulkan is intentionally not offered without a compatible mobile backend.
