using Eto.Drawing;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Email.Oauth;
using NAPS2.Scan;

namespace NAPS2.ImportExport.Email;

internal class EmailProviderController
{
    private readonly IFormFactory _formFactory;
    private readonly Naps2Config _config;
    private readonly SystemEmailClients _systemEmailClients;
    private readonly GmailOauthProvider _gmailOauthProvider;
    private readonly OutlookNewEmailProvider _outlookNewEmailProvider;
    private readonly OutlookWebOauthProvider _outlookWebOauthProvider;
    private readonly ThunderbirdEmailProvider _thunderbirdProvider;
    private readonly IIconProvider _iconProvider;

    public EmailProviderController(IFormFactory formFactory, Naps2Config config, SystemEmailClients systemEmailClients,
        GmailOauthProvider gmailOauthProvider, OutlookNewEmailProvider outlookNewEmailProvider,
        OutlookWebOauthProvider outlookWebOauthProvider, ThunderbirdEmailProvider thunderbirdProvider,
        IIconProvider iconProvider)
    {
        _formFactory = formFactory;
        _config = config;
        _systemEmailClients = systemEmailClients;
        _gmailOauthProvider = gmailOauthProvider;
        _outlookNewEmailProvider = outlookNewEmailProvider;
        _outlookWebOauthProvider = outlookWebOauthProvider;
        _thunderbirdProvider = thunderbirdProvider;
        _iconProvider = iconProvider;
    }

    public List<EmailProviderWidget> GetWidgets()
    {
        var providerWidgets = new List<EmailProviderWidget>();
        var userSetup = _config.Get(c => c.EmailSetup);

        if (OperatingSystem.IsWindowsVersionAtLeast(7))
        {
            var systemClientNames = _systemEmailClients.GetNames();
            var defaultSystemClientName = _systemEmailClients.GetDefaultName();

            foreach (var clientName in systemClientNames.OrderBy(x =>
                         x == userSetup.SystemProviderName ? 0 : x == defaultSystemClientName ? 1 : 2))
            {
                providerWidgets.Add(GetWidget(EmailProviderType.System, clientName));
            }
        }

        void MaybeAddWidget(EmailProviderType type, bool condition)
        {
            if (condition)
            {
                providerWidgets.Add(GetWidget(type));
            }
        }

        // For Windows we expect Thunderbird to be used through MAPI. For Linux we need to handle it specially.
        MaybeAddWidget(EmailProviderType.Thunderbird, OperatingSystem.IsLinux());
        MaybeAddWidget(EmailProviderType.AppleMail, OperatingSystem.IsMacOS());
        MaybeAddWidget(EmailProviderType.OutlookNew, _outlookNewEmailProvider.IsAvailable);
        MaybeAddWidget(EmailProviderType.Gmail, _gmailOauthProvider.HasClientCreds);
        MaybeAddWidget(EmailProviderType.OutlookWeb, _outlookWebOauthProvider.HasClientCreds);

        // Sort the currently-selected provider to the top
        return providerWidgets.OrderBy(widget => widget.ProviderType == userSetup.ProviderType ? 0 : 1).ToList();
    }

    private EmailProviderWidget GetWidget(EmailProviderType type, string? clientName = null)
    {
        return type switch
        {
            EmailProviderType.System => new EmailProviderWidget
            {
                ProviderType = EmailProviderType.System,
                ProviderIcon = GetSystemIcon(clientName!),
                ProviderIconName = "mail_yellow",
                ProviderName = clientName!,
                Choose = () => ChooseSystem(clientName!)
            },
            EmailProviderType.Thunderbird => new EmailProviderWidget
            {
                ProviderType = EmailProviderType.Thunderbird,
                ProviderIconName = "thunderbird",
                ProviderName = EmailProviderType.Thunderbird.Description(),
                Choose = () => ChooseProviderType(EmailProviderType.Thunderbird),
                // When Thunderbird isn't available, we disable it rather than hide it.
                // The point is to give a hint to the user that Thunderbird support is present.
                Enabled = _thunderbirdProvider.IsAvailable
            },
            EmailProviderType.AppleMail => new EmailProviderWidget
            {
                ProviderType = EmailProviderType.AppleMail,
                ProviderIconName = "apple_mail",
                ProviderName = EmailProviderType.AppleMail.Description(),
                Choose = () => ChooseProviderType(EmailProviderType.AppleMail),
            },
            EmailProviderType.Gmail => new EmailProviderWidget
            {
                ProviderType = EmailProviderType.Gmail,
                ProviderIconName = "gmail",
                ProviderName = EmailProviderType.Gmail.Description(),
                Choose = () => ChooseOauth(_gmailOauthProvider)
            },
            EmailProviderType.OutlookNew => new EmailProviderWidget
            {
                ProviderType = EmailProviderType.OutlookNew,
                ProviderIconName = "outlooknew",
                ProviderName = EmailProviderType.OutlookNew.Description(),
                Choose = () => ChooseProviderType(EmailProviderType.OutlookNew),
            },
            EmailProviderType.OutlookWeb => new EmailProviderWidget
            {
                ProviderType = EmailProviderType.OutlookWeb,
                ProviderIconName = "outlookweb",
                ProviderName = EmailProviderType.OutlookWeb.Description(),
                Choose = () => ChooseOauth(_outlookWebOauthProvider)
            },
            // EmailProviderType.CustomSmtp => new EmailProviderWidget
            // {
            //     ProviderType = EmailProviderType.CustomSmtp,
            //     ProviderIconName = "email_setting",
            //     ProviderName = EmailProviderType.CustomSmtp.Description(),
            //     Choose = ChooseCustomSmtp
            // },
            _ => throw new ArgumentException()
        };
    }

    private Bitmap? GetSystemIcon(string clientName)
    {
#if NET6_0_OR_GREATER
        if (!OperatingSystem.IsWindowsVersionAtLeast(7))
        {
            throw new InvalidOperationException();
        }
#endif
        var exePath = _systemEmailClients.GetExePath(clientName);
        return exePath == null ? null : EtoPlatform.Current.ExtractAssociatedIcon(exePath);
    }

    private bool ChooseSystem(string clientName)
    {
        var transact = _config.User.BeginTransaction();
        transact.Remove(c => c.EmailSetup);
        transact.Set(c => c.EmailSetup.SystemProviderName, clientName);
        transact.Set(c => c.EmailSetup.ProviderType, EmailProviderType.System);
        transact.Commit();
        return true;
    }

    private bool ChooseProviderType(EmailProviderType providerType)
    {
        var transact = _config.User.BeginTransaction();
        transact.Remove(c => c.EmailSetup);
        transact.Set(c => c.EmailSetup.ProviderType, providerType);
        transact.Commit();
        return true;
    }

    private bool ChooseOauth(OauthProvider provider)
    {
        var authForm = _formFactory.Create<AuthorizeForm>();
        authForm.OauthProvider = provider;
        authForm.ShowModal();
        return authForm.Result;
    }
}