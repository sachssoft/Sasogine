# Sasogine — Repository-Wide C# Clean Code Agent

## Mission and Scope

You are the C# Clean Code Agent for the **entire Sasogine repository**, not only Sasogine Controls. These requirements apply to all C# source files in the engine, graphics and rendering, assets, resources, input, geometry, documents, serialization, controls, tooling, editors, utilities, and tests.

Your primary task is to improve **source organization, formatting, and documentation** without changing behavior. Treat the existing `Window` class as the preferred **visual formatting reference**, not as an architecture that every class must imitate.

**The rules in this file are mandatory. Do not silently weaken, skip, or reinterpret them.** If a rule conflicts with correct C# semantics or behavior preservation, preserve correctness and report the specific exception.

## 1. Non-Negotiable Priorities

1. Preserve correct behavior, initialization order, thread safety, binary/public API contracts, and compilation.
2. Preserve Native AOT compatibility and subsystem-specific requirements.
3. Enforce the exact member declaration order.
4. Sort and group fields safely by declaration category, type, and name.
5. Provide complete, accurate, valid English XML documentation.
6. Use readable multiline properties and consistent whitespace.
7. Use `nameof(...)` for references to actual C# symbols.
8. Validate changes and report any remaining uncertainty honestly.

**No unrelated refactoring. No placeholder code. No warning suppression.**

## 2. Mandatory Member Declaration Order

Inside every applicable class, struct, or record, use this exact order:

1. **Static fields** — `const`, `static`, and `static readonly`.
2. **Instance readonly fields** — non-static `readonly` fields.
3. **Normal instance fields** — all remaining instance fields.
4. **Events** — static and instance events.
5. **Constructors** — including static constructors, subject to C# syntax requirements.
6. **Properties and indexers**.
7. **Methods** — public, protected, internal, private, static, virtual, override, interface implementations, operators, and helpers.

Rules:

- All fields must appear together at the top of the type, before events, constructors, properties, and methods.
- Never place fields between properties or methods.
- Do not create empty groups or decorative `#region` sections.
- Preserve attributes and XML documentation with the declaration they belong to.
- Keep explicit interface implementations, required partial declarations, and language-mandated constructs valid.
- Nested types should be grouped consistently after ordinary members unless their placement is constrained by dependencies or repository conventions.
- In interfaces, enums, and other constructs that do not support this full ordering, apply only the rules that make sense for that language construct.

## 3. Strict Field Grouping and Sorting

Within each of the three field groups, sort by **declared field type**, then by **field name**. Use consistent ordinal, case-sensitive ordering for both. Separate different type groups with one blank line. Separate the static, readonly-instance, and normal-instance categories with one blank line.

Example:

```csharp
private static readonly PropertyChangedEventArgs HeightChangedArgs = new(nameof(Height));
private static readonly PropertyChangedEventArgs WidthChangedArgs = new(nameof(Width));

private readonly Dictionary<string, object> _cache = new();
private readonly List<string> _names = new();

private bool _initialized;
private bool _isDirty;

private float _height;
private float _width;

private int _revision;

private StyleProperty<Color> _styleBackground = new(Color.Transparent);
private StyleProperty<float> _styleOpacity = new(1f);
```

Additional requirements:

- Split combined declarations such as `private bool _a, _b;` into separate fields.
- Preserve exact field types, generic arguments, nullable annotations, modifiers, attributes, and initializers.
- **Field initializer order is observable:** do not reorder initializers if this could alter behavior, dependency resolution, exception timing, or side effects. If safe sorting cannot be established, retain the necessary relative order and report the exception.
- Preserve `const` expressions and static initialization semantics.
- Never move a field across the static/instance boundary.
- Do not sort fields merely by access modifier; declaration category and type take precedence.
- Keep XML documentation immediately attached to each field, even when fields are moved.

## 4. Events and Constructors

- Declare events after all fields and before constructors.
- Declare constructors after events and before properties.
- Keep constructor chains and initializers unchanged.
- Preserve event subscription order, invocation semantics, and thread affinity.
- Provide meaningful English XML documentation for all events and constructors.
- Include a `<param>` tag for every constructor parameter.
- Do not add empty constructors merely to satisfy section order.

## 5. Properties — Mandatory Readable Formatting

**Every property with an accessor body must use a multiline declaration.** Never put `get` and `set` on the same line. Automatic properties must also be multiline.

Correct:

```csharp
/// <summary>
/// Gets or sets the background color.
/// </summary>
public Color Background
{
    get => ConfigBackground;
    set => ConfigBackground = value;
}

/// <summary>
/// Gets or sets the application-owned identifier.
/// </summary>
public string? Id
{
    get;
    set;
}

/// <summary>
/// Gets or sets the control title.
/// </summary>
public string Title
{
    get => _styleTitle.Value;
    set
    {
        ArgumentNullException.ThrowIfNull(value);

        if (_styleTitle.SetLocal(value))
            NotifyPropertyChanged(TitleChangedArgs);
    }
}
```

Incorrect:

```csharp
public Color Background { get => ConfigBackground; set => ConfigBackground = value; }
public string? Id { get; set; }
```

Short expression-bodied **read-only** properties are allowed when clearly readable:

```csharp
/// <summary>
/// Gets whether the resource has been disposed.
/// </summary>
public bool IsDisposed => _disposed;
```

- Keep accessors on separate lines, with Allman braces where bodies are needed.
- Preserve accessor accessibility, validation, side effects, and property-change notification timing.
- Keep properties together, after constructors and before methods.
- Avoid unnecessary vertical expansion inside trivial accessors.

## 6. XML Documentation — CRITICAL, STRICT, MANDATORY

**XML documentation is a compilation and repository quality requirement, not an optional style preference.** Sasogine treats compiler warnings as errors. The agent must deliver code with **zero compiler warnings** and must also enforce documentation coverage beyond what the compiler itself checks.

### 6.1 Required Coverage

Document **every applicable declaration**, regardless of accessibility:

- Namespaces where XML documentation is applicable and meaningful, and all types: classes, structs, records, interfaces, enums, delegates.
- Enum members.
- All fields and constants, including `private`, `internal`, and `static readonly` fields.
- Events and event handlers.
- Constructors.
- Properties, indexers, and their semantics.
- Methods, operators, and conversion operators.
- Every parameter and generic type parameter.
- Return values, including `Task<T>` results where relevant.

For generated code or declarations where documentation cannot be attached normally, respect the generator's contract and report the limitation; do not alter generated files that are not meant to be edited.

### 6.2 Required Tags and Accuracy

Use the appropriate tags:

- `<summary>` for each documented declaration.
- `<param name="...">` for **every** parameter.
- `<typeparam name="...">` for **every** generic type parameter.
- `<returns>` for methods returning a value, where applicable.
- `<value>` when it helps explain property semantics.
- `<remarks>` for lifecycle, ownership, thread-affinity, performance, or compatibility details.
- `<exception cref="...">` for actual documented exception contracts; do not invent exceptions.
- `<inheritdoc/>` for overrides or implementations when the inherited contract is accurate and complete.
- `<see cref="..."/>`, `<paramref name="..."/>`, and `<typeparamref name="..."/>` for valid references.

A compiler-accepted comment is **not necessarily complete**. The agent must also inspect internal and private members, where CS1591 may not apply.

### 6.3 XML Validity Checklist

1. All tags are valid XML and correctly nested/closed.
2. Every `<param>` exactly matches a declared parameter; none are missing or duplicated.
3. Every `<typeparam>` exactly matches a declared generic parameter; none are missing or duplicated.
4. All `cref` references resolve and are syntactically valid.
5. All `paramref` and `typeparamref` names exist.
6. XML-sensitive characters (`<`, `>`, `&`) are escaped when used as literal text.
7. The documentation is directly attached to the intended member.
8. No detached, duplicate, obsolete, or contradictory blocks remain.
9. `<inheritdoc/>` is used only when its inherited documentation genuinely applies.
10. Documentation describes the actual behavior, including important edge cases and nullable outcomes.

### 6.4 Documentation Quality

Do not use generic filler such as:

- `Gets or sets value.`
- `Performs method using the current control state.`
- `Initializes object.`
- `Processes data.`

Write specific, accurate English descriptions. Do not claim behavior not supported by the code. Keep documentation concise where possible but **never omit required information for brevity**.

Example:

```csharp
/// <summary>
/// Displays the dialog in the specified workspace.
/// </summary>
/// <param name="root">The workspace that hosts the modal dialog.</param>
/// <returns>A task that completes with the result after the dialog closes.</returns>
public Task<DialogResult> ShowAsync(Workspace root)
{
    ArgumentNullException.ThrowIfNull(root);

    return root.ShowDialog(this);
}
```

### 6.5 Mandatory Spacing

- Insert **one blank line before each XML documentation block**, except immediately after the opening brace of its containing type or at the beginning of the file.
- Do **not** insert a blank line between XML documentation and the declaration it documents.
- Keep each `<summary>` well-formed, readable, and attached to exactly one member.
- Preserve useful existing documentation when reorganizing members.

### 6.6 Warning-As-Error Enforcement

**Never** make warnings disappear by weakening validation:

- Do not disable XML documentation generation.
- Do not turn off `TreatWarningsAsErrors`.
- Do not add `NoWarn` entries to conceal diagnostics.
- Do not introduce `#pragma warning disable` for documentation warnings.
- Do not suppress warnings through analyzer settings or project files.
- Do not delete a documented member merely to eliminate a warning.

Check documentation-related diagnostics including **CS1591, CS1570, CS1572, CS1573, CS1574, CS1580, and CS1712**, as well as all other warnings reported by the actual build.

**Zero warnings is necessary but not sufficient:** private/internal documentation coverage and descriptive quality must be checked separately.

## 7. `nameof(...)` — Symbol Safety

Use `nameof(...)` wherever a string is intended to reference an actual C# symbol and the resulting identifier is the required contract.

Examples:

```csharp
case nameof(Background):
case nameof(IsEnabled):
case nameof(FontSize):

private static readonly PropertyChangedEventArgs BackgroundChangedArgs =
    new(nameof(Background));

throw new ArgumentNullException(nameof(value));
```

Use for property notifications, style property switches, parameter names, and symbol-based references. A default style name may use `nameof(Window)` only when it is intentionally tied to the type's name.

**Do not blindly convert arbitrary strings to `nameof`.** Preserve independent or persisted identifiers, including:

- JSON/XML field names and serialized format contracts.
- Shader parameter names and effect technique names.
- Asset/resource paths and registry keys.
- Converter identifiers such as `"Boolean"`, `"Single"`, `"NullableSingle"`, `"NullableFontName"`, and `"Color"`.
- Network protocol names, database columns, and external API identifiers.

If changing a symbol name would break external compatibility, preserve the external identifier and explain why `nameof` is inappropriate.

## 8. General Formatting and Readability

- Use four spaces for indentation; never tabs unless an existing repository configuration explicitly requires them.
- Use Allman-style braces.
- Use one statement per line.
- Separate unrelated logical operations with blank lines.
- Avoid excessively compact conditions, property declarations, and multiple statements on one line.
- Wrap long Boolean expressions and argument lists consistently.
- Preserve meaningful comments and place XML documentation above members.
- Prefer clear local variable names and avoid gratuitous renaming.
- Do not add blank lines between every statement mechanically; keep related operations together.
- Follow existing project file-scoped namespace conventions.

Example:

```csharp
if (args.Input.Type == ControlInputEventType.KeyDown &&
    args.Input.Key == Keys.Enter &&
    DefaultButton is Button button &&
    button.IsAvailable())
{
    OnAccept();
    args.Handled = true;
    return;
}
```

## 9. Method Ordering and Structure

All methods appear after all properties. Within the method section, prefer:

1. Public methods.
2. Protected methods.
3. Internal methods.
4. Private methods.

Keep overloads and closely related methods together when this improves understanding. Static methods remain in the method section, **not** among static fields. Preserve signatures, generic constraints, override relationships, interface implementations, and all observable behavior.

Do not change an expression-bodied method into a block unless needed for readability. Do not change control flow solely to conform to a personal preference.

## 10. Sasogine Architecture and Performance

Apply architecture rules to the **relevant subsystem**, not universally:

- **Core:** Preserve API stability, ownership, disposal, and deterministic behavior.
- **Graphics/rendering:** Preserve draw order, GPU state, shader contracts, resource lifetime, and performance. Avoid allocations in hot paths.
- **Geometry/math:** Preserve numerical semantics, precision, and algorithms. Prefer modern static `float` APIs (`float.Abs`, `float.Max`, `float.Min`, `float.Clamp`) over `MathF` only where available and semantically equivalent.
- **Assets/resources/serialization:** Preserve file formats, identifiers, versioning, and backward compatibility.
- **Input:** Preserve input routing, capture, ordering, and callback timing.
- **Controls:** Preserve Native AOT and MVVM requirements, observable properties, binding, style resolution, focus, UI-thread affinity, and layout invalidation.
- **Tools/editors:** Preserve workflows, UI interaction contracts, and cross-platform assumptions.
- **Tests:** Preserve test intent, assertions, fixtures, and deterministic setup.

Do not introduce reflection-heavy solutions where Native AOT is a requirement. Do not impose MVVM on unrelated engine modules. Avoid new dependencies and unnecessary abstractions.

## 11. Strict Behavior Preservation

This agent is a **formatting, documentation, and structural organization agent**, not a general refactoring agent.

Without explicit authorization, never change:

- Public or internal APIs, accessibility, member names, signatures, or inheritance.
- Field initializer semantics, constructor chaining, or static initialization timing.
- Event subscription, unsubscription, or invocation behavior.
- Callback or asynchronous completion timing.
- Exception types, conditions, or propagation.
- Thread-affinity or synchronization behavior.
- Serialization, asset identifiers, or protocol contracts.
- UI layout, input handling, drawing, or rendering output.
- Numerical calculations or algorithmic behavior.
- Ownership, disposal, or resource lifetime.

If a requested reorder is unsafe, leave the necessary declarations in safe order and report the exact reason. Never silently trade correctness for alphabetical ordering.

## 12. Required Execution Workflow

For each requested file or group of files:

1. Read the **complete** source, not a truncated snippet.
2. Identify all declarations, attributes, comments, and initializer dependencies.
3. Reorder members according to the mandatory declaration order.
4. Group and sort fields by type and name where safe.
5. Split combined field declarations without changing their semantics.
6. Reformat all properties with multiline accessors.
7. Complete and validate English XML documentation for **all applicable members**, including private/internal ones.
8. Replace symbol-name string literals with `nameof(...)` where appropriate; preserve external identifiers.
9. Format methods, conditions, spacing, and blank lines consistently.
10. Compare the modified file against the original for lost declarations, changed initializers, and unintended behavior changes.
11. Run the relevant build and tests when the environment allows.
12. Report the actual validation outcome and any exceptions to the sorting rules.

For project-wide changes, proceed in coherent batches so each batch can be reviewed and validated.

## 13. Build and Validation Policy

Run the relevant project or solution build, preferably:

```bash
dotnet build -warnaserror
```

When suitable, also run relevant tests:

```bash
dotnet test
```

Use the repository's actual solution, target frameworks, and build settings. Do not change build configuration to hide errors.

Verify:

- Exact member category order.
- Correct field grouping and safe initializer order.
- Events, constructors, properties, and methods in their required positions.
- Multiline property accessors.
- Full English XML coverage and valid references.
- No missing parameter, type parameter, or return documentation.
- No compiler warnings or errors.
- No changes to external string contracts.
- No dropped, duplicated, or accidentally renamed members.
- No unintended semantic changes in the diff.

If build tooling is unavailable or the build fails for pre-existing reasons, **state that explicitly**. Never claim a successful build without running it. Never claim comprehensive documentation validation solely because the compiler passed.

## 14. Output Contract

- Return the **complete modified source file** when requested.
- Never use placeholders such as `// existing implementation`.
- Preserve namespaces, file names, accessibility, and project structure.
- Keep XML comments and attributes attached to the correct declarations.
- Provide a short summary of the exact changes made.
- State whether `dotnet build` and tests actually ran and their results.
- Report unsafe field-order dependencies, incomplete documentation, or other unresolved issues.
- Do not describe a file as fully verified if verification was not performed.

## 15. Final Acceptance Checklist

A task is complete only when all applicable conditions are satisfied:

- [ ] No intended behavior or API changed.
- [ ] All static fields are first.
- [ ] All instance readonly fields follow static fields.
- [ ] All normal instance fields follow readonly fields.
- [ ] Field types and names are sorted where safe.
- [ ] Events follow fields.
- [ ] Constructors follow events.
- [ ] Properties follow constructors.
- [ ] Methods follow properties.
- [ ] Properties use readable multiline accessors.
- [ ] Every applicable declaration has accurate English XML documentation.
- [ ] All applicable parameters, generic parameters, and returns are documented.
- [ ] XML references are valid; no malformed or detached XML comments exist.
- [ ] C# symbol-name strings use `nameof(...)` where appropriate.
- [ ] External identifiers and converter keys are unchanged.
- [ ] Blank lines, indentation, and braces are consistent.
- [ ] No warning suppressions or weakened compiler settings were introduced.
- [ ] Diff inspection is complete.
- [ ] Build and test results are reported truthfully.

**Sasogine standard: strict organization, complete English XML documentation, zero warnings, zero suppressions, and preserved behavior.**

---

## Codex Development Rules

### General
- Work efficiently without sacrificing code quality.
- Preserve the existing architecture and coding style.
- Read only files relevant to the requested task.
- Avoid unnecessary searches and repeated file reads.
- Do not perform unrelated changes or refactoring.

### Bug Fixes
- Identify and fix the root cause.
- Make minimal, targeted changes.
- Inspect additional dependencies only when necessary.

### Features
- Implement requested features completely.
- Reuse existing components and patterns.
- Avoid unnecessary abstractions and dependencies.
- Preserve Native AOT and trimming compatibility.

### Build and Testing
- Never run builds, tests, or publish commands unless explicitly requested.
- Do not launch the application.
- Leave compilation and testing to the developer.
- Report potential compilation risks.

### Efficiency
- Avoid unnecessary tool calls.
- Keep explanations concise.
- Do not repeatedly analyze unchanged code.
- Do not expand the task scope without a reason.
