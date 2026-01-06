using System.Net;
using System.Net.Mail;

namespace PterodactylToCloudflareDNS;

public static class EmailProvider
{
	public static string CompanyName = "";
	public static string BrandColor = "";
	public static string CompanyLogoUrl = "";

	private static string _smtpHost = "";
	private static int _smtpPort = 587;
	private static string _smtpUsername = "";
	private static string _smtpPassword = "";
	private static string _fromAddress = "";
	private static string[] _subscribers = [];
	private static bool _isEnabled = false;

	public static void Initialize(string companyName, string brandColor, string companyLogoUrl,
		string smtpHost, int smtpPort, string smtpUsername, string smtpPassword,
		string fromAddress, string[] subscribers)
	{
		CompanyName = companyName;
		BrandColor = brandColor;
		CompanyLogoUrl = companyLogoUrl;
		_smtpHost = smtpHost;
		_smtpPort = smtpPort;
		_smtpUsername = smtpUsername;
		_smtpPassword = smtpPassword;
		_fromAddress = fromAddress;
		_subscribers = subscribers;
		_isEnabled = !string.IsNullOrEmpty(smtpHost) && subscribers.Length > 0;
	}

	public static async Task SendEmail(string subject, string htmlBody)
	{
		if (!_isEnabled)
		{
			await Logging.LogDebug("Email", "Email notifications are disabled. Skipping email send.");
			return;
		}

		try
		{
			var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
			{
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
				EnableSsl = true
			};

			foreach (var subscriber in _subscribers)
			{
				var mailMessage = new MailMessage
				{
					From = new MailAddress(_fromAddress, CompanyName),
					Subject = subject,
					Body = htmlBody,
					IsBodyHtml = true,
					BodyEncoding = System.Text.Encoding.UTF8,
					SubjectEncoding = System.Text.Encoding.UTF8
				};
				mailMessage.To.Add(subscriber);

				await smtpClient.SendMailAsync(mailMessage);
				await Logging.LogDebug("Email", $"Email sent to {subscriber}: {subject}");
			}
		}
		catch (Exception ex)
		{
			await Logging.LogError("Email", $"Failed to send email: {ex.Message}");
		}
	}
}

public static class EmailTemplates
{
	public static string NewDnsRecordsCreated(string domain, string aRecordIp, string srvRecordName, int srvPriority, int srvWeight, int srvPort, string srvTarget)
	{
		return File.ReadAllText("templates/NewDnsRecordsCreated.html")
			.Replace("{{companyName}}", EmailProvider.CompanyName)
			.Replace("{{brandColor}}", EmailProvider.BrandColor)
			.Replace("{{companyLogoUrl}}", EmailProvider.CompanyLogoUrl)
			.Replace("{{domain}}", domain)
			.Replace("{{aRecordIp}}", aRecordIp)
			.Replace("{{srvRecordName}}", srvRecordName)
			.Replace("{{srvPriority}}", srvPriority.ToString())
			.Replace("{{srvWeight}}", srvWeight.ToString())
			.Replace("{{srvPort}}", srvPort.ToString())
			.Replace("{{srvTarget}}", srvTarget)
			.Replace("{{timestamp}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
	}

	public static string DnsRecordsUpdated(string domain, string aRecordIp, string srvRecordName, int srvPriority, int srvWeight, int srvPort, string srvTarget)
	{
		return File.ReadAllText("templates/DnsRecordsUpdated.html")
			.Replace("{{companyName}}", EmailProvider.CompanyName)
			.Replace("{{brandColor}}", EmailProvider.BrandColor)
			.Replace("{{companyLogoUrl}}", EmailProvider.CompanyLogoUrl)
			.Replace("{{domain}}", domain)
			.Replace("{{aRecordIp}}", aRecordIp)
			.Replace("{{srvRecordName}}", srvRecordName)
			.Replace("{{srvPriority}}", srvPriority.ToString())
			.Replace("{{srvWeight}}", srvWeight.ToString())
			.Replace("{{srvPort}}", srvPort.ToString())
			.Replace("{{srvTarget}}", srvTarget)
			.Replace("{{timestamp}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
	}

	public static string DnsRecordsDeleted(string domain, string aRecordIp, string srvRecordName, int srvPriority, int srvWeight, int srvPort, string srvTarget, string reason = "Server no longer exists")
	{
		return File.ReadAllText("templates/DnsRecordsDeleted.html")
			.Replace("{{companyName}}", EmailProvider.CompanyName)
			.Replace("{{brandColor}}", EmailProvider.BrandColor)
			.Replace("{{companyLogoUrl}}", EmailProvider.CompanyLogoUrl)
			.Replace("{{domain}}", domain)
			.Replace("{{aRecordIp}}", aRecordIp)
			.Replace("{{srvRecordName}}", srvRecordName)
			.Replace("{{srvPriority}}", srvPriority.ToString())
			.Replace("{{srvWeight}}", srvWeight.ToString())
			.Replace("{{srvPort}}", srvPort.ToString())
			.Replace("{{srvTarget}}", srvTarget)
			.Replace("{{reason}}", reason)
			.Replace("{{timestamp}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
	}

	public static string DnsRecordCreationFailed(string domain, string type, string reason)
	{
		return File.ReadAllText("templates/DnsRecordError.html")
			.Replace("{{companyName}}", EmailProvider.CompanyName)
			.Replace("{{brandColor}}", EmailProvider.BrandColor)
			.Replace("{{companyLogoUrl}}", EmailProvider.CompanyLogoUrl)
			.Replace("{{domain}}", domain)
			.Replace("{{recordType}}", type)
			.Replace("{{errorMessage}}", reason)
			.Replace("{{operation}}", "creation")
			.Replace("{{timestamp}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
	}

	public static string DnsRecordUpdateFailed(string domain, string type, string reason)
	{
		return File.ReadAllText("templates/DnsRecordError.html")
			.Replace("{{companyName}}", EmailProvider.CompanyName)
			.Replace("{{brandColor}}", EmailProvider.BrandColor)
			.Replace("{{companyLogoUrl}}", EmailProvider.CompanyLogoUrl)
			.Replace("{{domain}}", domain)
			.Replace("{{recordType}}", type)
			.Replace("{{errorMessage}}", reason)
			.Replace("{{operation}}", "update")
			.Replace("{{timestamp}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
	}

	public static string DnsRecordDeletionFailed(string domain, string type, string reason)
	{
		return File.ReadAllText("templates/DnsRecordError.html")
			.Replace("{{companyName}}", EmailProvider.CompanyName)
			.Replace("{{brandColor}}", EmailProvider.BrandColor)
			.Replace("{{companyLogoUrl}}", EmailProvider.CompanyLogoUrl)
			.Replace("{{domain}}", domain)
			.Replace("{{recordType}}", type)
			.Replace("{{errorMessage}}", reason)
			.Replace("{{operation}}", "deletion")
			.Replace("{{timestamp}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
	}
}