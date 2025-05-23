using System.Xml.Serialization;
using NAPS2.Escl;
using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Scan;

namespace NAPS2.Config.ObsoleteTypes;

[XmlType("AppConfig")]
public class AppConfigV0
{
    public int Version { get; set; }

    public string? DefaultCulture { get; set; }

    public string? StartupMessageTitle { get; set; }

    public string? StartupMessageText { get; set; }

    public MessageBoxIcon StartupMessageIcon { get; set; }

    public ScanProfile? DefaultProfileSettings { get; set; }

    public Theme Theme { get; set; }

    public bool ShowPageNumbers { get; set; }

    public bool ShowProfilesToolbar { get; set; }

    public bool ScanChangesDefaultProfile { get; set; }

    public ScanButtonDefaultAction ScanButtonDefaultAction { get; set; }

    public SaveButtonDefaultAction SaveButtonDefaultAction { get; set; }

    public bool DeleteAfterSaving { get; set; }

    public bool KeepSession { get; set; }

    public bool SingleInstance { get; set; }

    public bool HideOcrButton { get; set; }

    public bool HideImportButton { get; set; }

    public bool HideSavePdfButton { get; set; }

    public bool HideSaveImagesButton { get; set; }

    public bool HideEmailButton { get; set; }

    public bool HidePrintButton { get; set; }

    public bool HideSettingsButton { get; set; }

    public bool HideDonateButton { get; set; }

    public bool HideSidebar { get; set; }

    public bool DisableAutoSave { get; set; }

    public bool LockSystemProfiles { get; set; }

    public bool LockUnspecifiedDevices { get; set; }

    public bool NoUserProfiles { get; set; }

    public bool AlwaysRememberDevice { get; set; }

    public bool NoUpdatePrompt { get; set; }

    public bool NoDebugLogging { get; set; }

    public bool NoScannerSharing { get; set; }

    public bool DisableSaveNotifications { get; set; }

    public string? ComponentsPath { get; set; }

    public double OcrTimeoutInSeconds { get; set; }

    public OcrState OcrState { get; set; }

    public string? OcrDefaultLanguage { get; set; }

    public LocalizedOcrMode OcrDefaultMode { get; set; }

    public bool OcrDefaultAfterScanning { get; set; }

    public PdfCompat ForcePdfCompat { get; set; }

    public EsclSecurityPolicy EsclSecurityPolicy { get; set; }

    public string? EsclServerCertificatePath { get; set; }

    public EventType EventLogging { get; set; }

    public KeyboardShortcuts? KeyboardShortcuts { get; set; }
}