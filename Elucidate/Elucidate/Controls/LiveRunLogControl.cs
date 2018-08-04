﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Elucidate.Logging;
using Elucidate.wyDay.Controls;

namespace Elucidate.Controls
{
    public partial class LiveRunLogControl : UserControl
    {
        public LiveRunLogControl()
        {
            InitializeComponent();
            AddThreadingCallbacks();
        }

        /// <summary>
        /// eventing idea taken from http://stackoverflow.com/questions/1423484/using-bashcygwin-inside-c-program
        /// </summary>
        private readonly ManualResetEvent _mreProcessExit = new ManualResetEvent(false);
        private readonly ManualResetEvent _mreOutputDone = new ManualResetEvent(false);
        private readonly ManualResetEvent _mreErrorDone = new ManualResetEvent(false);
        private ProcessPriorityClass _requested = ProcessPriorityClass.Normal;
        private string _lastError;

        private class ThreadObject
        {
            public BackgroundWorker BgdWorker;
            public Process CmdProcess;
        }

        public enum CommandType { Status, Diff, Check, Sync, Scrub, Fix, Dup, Undelete }

        public readonly BackgroundWorker ActionWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true,
        };
        
        private void AddThreadingCallbacks()
        {
            // Add threading callbacks
            ActionWorker.DoWork += actionWorker_DoWork;
            ActionWorker.ProgressChanged += actionWorker_ProgressChanged;
            ActionWorker.RunWorkerCompleted += actionWorker_RunWorkerCompleted;
            comboBox1.Text = @"Stopped";
            comboBox1.Enabled = false;
            timer1.Enabled = true;
        }

        internal void StartSnapRaidProcess(CommandType action)
        {
            if (ActionWorker.IsBusy)
            {
                Log.Instance.Debug("Command button clicked but a command is still running.");
                return;
            }
            //UseWaitCursor = true;
            //EnableIfValid(false);
            StringBuilder command = new StringBuilder();
            switch (action)
            {
                case CommandType.Status:
                    command.Append(@"status");
                    break;
                case CommandType.Diff:
                    command.Append(@"diff");
                    break;
                case CommandType.Check:
                    command.Append(@"check");
                    break;
                case CommandType.Sync:
                    command.Append(@"sync");
                    break;
                case CommandType.Scrub:
                    command.Append(@"scrub ");
                    command.Append(!string.IsNullOrWhiteSpace(txtAddCommands.Text) ? txtAddCommands.Text : @"-p100 -o0");
                    break;
                case CommandType.Fix:
                    command.Append(@"fix ");
                    command.Append(!string.IsNullOrWhiteSpace(txtAddCommands.Text) ? txtAddCommands.Text : @"-e");
                    break;
                case CommandType.Dup:
                    command.Append(@"dup");
                    break;
                case CommandType.Undelete:
                    command.Append(@"fix ");
                    command.Append(!string.IsNullOrWhiteSpace(txtAddCommands.Text) ? txtAddCommands.Text : @"-m");
                    break;
            }
            
            comboBox1.Enabled = true;
            comboBox1.Text = @"Running";
            _requested = ProcessPriorityClass.Normal;
            ActionWorker.RunWorkerAsync(command.ToString());
            toolStripStatusLabel1.Text = DateTime.Now.ToString("u");
            toolStripProgressBar1.DisplayText = "Running...";
            toolStripProgressBar1.State = ProgressBarState.Normal;
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
            toolStripProgressBar1.Value = 0;
        }
        
        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!LogQueue.Any())
                {
                    //Thread.Sleep(250);
                    return;
                }
                // Now lock in case the timer is overlapping !
                lock (this)
                {
                    textBoxLiveLog._Paint = false; // turn off flag to ignore WM_PAINT messages
                    if (textBoxLiveLog.Lines.Length > Properties.Settings.Default.MaxNumberOfRealTimeLines)
                    {
                        //textBox1.SelectionStart = 0;
                        //textBox1.SelectionLength = textBox1.GetFirstCharIndexFromLine(Properties.Settings.Default.MaxNumberOfRealTimeLines - 10);
                        //textBox1.SelectedText = string.Empty;
                        // The above makes it beep as well !!
                        textBoxLiveLog.Clear();
                    }
                    //read out of the file until the EOF
                    while (LogQueue.Any())
                    {
                        LogString log = LogQueue.Dequeue();
                        switch (log.LevelUppercase)
                        {
                            case "FATAL":
                                textBoxLiveLog.SelectionColor = Color.DarkViolet;
                                break;

                            case "ERROR":
                                textBoxLiveLog.SelectionColor = Color.Red;
                                break;

                            case "WARN":
                                textBoxLiveLog.SelectionColor = Color.RoyalBlue;
                                break;

                            case "INFO":
                                textBoxLiveLog.SelectionColor = Color.Black;
                                break;

                            case "DEBUG":
                                textBoxLiveLog.SelectionColor = Color.DarkGray;
                                break;

                            case "TRACE":
                                textBoxLiveLog.SelectionColor = Color.DimGray;
                                break;
                        }
                        textBoxLiveLog.AppendText(log.Message + Environment.NewLine);
                        textBoxLiveLog.ScrollToCaret();
                    }
                    // check if our textbox is getting too full
                    int textLength = textBoxLiveLog.TextLength;
                    if (textLength > (textBoxLiveLog.MaxLength - 1000))
                    {
                        //max possible on control = 2147483647
                        textBoxLiveLog.Clear();
                        Log.Instance.Debug("CLEAR");
                    }
                }
            }
            catch
            {
                // ignored
            }

            textBoxLiveLog._Paint = true; // restore flag so we can paint the control
        }

        private class LogString
        {
            public string LevelUppercase;
            public string Message;
        };

        private static readonly Queue<LogString> LogQueue = new Queue<LogString>();

        // ReSharper disable UnusedMember.Global
        // This is used by the logging to force all to the output window
        public static void LogMethod(string levelUppercase, string message)
        {
            LogQueue.Enqueue(new LogString
            {
                LevelUppercase = levelUppercase,
                Message = message
            });
        }
        
        private void actionWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Log.Instance.Debug("actionWorker_DoWork");
                BackgroundWorker worker = sender as BackgroundWorker;
                string command = e.Argument as string;
                _lastError = string.Empty;

                if (worker == null)
                {
                    Log.Instance.Error("Passed in worker is null");
                    e.Cancel = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(command))
                {
                    Log.Instance.Error("Passed in command is null");
                    e.Cancel = true;
                    return;
                }

                int exitCode;
                _mreProcessExit.Reset();
                _mreOutputDone.Reset();
                _mreErrorDone.Reset();

                string args = Util.FormatSnapRaidCommandArgs(command, txtAddCommands.Text, out string appPath);

                if (runWithoutCaptureMenuItem.Checked)
                {
                    Log.Instance.Warn("Running without Capture mode");
                    Log.Instance.Info("Starting minimised {0} {1}", appPath, args);
                    Log.Instance.Info("Working...");
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "CMD.exe",
                        Arguments = $" /K \"{appPath} {args}\"",
                        WorkingDirectory = $"{Path.GetDirectoryName(Properties.Settings.Default.SnapRAIDFileLocation)}",
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Minimized,
                        ErrorDialog = true
                    };
                    Process process = Process.Start(startInfo);
                    if (process != null) Log.Instance.Info("Process is running PID[{0}]", process.Id);
                    return;
                }

                using (Process process = new Process
                {
                    StartInfo = new ProcessStartInfo(appPath, args)
                    {
                        UseShellExecute = false,      // We're redirecting !
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8,   // SnapRAID uses UTF-8 for output
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true
                    },
                    EnableRaisingEvents = true
                })
                {
                    Log.Instance.Info("Using: {0}", process.StartInfo.FileName);
                    Log.Instance.Info("with: {0}", process.StartInfo.Arguments);
                    process.Exited += Exited;

                    process.Start();
                    // Redirect the output stream of the child process.
                    ThreadObject threadObject = new ThreadObject { BgdWorker = worker, CmdProcess = process };
                    ThreadPool.QueueUserWorkItem(o => ReadStandardOutput(threadObject));
                    ThreadPool.QueueUserWorkItem(o => ReadStandardError(threadObject));

                    if (process.HasExited)
                    {
                        _mreProcessExit.Set();
                    }

                    while (!WaitHandle.WaitAll(new WaitHandle[] { _mreErrorDone, _mreOutputDone, _mreProcessExit }, 250))
                    {
                        if (process.HasExited) continue;
                        if (worker.CancellationPending)
                        {
                            Log.Instance.Fatal("Attempting process KILL");
                            process.Kill();
                        }
                        else
                        {
                            ProcessPriorityClass current = process.PriorityClass;
                            if (current == _requested) continue;
                            Log.Instance.Fatal("Setting the processpriority to[{0}]", _requested);
                            process.PriorityClass = _requested;
                        }
                    }

                    exitCode = process.ExitCode;
                    if (exitCode == 0)
                    {
                        Log.Instance.Info("ExitCode=[{0}]", exitCode);
                    }
                    else
                    {
                        Log.Instance.Error("ExitCode=[{0}]", exitCode);
                    }
                    process.Close();
                }
                switch (exitCode)
                {
                    case 0:
                        break;
                    case 1:
                        worker.ReportProgress(101, "Error");
                        break;
                    default:
                        Log.Instance.Debug($"Unhandled ExitCode of {exitCode}");
                        break;
                }
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                _lastError = "Thread closing abnormally";
                ExceptionHandler.ReportException(ex, _lastError);
                throw;
            }
        }


        private void ReadStandardError(ThreadObject threadObject)
        {
            try
            {
                Log.Instance.Info("Start Verbose handler");
                string buf;
                do
                {
                    if (!string.IsNullOrEmpty(buf = threadObject.CmdProcess.StandardError.ReadLine()))
                    {
                        _lastError = buf;
                        Log.Instance.Warn("Verbose[{0}]", buf);
                    }
                    else
                    {
                        _mreProcessExit.WaitOne(100);
                    }
                } while (!string.IsNullOrEmpty(buf)
                         || !_mreProcessExit.WaitOne(0) // If millisecondsTimeout is zero, the method does not block. It tests the state of the wait handle and returns immediately.
                );
            }
            catch (Exception ex)
            {
                _lastError = "Thread closing abnormally";
                ExceptionHandler.ReportException(ex, _lastError);
            }
            _mreErrorDone.Set();
        }

        private void ReadStandardOutput(ThreadObject threadObject)
        {
            try
            {
                Log.Instance.Debug("Start StdOut handler");
                string buf;
                do
                {
                    if (!string.IsNullOrEmpty(buf = threadObject.CmdProcess.StandardOutput.ReadLine()))
                    {
                        Log.Instance.Info("StdOut[{0}]", buf);
                        if (!buf.Contains("%")) continue;
                        string[] splits = buf.Split('%');
                        if (int.TryParse(splits[0], out int percentProgress))
                        {
                            threadObject.BgdWorker.ReportProgress(percentProgress, buf);
                        }
                    }
                    else
                    {
                        _mreProcessExit.WaitOne(100);
                    }
                    //Log.Instance.Debug("sleep");
                    //Thread.Sleep(1000);
                } while (!string.IsNullOrEmpty(buf)
                         || !_mreProcessExit.WaitOne(0) // If millisecondsTimeout is zero, the method does not block. It tests the state of the wait handle and returns immediately.
                );
            }
            catch (Exception ex)
            {
                ExceptionHandler.ReportException(ex, "ReadStandardOutput: ");
            }
            _mreOutputDone.Set();
        }

        private void Exited(object o, EventArgs e)
        {
            Log.Instance.Debug("Exited.");
            Thread.Sleep(1000);  // When the process has exited, the buffers are _still_ flushing
            _mreProcessExit.Set();
        }

        private void actionWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (toolStripProgressBar1.Style == ProgressBarStyle.Marquee)
            {
                toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            }
            if (e.ProgressPercentage < 101)
            {
                toolStripProgressBar1.Value = e.ProgressPercentage;
            }
            else if (e.ProgressPercentage == 101)
            {
                toolStripProgressBar1.State = ProgressBarState.Error;
                toolStripProgressBar1.Value = 100;
                _lastError = e.UserState.ToString();
            }
        }

        private void actionWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //UseWaitCursor = false;
            //EnableIfValid(true);

            if (toolStripProgressBar1.Style == ProgressBarStyle.Marquee)
            {
                toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            }
            if (e.Cancelled)
            {
                toolStripProgressBar1.State = ProgressBarState.Pause;
                toolStripProgressBar1.DisplayText = "Cancelled";
                Log.Instance.Error("The thread has been cancelled");
                comboBox1.Text = @"Abort";
            }
            else if (e.Error != null)
            {
                toolStripProgressBar1.State = ProgressBarState.Error;
                toolStripProgressBar1.DisplayText = "Error";
                Log.Instance.Error(e.Error, "Thread threw: ");
                comboBox1.Text = @"Abort";
            }
            else
            {
                if (toolStripProgressBar1.State == ProgressBarState.Normal)
                {
                    toolStripProgressBar1.DisplayText = "Completed";
                    toolStripProgressBar1.Value = 100;
                    comboBox1.Text = @"Stopped";
                }
            }

            if (!string.IsNullOrEmpty(_lastError))
            {
                toolStripProgressBar1.DisplayText = "Error";
                Log.Instance.Info($"Last Error: {_lastError}");
            }
            comboBox1.Enabled = false;
        }

        private void LiveRunLogControl_Load(object sender, EventArgs e)
        {
            //AddThreadingCallbacks();
        }
    }
}
