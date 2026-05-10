using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace KitforgeLabs.MobileUIKit.Editor.Hub.Help
{
    internal sealed class KitforgeHelpTab
    {
        private const string CheatsheetRelative = "Documentation~/CHEATSHEET.md";
        private const string AuditMenu = "Tools/Kitforge/UI Kit/Audit";
        private const string BuildGroupAMenu = "Tools/Kitforge/UI Kit/Build Group A Sample";
        private static readonly Regex LinkRegex = new(@"\[([^\]]+)\]\(([^)]+)\)", RegexOptions.Compiled);

        private VisualElement _root;
        private string _cheatsheetAbsolutePath;
        private string _packageResolvedPath;
        private string _loadFailureReason;
        private readonly List<HeadingAnchor> _anchors = new();

        private readonly struct HeadingAnchor
        {
            public readonly int Level;
            public readonly string Text;
            public readonly VisualElement Element;
            public HeadingAnchor(int level, string text, VisualElement element) { Level = level; Text = text; Element = element; }
        }

        public VisualElement Build()
        {
            _root = new VisualElement();
            _root.AddToClassList("kfh-help");
            BuildToolbar();
            BuildBody();
            return _root;
        }

        private void BuildToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.AddToClassList("kfh-help-toolbar");
            toolbar.Add(MakeToolbarButton("Open in editor", OpenCheatsheetExternally));
            toolbar.Add(MakeToolbarButton("Run Audit", () => EditorApplication.ExecuteMenuItem(AuditMenu)));
            toolbar.Add(MakeToolbarButton("Build Group A Sample", () => EditorApplication.ExecuteMenuItem(BuildGroupAMenu)));
            _root.Add(toolbar);
        }

        private static Button MakeToolbarButton(string label, System.Action onClick)
        {
            var btn = new Button(onClick) { text = label };
            btn.AddToClassList("kfh-help-toolbar-button");
            return btn;
        }

        private void BuildBody()
        {
            _anchors.Clear();
            var body = new ScrollView(ScrollViewMode.Vertical);
            body.AddToClassList("kfh-help-body");
            var text = LoadCheatsheetText();
            if (text == null) { body.Add(BuildMissingFileMessage()); _root.Add(body); return; }
            var tocHost = new VisualElement();
            tocHost.AddToClassList("kfh-help-toc");
            body.Add(tocHost);
            RenderMarkdown(body, text);
            PopulateToc(tocHost, body);
            _root.Add(body);
        }

        private VisualElement BuildMissingFileMessage()
        {
            var box = new VisualElement();
            box.AddToClassList("kfh-help-missing");
            var msg = new Label(_loadFailureReason ?? $"Could not load CHEATSHEET.md from package. Verify install integrity (expected at <package>/{CheatsheetRelative}).");
            msg.AddToClassList("kfh-help-missing-message");
            box.Add(msg);
            return box;
        }

        private string LoadCheatsheetText()
        {
            var info = PackageInfo.FindForAssembly(typeof(KitforgeHelpTab).Assembly);
            if (info == null)
            {
                _loadFailureReason = "Package metadata not resolved — KitforgeHelpTab assembly is not running from a UPM package context. Ensure the kit is installed via Package Manager (not loose under Assets/).";
                return null;
            }
            _packageResolvedPath = info.resolvedPath;
            _cheatsheetAbsolutePath = Path.Combine(_packageResolvedPath, CheatsheetRelative);
            if (!File.Exists(_cheatsheetAbsolutePath))
            {
                _loadFailureReason = $"CHEATSHEET.md not found at '{_cheatsheetAbsolutePath}'. Verify install integrity — Documentation~/CHEATSHEET.md ships with the package.";
                return null;
            }
            try { return File.ReadAllText(_cheatsheetAbsolutePath); }
            catch (System.IO.IOException ex) { return ReportReadFailure($"Could not read '{_cheatsheetAbsolutePath}' — {ex.Message}. File may be locked."); }
            catch (System.UnauthorizedAccessException ex) { return ReportReadFailure($"Permission denied for '{_cheatsheetAbsolutePath}' — {ex.Message}."); }
        }

        private string ReportReadFailure(string reason)
        {
            _loadFailureReason = reason;
            Debug.LogWarning($"[KitforgeHelpTab] {reason}");
            return null;
        }

        private void OpenCheatsheetExternally()
        {
            if (string.IsNullOrEmpty(_cheatsheetAbsolutePath) || !File.Exists(_cheatsheetAbsolutePath))
            {
                Debug.LogWarning("[KitforgeHelpTab] CHEATSHEET.md not resolved — cannot open externally.");
                return;
            }
            EditorUtility.OpenWithDefaultApp(_cheatsheetAbsolutePath);
        }

        private void RenderMarkdown(ScrollView host, string text)
        {
            var lines = text.Replace("\r\n", "\n").Split('\n');
            var inCodeFence = false;
            var codeBuffer = new System.Text.StringBuilder();
            foreach (var raw in lines)
            {
                if (raw.StartsWith("```"))
                {
                    inCodeFence = FlushOrToggleFence(host, codeBuffer, inCodeFence);
                    continue;
                }
                if (inCodeFence) { codeBuffer.AppendLine(raw); continue; }
                host.Add(BuildLineElement(raw));
            }
            if (inCodeFence) host.Add(BuildCodeBlock(codeBuffer.ToString()));
        }

        private static bool FlushOrToggleFence(ScrollView host, System.Text.StringBuilder buffer, bool inFence)
        {
            if (!inFence) return true;
            host.Add(BuildCodeBlock(buffer.ToString()));
            buffer.Clear();
            return false;
        }

        private VisualElement BuildLineElement(string line)
        {
            if (line.StartsWith("### ")) return BuildHeading(line.Substring(4), 3, "kfh-help-h3");
            if (line.StartsWith("## ")) return BuildHeading(line.Substring(3), 2, "kfh-help-h2");
            if (line.StartsWith("# ")) return BuildHeading(line.Substring(2), 1, "kfh-help-h1");
            if (line.StartsWith("> ")) return BuildLineWithLinks(line.Substring(2), "kfh-help-quote");
            if (line.StartsWith("---")) return MakeSeparator();
            if (line.StartsWith("|")) return BuildLineWithLinks(line, "kfh-help-table-row");
            if (string.IsNullOrWhiteSpace(line)) return MakeSpacer();
            return BuildLineWithLinks(line, "kfh-help-paragraph");
        }

        private VisualElement BuildHeading(string text, int level, string ussClass)
        {
            var label = new Label(text);
            label.AddToClassList(ussClass);
            _anchors.Add(new HeadingAnchor(level, text, label));
            return label;
        }

        private VisualElement BuildLineWithLinks(string line, string ussClass)
        {
            var matches = LinkRegex.Matches(line);
            if (matches.Count == 0) return MakeStyledLabel(line, ussClass);
            var row = new VisualElement();
            row.AddToClassList("kfh-help-line");
            row.AddToClassList(ussClass);
            AppendLineSegments(row, line, matches, ussClass);
            return row;
        }

        private void AppendLineSegments(VisualElement row, string line, MatchCollection matches, string textUssClass)
        {
            var cursor = 0;
            foreach (Match m in matches)
            {
                if (m.Index > cursor) row.Add(MakeStyledLabel(line.Substring(cursor, m.Index - cursor), textUssClass));
                row.Add(MakeLinkElement(m.Groups[1].Value, m.Groups[2].Value));
                cursor = m.Index + m.Length;
            }
            if (cursor < line.Length) row.Add(MakeStyledLabel(line.Substring(cursor), textUssClass));
        }

        private VisualElement MakeLinkElement(string label, string href)
        {
            var link = new Label(label);
            link.AddToClassList("kfh-help-link");
            link.tooltip = href;
            link.RegisterCallback<ClickEvent>(_ => OpenLink(href));
            return link;
        }

        private void OpenLink(string href)
        {
            if (href.StartsWith("http://") || href.StartsWith("https://")) { Application.OpenURL(href); return; }
            if (string.IsNullOrEmpty(_packageResolvedPath))
            {
                Debug.LogWarning($"[KitforgeHelpTab] Cannot resolve relative link '{href}' — package path unknown.");
                return;
            }
            var docsRoot = Path.GetDirectoryName(_cheatsheetAbsolutePath);
            var absolute = Path.GetFullPath(Path.Combine(docsRoot ?? _packageResolvedPath, href));
            if (Directory.Exists(absolute) || File.Exists(absolute))
            {
                EditorUtility.OpenWithDefaultApp(absolute);
                return;
            }
            Debug.LogWarning($"[KitforgeHelpTab] Link target '{absolute}' (from '{href}') does not exist on disk.");
        }

        private static Label MakeStyledLabel(string text, string ussClass)
        {
            var label = new Label(text);
            label.AddToClassList(ussClass);
            return label;
        }

        private static VisualElement MakeSeparator()
        {
            var sep = new VisualElement();
            sep.AddToClassList("kfh-help-separator");
            return sep;
        }

        private static VisualElement MakeSpacer()
        {
            var spacer = new VisualElement();
            spacer.AddToClassList("kfh-help-spacer");
            return spacer;
        }

        private static VisualElement BuildCodeBlock(string code)
        {
            var box = new VisualElement();
            box.AddToClassList("kfh-help-code-block");
            var field = new TextField { value = code.TrimEnd('\n', '\r'), multiline = true, isReadOnly = true };
            field.AddToClassList("kfh-help-code-text");
            box.Add(field);
            return box;
        }

        private void PopulateToc(VisualElement host, ScrollView body)
        {
            if (_anchors.Count == 0) return;
            var header = new Label("Jump to section");
            header.AddToClassList("kfh-help-toc-header");
            host.Add(header);
            var row = new VisualElement();
            row.AddToClassList("kfh-help-toc-row");
            foreach (var anchor in _anchors)
            {
                if (anchor.Level != 2) continue;
                row.Add(MakeTocChip(anchor, body));
            }
            host.Add(row);
        }

        private static VisualElement MakeTocChip(HeadingAnchor anchor, ScrollView body)
        {
            var chip = new Button(() => body.ScrollTo(anchor.Element)) { text = anchor.Text };
            chip.AddToClassList("kfh-help-toc-chip");
            return chip;
        }
    }
}
