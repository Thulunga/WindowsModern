namespace Soti.MobiControl.WindowsModern.Web.Contracts;

/// <summary>
/// The Peripheral Email Report Parameters class.
/// </summary>
public sealed class PeripheralEmailReportParameters
{
    /// <summary>
    /// Gets or sets the name of the profile to use when sending the email.
    /// </summary>
    public string EmailProfileName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating If set to 'true' then append the list of recipients from <see cref="ToAddresses"/>, <see cref="CcAddresses"/>, and <see cref="BccAddresses"/>.
    /// </summary>
    public bool AppendAddresses { get; set; }

    /// <summary>
    /// Gets or sets The addresses to which the email will be sent. If specified, it will replace all 'To' addresses present in the email profile unless <see cref="AppendAddresses"/>.
    /// </summary>
    public string[] ToAddresses { get; set; }

    /// <summary>
    /// Gets or sets Any addresses to include in the CC field of the email. If specified, it will replace all CC addresses present in the email profile unless <see cref="AppendAddresses"/>.
    /// </summary>
    public string[] CcAddresses { get; set; }

    /// <summary>
    /// Gets or sets Any addresses to include in the BCC field of the email. If specified, it will replace all BCC addresses present in the email profile unless <see cref="AppendAddresses"/>.
    /// </summary>
    public string[] BccAddresses { get; set; }

    /// <summary>
    /// Gets or sets The subject of the email.If no subject is set, the email will be sent with an empty subject.
    /// </summary>
    public string EmailSubject { get; set; }

    /// <summary>
    /// Gets or sets The email message to send.If no message is set, the email will be sent with an empty message.
    /// </summary>
    public string EmailBody { get; set; }

    /// <summary>
    /// Gets or sets name contains.
    /// </summary>
    public string NameContains { get; set; }

    /// <summary>
    /// Gets or sets Field names to be included in the report, these will be used as the header of the attached CSV file.
    /// </summary>
    public string[] ReportHeaderFields { get; set; }

    /// <summary>
    /// Gets or sets policy statuses.
    /// </summary>
    public string[] Statuses { get; set; }

    /// <summary>
    /// Gets or sets peripheral type.
    /// </summary>
    public string[] PeripheralType { get; set; }

    /// <summary>
    /// Gets or sets the local time zone offset from UTC.
    /// </summary>
    public int? TimeOffset { get; set; }
}
