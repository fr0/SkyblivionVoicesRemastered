using System.Diagnostics;

namespace SkyblivionVoicesRemastered.App;

public partial class MainForm : Form
{
  private readonly AppSettings _settings = AppSettings.Load();
  // controls that only appear when "Show advanced options" is checked
  private readonly List<Control> _advancedControls;
  private readonly UiLogWriter _logWriter;
  private CancellationTokenSource? _cts;

  public MainForm()
  {
    InitializeComponent();
    _logWriter = new UiLogWriter(_log);
    _advancedControls =
    [
      // an auto-sized grid row collapses to nothing once every control in it is hidden
      pluginsLabel, _plugins,
      workLabel, _work, workBrowse,
      pluginNameLabel, _pluginName,
      xwmaLabel, _xwma, xwmaBrowse,
      lipGenLabel, _lipGen, lipGenBrowse,
      // the whole build options group is advanced; the defaults are the right choice for a normal run
      optionsGroup,
      _scan, _map, _build, _verify,
    ];
    // necesssary for proper DPI scaling
    pathsGrid.ColumnStyles[0].Width = LogicalToDeviceUnits((int)pathsGrid.ColumnStyles[0].Width);
    statusRow.ColumnStyles[0].Width = LogicalToDeviceUnits((int)statusRow.ColumnStyles[0].Width);
    statusRow.RowStyles[0].Height = LogicalToDeviceUnits((int)statusRow.RowStyles[0].Height);
    _lipMode.Width = LogicalToDeviceUnits(_lipMode.Width);
    _parallel.Width = LogicalToDeviceUnits(_parallel.Width);
    _limit.Width = LogicalToDeviceUnits(_limit.Width);
    foreach (ColumnHeader column in _groups.Columns)
      column.Width = LogicalToDeviceUnits(column.Width);
    foreach (ColumnHeader column in _mapStats.Columns)
      column.Width = LogicalToDeviceUnits(column.Width);
    LoadSettings();
  }

  // =============== form events =======================

  private void MainForm_Load(object? sender, EventArgs e)
  {
    // the window size itself is not auto-scaled, so size it for the monitor's DPI and never bigger than the monitor
    var scale = DeviceDpi / 96f;
    var area = Screen.FromControl(this).WorkingArea;
    MinimumSize = new Size((int)(900 * scale), (int)(640 * scale));
    Size = new Size(Math.Min((int)(1120 * scale), area.Width), Math.Min((int)(820 * scale), area.Height));
    if (Left < area.Left || Top < area.Top || Right > area.Right || Bottom > area.Bottom)
      Location = new Point(Math.Max(area.Left, area.Left + (area.Width - Width) / 2), Math.Max(area.Top, area.Top + (area.Height - Height) / 2));
  }

  private void MainForm_Shown(object? sender, EventArgs e)
  {
    AutoDetect(onlyEmpty: true);
    TryLoadExistingResults();
  }

  private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
  {
    SaveSettings();
    _cts?.Cancel();
  }

  // =============== control events =======================

  private void RemasterBrowse_Click(object? sender, EventArgs e) => BrowseFolder(_remaster);
  private void SkyblivionBrowse_Click(object? sender, EventArgs e) => BrowseFolder(_skyblivion);
  private void WorkBrowse_Click(object? sender, EventArgs e) => BrowseFolder(_work);
  private void OutputBrowse_Click(object? sender, EventArgs e) => BrowseFolder(_output);
  private void XwmaBrowse_Click(object? sender, EventArgs e) => BrowseFile(_xwma, "xWMAEncode.exe|xWMAEncode.exe;xwmaencode.exe|Executables|*.exe");
  private void LipGenBrowse_Click(object? sender, EventArgs e) => BrowseFile(_lipGen, "LipGenerator.exe|LipGenerator.exe|Executables|*.exe");
  private void DetectButton_Click(object? sender, EventArgs e) => AutoDetect(onlyEmpty: false);
  private void ShowAdvanced_CheckedChanged(object? sender, EventArgs e) => ApplyAdvancedVisibility();

  private async void Scan_Click(object? sender, EventArgs e) => await RunStep("Scan", s => Pipeline.Scan(s, _logWriter), afterScan: true);
  private async void Map_Click(object? sender, EventArgs e) => await RunStep("Map", s => Pipeline.Map(s, _logWriter), afterMap: true);
  private async void Build_Click(object? sender, EventArgs e) => await RunStep("Build", (s, p, ct) => Pipeline.Build(s, _logWriter, p, ct));
  private async void Verify_Click(object? sender, EventArgs e) => await RunStep("Verify", s => Pipeline.Verify(s, _logWriter));

  private async void All_Click(object? sender, EventArgs e) => await RunStep("Run all", (s, p, ct) =>
  {
    Pipeline.Scan(s, _logWriter);
    ct.ThrowIfCancellationRequested();
    Pipeline.Map(s, _logWriter);
    ct.ThrowIfCancellationRequested();
    return Pipeline.Build(s, _logWriter, p, ct);
  }, afterScan: true, afterMap: true);

  private void Cancel_Click(object? sender, EventArgs e)
  {
    _cts?.Cancel();
    _status.Text = "Cancelling...";
  }

  private void OpenOutput_Click(object? sender, EventArgs e)
  {
    var dir = CurrentSettings().ResolvedOutputDir;
    if (Directory.Exists(dir))
      Process.Start(new ProcessStartInfo("explorer.exe", $"\"{Path.GetFullPath(dir)}\"") { UseShellExecute = true });
    else
      MessageBox.Show(this, "The output folder does not exist yet.", Text);
  }

  private void ApplyAdvancedVisibility()
  {
    SuspendLayout();
    foreach (var c in _advancedControls)
      c.Visible = _showAdvanced.Checked;
    ResumeLayout(true);
  }

  private void BrowseFolder(TextBox target)
  {
    using var dlg = new FolderBrowserDialog { SelectedPath = Directory.Exists(target.Text) ? target.Text : "" };
    if (dlg.ShowDialog(this) == DialogResult.OK)
      target.Text = dlg.SelectedPath;
  }

  private void BrowseFile(TextBox target, string filter)
  {
    using var dlg = new OpenFileDialog { Filter = filter, FileName = File.Exists(target.Text) ? target.Text : "" };
    if (dlg.ShowDialog(this) == DialogResult.OK)
      target.Text = dlg.FileName;
  }

  // ===================== settings ====================

  private void LoadSettings()
  {
    _remaster.Text = _settings.RemasterPath;
    _skyblivion.Text = _settings.SkyblivionDataPath;
    _plugins.Text = _settings.SkyblivionPlugins;
    _work.Text = string.IsNullOrWhiteSpace(_settings.WorkDir)
      ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SkyblivionVoicesRemastered", "work")
      : _settings.WorkDir;
    _output.Text = _settings.OutputDir;
    _pluginName.Text = _settings.PluginName;
    _xwma.Text = _settings.XwmaEncodePath;
    _lipGen.Text = _settings.LipGeneratorPath;
    _lipMode.SelectedItem = _lipMode.Items.Contains(_settings.LipMode) ? _settings.LipMode : "Generate";
    _parallel.Value = Math.Clamp(_settings.Parallelism, 1, 64);
    _limit.Value = Math.Clamp(_settings.Limit, 0, 1_000_000);
    _allVoiceTypes.Checked = _settings.AllVoiceTypes;
    _esl.Checked = _settings.Esl;
    _force.Checked = _settings.Force;
    _showAdvanced.Checked = _settings.ShowAdvanced;
    ApplyAdvancedVisibility();
  }

  private void SaveSettings()
  {
    _settings.RemasterPath = _remaster.Text;
    _settings.SkyblivionDataPath = _skyblivion.Text;
    _settings.SkyblivionPlugins = _plugins.Text;
    _settings.WorkDir = _work.Text;
    _settings.OutputDir = _output.Text;
    _settings.PluginName = _pluginName.Text;
    _settings.XwmaEncodePath = _xwma.Text;
    _settings.LipGeneratorPath = _lipGen.Text;
    _settings.LipMode = _lipMode.SelectedItem?.ToString() ?? "Generate";
    _settings.Parallelism = (int)_parallel.Value;
    _settings.Limit = (int)_limit.Value;
    _settings.AllVoiceTypes = _allVoiceTypes.Checked;
    _settings.Esl = _esl.Checked;
    _settings.Force = _force.Checked;
    _settings.ShowAdvanced = _showAdvanced.Checked;
    _settings.Save();
  }

  private PipelineSettings CurrentSettings()
  {
    var checkedGroups = _groups.CheckedItems.Cast<ListViewItem>().Select(i => i.Text).ToList();
    return new PipelineSettings
    {
      RemasterPath = Blank(_remaster.Text),
      SkyblivionDataPath = Blank(_skyblivion.Text),
      SkyblivionPlugins = string.IsNullOrWhiteSpace(_plugins.Text) ? Constants.SkyblivionESM : _plugins.Text,
      WorkDir = _work.Text,
      OutputDir = Blank(_output.Text),
      PluginName = string.IsNullOrWhiteSpace(_pluginName.Text) ? Constants.PluginName : _pluginName.Text,
      XwmaEncodePath = Blank(_xwma.Text),
      LipGeneratorPath = Blank(_lipGen.Text),
      LipMode = Enum.Parse<LipMode>(_lipMode.SelectedItem?.ToString() ?? "Generate"),
      Parallelism = (int)_parallel.Value,
      Limit = _limit.Value > 0 ? (int)_limit.Value : null,
      AllVoiceTypes = _allVoiceTypes.Checked,
      Esl = _esl.Checked,
      Force = _force.Checked,
      Groups = checkedGroups.Count > 0 ? checkedGroups : null
    };
  }

  private static string? Blank(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

  private void AutoDetect(bool onlyEmpty)
  {
    if (!onlyEmpty || string.IsNullOrWhiteSpace(_remaster.Text))
      _remaster.Text = ToolLocator.FindOblivionRemastered() ?? _remaster.Text;
    var skyrim = ToolLocator.FindSkyrimSpecialEdition();
    if (skyrim != null)
    {
      if (!onlyEmpty || string.IsNullOrWhiteSpace(_skyblivion.Text))
        _skyblivion.Text = Path.Combine(skyrim, "Data");
      if (!onlyEmpty || string.IsNullOrWhiteSpace(_xwma.Text))
        _xwma.Text = ToolLocator.FindTool(ToolLocator.XwmaEncodeRelative, skyrim) ?? _xwma.Text;
      if (!onlyEmpty || string.IsNullOrWhiteSpace(_lipGen.Text))
        _lipGen.Text = ToolLocator.FindTool(ToolLocator.LipGeneratorRelative, skyrim) ?? _lipGen.Text;
    }
    var notes = new List<string>();
    if (string.IsNullOrWhiteSpace(_remaster.Text))
      notes.Add("Oblivion Remastered not found");
    if (string.IsNullOrWhiteSpace(_xwma.Text))
      notes.Add("xWMAEncode.exe not found (install the Skyrim SE Creation Kit)");
    if (string.IsNullOrWhiteSpace(_lipGen.Text))
      notes.Add("LipGenerator.exe not found (lip files will be reused instead of generated)");
    _status.Text = notes.Count == 0 ? "Game installs and Creation Kit tools detected." : string.Join("; ", notes);
  }

  private void TryLoadExistingResults()
  {
    try
    {
      var s = CurrentSettings();
      if (File.Exists(s.ScanPath))
        ShowScan(Pipeline.LoadScan(s));
      if (File.Exists(s.MapPath))
        ShowMap(Pipeline.LoadMap(s));
    }
    catch (Exception ex) { _logWriter.WriteLine($"Could not load previous results: {ex.Message}"); }
  }

  // ==================== running steps =================

  private Task RunStep(string name, Action<PipelineSettings> action, bool afterScan = false, bool afterMap = false)
      => RunStep(name, (s, _, _) => { action(s); return null; }, afterScan, afterMap);

  private async Task RunStep(string name, Func<PipelineSettings, IProgress<VoiceBuildProgress>, CancellationToken, object?> action, bool afterScan = false, bool afterMap = false)
  {
    if (_cts != null)
      return;
    SaveSettings();
    var settings = CurrentSettings();
    _cts = new CancellationTokenSource();
    SetBusy(true);
    _status.Text = $"{name} running...";
    _progress.Value = 0;
    _logWriter.WriteLine($"=== {name} started {DateTime.Now:T} ===");
    var progress = new Progress<VoiceBuildProgress>(p =>
    {
      _progress.Value = p.Total == 0 ? 0 : (int)(1000L * p.Done / p.Total);
      var remaining = p.PerSecond > 0
        ? TimeSpan.FromSeconds((p.Total - p.Done) / p.PerSecond)
        : TimeSpan.Zero;
      _status.Text = $"{name}: {p.Done:N0} / {p.Total:N0} recordings ({p.PerSecond:F1}/s, about {remaining:h\\:mm\\:ss} left) written={p.Written:N0} skipped={p.Skipped:N0} failed={p.Failed:N0}";
    });
    var sw = Stopwatch.StartNew();
    try
    {
      var token = _cts.Token;
      var result = await Task.Run(() => action(settings, progress, token), token);
      if (afterScan && File.Exists(settings.ScanPath))
        ShowScan(Pipeline.LoadScan(settings));
      if (afterMap && File.Exists(settings.MapPath))
        ShowMap(Pipeline.LoadMap(settings));
      var cancelled = result is BuildResult { Cancelled: true };
      _status.Text = cancelled
        ? $"{name} cancelled after {sw.Elapsed:h\\:mm\\:ss}."
        : $"{name} finished in {sw.Elapsed:h\\:mm\\:ss}.";
      _progress.Value = cancelled ? _progress.Value : 1000;
      _logWriter.WriteLine($"=== {name} {(cancelled ? "cancelled" : "finished")} in {sw.Elapsed:h\\:mm\\:ss} ===");
      if (result is BuildResult b && !cancelled)
        MessageBox.Show(this, $"Mod folder ready:\n{Path.GetFullPath(b.OutputDir)}\n\nVoice types: {b.Patch.VoiceTypes}\nNPC overrides: {b.Patch.NpcOverrides}\nVoice files written: {b.Voices.Written:N0} (skipped {b.Voices.Skipped:N0}, failed {b.Voices.Failed:N0})\n\nInstall the folder as a mod and load {settings.PluginName} after Skyblivion.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    catch (OperationCanceledException)
    {
      _status.Text = $"{name} cancelled.";
      _logWriter.WriteLine($"=== {name} cancelled ===");
    }
    catch (Exception ex)
    {
      _status.Text = $"{name} failed: {ex.Message}";
      _logWriter.WriteLine($"ERROR: {ex}");
      MessageBox.Show(this, ex.Message, $"{name} failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
      _cts.Dispose();
      _cts = null;
      SetBusy(false);
    }
  }

  private void SetBusy(bool busy)
  {
    foreach (var b in new[] { _scan, _map, _build, _verify, _all })
      b.Enabled = !busy;
    _cancel.Enabled = busy;
    UseWaitCursor = busy;
  }

  private void ShowScan(ScanResult scan)
  {
    var previouslyChecked = _groups.CheckedItems
      .Cast<ListViewItem>()
      .Select(i => i.Text)
      .ToHashSet(StringComparer.OrdinalIgnoreCase);
    _groups.BeginUpdate();
    _groups.Items.Clear();
    foreach (var g in scan.Groups)
    {
      var item = new ListViewItem([g.Key, g.LineCount.ToString("N0"), g.NpcCount.ToString("N0"), g.VoiceTypeEditorId]) { Checked = previouslyChecked.Contains(g.Key) };
      if (g.NpcCount == 0)
        item.ForeColor = SystemColors.GrayText;
      _groups.Items.Add(item);
    }
    _groups.EndUpdate();
  }

  private void ShowMap(MapResult map)
  {
    _mapStats.BeginUpdate();
    _mapStats.Items.Clear();
    foreach (var kv in map.Stats)
      _mapStats.Items.Add(new ListViewItem([kv.Key, kv.Value.ToString("N0")]));
    foreach (var w in map.Warnings)
      _mapStats.Items.Add(new ListViewItem(["WARNING: " + w, ""]) { ForeColor = Color.DarkRed });
    _mapStats.EndUpdate();
  }
}
