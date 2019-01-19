﻿#region Copyright (C)

// ---------------------------------------------------------------------------------------------------------------
//  <copyright file="ElucidateForm.cs" company="Smurf-IV">
//
//  Copyright (C) 2010-2019 Simon Coghlan (Aka Smurf-IV)
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 2 of the License, or
//   any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, see http://www.gnu.org/licenses/.
//  </copyright>
//  <summary>
//  Url: https://github.com/Smurf-IV/Elucidate
//  Email: https://github.com/Smurf-IV
//  </summary>
// --------------------------------------------------------------------------------------------------------------------

#endregion Copyright (C)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using CommandLine;

using ComponentFactory.Krypton.Toolkit;

using Elucidate.CmdLine;
using Elucidate.Controls;
using Elucidate.Shared;

using NLog;

namespace Elucidate
{
    public sealed partial class ElucidateForm : KryptonForm
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly ConfigFileHelper srConfig = new ConfigFileHelper();

        public ElucidateForm()
        {
            InitializeComponent();

            if (Properties.Settings.Default.UpdateRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateRequired = false;
                Properties.Settings.Default.Save();
            }
            WindowLocation.GeometryFromString(Properties.Settings.Default.WindowLocation, this);

            liveRunLogControl1.ActionWorker.RunWorkerCompleted += liveRunLogControl1_RunWorkerCompleted;
            recover1.TaskStarted += Recover1_TaskStarted;
            recover1.TaskCompleted += Recover1_TaskCompleted;
            Settings.ConfigSaved += Settings_ConfigUpdated;
        }

        private void Settings_ConfigUpdated(object sender, EventArgs e)
        {
            if (File.Exists(Properties.Settings.Default.ConfigFileLocation))
            {
                LoadConfigFile(Properties.Settings.Default.ConfigFileLocation);
            }

            EnableIfValid(Properties.Settings.Default.ConfigFileIsValid);
        }

        private void ElucidateForm_Load(object sender, EventArgs e)
        {
            if (!File.Exists(Properties.Settings.Default.ConfigFileLocation))
            {
                Properties.Settings.Default.ConfigFileIsValid = false;
            }
            TextExtra = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void ElucidateForm_Shown(object sender, EventArgs e)
        {
            if (File.Exists(Properties.Settings.Default.ConfigFileLocation))
            {
                LoadConfigFile(Properties.Settings.Default.ConfigFileLocation);
            }

            EnableIfValid(srConfig.IsValid);

            // display any warnings from the config validation
            if (srConfig.IsWarnings)
            {
                MessageBoxExt.Show(
                    this,
                    $"There are warnings for the configuration file:{Environment.NewLine} - {string.Join(" - ", srConfig.ConfigWarnings)}",
                    "Configuration File Warnings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            else
            {
                string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
                if (args.Any())
                {
                    liveRunLogControl1.timerScantilla.Enabled = true; // got to display "helpful" information.

                    // https://github.com/commandlineparser/commandline
                    ParserResult<AllOptions> optsResult = Parser.Default.ParseArguments<AllOptions>(args);
                    optsResult.WithParsed<AllOptions>(DisplayStdOptions);
                    optsResult.WithNotParsed(DisplayErrors);
                    ParserResult<object> parserResult = Parser.Default.ParseArguments<SyncVerb, DiffVerb, CheckVerb, FixVerb, ScrubVerb, DupVerb, StatusVerb>(args);
                    parserResult.WithNotParsed(DisplayErrors);
                    // Order is important as commands "Can" be chained"
                    // See http://www.snapraid.it/manual scrubbing for an indication of order
                    parserResult.WithParsed<DiffVerb>(verb => DisplayAndCall(verb, Diff_Click));
                    parserResult.WithParsed<CheckVerb>(verb => DisplayAndCall(verb, Check_Click));
                    parserResult.WithParsed<SyncVerb>(verb => DisplayAndCall(verb, Sync_Click));
                    parserResult.WithParsed<ScrubVerb>(verb => DisplayAndCall(verb, Scrub_Click));
                    parserResult.WithParsed<DupVerb>(verb => DisplayAndCall(verb, DupFinder_Click));
                    parserResult.WithParsed<StatusVerb>(verb => DisplayAndCall(verb, btnStatus_Click));
                    parserResult.WithParsed<FixVerb>(verb => DisplayAndCall(verb, Fix_Click));
                    // Verbs not done as they do not have buttons yet
                    // list
                    // smart
                    // up
                    // down
                    // pool
                    // devices
                    // touch
                    // rehash
                }
            }
        }

        private void DisplayAndCall<T>(T verb, EventHandler<EventArgs> clickCall)
        {
            DisplayStdOptions(verb);
            clickCall(this, EventArgs.Empty);
        }

        private void DisplayStdOptions<TO>(TO sv)
        {
            string commandLineRead = string.Join(" ", Environment.GetCommandLineArgs());
            Log.Error(@"CommandLine Read: [{0}]", commandLineRead);
            string commandLine = CommandLine.Parser.Default.FormatCommandLine(sv);
            if (!string.IsNullOrWhiteSpace(commandLine))
            {
                Log.Info(@"CommandLine options Interpreted: [{0}]", commandLine);
                liveRunLogControl1.txtAddCommands.Text = commandLine;
                liveRunLogControl1.checkBoxCommandLineOptions.Checked = true;
                if ((sv as StdOptions).Verbose)
                {
                    liveRunLogControl1.checkBoxDisplayOutput.Checked = true;
                }
            }
        }

        private void DisplayErrors(IEnumerable<Error> errs)
        {
            foreach (Error err in errs)
            {
                Log.Error(err.Tag);
            }

            StringWriter writer = new StringWriter();
            {
                // Force the output of the help for each verb
                Parser parser = new Parser(with => with.HelpWriter = writer);
                parser.ParseArguments<SyncVerb, DiffVerb, CheckVerb, FixVerb, ScrubVerb, DupVerb, StatusVerb>(new[] { @"--help" });

                Log.Info(writer.ToString());
            }
        }

        private void Recover1_TaskStarted(object sender, EventArgs e)
        {
            tabControl.Deselecting += tabControl_Deselecting;
        }

        private void Recover1_TaskCompleted(object sender, EventArgs e)
        {
            tabControl.Deselecting -= tabControl_Deselecting;
        }

        private void liveRunLogControl1_RunWorkerCompleted(object sender, EventArgs e) => SetCommonButtonsEnabledState(true);

        private void SetCommonButtonsEnabledState(bool enabled)
        {
            btnDiff.Enabled = enabled;
            btnSync.Enabled = enabled;
            btnCheck.Enabled = enabled;
            btnStatus.Enabled = enabled;
            btnScrub.Enabled = enabled;
            btnFix.Enabled = enabled;
            btnDupFinder.Enabled = enabled;
            btnForceFullSync.Enabled = enabled;
            logViewToolStripMenuItem.Enabled = enabled;
            if (enabled)
            {
                tabControl.Deselecting -= tabControl_Deselecting;
            }
            else
            {
                tabControl.Deselecting += tabControl_Deselecting;
            }
        }

        private void tabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (tabControl.SelectedTab.Name == "tabCoveragePage")
            {
                driveSpace.RefreshGraph();
            }
        }

        private void tabControl_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            e.Cancel = true;
        }

        // ReSharper disable once InconsistentNaming
        private const int CS_DROPSHADOW = 0x20000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private void EnableIfValid(bool enabled)
        {
            SetCommonButtonsEnabledState(enabled);
        }

        #region Main Menu Toolbar Handlers

        private void RefreshDriveDisplayAfterConfigSaved(object sender, FormClosedEventArgs formClosedEventArgs)
        {
            driveSpace.RefreshGraph();
        }

        private void logViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveRunLogControl1.ActionWorker.IsBusy) { return; }

            string userAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Elucidate");

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Path.Combine(userAppData, @"Logs"),
                Filter = @"Log files (*.log)|*.log|Archive logs (*.*)|*.*",
                FileName = "*.log",
                FilterIndex = 2,
                Title = @"Select name to view contents"
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (Properties.Settings.Default.UseWindowsSettings)
            {
                Process word = Process.Start("Wordpad.exe", '"' + openFileDialog.FileName + '"');
                if (word == null)
                {
                    return;
                }

                word.WaitForInputIdle();
                SendKeys.SendWait("^{END}");
            }
            else
            {
                // Launch whatever "Knows" how to view log files
                Process.Start('"' + openFileDialog.FileName + '"');
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(@"https://github.com/BlueBlock/Elucidate/wiki/Documentation");
        }
        #endregion Menu Handlers

        private void btnStatus_Click(object sender, EventArgs e)
        {
            SetCommonButtonsEnabledState(false);
            liveRunLogControl1.StartSnapRaidProcess(LiveRunLogControl.CommandType.Status);
        }

        private void Diff_Click(object sender, EventArgs e)
        {
            SetCommonButtonsEnabledState(false);
            liveRunLogControl1.StartSnapRaidProcess(LiveRunLogControl.CommandType.Diff);
        }

        private void Check_Click(object sender, EventArgs e)
        {
            SetCommonButtonsEnabledState(false);
            if (Util.IsExecutableRunning(Properties.Settings.Default.SnapRAIDFileLocation))
            {
                SetCommonButtonsEnabledState(true);
                KryptonMessageBox.Show(this, @"A SnapRAID process is already running");
                return;
            }
            liveRunLogControl1.StartSnapRaidProcess(LiveRunLogControl.CommandType.Check);
        }

        private void Sync_Click(object sender, EventArgs e)
        {
            SetCommonButtonsEnabledState(false);
            liveRunLogControl1.StartSnapRaidProcess(LiveRunLogControl.CommandType.Sync);
        }

        private void Scrub_Click(object sender, EventArgs e)
        {
            SetCommonButtonsEnabledState(false);
            liveRunLogControl1.StartSnapRaidProcess(LiveRunLogControl.CommandType.Scrub);
        }

        private void Fix_Click(object sender, EventArgs e)
        {
            SetCommonButtonsEnabledState(false);
            liveRunLogControl1.StartSnapRaidProcess(LiveRunLogControl.CommandType.Fix);
        }

        private void DupFinder_Click(object sender, EventArgs e)
        {
            SetCommonButtonsEnabledState(false);
            liveRunLogControl1.StartSnapRaidProcess(LiveRunLogControl.CommandType.Dup);
        }

        private void ElucidateForm_ResizeEnd(object sender, EventArgs e)
        {
            // persist our geometry string.
            Properties.Settings.Default.WindowLocation = WindowLocation.GeometryToString(this);
            Properties.Settings.Default.Save();
        }

        private void ForceFullSync_Click(object sender, EventArgs e)
        {
            SetCommonButtonsEnabledState(false);
            liveRunLogControl1.StartSnapRaidProcess(LiveRunLogControl.CommandType.ForceFullSync);
        }

        private void OpenSnapRAIDConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = @"Config Files|*.conf*|All files (*.*)|*.*",
                Title = @"Select a Config File"
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadConfigFile(openFileDialog1.FileName);
            }
        }

        private void LoadConfigFile(string configFile)
        {
            srConfig.LoadConfigFile(configFile);

            Properties.Settings.Default.ConfigFileIsValid = srConfig.IsValid;

            Properties.Settings.Default.ConfigFileLocation = configFile;
            editSnapRAIDConfigToolStripMenuItem.Enabled = !string.IsNullOrWhiteSpace(configFile);

            if (srConfig.IsValid)
            {
                BeginInvoke((MethodInvoker)delegate { SetElucidateFormTitle(configFile); });
            }
            else
            {
                MessageBoxExt.Show(
                    this,
                    $"The config file is not valid.{Environment.NewLine} - {string.Join($@"{Environment.NewLine} - ", srConfig.ConfigErrors)}",
                    "Config File Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            EnableIfValid(Properties.Settings.Default.ConfigFileIsValid);
        }

        private void SetElucidateFormTitle(string filePath)
        {
            string newTitle = "Elucidate";

            if (!string.IsNullOrEmpty(filePath))
            {
                newTitle += $" - {filePath}";
            }

            Text = newTitle;
        }

        private void EditSnapRAIDConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveRunLogControl1.ActionWorker.IsBusy) { return; }

            using (Settings settingsForm = new Settings() )
            {
                settingsForm.FormClosed += RefreshDriveDisplayAfterConfigSaved;
                settingsForm.ShowDialog(this);
            }

            EnableIfValid(Properties.Settings.Default.ConfigFileIsValid);
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ElucidateForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void CloseSnapRAIDConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate { SetElucidateFormTitle(string.Empty); });
            Properties.Settings.Default.ConfigFileLocation = string.Empty;
            Properties.Settings.Default.ConfigFileIsValid = false;
            EnableIfValid(false);
        }

        private void deleteAllSnapRAIDRaidFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> parityFiles = new List<string>();

            List<string> contentFiles = new List<string>();

            if (!string.IsNullOrWhiteSpace(srConfig.ParityFile1))
            {
                parityFiles.Add(srConfig.ParityFile1);
            }

            if (!string.IsNullOrWhiteSpace(srConfig.ParityFile2))
            {
                parityFiles.Add(srConfig.ParityFile2);
            }

            if (!string.IsNullOrWhiteSpace(srConfig.ZParityFile))
            {
                parityFiles.Add(srConfig.ZParityFile);
            }

            if (!string.IsNullOrWhiteSpace(srConfig.ParityFile3))
            {
                parityFiles.Add(srConfig.ParityFile3);
            }

            if (!string.IsNullOrWhiteSpace(srConfig.ParityFile4))
            {
                parityFiles.Add(srConfig.ParityFile4);
            }

            if (!string.IsNullOrWhiteSpace(srConfig.ParityFile5))
            {
                parityFiles.Add(srConfig.ParityFile5);
            }

            if (!string.IsNullOrWhiteSpace(srConfig.ParityFile6))
            {
                parityFiles.Add(srConfig.ParityFile6);
            }

            foreach (string file in srConfig.ContentFiles)
            {
                contentFiles.Add(file);
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@"Are you sure you want to remove the files below?");

            sb.AppendLine(@"This action cannot be undone.");

            sb.AppendLine();

            foreach (string file in parityFiles)
            {
                sb.AppendLine($@"Parity File: {file}");
            }

            foreach (string file in contentFiles)
            {
                sb.AppendLine($@"Content File: {file}");
            }

            DialogResult result = KryptonMessageBox.Show(
                this,
                sb.ToString(),
                @"Delete All SnapRAID Files",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    foreach (string file in parityFiles)
                    {
                        File.Delete(file);
                    }

                    foreach (string file in contentFiles)
                    {
                        File.Delete(file);
                    }

                    MessageBoxExt.Show(this, @"The SnapRAID files have been removed", @"Files Removed");
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
    }
}