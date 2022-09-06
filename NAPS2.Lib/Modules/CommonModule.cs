﻿using NAPS2.EtoForms;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Ocr;
using NAPS2.Platform.Windows;
using NAPS2.Recovery;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Unmanaged;
using NAPS2.WinForms;
using Ninject;
using Ninject.Modules;
using ILogger = NAPS2.Logging.ILogger;

namespace NAPS2.Modules;

public class CommonModule : NinjectModule
{
    public override void Load()
    {
        // Import
        Bind<IScannedImageImporter>().To<ScannedImageImporter>();
        Bind<IPdfImporter>().To<PdfImporter>();
        Bind<IImageImporter>().To<ImageImporter>();
        Bind<RecoveryManager>().ToSelf();

        // Export
        Bind<IPdfExporter>().To<PdfExporter>();
        Bind<IEmailProviderFactory>().To<NinjectEmailProviderFactory>();
        Bind<IMapiWrapper>().To<MapiWrapper>();
        Bind<OcrRequestQueue>().ToSelf().InSingletonScope();

        // Scan
        Bind<IScanPerformer>().To<ScanPerformer>();
        Bind<ILocalPostProcessor>().To<LocalPostProcessor>();
        Bind<IRemotePostProcessor>().To<RemotePostProcessor>();
        Bind<IScanBridgeFactory>().To<ScanBridgeFactory>();
        Bind<IScanDriverFactory>().To<ScanDriverFactory>();
        Bind<IRemoteScanController>().To<RemoteScanController>();
        Bind<InProcScanBridge>().ToSelf();
        Bind<WorkerScanBridge>().ToSelf();
        Bind<NetworkScanBridge>().ToSelf();

        // Config
        Bind<Naps2Config>().ToMethod(_ =>
            new Naps2Config(Path.Combine(Paths.Executable, "appsettings.xml"),
                Path.Combine(Paths.AppData, "config.xml"))).InSingletonScope();

        // Host
        Bind<IWorkerFactory>().To<WorkerFactory>().InSingletonScope();

        // Misc
        Bind<IFormFactory>().To<NinjectFormFactory>();
        Bind<IOperationFactory>().To<NinjectOperationFactory>();
        Bind<ILogger>().To<NLogLogger>().InSingletonScope();
        Bind<UiImageList>().ToSelf().InSingletonScope();
        Bind<StillImage>().ToSelf().InSingletonScope();
        Bind<AutoSaver>().ToSelf();
        // TODO: Use PdfiumWorkerCoordinator?
        Bind<IPdfRenderer>().To<PdfiumPdfRenderer>();
        Bind<ScanningContext>().ToSelf().InSingletonScope();
        Bind<OcrOperationManager>().ToSelf().InSingletonScope();
        Bind<ThumbnailRenderQueue>().ToSelf().InSingletonScope();
        Bind<IDevicePrompt>().To<EtoDevicePrompt>();

        //Kernel.Get<ImageContext>().PdfRenderer = Kernel.Get<PdfiumWorkerCoordinator>();

        Bind<IProfileManager>().ToMethod(ctx =>
        {
            var config = ctx.Kernel.Get<Naps2Config>();
            return new ProfileManager(
                Path.Combine(Paths.AppData, "profiles.xml"),
                Path.Combine(AssemblyHelper.EntryFolder, "profiles.xml"),
                config.Get(c => c.LockSystemProfiles),
                config.Get(c => c.LockUnspecifiedDevices),
                config.Get(c => c.NoUserProfiles));
        }).InSingletonScope();

        Bind<TesseractLanguageManager>().ToMethod(ctx =>
        {
            var config = ctx.Kernel.Get<Naps2Config>();
            var customComponentsPath = config.Get(c => c.ComponentsPath);
            var componentsPath = string.IsNullOrWhiteSpace(customComponentsPath)
                ? Paths.Components
                : Environment.ExpandEnvironmentVariables(customComponentsPath);
            return new TesseractLanguageManager(componentsPath);
        }).InSingletonScope();
        Bind<IOcrEngine>().ToMethod(ctx =>
        {
            var tesseractPath = PlatformCompat.System.UseSystemTesseract
                ? "tesseract"
                : NativeLibrary.FindPath(PlatformCompat.System.TesseractExecutableName!);
            return new TesseractOcrEngine(
                tesseractPath,
                ctx.Kernel.Get<TesseractLanguageManager>().TessdataBasePath,
                Paths.Temp);
        }).InSingletonScope();
    }
}