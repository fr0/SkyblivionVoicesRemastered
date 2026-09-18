namespace SkyblivionVoicesRemastered.App;

partial class MainForm
{
  /// <summary>Required designer variable.</summary>
  private System.ComponentModel.IContainer components = null;

  /// <summary>Clean up any resources being used.</summary>
  /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
  protected override void Dispose(bool disposing)
  {
    if (disposing && (components != null))
    {
      components.Dispose();
    }
    base.Dispose(disposing);
  }

  #region Windows Form Designer generated code

  /// <summary>
  /// Required method for Designer support - do not modify
  /// the contents of this method with the code editor.
  /// </summary>
  private void InitializeComponent()
  {
    root = new TableLayoutPanel();
    pathsGroup = new GroupBox();
    pathsGrid = new TableLayoutPanel();
    remasterLabel = new Label();
    _remaster = new TextBox();
    remasterBrowse = new Button();
    skyblivionLabel = new Label();
    _skyblivion = new TextBox();
    skyblivionBrowse = new Button();
    pluginsLabel = new Label();
    _plugins = new TextBox();
    workLabel = new Label();
    _work = new TextBox();
    workBrowse = new Button();
    outputLabel = new Label();
    _output = new TextBox();
    outputBrowse = new Button();
    pluginNameLabel = new Label();
    _pluginName = new TextBox();
    xwmaLabel = new Label();
    _xwma = new TextBox();
    xwmaBrowse = new Button();
    lipGenLabel = new Label();
    _lipGen = new TextBox();
    lipGenBrowse = new Button();
    detectButton = new Button();
    optionsAndButtons = new TableLayoutPanel();
    optionsGroup = new GroupBox();
    optionRows = new TableLayoutPanel();
    optionRow1 = new FlowLayoutPanel();
    lipModePanel = new FlowLayoutPanel();
    lipModeLabel = new Label();
    _lipMode = new ComboBox();
    parallelPanel = new FlowLayoutPanel();
    parallelLabel = new Label();
    _parallel = new NumericUpDown();
    limitPanel = new FlowLayoutPanel();
    limitLabel = new Label();
    _limit = new NumericUpDown();
    optionRow2 = new FlowLayoutPanel();
    _allVoiceTypes = new CheckBox();
    _esl = new CheckBox();
    _force = new CheckBox();
    buttons = new FlowLayoutPanel();
    _scan = new Button();
    _map = new Button();
    _build = new Button();
    _verify = new Button();
    _all = new Button();
    _cancel = new Button();
    _openOutput = new Button();
    _showAdvanced = new CheckBox();
    results = new TableLayoutPanel();
    groupsPanel = new TableLayoutPanel();
    groupsHeader = new Label();
    _groups = new ListView();
    groupsColumnKey = new ColumnHeader();
    groupsColumnRecordings = new ColumnHeader();
    groupsColumnNpcs = new ColumnHeader();
    groupsColumnVoiceType = new ColumnHeader();
    mapStatsPanel = new TableLayoutPanel();
    mapStatsHeader = new Label();
    _mapStats = new ListView();
    mapStatsColumnName = new ColumnHeader();
    mapStatsColumnCount = new ColumnHeader();
    logPanel = new TableLayoutPanel();
    logHeader = new Label();
    _log = new TextBox();
    statusRow = new TableLayoutPanel();
    _progress = new ProgressBar();
    _status = new Label();
    root.SuspendLayout();
    pathsGroup.SuspendLayout();
    pathsGrid.SuspendLayout();
    optionsAndButtons.SuspendLayout();
    optionsGroup.SuspendLayout();
    optionRows.SuspendLayout();
    optionRow1.SuspendLayout();
    lipModePanel.SuspendLayout();
    parallelPanel.SuspendLayout();
    ((System.ComponentModel.ISupportInitialize)_parallel).BeginInit();
    limitPanel.SuspendLayout();
    ((System.ComponentModel.ISupportInitialize)_limit).BeginInit();
    optionRow2.SuspendLayout();
    buttons.SuspendLayout();
    results.SuspendLayout();
    groupsPanel.SuspendLayout();
    mapStatsPanel.SuspendLayout();
    logPanel.SuspendLayout();
    statusRow.SuspendLayout();
    SuspendLayout();
    // 
    // root
    // 
    root.ColumnCount = 1;
    root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    root.Controls.Add(pathsGroup, 0, 0);
    root.Controls.Add(optionsAndButtons, 0, 1);
    root.Controls.Add(results, 0, 2);
    root.Controls.Add(logPanel, 0, 3);
    root.Controls.Add(statusRow, 0, 4);
    root.Dock = DockStyle.Fill;
    root.Location = new Point(0, 0);
    root.Name = "root";
    root.Padding = new Padding(8);
    root.RowCount = 5;
    root.RowStyles.Add(new RowStyle());
    root.RowStyles.Add(new RowStyle());
    root.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
    root.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
    root.RowStyles.Add(new RowStyle());
    root.Size = new Size(1104, 781);
    root.TabIndex = 0;
    // 
    // pathsGroup
    // 
    pathsGroup.AutoSize = true;
    pathsGroup.Controls.Add(pathsGrid);
    pathsGroup.Dock = DockStyle.Top;
    pathsGroup.Location = new Point(11, 11);
    pathsGroup.Name = "pathsGroup";
    pathsGroup.Padding = new Padding(8);
    pathsGroup.Size = new Size(1082, 401);
    pathsGroup.TabIndex = 0;
    pathsGroup.TabStop = false;
    pathsGroup.Text = "Paths";
    // 
    // pathsGrid
    // 
    pathsGrid.AutoSize = true;
    pathsGrid.ColumnCount = 3;
    pathsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350F));
    pathsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    pathsGrid.ColumnStyles.Add(new ColumnStyle());
    pathsGrid.Controls.Add(remasterLabel, 0, 0);
    pathsGrid.Controls.Add(_remaster, 1, 0);
    pathsGrid.Controls.Add(remasterBrowse, 2, 0);
    pathsGrid.Controls.Add(skyblivionLabel, 0, 1);
    pathsGrid.Controls.Add(_skyblivion, 1, 1);
    pathsGrid.Controls.Add(skyblivionBrowse, 2, 1);
    pathsGrid.Controls.Add(pluginsLabel, 0, 2);
    pathsGrid.Controls.Add(_plugins, 1, 2);
    pathsGrid.Controls.Add(workLabel, 0, 3);
    pathsGrid.Controls.Add(_work, 1, 3);
    pathsGrid.Controls.Add(workBrowse, 2, 3);
    pathsGrid.Controls.Add(outputLabel, 0, 4);
    pathsGrid.Controls.Add(_output, 1, 4);
    pathsGrid.Controls.Add(outputBrowse, 2, 4);
    pathsGrid.Controls.Add(pluginNameLabel, 0, 5);
    pathsGrid.Controls.Add(_pluginName, 1, 5);
    pathsGrid.Controls.Add(xwmaLabel, 0, 6);
    pathsGrid.Controls.Add(_xwma, 1, 6);
    pathsGrid.Controls.Add(xwmaBrowse, 2, 6);
    pathsGrid.Controls.Add(lipGenLabel, 0, 7);
    pathsGrid.Controls.Add(_lipGen, 1, 7);
    pathsGrid.Controls.Add(lipGenBrowse, 2, 7);
    pathsGrid.Controls.Add(detectButton, 2, 8);
    pathsGrid.Dock = DockStyle.Top;
    pathsGrid.Location = new Point(8, 32);
    pathsGrid.Name = "pathsGrid";
    pathsGrid.RowCount = 9;
    pathsGrid.RowStyles.Add(new RowStyle());
    pathsGrid.RowStyles.Add(new RowStyle());
    pathsGrid.RowStyles.Add(new RowStyle());
    pathsGrid.RowStyles.Add(new RowStyle());
    pathsGrid.RowStyles.Add(new RowStyle());
    pathsGrid.RowStyles.Add(new RowStyle());
    pathsGrid.RowStyles.Add(new RowStyle());
    pathsGrid.RowStyles.Add(new RowStyle());
    pathsGrid.RowStyles.Add(new RowStyle());
    pathsGrid.Size = new Size(1066, 361);
    pathsGrid.TabIndex = 0;
    // 
    // remasterLabel
    // 
    remasterLabel.Dock = DockStyle.Fill;
    remasterLabel.Location = new Point(3, 0);
    remasterLabel.Name = "remasterLabel";
    remasterLabel.Size = new Size(344, 41);
    remasterLabel.TabIndex = 0;
    remasterLabel.Text = "Oblivion Remastered install folder";
    remasterLabel.TextAlign = ContentAlignment.MiddleLeft;
    // 
    // _remaster
    // 
    _remaster.Dock = DockStyle.Fill;
    _remaster.Location = new Point(353, 3);
    _remaster.Name = "_remaster";
    _remaster.Size = new Size(587, 31);
    _remaster.TabIndex = 1;
    // 
    // remasterBrowse
    // 
    remasterBrowse.AutoSize = true;
    remasterBrowse.Location = new Point(946, 3);
    remasterBrowse.Name = "remasterBrowse";
    remasterBrowse.Size = new Size(91, 35);
    remasterBrowse.TabIndex = 2;
    remasterBrowse.Text = "Browse...";
    remasterBrowse.UseVisualStyleBackColor = true;
    remasterBrowse.Click += RemasterBrowse_Click;
    // 
    // skyblivionLabel
    // 
    skyblivionLabel.Dock = DockStyle.Fill;
    skyblivionLabel.Location = new Point(3, 41);
    skyblivionLabel.Name = "skyblivionLabel";
    skyblivionLabel.Size = new Size(344, 41);
    skyblivionLabel.TabIndex = 3;
    skyblivionLabel.Text = "Skyrim SE Data folder (with Skyblivion)";
    skyblivionLabel.TextAlign = ContentAlignment.MiddleLeft;
    // 
    // _skyblivion
    // 
    _skyblivion.Dock = DockStyle.Fill;
    _skyblivion.Location = new Point(353, 44);
    _skyblivion.Name = "_skyblivion";
    _skyblivion.Size = new Size(587, 31);
    _skyblivion.TabIndex = 4;
    // 
    // skyblivionBrowse
    // 
    skyblivionBrowse.AutoSize = true;
    skyblivionBrowse.Location = new Point(946, 44);
    skyblivionBrowse.Name = "skyblivionBrowse";
    skyblivionBrowse.Size = new Size(91, 35);
    skyblivionBrowse.TabIndex = 5;
    skyblivionBrowse.Text = "Browse...";
    skyblivionBrowse.UseVisualStyleBackColor = true;
    skyblivionBrowse.Click += SkyblivionBrowse_Click;
    // 
    // pluginsLabel
    // 
    pluginsLabel.Dock = DockStyle.Fill;
    pluginsLabel.Location = new Point(3, 82);
    pluginsLabel.Name = "pluginsLabel";
    pluginsLabel.Size = new Size(344, 37);
    pluginsLabel.TabIndex = 6;
    pluginsLabel.Text = "Skyblivion plugin(s)";
    pluginsLabel.TextAlign = ContentAlignment.MiddleLeft;
    // 
    // _plugins
    // 
    _plugins.Dock = DockStyle.Fill;
    _plugins.Location = new Point(353, 85);
    _plugins.Name = "_plugins";
    _plugins.Size = new Size(587, 31);
    _plugins.TabIndex = 7;
    // 
    // workLabel
    // 
    workLabel.Dock = DockStyle.Fill;
    workLabel.Location = new Point(3, 119);
    workLabel.Name = "workLabel";
    workLabel.Size = new Size(344, 41);
    workLabel.TabIndex = 8;
    workLabel.Text = "Work folder (scan/map results)";
    workLabel.TextAlign = ContentAlignment.MiddleLeft;
    // 
    // _work
    // 
    _work.Dock = DockStyle.Fill;
    _work.Location = new Point(353, 122);
    _work.Name = "_work";
    _work.Size = new Size(587, 31);
    _work.TabIndex = 9;
    // 
    // workBrowse
    // 
    workBrowse.AutoSize = true;
    workBrowse.Location = new Point(946, 122);
    workBrowse.Name = "workBrowse";
    workBrowse.Size = new Size(91, 35);
    workBrowse.TabIndex = 10;
    workBrowse.Text = "Browse...";
    workBrowse.UseVisualStyleBackColor = true;
    workBrowse.Click += WorkBrowse_Click;
    // 
    // outputLabel
    // 
    outputLabel.Dock = DockStyle.Fill;
    outputLabel.Location = new Point(3, 160);
    outputLabel.Name = "outputLabel";
    outputLabel.Size = new Size(344, 41);
    outputLabel.TabIndex = 11;
    outputLabel.Text = "Output mod folder";
    outputLabel.TextAlign = ContentAlignment.MiddleLeft;
    // 
    // _output
    // 
    _output.Dock = DockStyle.Fill;
    _output.Location = new Point(353, 163);
    _output.Name = "_output";
    _output.Size = new Size(587, 31);
    _output.TabIndex = 12;
    // 
    // outputBrowse
    // 
    outputBrowse.AutoSize = true;
    outputBrowse.Location = new Point(946, 163);
    outputBrowse.Name = "outputBrowse";
    outputBrowse.Size = new Size(91, 35);
    outputBrowse.TabIndex = 13;
    outputBrowse.Text = "Browse...";
    outputBrowse.UseVisualStyleBackColor = true;
    outputBrowse.Click += OutputBrowse_Click;
    // 
    // pluginNameLabel
    // 
    pluginNameLabel.Dock = DockStyle.Fill;
    pluginNameLabel.Location = new Point(3, 201);
    pluginNameLabel.Name = "pluginNameLabel";
    pluginNameLabel.Size = new Size(344, 37);
    pluginNameLabel.TabIndex = 14;
    pluginNameLabel.Text = "Patch plugin file name";
    pluginNameLabel.TextAlign = ContentAlignment.MiddleLeft;
    // 
    // _pluginName
    // 
    _pluginName.Dock = DockStyle.Fill;
    _pluginName.Location = new Point(353, 204);
    _pluginName.Name = "_pluginName";
    _pluginName.Size = new Size(587, 31);
    _pluginName.TabIndex = 15;
    // 
    // xwmaLabel
    // 
    xwmaLabel.Dock = DockStyle.Fill;
    xwmaLabel.Location = new Point(3, 238);
    xwmaLabel.Name = "xwmaLabel";
    xwmaLabel.Size = new Size(344, 41);
    xwmaLabel.TabIndex = 16;
    xwmaLabel.Text = "xWMAEncode.exe (Creation Kit)";
    xwmaLabel.TextAlign = ContentAlignment.MiddleLeft;
    // 
    // _xwma
    // 
    _xwma.Dock = DockStyle.Fill;
    _xwma.Location = new Point(353, 241);
    _xwma.Name = "_xwma";
    _xwma.Size = new Size(587, 31);
    _xwma.TabIndex = 17;
    // 
    // xwmaBrowse
    // 
    xwmaBrowse.AutoSize = true;
    xwmaBrowse.Location = new Point(946, 241);
    xwmaBrowse.Name = "xwmaBrowse";
    xwmaBrowse.Size = new Size(91, 35);
    xwmaBrowse.TabIndex = 18;
    xwmaBrowse.Text = "Browse...";
    xwmaBrowse.UseVisualStyleBackColor = true;
    xwmaBrowse.Click += XwmaBrowse_Click;
    // 
    // lipGenLabel
    // 
    lipGenLabel.Dock = DockStyle.Fill;
    lipGenLabel.Location = new Point(3, 279);
    lipGenLabel.Name = "lipGenLabel";
    lipGenLabel.Size = new Size(344, 41);
    lipGenLabel.TabIndex = 19;
    lipGenLabel.Text = "LipGenerator.exe (Creation Kit)";
    lipGenLabel.TextAlign = ContentAlignment.MiddleLeft;
    // 
    // _lipGen
    // 
    _lipGen.Dock = DockStyle.Fill;
    _lipGen.Location = new Point(353, 282);
    _lipGen.Name = "_lipGen";
    _lipGen.Size = new Size(587, 31);
    _lipGen.TabIndex = 20;
    // 
    // lipGenBrowse
    // 
    lipGenBrowse.AutoSize = true;
    lipGenBrowse.Location = new Point(946, 282);
    lipGenBrowse.Name = "lipGenBrowse";
    lipGenBrowse.Size = new Size(91, 35);
    lipGenBrowse.TabIndex = 21;
    lipGenBrowse.Text = "Browse...";
    lipGenBrowse.UseVisualStyleBackColor = true;
    lipGenBrowse.Click += LipGenBrowse_Click;
    // 
    // detectButton
    // 
    detectButton.AutoSize = true;
    detectButton.Location = new Point(946, 323);
    detectButton.Name = "detectButton";
    detectButton.Size = new Size(117, 35);
    detectButton.TabIndex = 22;
    detectButton.Text = "Auto-detect";
    detectButton.UseVisualStyleBackColor = true;
    detectButton.Click += DetectButton_Click;
    // 
    // optionsAndButtons
    // 
    optionsAndButtons.AutoSize = true;
    optionsAndButtons.ColumnCount = 1;
    optionsAndButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    optionsAndButtons.Controls.Add(optionsGroup, 0, 0);
    optionsAndButtons.Controls.Add(buttons, 0, 1);
    optionsAndButtons.Dock = DockStyle.Top;
    optionsAndButtons.Location = new Point(11, 418);
    optionsAndButtons.Name = "optionsAndButtons";
    optionsAndButtons.RowCount = 2;
    optionsAndButtons.RowStyles.Add(new RowStyle());
    optionsAndButtons.RowStyles.Add(new RowStyle());
    optionsAndButtons.Size = new Size(1082, 167);
    optionsAndButtons.TabIndex = 1;
    // 
    // optionsGroup
    // 
    optionsGroup.AutoSize = true;
    optionsGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
    optionsGroup.Controls.Add(optionRows);
    optionsGroup.Dock = DockStyle.Top;
    optionsGroup.Location = new Point(3, 3);
    optionsGroup.Name = "optionsGroup";
    optionsGroup.Padding = new Padding(8);
    optionsGroup.Size = new Size(1076, 114);
    optionsGroup.TabIndex = 0;
    optionsGroup.TabStop = false;
    optionsGroup.Text = "Build options";
    // 
    // optionRows
    // 
    optionRows.AutoSize = true;
    optionRows.ColumnCount = 1;
    optionRows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    optionRows.Controls.Add(optionRow1, 0, 0);
    optionRows.Controls.Add(optionRow2, 0, 1);
    optionRows.Dock = DockStyle.Top;
    optionRows.Location = new Point(8, 32);
    optionRows.Name = "optionRows";
    optionRows.RowCount = 2;
    optionRows.RowStyles.Add(new RowStyle());
    optionRows.RowStyles.Add(new RowStyle());
    optionRows.Size = new Size(1060, 74);
    optionRows.TabIndex = 0;
    // 
    // optionRow1
    // 
    optionRow1.AutoSize = true;
    optionRow1.Controls.Add(lipModePanel);
    optionRow1.Controls.Add(parallelPanel);
    optionRow1.Controls.Add(limitPanel);
    optionRow1.Location = new Point(0, 0);
    optionRow1.Margin = new Padding(0);
    optionRow1.Name = "optionRow1";
    optionRow1.Size = new Size(606, 39);
    optionRow1.TabIndex = 0;
    optionRow1.WrapContents = false;
    // 
    // lipModePanel
    // 
    lipModePanel.AutoSize = true;
    lipModePanel.Controls.Add(lipModeLabel);
    lipModePanel.Controls.Add(_lipMode);
    lipModePanel.Location = new Point(0, 0);
    lipModePanel.Margin = new Padding(0, 0, 16, 0);
    lipModePanel.Name = "lipModePanel";
    lipModePanel.Size = new Size(199, 39);
    lipModePanel.TabIndex = 0;
    lipModePanel.WrapContents = false;
    // 
    // lipModeLabel
    // 
    lipModeLabel.AutoSize = true;
    lipModeLabel.Location = new Point(0, 6);
    lipModeLabel.Margin = new Padding(0, 6, 4, 0);
    lipModeLabel.Name = "lipModeLabel";
    lipModeLabel.Size = new Size(79, 25);
    lipModeLabel.TabIndex = 0;
    lipModeLabel.Text = "Lip sync:";
    // 
    // _lipMode
    // 
    _lipMode.DropDownStyle = ComboBoxStyle.DropDownList;
    _lipMode.Items.AddRange(new object[] { "Generate", "Reuse", "None" });
    _lipMode.Location = new Point(86, 3);
    _lipMode.Name = "_lipMode";
    _lipMode.Size = new Size(110, 33);
    _lipMode.TabIndex = 1;
    // 
    // parallelPanel
    // 
    parallelPanel.AutoSize = true;
    parallelPanel.Controls.Add(parallelLabel);
    parallelPanel.Controls.Add(_parallel);
    parallelPanel.Location = new Point(215, 0);
    parallelPanel.Margin = new Padding(0, 0, 16, 0);
    parallelPanel.Name = "parallelPanel";
    parallelPanel.Size = new Size(151, 37);
    parallelPanel.TabIndex = 1;
    parallelPanel.WrapContents = false;
    // 
    // parallelLabel
    // 
    parallelLabel.AutoSize = true;
    parallelLabel.Location = new Point(0, 6);
    parallelLabel.Margin = new Padding(0, 6, 4, 0);
    parallelLabel.Name = "parallelLabel";
    parallelLabel.Size = new Size(81, 25);
    parallelLabel.TabIndex = 0;
    parallelLabel.Text = "Workers:";
    // 
    // _parallel
    // 
    _parallel.Location = new Point(88, 3);
    _parallel.Maximum = new decimal(new int[] { 64, 0, 0, 0 });
    _parallel.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
    _parallel.Name = "_parallel";
    _parallel.Size = new Size(60, 31);
    _parallel.TabIndex = 1;
    _parallel.Value = new decimal(new int[] { 1, 0, 0, 0 });
    // 
    // limitPanel
    // 
    limitPanel.AutoSize = true;
    limitPanel.Controls.Add(limitLabel);
    limitPanel.Controls.Add(_limit);
    limitPanel.Location = new Point(382, 0);
    limitPanel.Margin = new Padding(0, 0, 16, 0);
    limitPanel.Name = "limitPanel";
    limitPanel.Size = new Size(208, 37);
    limitPanel.TabIndex = 2;
    limitPanel.WrapContents = false;
    // 
    // limitLabel
    // 
    limitLabel.AutoSize = true;
    limitLabel.Location = new Point(0, 6);
    limitLabel.Margin = new Padding(0, 6, 4, 0);
    limitLabel.Name = "limitLabel";
    limitLabel.Size = new Size(118, 25);
    limitLabel.TabIndex = 0;
    limitLabel.Text = "Limit (0 = all):";
    // 
    // _limit
    // 
    _limit.Increment = new decimal(new int[] { 100, 0, 0, 0 });
    _limit.Location = new Point(125, 3);
    _limit.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
    _limit.Name = "_limit";
    _limit.Size = new Size(80, 31);
    _limit.TabIndex = 1;
    // 
    // optionRow2
    // 
    optionRow2.AutoSize = true;
    optionRow2.Controls.Add(_allVoiceTypes);
    optionRow2.Controls.Add(_esl);
    optionRow2.Controls.Add(_force);
    optionRow2.Location = new Point(0, 45);
    optionRow2.Margin = new Padding(0, 6, 0, 0);
    optionRow2.Name = "optionRow2";
    optionRow2.Size = new Size(692, 29);
    optionRow2.TabIndex = 1;
    optionRow2.WrapContents = false;
    // 
    // _allVoiceTypes
    // 
    _allVoiceTypes.AutoSize = true;
    _allVoiceTypes.Location = new Point(0, 0);
    _allVoiceTypes.Margin = new Padding(0, 0, 16, 0);
    _allVoiceTypes.Name = "_allVoiceTypes";
    _allVoiceTypes.Size = new Size(266, 29);
    _allVoiceTypes.TabIndex = 0;
    _allVoiceTypes.Text = "Include unused voice groups";
    _allVoiceTypes.UseVisualStyleBackColor = true;
    // 
    // _esl
    // 
    _esl.AutoSize = true;
    _esl.Location = new Point(282, 0);
    _esl.Margin = new Padding(0, 0, 16, 0);
    _esl.Name = "_esl";
    _esl.Size = new Size(180, 29);
    _esl.TabIndex = 1;
    _esl.Text = "Flag plugin as ESL";
    _esl.UseVisualStyleBackColor = true;
    // 
    // _force
    // 
    _force.AutoSize = true;
    _force.Location = new Point(478, 0);
    _force.Margin = new Padding(0, 0, 16, 0);
    _force.Name = "_force";
    _force.Size = new Size(198, 29);
    _force.TabIndex = 2;
    _force.Text = "Rebuild existing files";
    _force.UseVisualStyleBackColor = true;
    // 
    // buttons
    // 
    buttons.AutoSize = true;
    buttons.Controls.Add(_scan);
    buttons.Controls.Add(_map);
    buttons.Controls.Add(_build);
    buttons.Controls.Add(_verify);
    buttons.Controls.Add(_all);
    buttons.Controls.Add(_cancel);
    buttons.Controls.Add(_openOutput);
    buttons.Controls.Add(_showAdvanced);
    buttons.Dock = DockStyle.Top;
    buttons.Location = new Point(3, 123);
    buttons.Name = "buttons";
    buttons.Size = new Size(1076, 41);
    buttons.TabIndex = 1;
    buttons.WrapContents = false;
    // 
    // _scan
    // 
    _scan.AutoSize = true;
    _scan.Location = new Point(3, 3);
    _scan.Name = "_scan";
    _scan.Size = new Size(156, 35);
    _scan.TabIndex = 0;
    _scan.Text = "1. Scan Remaster";
    _scan.UseVisualStyleBackColor = true;
    _scan.Click += Scan_Click;
    // 
    // _map
    // 
    _map.AutoSize = true;
    _map.Location = new Point(165, 3);
    _map.Name = "_map";
    _map.Size = new Size(185, 35);
    _map.TabIndex = 1;
    _map.Text = "2. Map to Skyblivion";
    _map.UseVisualStyleBackColor = true;
    _map.Click += Map_Click;
    // 
    // _build
    // 
    _build.AutoSize = true;
    _build.Location = new Point(356, 3);
    _build.Name = "_build";
    _build.Size = new Size(123, 35);
    _build.TabIndex = 2;
    _build.Text = "3. Build mod";
    _build.UseVisualStyleBackColor = true;
    _build.Click += Build_Click;
    // 
    // _verify
    // 
    _verify.AutoSize = true;
    _verify.Location = new Point(485, 3);
    _verify.Name = "_verify";
    _verify.Size = new Size(85, 35);
    _verify.TabIndex = 3;
    _verify.Text = "4. Verify";
    _verify.UseVisualStyleBackColor = true;
    _verify.Click += Verify_Click;
    // 
    // _all
    // 
    _all.AutoSize = true;
    _all.BackColor = Color.LightGreen;
    _all.Location = new Point(576, 3);
    _all.Name = "_all";
    _all.Size = new Size(75, 35);
    _all.TabIndex = 4;
    _all.Text = "Run all";
    _all.UseVisualStyleBackColor = false;
    _all.Click += All_Click;
    // 
    // _cancel
    // 
    _cancel.AutoSize = true;
    _cancel.Enabled = false;
    _cancel.Location = new Point(657, 3);
    _cancel.Name = "_cancel";
    _cancel.Size = new Size(73, 35);
    _cancel.TabIndex = 5;
    _cancel.Text = "Cancel";
    _cancel.UseVisualStyleBackColor = true;
    _cancel.Click += Cancel_Click;
    // 
    // _openOutput
    // 
    _openOutput.AutoSize = true;
    _openOutput.Location = new Point(736, 3);
    _openOutput.Name = "_openOutput";
    _openOutput.Size = new Size(177, 35);
    _openOutput.TabIndex = 6;
    _openOutput.Text = "Open output folder";
    _openOutput.UseVisualStyleBackColor = true;
    _openOutput.Click += OpenOutput_Click;
    // 
    // _showAdvanced
    // 
    _showAdvanced.AutoSize = true;
    _showAdvanced.Location = new Point(932, 6);
    _showAdvanced.Margin = new Padding(16, 6, 0, 0);
    _showAdvanced.Name = "_showAdvanced";
    _showAdvanced.Size = new Size(229, 29);
    _showAdvanced.TabIndex = 7;
    _showAdvanced.Text = "Show advanced options";
    _showAdvanced.UseVisualStyleBackColor = true;
    _showAdvanced.CheckedChanged += ShowAdvanced_CheckedChanged;
    // 
    // results
    // 
    results.ColumnCount = 2;
    results.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
    results.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
    results.Controls.Add(groupsPanel, 0, 0);
    results.Controls.Add(mapStatsPanel, 1, 0);
    results.Dock = DockStyle.Fill;
    results.Location = new Point(11, 591);
    results.Name = "results";
    results.RowCount = 1;
    results.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
    results.Size = new Size(1082, 54);
    results.TabIndex = 2;
    // 
    // groupsPanel
    // 
    groupsPanel.ColumnCount = 1;
    groupsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    groupsPanel.Controls.Add(groupsHeader, 0, 0);
    groupsPanel.Controls.Add(_groups, 0, 1);
    groupsPanel.Dock = DockStyle.Fill;
    groupsPanel.Location = new Point(3, 3);
    groupsPanel.Name = "groupsPanel";
    groupsPanel.RowCount = 2;
    groupsPanel.RowStyles.Add(new RowStyle());
    groupsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
    groupsPanel.Size = new Size(643, 48);
    groupsPanel.TabIndex = 0;
    // 
    // groupsHeader
    // 
    groupsHeader.AutoSize = true;
    groupsHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    groupsHeader.Location = new Point(3, 0);
    groupsHeader.Name = "groupsHeader";
    groupsHeader.Size = new Size(629, 50);
    groupsHeader.TabIndex = 0;
    groupsHeader.Text = "Voice groups found by Scan (check the groups to build; none checked = all)";
    // 
    // _groups
    // 
    _groups.CheckBoxes = true;
    _groups.Columns.AddRange(new ColumnHeader[] { groupsColumnKey, groupsColumnRecordings, groupsColumnNpcs, groupsColumnVoiceType });
    _groups.Dock = DockStyle.Fill;
    _groups.FullRowSelect = true;
    _groups.Location = new Point(3, 53);
    _groups.Name = "_groups";
    _groups.Size = new Size(637, 1);
    _groups.TabIndex = 1;
    _groups.UseCompatibleStateImageBehavior = false;
    _groups.View = View.Details;
    // 
    // groupsColumnKey
    // 
    groupsColumnKey.Text = "Voice group (race/sex/variant)";
    groupsColumnKey.Width = 220;
    // 
    // groupsColumnRecordings
    // 
    groupsColumnRecordings.Text = "Recordings";
    groupsColumnRecordings.TextAlign = HorizontalAlignment.Right;
    groupsColumnRecordings.Width = 90;
    // 
    // groupsColumnNpcs
    // 
    groupsColumnNpcs.Text = "NPCs";
    groupsColumnNpcs.TextAlign = HorizontalAlignment.Right;
    // 
    // groupsColumnVoiceType
    // 
    groupsColumnVoiceType.Text = "Voice type";
    groupsColumnVoiceType.Width = 200;
    // 
    // mapStatsPanel
    // 
    mapStatsPanel.ColumnCount = 1;
    mapStatsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    mapStatsPanel.Controls.Add(mapStatsHeader, 0, 0);
    mapStatsPanel.Controls.Add(_mapStats, 0, 1);
    mapStatsPanel.Dock = DockStyle.Fill;
    mapStatsPanel.Location = new Point(652, 3);
    mapStatsPanel.Name = "mapStatsPanel";
    mapStatsPanel.RowCount = 2;
    mapStatsPanel.RowStyles.Add(new RowStyle());
    mapStatsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
    mapStatsPanel.Size = new Size(427, 48);
    mapStatsPanel.TabIndex = 1;
    // 
    // mapStatsHeader
    // 
    mapStatsHeader.AutoSize = true;
    mapStatsHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    mapStatsHeader.Location = new Point(3, 0);
    mapStatsHeader.Name = "mapStatsHeader";
    mapStatsHeader.Size = new Size(111, 25);
    mapStatsHeader.TabIndex = 0;
    mapStatsHeader.Text = "Map results";
    // 
    // _mapStats
    // 
    _mapStats.Columns.AddRange(new ColumnHeader[] { mapStatsColumnName, mapStatsColumnCount });
    _mapStats.Dock = DockStyle.Fill;
    _mapStats.FullRowSelect = true;
    _mapStats.HeaderStyle = ColumnHeaderStyle.Nonclickable;
    _mapStats.Location = new Point(3, 28);
    _mapStats.Name = "_mapStats";
    _mapStats.Size = new Size(421, 17);
    _mapStats.TabIndex = 1;
    _mapStats.UseCompatibleStateImageBehavior = false;
    _mapStats.View = View.Details;
    // 
    // mapStatsColumnName
    // 
    mapStatsColumnName.Text = "Mapping statistic";
    mapStatsColumnName.Width = 260;
    // 
    // mapStatsColumnCount
    // 
    mapStatsColumnCount.Text = "Count";
    mapStatsColumnCount.TextAlign = HorizontalAlignment.Right;
    mapStatsColumnCount.Width = 90;
    // 
    // logPanel
    // 
    logPanel.ColumnCount = 1;
    logPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    logPanel.Controls.Add(logHeader, 0, 0);
    logPanel.Controls.Add(_log, 0, 1);
    logPanel.Dock = DockStyle.Fill;
    logPanel.Location = new Point(11, 651);
    logPanel.Name = "logPanel";
    logPanel.RowCount = 2;
    logPanel.RowStyles.Add(new RowStyle());
    logPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
    logPanel.Size = new Size(1082, 84);
    logPanel.TabIndex = 3;
    // 
    // logHeader
    // 
    logHeader.AutoSize = true;
    logHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    logHeader.Location = new Point(3, 0);
    logHeader.Name = "logHeader";
    logHeader.Size = new Size(43, 25);
    logHeader.TabIndex = 0;
    logHeader.Text = "Log";
    // 
    // _log
    // 
    _log.Dock = DockStyle.Fill;
    _log.Font = new Font("Consolas", 9F);
    _log.Location = new Point(3, 28);
    _log.Multiline = true;
    _log.Name = "_log";
    _log.ReadOnly = true;
    _log.ScrollBars = ScrollBars.Both;
    _log.Size = new Size(1076, 53);
    _log.TabIndex = 1;
    _log.WordWrap = false;
    // 
    // statusRow
    // 
    statusRow.AutoSize = true;
    statusRow.ColumnCount = 2;
    statusRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
    statusRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    statusRow.Controls.Add(_progress, 0, 0);
    statusRow.Controls.Add(_status, 1, 0);
    statusRow.Dock = DockStyle.Top;
    statusRow.Location = new Point(11, 741);
    statusRow.Name = "statusRow";
    statusRow.RowCount = 1;
    statusRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
    statusRow.Size = new Size(1082, 28);
    statusRow.TabIndex = 4;
    // 
    // _progress
    // 
    _progress.Dock = DockStyle.Fill;
    _progress.Location = new Point(3, 3);
    _progress.Maximum = 1000;
    _progress.Name = "_progress";
    _progress.Size = new Size(294, 22);
    _progress.TabIndex = 0;
    // 
    // _status
    // 
    _status.AutoEllipsis = true;
    _status.Dock = DockStyle.Fill;
    _status.Location = new Point(303, 0);
    _status.Name = "_status";
    _status.Size = new Size(776, 28);
    _status.TabIndex = 1;
    _status.TextAlign = ContentAlignment.MiddleLeft;
    // 
    // MainForm
    // 
    AutoScaleMode = AutoScaleMode.None;
    ClientSize = new Size(1104, 781);
    Controls.Add(root);
    Name = "MainForm";
    StartPosition = FormStartPosition.CenterScreen;
    Text = "Skyblivion Voices Remastered";
    FormClosing += MainForm_FormClosing;
    Load += MainForm_Load;
    Shown += MainForm_Shown;
    root.ResumeLayout(false);
    root.PerformLayout();
    pathsGroup.ResumeLayout(false);
    pathsGroup.PerformLayout();
    pathsGrid.ResumeLayout(false);
    pathsGrid.PerformLayout();
    optionsAndButtons.ResumeLayout(false);
    optionsAndButtons.PerformLayout();
    optionsGroup.ResumeLayout(false);
    optionsGroup.PerformLayout();
    optionRows.ResumeLayout(false);
    optionRows.PerformLayout();
    optionRow1.ResumeLayout(false);
    optionRow1.PerformLayout();
    lipModePanel.ResumeLayout(false);
    lipModePanel.PerformLayout();
    parallelPanel.ResumeLayout(false);
    parallelPanel.PerformLayout();
    ((System.ComponentModel.ISupportInitialize)_parallel).EndInit();
    limitPanel.ResumeLayout(false);
    limitPanel.PerformLayout();
    ((System.ComponentModel.ISupportInitialize)_limit).EndInit();
    optionRow2.ResumeLayout(false);
    optionRow2.PerformLayout();
    buttons.ResumeLayout(false);
    buttons.PerformLayout();
    results.ResumeLayout(false);
    groupsPanel.ResumeLayout(false);
    groupsPanel.PerformLayout();
    mapStatsPanel.ResumeLayout(false);
    mapStatsPanel.PerformLayout();
    logPanel.ResumeLayout(false);
    logPanel.PerformLayout();
    statusRow.ResumeLayout(false);
    ResumeLayout(false);
  }

  #endregion

  private TableLayoutPanel root;
  private GroupBox pathsGroup;
  private TableLayoutPanel pathsGrid;
  private Label remasterLabel;
  private TextBox _remaster;
  private Button remasterBrowse;
  private Label skyblivionLabel;
  private TextBox _skyblivion;
  private Button skyblivionBrowse;
  private Label pluginsLabel;
  private TextBox _plugins;
  private Label workLabel;
  private TextBox _work;
  private Button workBrowse;
  private Label outputLabel;
  private TextBox _output;
  private Button outputBrowse;
  private Label pluginNameLabel;
  private TextBox _pluginName;
  private Label xwmaLabel;
  private TextBox _xwma;
  private Button xwmaBrowse;
  private Label lipGenLabel;
  private TextBox _lipGen;
  private Button lipGenBrowse;
  private Button detectButton;
  private TableLayoutPanel optionsAndButtons;
  private GroupBox optionsGroup;
  private TableLayoutPanel optionRows;
  private FlowLayoutPanel optionRow1;
  private FlowLayoutPanel lipModePanel;
  private Label lipModeLabel;
  private ComboBox _lipMode;
  private FlowLayoutPanel parallelPanel;
  private Label parallelLabel;
  private NumericUpDown _parallel;
  private FlowLayoutPanel limitPanel;
  private Label limitLabel;
  private NumericUpDown _limit;
  private FlowLayoutPanel optionRow2;
  private CheckBox _allVoiceTypes;
  private CheckBox _esl;
  private CheckBox _force;
  private FlowLayoutPanel buttons;
  private Button _scan;
  private Button _map;
  private Button _build;
  private Button _verify;
  private Button _all;
  private Button _cancel;
  private Button _openOutput;
  private CheckBox _showAdvanced;
  private TableLayoutPanel results;
  private TableLayoutPanel groupsPanel;
  private Label groupsHeader;
  private ListView _groups;
  private ColumnHeader groupsColumnKey;
  private ColumnHeader groupsColumnRecordings;
  private ColumnHeader groupsColumnNpcs;
  private ColumnHeader groupsColumnVoiceType;
  private TableLayoutPanel mapStatsPanel;
  private Label mapStatsHeader;
  private ListView _mapStats;
  private ColumnHeader mapStatsColumnName;
  private ColumnHeader mapStatsColumnCount;
  private TableLayoutPanel logPanel;
  private Label logHeader;
  private TextBox _log;
  private TableLayoutPanel statusRow;
  private ProgressBar _progress;
  private Label _status;
}
