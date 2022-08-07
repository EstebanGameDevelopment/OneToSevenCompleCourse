// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.
#if UNITY_EDITOR_WIN && UNITY_ANDROID && UNITY_2018_4_OR_NEWER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Pvr_BuildAndRunEW : EditorWindow
{

    static int currentStep;
    static string progressMessage;

    static string productName;

    static string gradleTempExport;
    static string gradleExport;
    static bool showCancel;
    static bool buildFailed;

    static Pvr_DirectorySyncer.CancellationTokenSource syncCancelToken;
    static Process gradleBuildProcess;

    static bool? apkOutputSuccessful;

    const string REMOTE_APK_PATH = "/data/local/tmp";
    const float USB_TRANSFER_SPEED_THRES = 25.0f;
    const float USB_3_TRANSFER_SPEED = 32.0f;
    const float TRANSFER_SPEED_CHECK_THRESHOLD = 4.0f;
    const int NUM_BUILD_AND_RUN_STEPS = 9;
    const int BYTES_TO_MEGABYTES = 1048576;

    private void OnGUI()
    {
        minSize = new Vector2(500, 170);
        maxSize = new Vector2(500, 170);

        Rect progressRect = EditorGUILayout.BeginVertical();
        progressRect.height = 25.0f;
        float progress = currentStep / (float)NUM_BUILD_AND_RUN_STEPS;
        EditorGUI.ProgressBar(progressRect, progress, progressMessage);
        if (showCancel)
        {
            GUIContent btnTxt = new GUIContent("Cancel");
            var rt = GUILayoutUtility.GetRect(btnTxt, GUI.skin.button, GUILayout.ExpandWidth(false));
            rt.center = new Vector2(EditorGUIUtility.currentViewWidth / 2, progressRect.height * 2);
            if (GUI.Button(rt, btnTxt, GUI.skin.button))
            {
                CancelBuild();
            }
        }
        EditorGUILayout.EndVertical();

        if (progress >= 1.0f || buildFailed)
        {
            Close();
        }
    }

    private void Update()
    {
        if (focusedWindow != null && focusedWindow.ToString().Contains("Pvr_BuildAndRunEW"))
        {
            Repaint();
        }
    }

#if UNITY_2018_4_OR_NEWER
    [MenuItem("Pvr_UnitySDK/Build Tool/Build And Run")]
    static void OpenBuildAndRun()
    {
        GetWindow<Pvr_BuildAndRunEW>();
        Pvr_BuildToolManager.GetScenesEnabled();
        EditorBuildSettings.sceneListChanged += Pvr_BuildToolManager.GetScenesEnabled;

        showCancel = false;
        buildFailed = false;
        currentStep = 0;
        IncrementProgressBar("Exporting Unity Project . . .");

        if (!Pvr_ADBTool.GetInstance().CheckADBDevices(log => { SetProgressBarMessage(log); }))
        {
            buildFailed = true;
            return;
        }

        apkOutputSuccessful = null;
        syncCancelToken = null;
        gradleBuildProcess = null;

        Debug.Log("PvrBuild: Starting Unity build ...");

        gradleTempExport = Path.Combine(Path.Combine(Application.dataPath, "../Temp"), "PvrGradleTempExport");
        gradleExport = Path.Combine(Path.Combine(Application.dataPath, "../Temp"), "PvrGradleExport");
        Pvr_DirectorySyncer.CreateDirectory(gradleExport);

        // 1. Get scenes to build in Unity, and export gradle project
        var buildResult = Pvr_BuildToolManager.UnityBuildPlayer(gradleTempExport, Pvr_BuildToolManager.buildSceneNameList.ToArray());

        if (buildResult.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            // Set static variables so build thread has updated data
            showCancel = true;
#if UNITY_2019_3_OR_NEWER
            productName = "launcher";
#else
            productName = Application.productName;
#endif
            BuildRun(IncrementProgressBar);
            return;
        }
        else if (buildResult.summary.result == UnityEditor.Build.Reporting.BuildResult.Cancelled)
        {
            Debug.Log("Build canceled.");
        }
        else
        {
            Debug.Log("Build failed.");
        }
        buildFailed = true;
    }
#endif

    void CancelBuild()
    {
        SetProgressBarMessage("Canceling . . .");

        if (syncCancelToken != null)
        {
            syncCancelToken.Cancel();
        }

        if (apkOutputSuccessful.HasValue && apkOutputSuccessful.Value)
        {
            buildFailed = true;
        }

        if (gradleBuildProcess != null && !gradleBuildProcess.HasExited)
        {
            var cancelThread = new Thread(delegate ()
            {
                CancelGradleBuild();
            });
            cancelThread.Start();
        }
    }

    void CancelGradleBuild()
    {
        Process cancelGradleProcess = new Process();
        string arguments = "-Xmx1024m -classpath \"" + Pvr_ADBTool.GetInstance().GetGradlePath() +
                           "\" org.gradle.launcher.GradleMain --stop";
        var processInfo = new System.Diagnostics.ProcessStartInfo
        {
            WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
            FileName = Pvr_ADBTool.GetInstance().GetJDKPath(),
            Arguments = arguments,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };

        cancelGradleProcess.StartInfo = processInfo;
        cancelGradleProcess.EnableRaisingEvents = true;

        cancelGradleProcess.OutputDataReceived += new DataReceivedEventHandler(
            (s, e) =>
            {
                if (e != null && e.Data != null && e.Data.Length != 0)
                {
                    UnityEngine.Debug.LogFormat("Gradle: {0}", e.Data);
                }
            }
        );

        apkOutputSuccessful = false;

        cancelGradleProcess.Start();
        cancelGradleProcess.BeginOutputReadLine();
        cancelGradleProcess.WaitForExit();

        buildFailed = true;
    }

    public static void BuildRun(Action<string> log)
    {
        // 2. Process gradle project
        log("Processing gradle project . . .");
        if (ProcessGradleProject())
        {
            // 3. Build gradle project
            log("Starting gradle build . . .");
            if (BuildGradleProject())
            {
                // 4. Deploy and run
                if (DeployAPK(IncrementProgressBar))
                {
                    return;
                }
            }
        }
        buildFailed = true;
    }

    private static bool BuildGradleProject()
    {
        gradleBuildProcess = new Process();
        string arguments = "-Xmx4096m -classpath \"" + Pvr_ADBTool.GetInstance().GetGradlePath() +
            "\" org.gradle.launcher.GradleMain assembleDebug -x validateSigningDebug --profile";
#if UNITY_2019_3_OR_NEWER
        var gradleProjectPath = gradleExport;
#else
        var gradleProjectPath = Path.Combine(gradleExport, productName);
#endif

        var processInfo = new ProcessStartInfo
        {
            WorkingDirectory = gradleProjectPath,
            WindowStyle = ProcessWindowStyle.Normal,
            FileName = Pvr_ADBTool.GetInstance().GetJDKPath(),
            Arguments = arguments,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };

        gradleBuildProcess.StartInfo = processInfo;
        gradleBuildProcess.EnableRaisingEvents = true;

        gradleBuildProcess.Exited += new System.EventHandler(
            (s, e) =>
            {
                Debug.Log("Gradle: Exited");
            }
        );

        gradleBuildProcess.OutputDataReceived += new DataReceivedEventHandler(
            (s, e) =>
            {
                if (e != null && e.Data != null &&
                    e.Data.Length != 0 &&
                    (e.Data.Contains("BUILD") || e.Data.StartsWith("See the profiling report at:")))
                {
                    Debug.LogFormat("Gradle: {0}", e.Data);
                    if (e.Data.Contains("SUCCESSFUL"))
                    {
                        Debug.LogFormat("APK Build Completed: {0}",
                            Path.Combine(Path.Combine(gradleProjectPath, "build\\outputs\\apk\\debug"), productName + "-debug.apk").Replace("/", "\\"));
                        if (!apkOutputSuccessful.HasValue)
                        {
                            apkOutputSuccessful = true;
                        }
                    }
                    else if (e.Data.Contains("FAILED"))
                    {
                        apkOutputSuccessful = false;
                    }
                }
            }
        );

        gradleBuildProcess.ErrorDataReceived += new DataReceivedEventHandler(
            (s, e) =>
            {
                if (e != null && e.Data != null &&
                    e.Data.Length != 0)
                {
                    Debug.LogErrorFormat("Gradle: {0}", e.Data);
                }
                apkOutputSuccessful = false;
            }
        );

        gradleBuildProcess.Start();
        gradleBuildProcess.BeginOutputReadLine();
        IncrementProgressBar("Building gradle project . . .");

        gradleBuildProcess.WaitForExit();

        // Add a timeout for if gradle unexpectedlly exits or errors out
        Stopwatch timeout = new Stopwatch();
        timeout.Start();
        while (apkOutputSuccessful == null)
        {
            if (timeout.ElapsedMilliseconds > 5000)
            {
                Debug.LogError("Gradle has exited unexpectedly.");
                apkOutputSuccessful = false;
            }
            Thread.Sleep(100);
        }

        return apkOutputSuccessful.HasValue && apkOutputSuccessful.Value;
    }

    private static bool ProcessGradleProject()
    {
        try
        {
            var ps = System.Text.RegularExpressions.Regex.Escape("" + Path.DirectorySeparatorChar);
            // ignore files .gradle/** build/** foo/.gradle/** and bar/build/**   
            var ignorePattern = string.Format("^([^{0}]+{0})?(\\.gradle|build){0}", ps);

            var syncer = new Pvr_DirectorySyncer(gradleTempExport,
                gradleExport, ignorePattern);

            syncCancelToken = new Pvr_DirectorySyncer.CancellationTokenSource();
            var syncResult = syncer.Synchronize(syncCancelToken.Token);
        }
        catch (Exception e)
        {
            Debug.Log("PvrBuild: Processing gradle project failed with exception: " +
                                  e.Message);
            return false;
        }

        if (syncCancelToken.IsCancellationRequested)
        {
            return false;
        }

        return true;
    }

    public static bool DeployAPK(Action<string> log)
    {
        // Create new instance of ADB Tool
        if (!Pvr_ADBTool.GetInstance().IsReady())
        {
            return false;
        }
        string apkPathLocal;
        string gradleExportFolder = Path.Combine(Path.Combine(gradleExport, productName), "build\\outputs\\apk\\debug");

        // Check to see if gradle output directory exists
        gradleExportFolder = gradleExportFolder.Replace("/", "\\");
        if (!Directory.Exists(gradleExportFolder))
        {
            Debug.LogError("Could not find the gradle project at the expected path: " + gradleExportFolder);
            return false;
        }

        // Search for output APK in gradle output directory
        apkPathLocal = Path.Combine(gradleExportFolder, productName + "-debug.apk");
        if (!File.Exists(apkPathLocal))
        {
            Debug.LogError(string.Format("Could not find {0} in the gradle project.", productName + "-debug.apk"));
            return false;
        }

        string output, error;
        DateTime timerStart;

        // Ensure that the Pico temp directory is on the device by making it
        log("Making Temp directory on device");
        string[] mkdirCommand = { "-d shell", "mkdir -p", REMOTE_APK_PATH };
        if (Pvr_ADBTool.GetInstance().RunCommand(mkdirCommand, null, out output, out error) != 0) return false;

        // Push APK to device, also time how long it takes
        timerStart = DateTime.Now;
        log("Pushing APK to device . . .");
        string[] pushCommand = { "-d push", "\"" + apkPathLocal + "\"", REMOTE_APK_PATH };
        if (Pvr_ADBTool.GetInstance().RunCommand(pushCommand, null, out output, out error) != 0) return false;

        // Calculate the transfer speed and determine if user is using USB 2.0 or 3.0
        TimeSpan pushTime = System.DateTime.Now - timerStart;
        bool trivialPush = pushTime.TotalSeconds < TRANSFER_SPEED_CHECK_THRESHOLD;
        long? apkSize = (trivialPush ? (long?)null : new System.IO.FileInfo(apkPathLocal).Length);
        double? transferSpeed = (apkSize / pushTime.TotalSeconds) / BYTES_TO_MEGABYTES;
        bool informLog = transferSpeed.HasValue && transferSpeed.Value < USB_TRANSFER_SPEED_THRES;
        Debug.Log("Pvr ADB Tool: Push Success");

        // Install the APK package on the device
        log("Installing APK . . .");
        string apkPath = REMOTE_APK_PATH + "/" + productName + "-debug.apk";
        apkPath = apkPath.Replace(" ", "\\ ");
        string[] installCommand = { "-d shell", "pm install -r", apkPath };
        if (Pvr_ADBTool.GetInstance().RunCommand(installCommand, null, out output, out error) != 0) return false;
        Debug.Log("Pvr ADB Tool: Install Success");

        if (!Pvr_BuildToolManager.RestartApp())
        {
            return false;
        }
        // Send back metrics on push and install steps
        log("Success!");

        // If the user is using a USB 2.0 cable, inform them about improved transfer speeds and estimate time saved
        if (informLog)
        {
            var usb3Time = apkSize.Value / (USB_3_TRANSFER_SPEED * BYTES_TO_MEGABYTES);
            Debug.Log(string.Format("Build has detected slow transfer speeds. A USB 3.0 cable is recommended to reduce the time it takes to deploy your project by approximatly {0:0.0} seconds", pushTime.TotalSeconds - usb3Time));
            return true;
        }

        return false;
    }

    public static void IncrementProgressBar(string message)
    {
        currentStep++;
        progressMessage = message;
        Debug.Log("Pvr Build: " + message);
    }

    private static void SetProgressBarMessage(string message)
    {
        progressMessage = message;
        Debug.Log("Pvr Build: " + message);
    }
}
#endif