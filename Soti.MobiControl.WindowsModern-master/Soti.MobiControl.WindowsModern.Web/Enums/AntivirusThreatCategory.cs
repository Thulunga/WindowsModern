namespace Soti.MobiControl.WindowsModern.Web.Enums;

/// <summary>
/// Enum representing different categories of antivirus threats.
/// </summary>
public enum AntivirusThreatCategory
{
    /// <summary>
    /// Unknown threat category.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// General malicious software.
    /// </summary>
    Malware = 1,

    /// <summary>
    /// Ad-supported software.
    /// </summary>
    Adware = 2,

    /// <summary>
    /// Software that dials phone numbers.
    /// </summary>
    Dialer = 3,

    /// <summary>
    /// Spyware that gathers user data.
    /// </summary>
    Spyware = 4,

    /// <summary>
    /// General application-based threat.
    /// </summary>
    App = 5,

    /// <summary>
    /// Invalid or unrecognized threat.
    /// </summary>
    Invalid = 6,

    /// <summary>
    /// Malware that steals passwords.
    /// </summary>
    PasswordStealer = 7,

    /// <summary>
    /// Downloads other malicious programs.
    /// </summary>
    TrojanDownloader = 8,

    /// <summary>
    /// Self-replicating network worm.
    /// </summary>
    Worm = 9,

    /// <summary>
    /// Malware providing unauthorized access.
    /// </summary>
    Backdoor = 10,

    /// <summary>
    /// Trojan allowing remote system access.
    /// </summary>
    RemoteAccessTrojan = 11,

    /// <summary>
    /// Malicious software disguised as legit.
    /// </summary>
    Trojan = 12,

    /// <summary>
    /// Software flooding inboxes with emails.
    /// </summary>
    EmailFlooder = 13,

    /// <summary>
    /// Software that logs keystrokes.
    /// </summary>
    Keylogger = 14,

    /// <summary>
    /// Software that monitors user activity.
    /// </summary>
    MonitoringSoftware = 15,

    /// <summary>
    /// Modifies browser behavior.
    /// </summary>
    BrowserModifier = 16,

    /// <summary>
    /// Small files stored by websites.
    /// </summary>
    Cookie = 17,

    /// <summary>
    /// Browser plugins with malicious intent.
    /// </summary>
    BrowserPlugin = 18,

    /// <summary>
    /// Exploits AOL software vulnerabilities.
    /// </summary>
    AOLExploit = 19,

    /// <summary>
    /// Software that crashes or damages systems.
    /// </summary>
    Nuker = 20,

    /// <summary>
    /// Disables security measures.
    /// </summary>
    SecurityDisabler = 21,

    /// <summary>
    /// Disruptive but generally harmless joke programs.
    /// </summary>
    JokeProgram = 22,

    /// <summary>
    /// Malicious ActiveX controls.
    /// </summary>
    HostileActiveXControl = 23,

    /// <summary>
    /// Software bundled with other apps.
    /// </summary>
    SoftwareBundler = 24,

    /// <summary>
    /// Stealthy modifications to evade detection.
    /// </summary>
    StealthModifier = 25,

    /// <summary>
    /// Modifies system settings.
    /// </summary>
    SettingsModifier = 26,

    /// <summary>
    /// Unwanted browser toolbars.
    /// </summary>
    Toolbar = 27,

    /// <summary>
    /// Remote control software, often used maliciously.
    /// </summary>
    RemoteControlSoftware1 = 28,

    /// <summary>
    /// Exploits FTP for malicious activities.
    /// </summary>
    TrojanFTP = 29,

    /// <summary>
    /// Potentially unwanted software.
    /// </summary>
    PotentialUnwantedSoftware = 30,

    /// <summary>
    /// Exploits ICQ vulnerabilities.
    /// </summary>
    ICQExploit = 31,

    /// <summary>
    /// Exploits Telnet for malicious purposes.
    /// </summary>
    TrojanTelnet = 32,

    /// <summary>
    /// Exploits vulnerabilities in software.
    /// </summary>
    Exploit = 33,

    /// <summary>
    /// File-sharing programs, sometimes used maliciously.
    /// </summary>
    FileSharingProgram = 34,

    /// <summary>
    /// Tools for creating malware.
    /// </summary>
    MalwareCreationTool = 35,

    /// <summary>
    /// Remote control software, often used maliciously.
    /// </summary>
    RemoteControlSoftware2 = 36,

    /// <summary>
    /// General tools used for malicious purposes.
    /// </summary>
    Tool = 37,

    /// <summary>
    /// Denial-of-service attacks via trojans.
    /// </summary>
    TrojanDenialOfService = 38,

    /// <summary>
    /// Trojan that drops other malware.
    /// </summary>
    TrojanDropper = 39,

    /// <summary>
    /// Trojan that sends mass spam emails.
    /// </summary>
    TrojanMassMailer = 40,

    /// <summary>
    /// Monitoring software delivered via trojan.
    /// </summary>
    TrojanMonitoringSoftware = 41,

    /// <summary>
    /// Trojan that acts as a proxy server.
    /// </summary>
    TrojanProxyServer = 42,

    /// <summary>
    /// Replicating malware that spreads.
    /// </summary>
    Virus = 43,

    /// <summary>
    /// Recognized and known threats.
    /// </summary>
    Known = 44,

    /// <summary>
    /// Software Protection Platform-related threats.
    /// </summary>
    SPP = 45,

    /// <summary>
    /// Behavior-based threat detection.
    /// </summary>
    Behavior = 46,

    /// <summary>
    /// Exploitable vulnerabilities.
    /// </summary>
    Vulnerability = 47,

    /// <summary>
    /// Policy-related threats.
    /// </summary>
    Policy = 48,

    /// <summary>
    /// End User Security-related threats.
    /// </summary>
    EUS = 49,

    /// <summary>
    /// Ransomware that holds data hostage.
    /// </summary>
    Ransomware = 50,

    /// <summary>
    /// Attack Surface Reduction rules.
    /// </summary>
    ASRRule = 51
}
